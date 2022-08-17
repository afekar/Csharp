using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steg_Lab_1
{
    public partial class Form2 : Form
    {
        byte[] bytecont1;
        byte[] result;
        public Form2(byte[] bytecont1, byte[] result)
        {
            InitializeComponent();
            this.bytecont1 = bytecont1;
            this.result = result;
        }
        private int [] count(List<byte> bytecont)
        {
            int[] res=new int [256];
            for (int i=0; i<bytecont.Count(); i++) { res[Convert.ToInt32(bytecont[i])] += 1; }
            return res;
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            var r = new List<byte>();
            var g = new List<byte>();
            var b = new List<byte>();
            var r_ = new List<byte>();
            var g_ = new List<byte>();
            var b_ = new List<byte>();
            for (int i=54; i<bytecont1.Length; i++)
            {
                if (i % 3 == 0) { r.Add(bytecont1[i]); r_.Add(result[i]); }
                if (i % 3 == 1) { g.Add(bytecont1[i]); g_.Add(result[i]); }
                if (i % 3 == 2) { b.Add(bytecont1[i]); b_.Add(result[i]); }
            }
            int[] arr = new int[256];
            for (int i=0; i<256; i++) { arr[i] = i; }
            chart1.Series["RED"].Points.DataBindXY(arr, count(r));
            chart2.Series["GREEN"].Points.DataBindXY(arr, count(g));
            chart3.Series["BLUE"].Points.DataBindXY(arr, count(b));
            chart4.Series["Series1"].Points.DataBindXY(arr, count(r_));
            chart5.Series["Series1"].Points.DataBindXY(arr, count(g_));
            chart6.Series["Series1"].Points.DataBindXY(arr, count(b_));
            chart1.ChartAreas[0].AxisX.Minimum = 0;chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart2.ChartAreas[0].AxisX.Minimum = 0;chart2.ChartAreas[0].AxisX.Maximum = 255;
            chart3.ChartAreas[0].AxisX.Minimum = 0;chart3.ChartAreas[0].AxisX.Maximum = 255;
            chart4.ChartAreas[0].AxisX.Minimum = 0;chart4.ChartAreas[0].AxisX.Maximum = 255;
            chart5.ChartAreas[0].AxisX.Minimum = 0;chart5.ChartAreas[0].AxisX.Maximum = 255;
            chart6.ChartAreas[0].AxisX.Minimum = 0;chart6.ChartAreas[0].AxisX.Maximum = 255;
        }
    }
}
