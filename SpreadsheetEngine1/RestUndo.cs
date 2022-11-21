using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine1
{
    // We have to create an interface
    public interface ICommands
    {
        ICommands Exec();
    }

    /// <summary>
    /// This class is to be able to restore the text back to what it was.
    /// </summary>
    public class RestoringText : ICommands
    {
        // We have to instatiate a cell type and the text to a string
        private Cell cells;
        private string texts;

        /// <summary>
        /// This is the constructor that has a cell and text in the parameter. It sets the cell and text to the memeber in the class.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="text"></param>
        public RestoringText(Cell cell, string text) { cells = cell; texts = text; }

        /// <summary>
        /// This is a function that calls the Icommands exec and gets the inverse of the current text cell.
        /// </summary>
        /// <returns></returns>
        public ICommands Exec()
        {
            // We get the inverse of the cell
            var inverse = new RestoringText(cells, cells.Text);
            cells.Text = texts;
            return inverse;
        }
    }

    /// <summary>
    /// This is the restoring color class that helps in getting the last color.
    /// </summary>
    public class RestoringColor : ICommands
    {
        // THis instantiates a cell member and a uint to represent colors
        private Cell cells;
        private uint colors;

        /// <summary>
        /// This is the contructor that sets the cells and color to what the user put.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="color"></param>
        public RestoringColor(Cell cell, uint color) { cells = cell; colors = color; }

        /// <summary>
        /// This calls the Icommans exec and puts in the inverese of the cell to be able to get the last color.
        /// </summary>
        /// <returns></returns>
        public ICommands Exec()
        {
            // We get the inverse of the cell
            var inverse = new RestoringColor(cells, cells.BGColor);
            cells.BGColor = colors;
            return inverse;
        }
    }

    /// <summary>
    /// This is the mulitcommands class that has all of the commands in a list.
    /// </summary>
    public class MulitCommand
    {
        // This sets an icommands type and string type to get the commands and command names
        private ICommands[] commands;
        private string commandNames;

        /// <summary>
        /// This is the multicommand contructor that could be instantiated as an empty parameter.
        /// </summary>
        public MulitCommand()
        {
        }

        /// <summary>
        /// THis is a multicommand contructor that accepts an icommands and a string as parameteres.
        /// </summary>
        /// <param name="multipleCommands"></param>
        /// <param name="commandName"></param>
        public MulitCommand(ICommands[] multipleCommands, string commandName)
        {
            commands = multipleCommands;
            commandNames = commandName;
        }

        /// <summary>
        /// This is the getter for the commandname
        /// </summary>
        public string CommandName { get { return commandNames; } }

        /// <summary>
        /// This is the multicomand exec function that sets a list to icommands type.
        /// </summary>
        /// <returns></returns>
        public MulitCommand Exec()
        {
            // This is a list of icommandlist that will be the list of commmands
            List<ICommands> commandList = new List<ICommands>();

            // Goes through a foreachloop to add the commands to the list
            foreach (ICommands command in commands)
            {
                commandList.Add(command.Exec());
            }

            return new MulitCommand(commandList.ToArray(), commandNames);
        }

    }
}
