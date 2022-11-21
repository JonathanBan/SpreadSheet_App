using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine1
{
    public class ExpressionTree
    {
        /// <summary>
        /// This is the base node class that has the evaluate function in it.
        /// </summary>
        private abstract class Node
        {
            // need the to add the evaluate into abstract class, basiclly the requirement in the hw
            // got the abstraction of evaluate from class
            public abstract double Evaluate();
        }

        /// <summary>
        /// This is the constant node class that has the value and has an instance of the evaluate function from the base class.
        /// </summary>
        private class ConstantNode : Node
        {
            // We set the value as double becuase the expression is going to be turned into a double
            private double value;

            // need to implement a constuctor that takes a value
            public ConstantNode(double newValue)
            {
                value = newValue;
            }

            // we will overide the abstract function in the base node class
            public override double Evaluate()
            {
                return value;
            }
        }

        /// <summary>
        /// This will incorpotrate the variable name and the variable declared int the dictionary from the class example
        /// THis will also call the evaluate function from the base class and overide it like the constant node class.
        /// </summary>
        private class VariableNode : Node
        {
            // We have to create a member for the variable name
            private string variableName;

            /// <summary>
            /// This is the contructor for the variable node that takes in a variable name.
            /// </summary>
            /// <param name="newVariableName"></param>
            public VariableNode(string newVariableName)
            {
                variableName = newVariableName;
            }

            public override double Evaluate()
            {
                // got this from microsoft documentation
                // Says if the variable is not already in the dictionary then it sets it to 0
                if (!variables.ContainsKey(variableName))
                {
                    variables[variableName] = 0.0;
                }

                return variables[variableName];
            }
        }

        private Node root;
        private static Dictionary<string, double> variables = new Dictionary<string, double>();

        /// <summary>
        /// THis is the operator node based in class discusion we will need to implement this in order to have the
        /// right and left subtree plus the operator.
        /// </summary>
        private class OperatorNode : Node
        {
            private char operate;
            private Node leftSubTree;
            private Node rightSubTree;

            /// <summary>
            /// In order to be able to have a better implentation i had a node for the sub trees and a private char for .
            /// the operator to implement a constructor and sets the new values to the private values.
            /// </summary>
            /// <param name="newOperator"></param>
            /// <param name="newLeftSubTree"></param>
            /// <param name="newRightSubTree"></param>
            public OperatorNode(char newOperator, Node newLeftSubTree, Node newRightSubTree)
            {
                operate = newOperator;
                leftSubTree = newLeftSubTree;
                rightSubTree = newRightSubTree;
            }

            public override double Evaluate()
            {
                // this helps evaluate the subtree nodes
                double left = leftSubTree.Evaluate();
                double right = rightSubTree.Evaluate();

                // we implement something like the example to call the correct operation for the node
                // the only operations we need this hw is +,-,*, and /
                switch (operate)
                {
                    // if the operator is + then we add left and right
                    case '+':
                        return left + right;

                    // if the operator is - we suntract left and right
                    case '-':
                        return left - right;

                    // if the operator is * we multiply left and right
                    case '*':
                        return left * right;

                    // if the operator is / then we devide left and right double number
                    case '/':
                        return left / right;

                    // if they aren't any of the above operators then we throw an exception got from example
                    default:
                        throw new NotSupportedException(
                        "Operator " + operate + " not supported.");
                }
            }
        }

        public ExpressionTree(string expression)
        {
            root = Compile(expression);
            variables.Clear();
        }

        /// <summary>
        /// Sets the specified variable within the ExpressionTree variables dictionary.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        public void SetVariable(string variableName, double variableValue)
        {
            // sets the variable dictionary by adding the variable vlaue
            variables[variableName] = variableValue;
        }

        public string[] GetVariableNames()
        {
            return variables.Keys.ToArray();
        }

        /// <summary>
        /// Implement this method with no parameters that evaluates the expression to a double value.
        /// </summary>
        /// <returns></returns>
        public double Evaluate()
        {
            if (root != null) return root.Evaluate();

            // Got this from microsoft documentation means that the value is not a num
            else return double.NaN;
        }

        /// <summary>
        /// I created a seperate function that was sort of in the class example in the compile function to make sure to either
        /// make it a constantNode type or a variable node type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static Node ConsOrVarNodeBuilder(string obj)
        {
            double turnDouble;

            // This checks if it could be made into a double
            if (double.TryParse(obj, out turnDouble))
            {
                return new ConstantNode(turnDouble);
            }

            // if it cant be parsed that means it will be a variableNode type since its a string
            return new VariableNode(obj);
        }

        /// <summary>
        /// The compile function to build the tree and implement the nodes we also have to assume the
        /// expression will be be implemented correctly with no parenthesis, so we don't have to check them.
        /// </summary>
        /// <param name="expression"> its the string of the expression.</param>
        /// <returns>returns the var or operator node.</returns>
        private static Node Compile(string expression)
        {
            // this will replace the spaces with blanks
            expression = expression.Replace(" ", "");

            // this is a book that checks if there are enclosed parthesis
            bool enclosed = CheckForEnclosed(expression);

            // This loops while enclosed is true
            while (enclosed == true)
            {
                expression = expression.Substring(1, expression.Length - 2);
                enclosed = CheckForEnclosed(expression);
            }

            // This sets the index to the lowest precedince index
            int index = LowPrecedenceOperator(expression);
            if (index != -1)
            {
                return new OperatorNode(expression[index], Compile(expression.Substring(0, index)), Compile(expression.Substring(index + 1)));
            }

            // returns wheter its a var or op which it shouldn't be for the last one
            return ConsOrVarNodeBuilder(expression);
        }

        /// <summary>
        /// THis function will return the lowest precedence operator, the index of it.
        /// </summary>
        /// <param name="expression"> a string which is an expression.</param>
        /// <returns> the index of the current operator.</returns>
        private static int LowPrecedenceOperator(string expression)
        {
            int index = -1;
            int amountParenthesis = 0;
            int size = expression.Length - 1;

            // This goes throught the entire expression it would start at the length -1 because the string is null terminated
            for (int i = size; i >= 0; i--)
            {
                // making a switch statement
                switch (expression[i])
                {
                    // depending if its a closed or open parenthesis we add or subract to the amoutParenthesis
                    case '(':
                        amountParenthesis++;
                        break;
                    case ')':
                        amountParenthesis--;
                        break;

                    // now if we get either a + or a minus and if there are no parenthesis then it just returns the index that its at
                    case '+':
                        if (amountParenthesis == 0)
                        {
                            return i;
                        }

                        break;
                    case '-':
                        if (amountParenthesis == 0)
                        {
                            return i;
                        }

                        break;

                    // Now if its either multiplication or division then it checks if the index is -1 and that there are no parenthesis and it sets the index to the current index i
                    case '*':
                        if (amountParenthesis == 0 && index == -1)
                        {
                            index = i;
                        }

                        break;
                    case '/':
                        if (amountParenthesis == 0 && index == -1)
                        {
                            index = i;
                        }

                        break;
                }
            }

            return index;
        }

        /// <summary>
        /// This function checks if the parathesis are enclosed and will return false if it isnt and true if it is.
        /// </summary>
        /// <param name="expression">  the expression string.</param>
        /// <returns>false or true.</returns>
        private static bool CheckForEnclosed(string expression)
        {
            // have to get the sieze and the ammount of parthensis to check for enclosed
            int amountParenthesis = 0;
            int size = expression.Length - 1;

            // we have to check if the begining and end have parthensis of the expression
            if ((expression[0] == '(') && (expression[size] == ')'))
            {
                // goes on a for loop to check if its the open or close parenthesis
                for (int i = 1; i < size; i++)
                {
                    switch (expression[i])
                    {
                        // If its an open parenthesis then it adds 1
                        case '(':
                            amountParenthesis++;
                            break;

                        // if its a close parenthesis then it checks if it's zero and if its not it returns false and subtracts
                        case ')':
                            if (amountParenthesis == 0)
                            {
                                return false;
                            }

                            amountParenthesis--;
                            break;
                    }
                }

                // if it ends with no more parenthesis it gets true
                if (amountParenthesis == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
