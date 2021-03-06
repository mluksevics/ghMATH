﻿using Grasshopper.Kernel;
using Mathos.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ghMath
{
    public class calculateMath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the calculateMath class.
        /// </summary>
        public calculateMath()
          : base("Run sMath calc.", "sMath Calculaton",
              "Run calculations based on process defined in sMath spreadsheet",
              "ghMath", "sMath")
        {
        }


        // We'll start by declaring input parameters and initializing those.
        string inputXML = string.Empty;
        List<string> inputNames = new List<string>();
        List<double> inputValues = new List<double>();
        List<string> inputUnits = new List<string>();
        string outputXMLpath = string.Empty;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            pManager.AddTextParameter("sMath XML", ".sm XML", "Contents of sMath .sm XML file", GH_ParamAccess.item);
            pManager.AddTextParameter("sMath input Names", "In Names", "Names of input variables", GH_ParamAccess.list);
            pManager.AddNumberParameter("sMath input values", "In Values", "Values of input variables for sMath spreadsheet", GH_ParamAccess.list);
            pManager.AddTextParameter("sMath input units", "In Units", "Units of variables for sMath spreadsheet", GH_ParamAccess.list);
            pManager.AddTextParameter("sMath file output path", "Out file path", "File path where .sm with outputs is to be saved ", GH_ParamAccess.item);

            pManager[4].Optional = true; //it is not mandatory to enter path for output of .sm file. If no path is provied, the file just does not get saved.

        }

        //Ddeclaring output parameters and initialising those.
        string outputXML = string.Empty;
        List<string> outputNames = new List<string>();
        List<double> outputValues = new List<double>();
        List<string> outputUnits = new List<string>();

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            pManager.AddTextParameter("sMath XML", ".sm XML", "Output of .sm XML file contents after calculation", GH_ParamAccess.item);
            pManager.AddTextParameter("sMath output Names", "Out Names", "Names of output variables", GH_ParamAccess.list);
            pManager.AddNumberParameter("sMath output values", "Out Values", "Values of output variables for sMath spreadsheet", GH_ParamAccess.list);
            pManager.AddTextParameter("sMath output units", "Out Units", "Units of variables for sMath spreadsheet", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref inputXML)) return;
            DA.GetDataList(1, inputNames);
            DA.GetDataList(2, inputValues);
            DA.GetDataList(3, inputUnits);
            DA.GetData(4, ref outputXMLpath);

            //Then let's clear and reset all output parameters.
            // this is done to ensure that if function is repeadedly run, then parameters are re-read and redefined
            outputNames.Clear();
            outputValues.Clear();
            outputUnits.Clear();
            outputXML = string.Empty;

            //Then let's run the actual funcionality
            CalculateMath(
            inputXML,
            inputNames,
            inputValues,
            inputUnits,
            outputXMLpath,
            ref outputNames,
            ref outputValues,
            ref outputUnits,
            ref outputXML);

            //Assign the ouputs to the output parameters
            DA.SetData(0, outputXML);
            DA.SetDataList(1, outputNames);
            DA.SetDataList(2, outputValues);
            DA.SetDataList(3, outputUnits);

            //Finally clear all input parameters:
            inputXML = string.Empty;
            inputNames.Clear();
            inputValues.Clear();
            inputUnits.Clear();
            outputXMLpath = string.Empty;

        }

        private void CalculateMath(
            string inputXML,
            List<string> inputVariablesNames,
            List<double> inputVariablesValues,
            List<string> inputVariablesUnits,
            string outputXMLpath,
            ref List<string> outputVariablesNames,
            ref List<double> outputVariablesValues,
            ref List<string> outputVariablesUnits,
            ref string outputXML
            )

        {
            // loading the XML data
            XmlDocument doc = new XmlDocument();
            if (inputXML == string.Empty) return;
            doc.LoadXml(inputXML);


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

            XmlNode smathContentRegions;
            ghXMLProcess.ExtractRegionNodes(doc, out smathContentRegions);

            // processing XML nodes describing regions
            foreach (XmlNode node in smathContentRegions.ChildNodes)
            {
                //accessing XML nodes with input equation and results
                XmlNode mathDescriptionNode;
                XmlNode mathInputEqationNode;
                XmlNode mathResultValueNode;
                XmlNode mathResultUnitsNode;
                if (!ghXMLProcess.ExtractMathNodes(node, out mathDescriptionNode, out mathInputEqationNode, out mathResultValueNode, out mathResultUnitsNode)) continue;


                //convert XML to "readable" one line equation
                //units will be already converted to SI system during the processing of equation
                string singleExpression = ghMath.ghMathProcessing.ConvertXMLequationToString(mathInputEqationNode);

                //split equation into variable defined and an expression for calculation
                string[] splitExpression = singleExpression.Split('=');

                //check the expression has two parts (variable and equation), assign variables
                string expressionVariable = string.Empty;
                string expressionEquation = string.Empty;


                //if (singleExpression.Substring(0, 2) == "if")
                //{
                //    string ifExpression;
                //    ifExpression = singleExpression.Trim();
                //    ifExpression = ifExpression.Substring(3, ifExpression.Length - 3);
                //    ifExpression = ifExpression.Substring(0, ifExpression.Length - 1);

                //    string[] ifExpressionparts = ifExpression.Split(',');
                //    double trueOrFalse = eval.Parse(ifExpressionparts[2]);

                //    if (trueOrFalse == 1)
                //    {
                //        expressionVariable = ifExpressionparts[1].Split('=')[0];
                //        expressionEquation = ifExpressionparts[1].Split('=')[1];
                //    }
                //    else
                //    {
                //        expressionVariable = ifExpressionparts[0].Split('=')[0];
                //        expressionEquation = ifExpressionparts[0].Split('=')[1];
                //    }



                //}
                //else 
                if (splitExpression.Length == 2)
                {
                    expressionVariable = splitExpression[0];
                    expressionEquation = splitExpression[1];
                }
                else
                {
                    continue;
                }


                //defining variables for storing the expression variable name and result;
                string variableName;
                double variableValue;

                //now lets check whether this variable is not already provided as input data
                //if it does, then it gets converted to SI units and stored
                //if it does not, then the expression (already in SI units) is evaluated and result stored;
                if (inputVariablesNames.Contains(ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionVariable)))
                {
                    //input variable names contains "dots" in their names, therefore value from expression needs to be reinstated to have "dots" in name as well
                    //variable name examples: "u.value" or "L.eff"
                    int i = inputVariablesNames.IndexOf(ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionVariable));
                    variableName = ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(inputVariablesNames[i]);
                    if (inputVariablesUnits[i] == "")
                    {
                        variableValue = inputVariablesValues[i];
                    }
                    else
                    {
                        variableValue = inputVariablesValues[i] * eval.Parse(inputVariablesUnits[i]);
                    }
                }
                else
                {
                    variableValue = eval.Parse(expressionEquation);
                    variableName = expressionVariable;
                }

                //replace dots with symbol because dots can not be used for variable name
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

                if (mathResultValueNode != null)
                {

                    if (mathResultUnitsNode != null)
                    {
                        resultUnitsExpression = ghMathProcessing.ConvertXMLequationToString(mathResultUnitsNode);
                        resultUnitsConversion = eval.Parse(resultUnitsExpression);

                        resultOuputValue = expressionResult / resultUnitsConversion;
                        // mathResultValueNode.ChildNodes[0].InnerText = resultOuputValue.ToString();
                    }

                    //if there are no "forced" units, then default output will be in SI units, so, just write the result into XML node.
                    else
                    {
                        resultUnitsExpression = "";
                        resultOuputValue = expressionResult;
                        //mathResultValueNode.ChildNodes[0].InnerText = expressionResult.ToString();
                    }

                    //convert to variable names with dots
                    string expressionVariableWithDots = ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionVariable);

                    //add variables for GH output:
                    outputVariablesNames.Add(expressionVariableWithDots);
                    outputVariablesValues.Add(resultOuputValue);
                    outputVariablesUnits.Add(resultUnitsExpression);

                }
            }


            //update XML file with the inputs and outputs just defined
            ghXMLProcess.UpdateSmathXMLwithResults(ref doc, inputVariablesNames, inputVariablesValues, outputVariablesNames, outputVariablesValues);

            //format XML to include indents and line-breaks. This is only done to improve readability of ouput file
            string formattedXML = ghXMLProcess.FormatXMLString(doc.OuterXml);

            //checking whether the output file path has been provided and writing to file.
            if (outputXMLpath != string.Empty)
            {
                StreamWriter writer = new StreamWriter(outputXMLpath);
                writer.Write(formattedXML);
                writer.Close();
            }

            //output XML to output parameter. Note that many special characters are "lost" when output to Grasshopper panel
            outputXML = formattedXML;


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return ghMath.Properties.Resources.icon_calculate;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7fa0327b-f9d7-49ec-bbea-271577c33f1b"); }
        }
    }
}