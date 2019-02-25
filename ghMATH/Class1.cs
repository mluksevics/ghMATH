using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using Mathos.Parser;


namespace ghMath
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // loading the XML data
            XmlDocument doc = new XmlDocument();
            if (xmlInputBox.Text == string.Empty) return;
            doc.LoadXml(xmlInputBox.Text);

            // loading information about input variables
            //for testing purposes only - this will be replaced by lists from Grasshopper
            List<string> inputVariablesNames = new List<string>();
            List<double> inputVariablesValues = new List<double>();
            List<string> inputVariablesUnits = new List<string>();
            //inputVariablesNames.Add("a");
            //inputVariablesValues.Add(3.3);
            //inputVariablesUnits.Add("mm");

            List<string> outputVariablesNames = new List<string>();
            List<double> outputVariablesValues = new List<double>();
            List<string> outputVariablesUnits = new List<string>();
            //outputVariablesNames.Add("a");
            //outputVariablesValues.Add(3.3);
            //outputVariablesUnits.Add("mm");


            //defining list with expressions
            List<string> expressionsList = new List<string>();

            // defining math evaluator for entire spreadsheet.
            //note that within evaluator object, there are no units stored. Instead, all variable values are in SI system.
            //class ghMathParser adds units interpretation and functions to parser object.
            var parser = new MathParser();
            var eval = ghMath.ghMathParser.additionsToMathParser(parser);

            //sMath document structure
            //  >> worksheet
            //       >>[0] Settings
            //       >>[1] Regions
            //             >> region
            //                   >> math
            //                      >> input (math equation sits here)
            //                      >> contract (this are "forced" units for result display)
            //                      >> result (this is result)

            //sMath content regions
            XmlNode smathContentRegions = doc.DocumentElement.ChildNodes[1];

            // processing XML nodes describing regions
            foreach (XmlNode node in smathContentRegions.ChildNodes)
            {
                // first checking whether this is math region
                // image, text, graphs are not processed
                if (node.ChildNodes[0].Name != "math") continue;

                //accessing XML node with math
                XmlNode mathNode = node.ChildNodes[0];

                //accessing XML node with input equation
                XmlNode mathInputEqationNode;
                int resultsNodeIndex;
                if (mathNode.ChildNodes[0].Name == "description")
                {
                    mathInputEqationNode = mathNode.ChildNodes[1];
                    resultsNodeIndex = 2;

                }
                else
                {
                    mathInputEqationNode = mathNode.ChildNodes[0];
                    resultsNodeIndex = 1;

                }
                //XmlNode mathInputEqationNode = mathNode.SelectSingleNode("/math/description[0]");

                //convert XML to "readable" one line equation
                //units will be already converted to SI system during the processing of equation
                string singleExpression = ghMath.ghMathProcessing.ConvertXMLequationToString(mathInputEqationNode);

                //split equation into variable defined and an expression for calculation
                string[] splitExpression = singleExpression.Split('=');

                //check the expression has two parts (variable and equation), assign variables
                if (splitExpression.Length != 2) continue;
                string expressionVariable = splitExpression[0];
                string expressionEquation = splitExpression[1];

                //defining variables for storing the expression variable name and result;
                string variableName;
                double variableValue;

                //now lets check whether this variable is not already provided as input data
                //if it does, then it gets converted to SI units and stored
                //if it does not, then the expression (already in SI units) is evaluated and result stored;
                if (inputVariablesNames.Contains(expressionVariable))
                {
                    int i = inputVariablesNames.IndexOf(expressionVariable);
                    variableName = inputVariablesNames[i];
                    variableValue = inputVariablesValues[i] * eval.Parse(inputVariablesUnits[i]);
                }
                else
                {
                    variableValue = eval.Parse(expressionEquation);
                    variableName = expressionVariable;
                }

                //replace dots with symbol because dots can be used for variable name
                string variableNameWithoutDots = ghMath.ghMathProcessing.ReplaceRestrictedVariableNamesCharacters(variableName);

                //storing the variable
                //if such variable already exists, override the value
                if (eval.LocalVariables.ContainsKey(variableNameWithoutDots)) eval.LocalVariables.Remove(variableNameWithoutDots);
                // add new value 
                eval.LocalVariables.Add(variableNameWithoutDots, variableValue);
                double expressionResult = variableValue;


                //now lets see whether the math region of XML has an "result" part of equation
                //in other words - whether in sMath the result of equation is visible and any unit conversion done for equation
                // if there is result part - then result of equation is converted to output units and also added to 
                //list with all the results - that will be output from Grasshopper node

                //if there are any "forced" units on result output (and normally there should be)
                //then expression for units (e.g. kN*m) are converted to conversion factor (e.g. kN to N = 1000, m to m = 1)
                //and then result in SI units divided by this conversion factor (e.g. 2000 N*m equalls 2000/1000 = 2 kN*m)
                string resultUnitsExpression = "";
                double resultUnitsConversion = 0;
                double resultOuputValue = 0;

                if (mathNode.ChildNodes[resultsNodeIndex] != null)
                {

                    if (mathNode.ChildNodes[resultsNodeIndex].Name == "contract")
                    {
                        XmlNode resultContractUnitsNode = mathNode.ChildNodes[resultsNodeIndex];
                        XmlNode resultNode = mathNode.ChildNodes[resultsNodeIndex + 1];

                        resultUnitsExpression = ghMath.ghMathProcessing.ConvertXMLequationToString(resultContractUnitsNode);
                        resultUnitsConversion = eval.Parse(resultUnitsExpression);

                        resultOuputValue = expressionResult / resultUnitsConversion;
                        resultNode.ChildNodes[0].InnerText = resultOuputValue.ToString();
                    }

                    //if there are no "forced" units, then default output will be in SI units, so, just write the result into XML node.
                    //if (mathNode.ChildNodes[1].Name == "result")
                    else
                    {
                        XmlNode resultNode = mathNode.ChildNodes[resultsNodeIndex];
                        resultNode.ChildNodes[0].InnerText = expressionResult.ToString();

                        resultOuputValue = expressionResult;
                        resultUnitsExpression = "";
                    }

                    //convert to variable names with dots
                    string expressionVariableWithDots = ghMath.ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionVariable);

                    //add variables for GH output:
                    outputVariablesNames.Add(expressionVariableWithDots);
                    outputVariablesValues.Add(resultOuputValue);
                    outputVariablesUnits.Add(resultUnitsExpression);

                    resultVariablesBox.Items.Add(expressionVariableWithDots);
                    resultsBox.Items.Add(resultOuputValue);
                    resultUnitsBox.Items.Add(resultUnitsExpression);

                    expressionsBox.Items.Add(ghMath.ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(singleExpression));
                }
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            // loading the XML data
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlInputBox.Text);

            //defining lists for output;
            List<string> inputParameterNames = new List<string>();
            List<string> inputParameterUnits = new List<string>();
            List<double> inputParameterDefaultValues = new List<double>();

            //sMath content regions
            XmlNode smathContentRegions = doc.DocumentElement.ChildNodes[1];

            // processing XML node-by-node
            foreach (XmlNode node in smathContentRegions.ChildNodes)
            {
                // reading from XML and assigning to variables
                if (node.ChildNodes[0].Name != "math")
                {
                    continue;
                }

                //accessing XML node with math
                XmlNode mathNode = node.ChildNodes[0];

                //accessing XML node with input equation
                XmlNode mathInputEqationNode = mathNode.ChildNodes[0];

                //convert XML to "readable" equation
                string singleExpression = ghMath.ghMathProcessing.ConvertXMLequationToString(mathInputEqationNode);

                //extracting the variable name of equation 
                string[] splitExpression = singleExpression.Split('=');
                if (splitExpression.Length != 2) continue;
                string expressionParameter = splitExpression[0];
                string expressionEquation = splitExpression[1];


                //further seeing whether there is a result part of the XML node with expression
                //if there is only one "childnode" , that will be input and everything is alright
                if (mathNode.ChildNodes.Count > 1)
                {
                    //if there is a result part, then skip this node with math region and go to next;
                    continue;
                }

                //separate variable value from the units;
                string expressionUnits;
                string expressionDetfaultValueString;

                int firstAsterixIndex = expressionEquation.IndexOf('*');
                if (firstAsterixIndex == -1)
                {
                    expressionDetfaultValueString = expressionEquation;
                    expressionUnits = "";

                }
                else
                {
                    string variableValueString = expressionEquation.Substring(0, firstAsterixIndex);
                    string variableUnitString = expressionEquation.Substring(firstAsterixIndex + 1);

                    expressionDetfaultValueString = variableValueString.Replace("(", "").Trim();
                    expressionUnits = "(" + variableUnitString;
                }

                //replace brackets added in equation conversion process. Initially replaces with blanks and then trimmed
                //string expressionDetfaultValueString = variableValueString.Replace("(", "").Trim();
                //string expressionUnits = ReplaceLastOccurrence(variableUnitString, ")","").Trim() ;

                //defining variable for trying to parse default value from string to double
                double expressionDetfaultValue = 0;

                //check if units are valid and value is double. If yes - add values to the list
                if (double.TryParse(expressionDetfaultValueString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out expressionDetfaultValue))
                {
                    //add input parameters to the list;
                    inputParameterNames.Add(ghMath.ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionParameter));
                    inputParameterUnits.Add(expressionUnits);
                    inputParameterDefaultValues.Add(expressionDetfaultValue);


                    resultsBox.Items.Add(ghMath.ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionParameter) + "=" + expressionDetfaultValue + " " + expressionUnits);
                }

            }



        }

        private void button3_Click(object sender, EventArgs e)
        {


        }

        //public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        //{
        //    int place = Source.LastIndexOf(Find);

        //    if (place == -1)
        //        return Source;

        //    string result = Source.Remove(place, Find.Length).Insert(place, Replace);
        //    return result;
        //}

        private void button4_Click(object sender, EventArgs e)
        {
            var parser1 = new MathParser();

            parser1.LocalVariables.Add("Bc", 0.5);
            parser1.LocalVariables.Add("Lrel", 2);
            resultsBox.Items.Add(parser1.Parse("(0.5*(((1+(Bc*((Lrel-0.3))))+(Lrel^2))))").ToString());

            //    resultsBox.Items.Add(parser1.Parse("(0.5 * (((1 + (βǝc * ((λǝrel_y - 0.3)))) + (λǝrel_y ^ 2))))").ToString());


        }
    }
}
