using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cpts321;
using SpreadsheetEngine1;

namespace ExpressionTreeApp
{
    class Program
    {
        /// <summary>
        /// THis is the main menu that runs the expression tree and allows user to change it
        /// </summary>
        public void Menu()
        {
            //set the basic variable names
            string expression = "A1-12-C1";
            string variableName;
            string variableValue;
            double doubleVariableValue;
            //made an expression tree
            ExpressionTree exp = new ExpressionTree(expression);
            //checks if we want to be in the loop still
            bool ran = true;
            //the while loop for the menu
            while (ran)
            {
                //print statments
                Console.WriteLine("Menu (current expression = " + expression + ")");
                Console.WriteLine("1 = Enter a new expression");
                Console.WriteLine("2 = Set a variable value");
                Console.WriteLine("3 = Evaluate Tree");
                Console.WriteLine("4 = Quit");
                //gets the option and turns it to a string
                string option = Console.ReadLine().ToString();
                //if 1 then user enters expression and is set to the expression tree
                if (option == "1")
                {
                    Console.Write("Enter new expression: ");
                    expression = Console.ReadLine();
                    exp = new ExpressionTree(expression);

                }
                //if 2 then user sets varaible name and value 
                else if (option == "2")
                {
                    Console.Write("Enter a variable name: ");
                    variableName = Console.ReadLine();
                    Console.Write("Enter a variable value: ");
                    variableValue = Console.ReadLine();
                    //gets set as the variable 
                    doubleVariableValue = Convert.ToDouble(variableValue);
                    exp.SetVariable(variableName, doubleVariableValue);

                }
                //if user gets 3 it shows the double value of the expression tree it calls the evaluate function
                else if (option == "3")
                {
                    Console.WriteLine(exp.Evaluate());
                }
                //if 4 stops the while loop
                else if (option == "4")
                {
                    ran = false;
                }
                //if user chooses 1-4 then its an invalid command and it continues
                else
                {
                    Console.WriteLine("//This is a command that the app will ignore");
                    continue;
                }
            }
            //writes done at the end
            Console.WriteLine("Done");
        }
    

        static void Main(string[] args)
        {
            //we call the namespace program and make the program call the menu
            Program prog = new Program();
            prog.Menu();

        }
    }
}
