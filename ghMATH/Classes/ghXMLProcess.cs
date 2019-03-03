using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ghMath
{
    public static class ghXMLProcess
    {


        //sMath document structure
        //  >> worksheet
        //       >>[0] Settings
        //       >>[1] Regions
        //             >> region
        //                   >> math
        //                      >> input (math equation sits here)
        //                      >> contract (this are "forced" units for result display)
        //                      >> result (this is result)



        public static bool ExtractRegionNodes (XmlDocument inputXMLdoc, out XmlNode regionNodes)
        {
            if (inputXMLdoc.DocumentElement.ChildNodes.Count < 2)
            {
                regionNodes = null;
                return false;
            }

            regionNodes = inputXMLdoc.DocumentElement.ChildNodes[1];
            return true;
        }

        public static bool ExtractMathNodes (XmlNode regionNode, out XmlNode descriptionNode, out XmlNode inputNode, out XmlNode resultValueNode, out XmlNode resultUnitsNode)
        {


            // first checking whether this is math region
            // image, text, graphs are not processed
            if (regionNode.ChildNodes[0].Name != "math")
            {
                inputNode = null;
                descriptionNode = null;
                resultValueNode = null;
                resultUnitsNode = null;
                return false;
            }

            //accessing XML node with math
            XmlNode mathNode = regionNode.ChildNodes[0];

            //accessing XML node with input equation

            //if there is node for description
            if (mathNode.ChildNodes[0].Name == "description")
            {
                descriptionNode = mathNode.ChildNodes[0];
                inputNode = mathNode.ChildNodes[1];

                //check whether there are some unit overrides defined for results.  Additional node with name "contract" defines the units.
                if (mathNode.ChildNodes.Count == 4)
                {
                    resultUnitsNode = mathNode.ChildNodes[2];
                    resultValueNode = mathNode.ChildNodes[3];
                }
                else
                {
                    resultUnitsNode = null;
                    resultValueNode = mathNode.ChildNodes[2];
                }

            }

            //if there is no node for description
            else
            {
                descriptionNode = null;
                inputNode = mathNode.ChildNodes[0];

                //check whether there are some unit overrides defined for results. Additional node with name "contract" defines the units.
                if (mathNode.ChildNodes.Count == 3)
                {
                    resultUnitsNode = mathNode.ChildNodes[1];
                    resultValueNode = mathNode.ChildNodes[2];
                }
                else
                {
                    resultUnitsNode = null;
                    resultValueNode = mathNode.ChildNodes[1];
                }
            }

            return true;
        }

        public static void UpdateSmathXMLwithResults(
    ref XmlDocument XMLdoc,
    List<string> inputNames,
    List<double> inputValues,
    List<string> outputNames,
    List<double> outputValues)
        {


            // processing XML nodes describing regions
            //defining conunter to cound the region that is processed

            int i = 0;
            foreach (XmlNode node in XMLdoc.DocumentElement.ChildNodes[1].ChildNodes)
            {
                //accessing XML nodes with input equation and results
                XmlNode mathDescriptionNode;
                XmlNode mathInputEqationNode;
                XmlNode mathResultUnitsNode;
                XmlNode mathResultValueNode;
                if (!ghXMLProcess.ExtractMathNodes(node, out mathDescriptionNode, out mathInputEqationNode, out mathResultValueNode, out mathResultUnitsNode))
                {
                    i++;
                    continue;
                }


                //sMath document structure
                //  >> worksheet
                //       >>[0] Settings
                //       >>[1] Regions
                //             >> region
                //                   >> math
                //                      >> input (math equation sits here)
                //                      >> contract (this are "forced" units for result display)
                //                      >> result (this is result)

                //variables for temporary storing names of smath parameters
                string inputVariableName;
                string outputVariableName;

                //determining the index of nodes of input and results
                int inputNodeIndex = -1;
                int resultNodeIndex = -1;

                if (mathDescriptionNode == null) inputNodeIndex = 0;
                if (mathDescriptionNode != null) inputNodeIndex = 1;

                if (mathDescriptionNode == null && mathResultUnitsNode == null) resultNodeIndex = 1;
                if (mathDescriptionNode != null && mathResultUnitsNode == null) resultNodeIndex = 2;
                if (mathDescriptionNode == null && mathResultUnitsNode != null) resultNodeIndex = 2;
                if (mathDescriptionNode != null && mathResultUnitsNode != null) resultNodeIndex = 3;


                //if this is input node
                if (mathInputEqationNode != null && mathResultValueNode == null)
                {

                    inputVariableName = mathInputEqationNode.ChildNodes[0].InnerText;

                    if (inputNames.Contains(inputVariableName))
                    {
                        int i1 = inputNames.IndexOf(inputVariableName);
                        string variableName = inputNames[i1];
                        string variableValue = inputValues[i1].ToString();
                        XMLdoc.DocumentElement.ChildNodes[1].ChildNodes[i].ChildNodes[0].ChildNodes[inputNodeIndex].ChildNodes[1].InnerText = variableValue;
                    }
                }

                //if this is result node
                if (mathResultValueNode != null)
                {

                    outputVariableName = mathResultValueNode.ChildNodes[0].InnerText;

                    if (outputNames.Contains(outputVariableName))
                    {
                        int i1 = outputNames.IndexOf(outputVariableName);
                        string variableName = outputNames[i1];
                        string variableValue = outputValues[i1].ToString();
                        XMLdoc.DocumentElement.ChildNodes[1].ChildNodes[i].ChildNodes[0].ChildNodes[resultNodeIndex].ChildNodes[0].InnerText = variableValue;
                    }

                }



                i++;
            }



        }

        public static string FormatXMLString(string sUnformattedXML)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXML);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }


    }
}
