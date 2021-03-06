﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Mathos.Parser;
using System.Xml;
using System.Globalization;

namespace ghMath
{
    public class ghMATHComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ghMATHComponent()
          : base( "Extract sMath inputs", "sMath Inputs",
              "Extract sMath input variables from the given spreadsheet",
              "ghMath", "sMath")
        {
        }


        // We'll start by declaring input parameters and initializing those.
        string inputXML = string.Empty;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
                // Use the pManager object to register your input parameters.
                pManager.AddTextParameter("sMath XML", ".sm XML", "Contents of sMath .sm XML file", GH_ParamAccess.item);
        }

        //Ddeclaring output parameters and initialising those.
        List<string> outInputNames = new List<string>();
        List<double> outInputValues = new List<double>();
        List<string> outInputUnits = new List<string>();

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            pManager.AddTextParameter("sMath Variable Names", "Names", "Names of variables that can be assigned within sMath spreadsheet", GH_ParamAccess.list);
            pManager.AddNumberParameter("sMath Variable default Values", "Values", "Default values of variables that can be assigned within sMath spreadsheet", GH_ParamAccess.list);
            pManager.AddTextParameter("sMath Variable Units", "Units", "Units of variables that can be assigned within sMath spreadsheet", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref inputXML)) return;


            //Then let's clear and reset all output parameters.
            // this is done to ensure that if function is repeadedly run, then parameters are re-read and redefined
            outInputNames.Clear();
            outInputValues.Clear();
            outInputUnits.Clear();


            //Then let's run the actual funcionality
            ExtractMathInputs(inputXML, ref outInputNames, ref outInputValues, ref outInputUnits);

            //Assign the ouputs to the output parameters
            DA.SetDataList(0, outInputNames);
            DA.SetDataList(1, outInputValues);
            DA.SetDataList(2, outInputUnits);

            //Finally clear all input parameters:
            inputXML = string.Empty;
        }

        /// <summary>
        /// This method takes sMath .sm file (XML format) and extracts all variables that are used as input (i.e. not calculated, not depending on other variables)
        /// </summary>
        /// <param name="inputXML">Input xml in string format</param>
        /// <param name="inputParameterNames"> Extracted parameter names</param>
        /// <param name="inputParameterDefaultValues">Extracted parameter default values</param>
        /// <param name="inputParameterUnits">Extracted parameter default units</param>
        private void ExtractMathInputs(string inputXML, ref List<string> inputParameterNames, ref List<double> inputParameterDefaultValues, ref List<string> inputParameterUnits)
        {

            // loading the XML data
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(inputXML);

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

            // processing XML node-by-node
            foreach (XmlNode node in smathContentRegions.ChildNodes)
            {
                //accessing XML nodes with input equation and results
                XmlNode mathDescriptionNode;
                XmlNode mathInputEqationNode;
                XmlNode mathResultValueNode;
                XmlNode mathResultUnitsNode;
                if (!ghXMLProcess.ExtractMathNodes(node, out mathDescriptionNode, out mathInputEqationNode, out mathResultValueNode, out mathResultUnitsNode)) continue;

                //convert XML to "readable" equation
                string singleExpression = ghMath.ghMathProcessing.ConvertXMLequationToString(mathInputEqationNode);


                //extracting the variable name of equation 
                string[] splitExpression = singleExpression.Split('=');
                if (splitExpression.Length != 2) continue;
                string expressionParameter = splitExpression[0];
                string expressionEquation = splitExpression[1];


                //further seeing whether there is a result part of the XML node with expression
                //if there is a result part, then skip this node with math region and go to next;
                if (mathResultValueNode != null) continue;


                //separate variable value from the units;
                string expressionUnits;
                string expressionDetfaultValueString;

                //find where the first multiplication sign is located
                int firstAsterixIndex = expressionEquation.IndexOf('*');

                //if there is no units - no multiplication sign in equation:
                if (firstAsterixIndex == -1)
                {
                    expressionDetfaultValueString = expressionEquation;
                    expressionUnits = "";

                }

                //if there are defined units - find first multiplocation sign and separate the units from numeric value
                else
                {
                    string variableValueString = expressionEquation.Substring(0, firstAsterixIndex);
                    string variableUnitString = expressionEquation.Substring(firstAsterixIndex + 1);

                    expressionDetfaultValueString = variableValueString.Replace("(", "").Trim();
                    expressionUnits = "(" + variableUnitString;
                }

                //check if units are valid and value is double. If yes - add values to the list
                double expressionDetfaultValue = 0;
                if (double.TryParse(expressionDetfaultValueString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out expressionDetfaultValue))
                {
                    //add input parameters to the list;
                    inputParameterNames.Add(ghMath.ghMathProcessing.ReinstateRestrictedVariableNamesCharacters(expressionParameter));
                    inputParameterUnits.Add(expressionUnits);
                    inputParameterDefaultValues.Add(expressionDetfaultValue);
                }
            }

        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                    //You can add image files to your project resources and access them like this:
                    // return Resources.IconForThisComponent;
                    return ghMath.Properties.Resources.icon_input;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("84e018b0-37d9-4b0a-a56a-e072c48c4c88"); }
        }
    }
}
