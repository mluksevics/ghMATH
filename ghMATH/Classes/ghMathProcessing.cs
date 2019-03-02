using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ghMath
{
    public class ghMathProcessing
    {
        /// <summary>
        /// sMath allows charactes like "." and "_" in variable names. Those are not accepted in Math Parser.
        /// This method replaces restricted characters with rarely used utf-16 characters to allow processing of math
        /// </summary>
        /// <param name="variableNameInput">input strig with variable name that is to be processed</param>
        /// <returns>output variable name without resticted characters</returns>
        public static string ReplaceRestrictedVariableNamesCharacters(string variableNameInput)
        {
            string output = variableNameInput.Replace(".", "ǝ");
            output = output.Replace("_", "ɔ");
            return output;
        }

        /// <summary>
        /// does reverse operation to ReplaceRestrictedVariableNamesCharacters()
        /// </summary>
        /// <param name="variableNameInput"></param>
        /// <returns></returns>
        public static string ReinstateRestrictedVariableNamesCharacters(string variableNameInput)
        {
            string output = variableNameInput.Replace("ǝ", ".");
            output = output.Replace("ɔ", "_");
            return output;
        }

        /// <summary>
        ///math equations in sMath XML is stored using "call stacks". 
        /// more on this approach of storing equations is located here: https://www.codeproject.com/Articles/752516/Math-Equation-Parsing-using-Call-Stacks
        /// this method converts "call stack" to a regular equation and does some extra stuff, such as
        /// -) converting equation to SI units
        /// -) converting notation of functions to something familiar for parser of math equations parser
        /// </summary>
        /// <param name="mathNode"> input of xml node containg "call stack"</param>
        /// <returns> returns string of equation in "readable" format, e.g. a=b*4 + y^7</returns>
        public static string ConvertXMLequationToString(XmlNode mathNode)
        {
            //this stack is for single expression
            Stack<string> expressionStack = new Stack<string>();

            foreach (XmlNode singleExpressionElement in mathNode.ChildNodes)
            {

                //temporary variables to enable use of stack.pop(). i.e. we will need to operate with top two values on the stack
                string temp1;
                string temp2;

                //temporary vaiable for testing the parts of equation
                double test;

                // if this is operand - variable or number, or unit that is to be operated with
                if (singleExpressionElement.Attributes["type"].Value == "operand")
                {
                    //check whether this is a unit. if yes, then it get replaced to the relevant "factor"
                    //to automatically convert the equation to SI system
                    if (ghMathUnits.isUnitValid(singleExpressionElement.InnerText))
                    {
                        expressionStack.Push(singleExpressionElement.InnerText);
                    }

                    //if this is not a unit, but contains letters, then this is variable
                    else if (!double.TryParse(singleExpressionElement.InnerText,out test))
                    {
                        expressionStack.Push(ReplaceRestrictedVariableNamesCharacters(singleExpressionElement.InnerText));
                    }
                    //if variable does not contain letters, then it is considered to be a number
                    else
                    {
                        expressionStack.Push(singleExpressionElement.InnerText);
                    }
                }

                //if this is operator with one or two variables (e.g. to achieve "-5" there is only one variable)
                if (singleExpressionElement.Attributes["type"].Value == "operator")
                {
                    //two variables
                    if (singleExpressionElement.Attributes["args"].Value == "2")
                    {
                        temp2 = expressionStack.Pop();
                        temp1 = expressionStack.Pop();

                        if (singleExpressionElement.InnerText == ":")
                        {
                            //if expression is ":" it actually means "equals" and therfore is replaced with "="
                            expressionStack.Push(temp1 + "=" + temp2);
                        }
                        else
                        {
                            //account for  < or > operators - replace sMath symbols
                            string op = singleExpressionElement.InnerText;
                            op = op.Replace("&gt;", ">");
                            op = op.Replace("&lt;", "<");

                            expressionStack.Push(" (" + temp1 + singleExpressionElement.InnerText + temp2 + ") ");
                        }
                    }

                    //one variable
                    if (singleExpressionElement.Attributes["args"].Value == "1")
                    {
                        temp1 = expressionStack.Pop();
                        expressionStack.Push(singleExpressionElement.InnerText + temp1);
                    }
                }

                //if this is bracket
                if (singleExpressionElement.Attributes["type"].Value == "bracket")
                {
                    temp1 = expressionStack.Pop();
                    expressionStack.Push("(" + temp1 + ")");
                }

                //if this is function with parameters
                if (singleExpressionElement.Attributes["type"].Value == "function")
                {
                    //read - how many parameters
                    int argCount = int.Parse(singleExpressionElement.Attributes["args"].Value);
                    var arguments = new string[argCount];

                    //take the number of parameters from stack and store into array;
                    for (int i = 0; i < argCount; i++) arguments[i] = expressionStack.Pop();

                    //convert function to string
                    string results = "";
                    if (singleExpressionElement.InnerText == "mat") //special case if function represents matrix
                    {
                        // process special cases - if this is matrix, then just ignore it
                        for (int i = 2; i < argCount - 1; i++) results = results + arguments[i] + ",";
                        results = results + arguments[argCount - 1];
                    }
                    else
                    {
                        //convert typical case
                        results = singleExpressionElement.InnerText + "(";
                        for (int i = 0; i < argCount - 1; i++) results = results + arguments[i] + ",";
                        results = results + arguments[argCount - 1] + ")";
                    }

                    //push back the stack
                    expressionStack.Push(results);

                }
            }

            // return converted equation
            string fullExpresion = expressionStack.Pop();
            return fullExpresion;

        }

    }
}
