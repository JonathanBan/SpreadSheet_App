// Jonathan Banos 11667134
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SpreadsheetEngine1;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Cpts321
{
    /// <summary>
    /// This is the spreadsheet class that has the properties for the spreadsheet.
    /// </summary>
    public class SpreadSheet
    {
        // This is the members for the rows and colums
        private int numberOfRows;
        private int numberOfColumns;

        // got from microsoft documentation
        public event PropertyChangedEventHandler CellPropertyChanged = delegate { };

        // calls the cell class and makes a cell in spreadsheet
        private Cell[,] cellSpreadsheet;

        private Dictionary<Cell, List<Cell>> dependencies;

        // Added the stack for muliplecommands that handles the redo and undo
        private Stack<MulitCommand> undos = new Stack<MulitCommand>();
        private Stack<MulitCommand> redos = new Stack<MulitCommand>();

        // THis is a hash set to help us deal with the cells visited to help deal with exceptions
        private HashSet<Cell> cellsVisit = new HashSet<Cell>();

        // since we cant create and instance cell we have to make one
        private class InstanceCell : Cell
        {
            public InstanceCell(int newRows, int newColumns) : base(newRows, newColumns)
            {
            }
        }

        /// <summary>
        /// This is the spreadsheet contructor with rowns and columns.
        /// </summary>
        /// <param name="newRows"></param>
        /// <param name="newColumns"></param>
        public SpreadSheet(int newRows, int newColumns)
        {
            // assign new rows anad columsns to the member rows and columns
            numberOfRows = newRows;
            numberOfColumns = newColumns;
            dependencies = new Dictionary<Cell, List<Cell>>();

            // 2d array
            cellSpreadsheet = new Cell[newRows, newColumns];

            // Going to need a nested loop to go through cells
            // This is a nested loop that makes cells into instance cells and adds it to the whole spreadsheet
            for (int i = 0; i < newRows; i++)
            {
                for (int j = 0; j < newColumns; j++)
                {
                    Cell newCell = new InstanceCell(i, j);
                    newCell.PropertyChanged += Update;
                    cellSpreadsheet[i, j] = newCell;
                }
            }
        }

        /// <summary>
        /// This function lets the cell be updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update(object sender, PropertyChangedEventArgs e)
        {
            // if the text property changed then we go into the statement
            if ("Text" == e.PropertyName)
            {
                Cell updateCell = sender as Cell;

                // We call the update function to take in the cell
                Update(updateCell);
            }
            else if ("Color" == e.PropertyName)
            {
                Cell updateCell = sender as Cell;
                CellPropertyChanged(updateCell, new PropertyChangedEventArgs("ColorChanged"));
            }

            // this tells the subsriber that the cell is changed
            // CellPropertyChanged(sender, new PropertyChangedEventArgs("CellChange"));
        }

        /// <summary>
        /// Need to make an overloaded function for update that will take
        /// in a cell to be updated.
        /// </summary>
        /// <param name="updateCell"></param>
        private void Update(Cell updateCell)
        {
            // have to remove the previous dependencies
            RemoveDependency(updateCell);

            // Have to check if the cell is empty first
            if (string.IsNullOrEmpty(updateCell.Text))
            {
                updateCell.Value = "";
            }
            else if (updateCell.Text[0] != '=')
            {
                // this is if the text doesn't begin with a =
                double value;

                if (double.TryParse(updateCell.Text, out value))
                {
                    // This will make the expresion three for the cells variable
                    ExpressionTree tree = new ExpressionTree(updateCell.Text);

                    // This sets the value to what the tree evaluates
                    value = tree.Evaluate();

                    // This sets the variable to the value
                    tree.SetVariable(updateCell.Name, value);

                    // We make the value to the double
                    updateCell.Value = value.ToString();
                }
                else
                {
                    // the value is set to what the text is
                    updateCell.Value = updateCell.Text;
                }
            }
            else
            {
                bool wrong = false;

                // We get the text and assign it to the expression
                string expression = updateCell.Text.Substring(1);

                // we have to create the expression tree just like above
                ExpressionTree tree = new ExpressionTree(expression);
                tree.Evaluate();

                string[] variableNames = tree.GetVariableNames();

                foreach (string variable in variableNames)
                {
                    // We get the cell and the value in the dictionary
                    double valuee = 0.0;
                    Cell relevantCell = GetCell(variable);

                    // Checking in each cell if the the relevant cell is nothing then its a bad reference
                    if (relevantCell == null)
                    {
                        updateCell.Value = "!(bad reference)";
                        CellPropertyChanged(updateCell, new PropertyChangedEventArgs("CellChange"));
                        wrong = true;
                    }

                    // checking if the cell name the user puts is pointing to the same cell
                    else if (updateCell.Name == variable)
                    {
                        updateCell.Value = "!(Self reference)";
                        CellPropertyChanged(updateCell, new PropertyChangedEventArgs("CellChange"));
                        wrong = true;
                    }

                    // If there is an error then we update the dependency and gets out of function
                    if (wrong == true)
                    {
                        if (dependencies.ContainsKey(updateCell))
                        {
                            UpdateDependency(updateCell);
                        }

                        return;
                    }

                    // parse ot the double and then set the variable
                    double.TryParse(relevantCell.Value, out valuee);
                    tree.SetVariable(variable, valuee);
                    cellsVisit.Add(relevantCell);
                }

                // we set the value to what the evaluate got and made it a string
                updateCell.Value = tree.Evaluate().ToString();

                // Added the dependency
                AddDependency(updateCell, variableNames);

                // checks if it's a circular reference
                if (CheckForCircularReference(updateCell))
                {
                    updateCell.Value = "!(circular reference)";
                    CellPropertyChanged(updateCell, new PropertyChangedEventArgs("CellChange"));
                    wrong = true;
                }

                // leaves the finction if theres an error
                if (wrong == true) return;
            }

            // it checks if there are cells that are depended
            if (dependencies.ContainsKey(updateCell))
            {
                // updates all the dependent cells if the update cell was in it
                UpdateDependency(updateCell);
            }

            // This tells the subscribers that the cell changed
            CellPropertyChanged(updateCell, new PropertyChangedEventArgs("CellChange"));
            cellsVisit.Clear();
        }

        // getter for the rows
        public int RowCount { get { return numberOfRows; } }

        // getter for the columns
        public int ColumnCount { get { return numberOfColumns; } }

        public Cell GetCell(int rowIndex, int columnIndex) { return cellSpreadsheet[rowIndex, columnIndex]; }

        // This is another get cell function that could take in a cellname
        public Cell GetCell(string cellName)
        {
            // checks if the first character is a letter
            if (!Char.IsLetter(cellName[0]))
            {
                return null;
            }

            // checks if the first letter is uppercase
            if (!Char.IsUpper(cellName[0]))
            {
                return null;
            }


            // This converts the character to the index number
            int column = cellName[0] - 'A';

            int row;


            // checks if its a valid integer row
            if (!Int32.TryParse(cellName.Substring(1), out row))
            {
                return null;
            }

            Cell cell;

            // we try to get the cell
            try
            {
                cell = GetCell(row - 1, column);
            }
            catch (Exception)
            {
                return null;
            }


            // returns the cell
            return cell;
        }

        /// <summary>
        /// This function adds dependecies for the cell.
        /// </summary>
        /// <param name="refferenceCell"></param>
        /// <param name="independents"></param>
        private void AddDependency(Cell refferenceCell, string[] independents)
        {
            foreach (string independentsCell in independents)
            {
                // gets the independent cell
                Cell independentCell = GetCell(independentsCell);

                // adds the referenced dell to the independedn cells ref
                if (!dependencies.ContainsKey(independentCell))
                {
                    dependencies[independentCell] = new List<Cell>();
                }

                dependencies[independentCell].Add(refferenceCell);
            }
        }

        /// <summary>
        /// THis just removes the dependency.
        /// </summary>
        /// <param name="refferenceCell"></param>
        private void RemoveDependency(Cell refferenceCell)
        {
            // goes through all of the values
            foreach (List<Cell> dependenCells in dependencies.Values)
            {
                // if the cell is int the list  then it will remove it with the list functions
                if (dependenCells.Contains(refferenceCell))
                {
                    dependenCells.Remove(refferenceCell);
                }
            }
        }

        /// <summary>
        /// updates all dependent cells.
        /// </summary>
        /// <param name="independenCells"></param>
        private void UpdateDependency(Cell independenCells)
        {
            foreach (Cell independenCell in dependencies[independenCells].ToArray())
            {
                Update(independenCell);
            }
        }

        public void AddUndo(MulitCommand undo)
        {
            undos.Push(undo);
        }

        // This is a function is a getter that returns true if the undo stack is empty
        public bool UndoIsEmpty { get { return undos.Count == 0; } }

        // This is a function is a getter that returns true if the redo stack is empty
        public bool RedoIsEmpty { get { return redos.Count == 0; } }

        /// <summary>
        /// THis is the undo command that gets the command name if it is there.
        /// </summary>
        public string UndoCommand
        {
            get
            {
                // Makes sure the stack isn't empty 
                if (!UndoIsEmpty)
                {
                    // returns the command name if its there
                    return undos.Peek().CommandName;
                }

                // This returns an empty string if there is nothing in the stack
                return "";
            }
        }

        /// <summary>
        /// THis is the redo command that gets the command name if it is there.
        /// </summary>
        public string RedoCommand
        {
            get
            {
                // Makes sure the stack isn't empty
                if (!RedoIsEmpty)
                {
                    // returns the command name if its there
                    return redos.Peek().CommandName;
                }

                // This returns an empty string if there is nothing in the stack
                return "";
            }
        }

        /// <summary>
        /// THis is the undo function that pops from the undo stack and pops it to the redo stack.
        /// </summary>
        public void Undo()
        {
            // This checks if theere is something in the stack first
            if (!UndoIsEmpty)
            {
                // this takes out a command from the stack
                MulitCommand commands = undos.Pop();

                // THis adds the command to the redo stack
                redos.Push(commands.Exec());
            }
        }

        /// <summary>
        /// This is the redo function that pops a command from the redo stack and puts it int the undo stack.
        /// </summary>
        public void Redo()
        {
            // This checks if theere is something in the stack first
            if (!RedoIsEmpty)
            {
                // this takes out a command from the stack
                MulitCommand commands = redos.Pop();

                // THis adds the command to the undos stack
                undos.Push(commands.Exec());
            }
        }

        /// <summary>
        /// Got a lot of this from microsoft documentation. This saves the file in your computer.
        /// </summary>
        /// <param name="outfile"></param>
        public void Save(Stream outfile)
        {
            // We call the file which creates it with the type xml
            XmlWriter file = XmlWriter.Create(outfile);
            file.WriteStartElement("spreadsheet");

            // We iterate through the rows
            for (int i = 0; i < RowCount; i++)
            {
                // We iterate through the columns so it ends up being through the rows and columns
                for (int j = 0; j < ColumnCount; j++)
                {
                    // We initialize a current cell member of type cell
                    Cell current = cellSpreadsheet[i, j];

                    // We have checks of 'ors' to check that there is nothing in text,value, and color
                    if (current.Text != "" || current.Value != "" || current.BGColor != 0)
                    {
                        file.WriteStartElement("cell");
                        file.WriteElementString("name", current.Name.ToString());
                        file.WriteElementString("text", current.Text.ToString());
                        file.WriteElementString("backgroundColor", current.BGColor.ToString());
                        file.WriteEndElement();
                    }
                }
            }

            // we have the final write end element and we also close the file
            file.WriteEndElement();
            file.Close();
        }

        /// <summary>
        /// This loads the the file when we put a file, it loads it.
        /// </summary>
        /// <param name="infile"></param>
        public void Load(Stream infile)
        {
            // This loads the xml file from the infile
            XDocument file = XDocument.Load(infile);

            // Goes through the cell tags in the file
            foreach (XElement tags in file.Root.Elements("cell"))
            {
                // We get the cells
                Cell current = GetCell(tags.Element("name").Value);

                // it goes through each element and it the text is not empty then it makes the text to the text element
                if (tags.Element("text") != null)
                {
                    current.Text = tags.Element("text").Value.ToString();
                }

                // it goes through each element and checks if there is not something in the backround color
                if (tags.Element("backgroundColor") != null)
                {
                    // we turn the string to a uint
                    uint color = Convert.ToUInt32(tags.Element("backgroundColor").Value.ToString());

                    current.BGColor = color;
                }
            }
        }

        /// <summary>
        /// THis function clears the redo and undo stack but calling the clear in the stack method.
        /// </summary>
        public void ClearStacks()
        {
            // We call the function Clear on the redo and undo to clear everything out from it
            undos.Clear();
            redos.Clear();
        }

        /// <summary>
        /// THis function is to check for circular referneces within the spreadsheet.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool CheckForCircularReference(Cell cell)
        {
            if (cellsVisit.Add(cell) == false) return true;
            return false;
        }
    }
}
