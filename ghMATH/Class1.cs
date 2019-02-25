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

            


        }

        private void button2_Click(object sender, EventArgs e)
        {
           


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
