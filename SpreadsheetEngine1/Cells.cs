using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SpreadsheetEngine1
{
    public abstract class Cell : INotifyPropertyChanged
    {
        // We set the row and column index
        // we set the text and values memeber as protected
        protected int rowIndex;
        protected int columnIndex;
        protected string texts;
        protected string values;
        protected string names;
        protected uint colors;

        // got this from microsoft documentations
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // contructor with no parameters
        public Cell()
        {
        }

        // set the cell constructor with new rows and columns

        /// <summary>
        /// Cell contructor with rows and columns.
        /// </summary>
        /// <param name="newRowIndex"></param>
        /// <param name="newColumnIndex"></param>
        public Cell(int newRowIndex, int newColumnIndex)
        {
            // This makes the index to the the parameters
            rowIndex = newRowIndex;
            columnIndex = newColumnIndex;
            texts = "";
            values = "";
            colors = 0xFFFFFFFFU;

            // This converts the row and column index to be able to turn into the cell name
            names += Convert.ToChar('A' + newColumnIndex);
            names += (newRowIndex + 1).ToString();
        }

        // Made the readonly row and column with getters
        public int RowIndex { get { return rowIndex; } }

        public int ColumnIndex { get { return columnIndex; } }

        /// <summary>
        /// Makes a string text.
        /// </summary>
        public string Text
        {
            // it gets the private text
            get { return texts; }

            // this sets the text member to values
            set
            {
                // if the text isnt changed then it returns
                if (texts == value)
                {
                    return;
                }

                // text equals value
                texts = value;

                // changes the subscribers so it knows that the text is changed
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        /// <summary>
        /// This is the value string.
        /// </summary>
        public string Value
        {
            // gets the value and returns the protected member
            get { return values; }

            // got this from microsoft documentation in access modifiers
            protected internal set
            {
                // It should all be like the text only difference is that its in an protected internal set
                if (values == value)
                {
                    return;
                }

                values = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }

        public string Name
        {
            get { return names; }
        }

        // BGcolor is set
        public uint BGColor
        {
            get { return colors; }

            set
            {
                if (colors == value)
                {
                    return;
                }

                colors = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }

        // this resets the text and vlaues to nothing and changes the color back too
        public void Clear()
        {
            texts = "";
            values = "";
            BGColor = 0xFFFFFFFFU;
        }
    }
}
