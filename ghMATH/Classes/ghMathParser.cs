using Mathos.Parser;
using System;
using System.Linq;

namespace ghMath
{
    class ghMathParser
    {
        /// <summary>
        /// method adds additional functions and variables to parser object
        /// variables include units - conversation to SI units happens within the parser
        /// </summary>
        /// <param name="inputParser">MathOSParser object https://github.com/MathosProject/Mathos-Parser </param>
        /// <returns>returns MathOSParser object with added items</returns>
        public static MathParser additionsToMathParser(MathParser inputParser)
        {
            // ADD CUSTOM FUNCTIONS

            inputParser.LocalFunctions.Add("nthroot", inputs => Math.Pow(inputs[1], (1 / inputs[0])));
            inputParser.LocalFunctions.Add("cot", inputs => 1 / (Math.Tan(inputs[0])));
            inputParser.LocalFunctions.Remove("log"); //redefine log - inputs are swapped for Math.Log and for sMath format
            inputParser.LocalFunctions.Add("log", inputs => Math.Log(inputs[1], inputs[0]));
            inputParser.LocalFunctions.Add("min", inputs => inputs.Min());
            inputParser.LocalFunctions.Add("max", inputs => inputs.Max());
            // inputParser.LocalFunctions.Add("&", inputs => inputs[0] == 1 && inputs[1] == 1 ? 1 : 0);
            //inputParser.LocalFunctions.Add("|", inputs => inputs[0] == 1 || inputs[1] == 1 ? 1 : 0);
            inputParser.LocalFunctions.Add("if", inputs =>
            {
                if (inputs[2] == 1)
                {
                    return inputs[1];
                }
                else
                {
                    return inputs[0];
                }
            });


            //remove % operator, because it is used as "units" to represent percent
            inputParser.Operators.Remove("%");
            inputParser.Operators.Add("&", (a, b) => a == 1 && b == 1 ? 1 : 0);
            inputParser.Operators.Add("|", (a, b) => a == 1 || b == 1 ? 1 : 0);
            inputParser.Operators.Add("¤", (a, b) => a != b ? 1 : 0);
            inputParser.Operators.Add("≤", (a, b) => a <= b ? 1 : 0);
            inputParser.Operators.Add("≥", (a, b) => a >= b ? 1 : 0);
            inputParser.Operators.Add("≡", (a, b) => a == b ? 1 : 0);
            inputParser.Operators.Add("≠", (a, b) => a != b ? 1 : 0);


            //ADD UNITS

            //math constants
            inputParser.LocalVariables.Add("π", Math.PI);
            inputParser.LocalVariables.Add("%", 0.01);


            //length units
            inputParser.LocalVariables.Add("mm", 0.001);
            inputParser.LocalVariables.Add("cm", 0.01);
            inputParser.LocalVariables.Add("dm", 0.1);
            inputParser.LocalVariables.Add("m", 1);
            inputParser.LocalVariables.Add("km", 1000);
            inputParser.LocalVariables.Add("ft", 0.3048);
            inputParser.LocalVariables.Add("in", 0.0254);
            inputParser.LocalVariables.Add("mile", 1609.344);
            inputParser.LocalVariables.Add("mi", 1609.344);

            //area units
            inputParser.LocalVariables.Add("acre", 4046.8564224);
            inputParser.LocalVariables.Add("ha", 10000);
            inputParser.LocalVariables.Add("hectare", 10000);

            //volume units
            inputParser.LocalVariables.Add("gal", 0.0037854119678);
            inputParser.LocalVariables.Add("liter", 0.001);

            //mass units
            inputParser.LocalVariables.Add("mg", 0.000001);
            inputParser.LocalVariables.Add("g", 0.001);
            inputParser.LocalVariables.Add("kg", 1);
            inputParser.LocalVariables.Add("oz", 0.028349523125);
            inputParser.LocalVariables.Add("tonne", 1000);

            //energy units
            inputParser.LocalVariables.Add("J", 1);
            inputParser.LocalVariables.Add("joule", 1);
            inputParser.LocalVariables.Add("kJ", 1000);
            inputParser.LocalVariables.Add("MJ", 1000000);

            //force
            inputParser.LocalVariables.Add("mN", 0.001);
            inputParser.LocalVariables.Add("N", 1);
            inputParser.LocalVariables.Add("kN", 1000);
            inputParser.LocalVariables.Add("MN", 1000000);
            inputParser.LocalVariables.Add("kgf", 9.80665);

            //pressure
            inputParser.LocalVariables.Add("Pa", 1);
            inputParser.LocalVariables.Add("kPa", 1000);
            inputParser.LocalVariables.Add("MPa", 1000000);
            inputParser.LocalVariables.Add("GPa", 1000000000);
            inputParser.LocalVariables.Add("psf", 47.8802589803358);
            inputParser.LocalVariables.Add("psi", 6894.75729316836);


            //angles
            inputParser.LocalVariables.Add("deg", Math.PI / 180);
            inputParser.LocalVariables.Add("°", Math.PI / 180);
            inputParser.LocalVariables.Add("grad", Math.PI / 200);

            //temperature
            inputParser.LocalVariables.Add("°C", -273.15);

            //electric
            inputParser.LocalVariables.Add("amp", 1);



            return inputParser;
        }
    }
}
