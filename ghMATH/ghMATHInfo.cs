using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ghMath
{
    public class ghMATHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ghMath";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("cfa97a75-5f53-4a18-a6d2-2cf958631248");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
