using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mathos.Parser;

namespace ghMath
{
    class ghMathUnits
    {
        /// <summary>
        /// Check whether units are valid
        /// This list should contain units defined in ghMathParser class.
        /// </summary>
        /// <param name="inputUnit">unit to be rested</param>
        /// <returns>bool, tells whether unit is supported</returns>
        /// 
        public static bool isUnitValid(string inputUnit)
        {
            var acceptedUnits = new List<string>();

            acceptedUnits.Add("%");
          
            //length units
            acceptedUnits.Add("mm");
            acceptedUnits.Add("cm");
            acceptedUnits.Add("dm");
            acceptedUnits.Add("m");
            acceptedUnits.Add("km");
            acceptedUnits.Add("ft");
            acceptedUnits.Add("in");
            acceptedUnits.Add("mile");
            acceptedUnits.Add("mi");

            //area units
            acceptedUnits.Add("acre");
            acceptedUnits.Add("ha");
            acceptedUnits.Add("hectare");

            //volume units
            acceptedUnits.Add("gal");
            acceptedUnits.Add("liter");

            //mass units
            acceptedUnits.Add("mg");
            acceptedUnits.Add("g");
            acceptedUnits.Add("kg");
            acceptedUnits.Add("oz");
            acceptedUnits.Add("tonne");

            //energy units
            acceptedUnits.Add("J");
            acceptedUnits.Add("joule");
            acceptedUnits.Add("kJ");
            acceptedUnits.Add("MJ");

            //force
            acceptedUnits.Add("mN");
            acceptedUnits.Add("N");
            acceptedUnits.Add("kN");
            acceptedUnits.Add("MN");
            acceptedUnits.Add("kgf");

            //pressure
            acceptedUnits.Add("Pa");
            acceptedUnits.Add("kPa");
            acceptedUnits.Add("MPa");
            acceptedUnits.Add("GPa");
            acceptedUnits.Add("psf");
            acceptedUnits.Add("psi");


            //angles
            acceptedUnits.Add("deg");
            acceptedUnits.Add("°");
            acceptedUnits.Add("grad");

            //temperature
            acceptedUnits.Add("°C");

            //electric
            acceptedUnits.Add("amp");

            bool unitTest = acceptedUnits.Contains(inputUnit);
            return unitTest;
        }
    }
}
