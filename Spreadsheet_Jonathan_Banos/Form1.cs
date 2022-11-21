// Jonathan Banos 11667134
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cpts321;
using SpreadsheetEngine1;
using System.IO;

namespace Spreadsheet_Jonathan_Banos
{
    public partial class Form1 : Form
    {
        // Calls the spreadsheet class to make a spreadsheet.
        private SpreadSheet spreadSheetMember;

        public Form1()
        {
            InitializeComponent();

            // We assign the new spreadsheet with a row of 50 and column of 26.
            spreadSheetMember = new SpreadSheet(50, 26);

            // subscribes to the cellchanged.
            spreadSheetMember.CellPropertyChanged += Cellchanged;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            updatedEditMenu();
        }

        /// <summary>
        /// This is the form load so everything within it loads once the form loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // we have to clear the rows and columns before anything so it wont interfere with the previous  form.
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            // column index is the index for the column.
            int columnIndex = 0;

            // This is to convert the column into a string.
            string columnString;

            // for loop from A-Z that is generated to create the column header.
            for (char i = 'A'; i <= 'Z'; i++)
            {
                // this gets the i and turns the char to string and assigns it to columnstring.
                columnString = i.ToString();

                // this adds to the column.
                dataGridView1.Columns.Add("column" + columnIndex, columnString);

                // Adds to the column index.
                columnIndex++;
            }

            // string for the row.
            string rowString;

            // row that starts at 1 because of the header
            int tempRow = 1;

            // we have to add the rows before because then it doesn't work
            dataGridView1.Rows.Add(50);

            // This for loop is to add the numbers to the row headers
            for (int j = 0; j < 50; j++)
            {
                // turns the row to a string
                rowString = tempRow.ToString();

                // This assings a row to the current row
                var row = dataGridView1.Rows[j];

                // makes the row header to the rowstring
                row.HeaderCell.Value = rowString;

                // this goes to the next row
                tempRow++;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        /// <summary>
        /// This is called when the demo button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // makes a random value
            Random random = new Random();

            // makes a for loop on the 50 random strings.
            for (int i = 0; i < 50; i++)
            {
                // makes random values between 0-50 and 0-26 for the rows and columns
                int randRow = random.Next(0, 50);
                int randCoumn = random.Next(0, 26);

                // makes a cell class and calls the spreadsheet with the getcell function
                Cell presentCell = spreadSheetMember.GetCell(randRow, randCoumn);

                // Makes the text of the cell to hello world
                presentCell.Text = "Hello World!";
            }

            // adding for loop to add column B plus the number
            for (int i = 0; i < 50; i++)
            {
                // we make the present cell by calling the cell class and we get the value in column 1
                Cell presentCell = spreadSheetMember.GetCell(i, 1);

                // This makes the text be this is cell B and the current cell
                presentCell.Text = "This is cell B" + (i + 1);
            }

            // need to get the column A be the same as B
            for (int i = 0; i < 50; i++)
            {
                // This it the present cell and gets the value from column 0 which would be A
                Cell presetCell = spreadSheetMember.GetCell(i, 0);
                presetCell.Text = "=B" + i;
            }
        }

        /// <summary>
        /// This function is to update the cell and changes the form to if cell is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cellchanged(object sender, PropertyChangedEventArgs e)
        {
            // would be just like what we declared in spreadsheet engine
            // if the cell is changed it goes into the if statement
            if ("CellChange" == e.PropertyName)
            {
                // calls the cell clase and makes the object sender as a cell
                Cell updateCell = sender as Cell;

                // if its not null it goes into the if statement
                if (updateCell != null)
                {
                    // will find row and column
                    int rowCell = updateCell.RowIndex;
                    int columnCell = updateCell.ColumnIndex;

                    // Changes the datagrid cell value
                    dataGridView1.Rows[rowCell].Cells[columnCell].Value = updateCell.Value;
                }
            }
            else if ("ColorChanged" == e.PropertyName)
            {
                Cell updateCell = sender as Cell;

                if (updateCell != null)
                {
                    // will find row and column
                    int rowCell = updateCell.RowIndex;
                    int columnCell = updateCell.ColumnIndex;

                    // got this from the assignment
                    int intColor = (int)updateCell.BGColor;
                    Color color = Color.FromArgb(intColor);

                    // Changes the datagrid cell value
                    dataGridView1.Rows[rowCell].Cells[columnCell].Style.BackColor = color;
                }
            }
        }

        /// <summary>
        /// Got most of this code from the microsoft documentation but this helps with when the cell starts to get edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // We set the row and columns to e
            int row = e.RowIndex;
            int column = e.ColumnIndex;

            // This gets the cell
            Cell updateCell = spreadSheetMember.GetCell(row, column);
            if (updateCell != null)
            {
                dataGridView1.Rows[row].Cells[column].Value = updateCell.Text;
            }
        }

        /// <summary>
        /// Got most of this code form the microsoft documentation but this helps when the cell isn't edited anymore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Gets the row and column
            int row = e.RowIndex;
            int column = e.ColumnIndex;

            // This gets the cell
            Cell updateCell = spreadSheetMember.GetCell(row, column);

            // This checks if the cell got edited
            bool checkEdit = true;

            // Made a resotring text command for the text change
            RestoringText[] undoText = new RestoringText[1];

            string lastText = updateCell.Text;

            undoText[0] = new RestoringText(updateCell, lastText);

            if (updateCell != null)
            {
                // Had to make an exception to see if the text is deleted
                try
                {
                    // need to check if the text didn't change
                    if (updateCell.Text == dataGridView1.Rows[row].Cells[column].Value.ToString())
                    {
                        checkEdit = false;
                    }

                    // updates the text in the cell
                    updateCell.Text = dataGridView1.Rows[row].Cells[column].Value.ToString();
                }
                catch
                {
                    if (updateCell.Text == null) checkEdit = false;
                    updateCell.Text = "";
                }

                // Updates the cell to be able to show the value
                dataGridView1.Rows[row].Cells[column].Value = updateCell.Value;

                if (checkEdit == true)
                {
                    spreadSheetMember.AddUndo(new MulitCommand(undoText, "cell text change"));
                    updatedEditMenu();
                }
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// This will update the color to something else, got most of this code from the microsoft documentation given
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeBackroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Got this from micorsoft documentation which instantiates a type colorDialog
            ColorDialog colorDialog = new ColorDialog();

            List<RestoringColor> undoColors = new List<RestoringColor>();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                // Goes thorugh all cells
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    Cell updateCell = spreadSheetMember.GetCell(cell.RowIndex, cell.ColumnIndex);

                    uint lastColor = updateCell.BGColor;

                    if (lastColor == 0) lastColor = (uint)Color.White.ToArgb();

                    updateCell.BGColor = (uint)colorDialog.Color.ToArgb();

                    RestoringColor undoColor = new RestoringColor(updateCell, lastColor);
                    undoColors.Add(undoColor);
                }
            }

            spreadSheetMember.AddUndo(new MulitCommand(undoColors.ToArray(), "changing cell background color"));
            updatedEditMenu();
        }

        /// <summary>
        /// When they click the undo button it calls the undo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoBackgroundColorChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadSheetMember.Undo();
            updatedEditMenu();
        }

        /// <summary>
        /// When they click the redo it calls the redo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void redoTextChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spreadSheetMember.Redo();
            updatedEditMenu();
        }

        /// <summary>
        /// I made a function where it edits the menu items if its undo or redo.
        /// </summary>
        private void updatedEditMenu()
        {
            ToolStripMenuItem editMenuItems = menuStrip1.Items[0] as ToolStripMenuItem;

            foreach (ToolStripMenuItem menuItem in editMenuItems.DropDownItems)
            {
                if (menuItem.Text.Substring(0, 4) == "Undo")
                {
                    menuItem.Enabled = !(spreadSheetMember.UndoIsEmpty);
                    menuItem.Text = "Undo" + spreadSheetMember.UndoCommand;
                }
                else if (menuItem.Text.Substring(0, 4) == "Redo")
                {
                    menuItem.Enabled = !(spreadSheetMember.RedoIsEmpty);
                    menuItem.Text = "Redo" + spreadSheetMember.RedoCommand;
                }
            }
        }

        /// <summary>
        /// This saves the file once the user has clikced the save button on the tool strip, got a lot of this from microsoft documentation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // we instantiate a save file diagolog to be able to save a file
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // if the user persses the ok button it goes into statement
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // we intatiate everything, this was in microsoft documentation
                Stream oufile = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);

                // we save the spreadsheet
                spreadSheetMember.Save(oufile);

                // then we dispose of everything in the outfile
                oufile.Dispose();
            }
        }

        /// <summary>
        /// This function is for the load button, if its clicked then it opens a file dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This instatiates a openfiledialog to get the file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // If the user presses the ok button it goes into the if statement
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // We clear the spreadsheet first
                ClearSheet();

                // We instantiate a file I got this from microsoft documentation
                Stream infile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);

                // we call the load function in spreadsheet
                spreadSheetMember.Load(infile);

                // We dispose of the contents in the infile
                infile.Dispose();

                // we clear all of the stacks in redo and undo in spreadsheet
                spreadSheetMember.ClearStacks();
            }

            // we call the update edit menu to update it
            updatedEditMenu();
        }

        /// <summary>
        /// This function goes through every cell in the spreadsheet and clears that current cell.
        /// </summary>
        private void ClearSheet()
        {
            // iterates thorugh the rows
            for (int i = 0; i < spreadSheetMember.RowCount; i++)
            {
                // iterates through the columns
                for (int j = 0; j < spreadSheetMember.ColumnCount; j++)
                {
                    // Gets whats on the current cell
                    Cell updateCell = spreadSheetMember.GetCell(i, j);

                    // clears the cell
                    updateCell.Clear();
                }
            }
        }
    }
}
