using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using MathNet.Numerics.Integration;
using MathNet.Numerics;


namespace Steg_Lab_4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        byte[] cont;
        byte[] inject;
        byte[] result;
        int source_length;
        private void button1_Click(object sender, EventArgs e)
        {
            cont = ReadFile(cont);
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(cont);
            Bitmap bmp = new Bitmap(img);
            pictureBox1.Image = bmp;
            ;
        }
        private byte[] ReadFile(byte[] bytes)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                bytes = new byte[filePath.Length];
                bytes = File.ReadAllBytes(filePath);
            }
            return bytes;
        }
        private bool[] Hamming_coder(bool[] bits)
        {
            bool[] code = new bool[bits.Length * 12 / 8];
            int j = 0;
            for (int i = 0; i < bits.Length; i += 8)
            {
                code[j] = bits[i] ^ bits[i + 1] ^ bits[i + 3] ^ bits[i + 4] ^ bits[i + 6]; //x1+x2+x4+x5+x7
                code[j + 1] = bits[i] ^ bits[i + 2] ^ bits[i + 3] ^ bits[i + 5] ^ bits[i + 6]; //x1+x3+x4+x6+x7
                code[j + 3] = bits[i + 1] ^ bits[i + 2] ^ bits[i + 3] ^ bits[i + 7]; //x2+x3+x4+x8
                code[j + 7] = bits[i + 4] ^ bits[i + 5] ^ bits[i + 6] ^ bits[i + 7]; //x5+x6+x7+x8
                code[j + 2] = bits[i]; code[j + 4] = bits[i + 1]; code[j + 5] = bits[i + 2]; code[j + 6] = bits[i + 3];
                code[j + 8] = bits[i + 4]; code[j + 9] = bits[i + 5]; code[j + 10] = bits[i + 6]; code[j + 11] = bits[i + 7];
                j += 12;
            }
            return code;
        }
        private bool[] Hamming_decoder(bool[] bits)
        {
            bool[] decode = new bool[bits.Length * 8 / 12];
            int j = 0;
            bool r1, r2, r3, r4;
            for (int i = 0; i < bits.Length; i += 12)
            {
                int temp = 0;
                r1 = bits[i + 2] ^ bits[i + 4] ^ bits[i + 6] ^ bits[i + 8] ^ bits[i + 10]; //x1+x2+x4+x5+x7
                r2 = bits[i + 2] ^ bits[i + 5] ^ bits[i + 6] ^ bits[i + 9] ^ bits[i + 10]; //x1+x3+x4+x6+x7
                r3 = bits[i + 4] ^ bits[i + 5] ^ bits[i + 6] ^ bits[i + 11]; //x2+x3+x4+x8
                r4 = bits[i + 8] ^ bits[i + 9] ^ bits[i + 10] ^ bits[i + 11]; //x5+x6+x7+x8
                if (r1 != bits[i]) { temp += 1; }
                if (r2 != bits[i + 1]) { temp += 2; }
                if (r3 != bits[i + 3]) { temp += 4; }
                if (r4 != bits[i + 7]) { temp += 8; }
                if (temp != 0)
                {
                    bits[i + temp - 1] = !bits[i + temp - 1];
                }
                decode[j] = bits[i + 2]; decode[j + 1] = bits[i + 4]; decode[j + 2] = bits[i + 5]; decode[j + 3] = bits[i + 6];
                decode[j + 4] = bits[i + 8]; decode[j + 5] = bits[i + 9]; decode[j + 6] = bits[i + 10]; decode[j + 7] = bits[i + 11];
                j += 8;
            }
            return decode;
        }
        private int f_glad(byte[] bytes)
        {
            int temp = 0;
            for (int i=0; i<bytes.Length-1;i++)
            {
                temp += Math.Abs((int)bytes[i + 1] - (int)bytes[i]);
            }
            return temp;
        }
        private byte flip(byte b,int m)
        {
            if (m == 1)
            {
                if ((int)b % 2 == 0)
                    b += 1;
                else { b -= 1; }
            }
            if (m == -1)
            {
                if ((int)b % 2 == 0)
                    b -= 1;
                else { b += 1; }
            }
            return b;
        }
        private double RS(byte[] bytes,int[] mask)
        {
            double res=0;
            int r_0 = 0; int r_0_inv = 0; int r_1 = 0; int r_1_inv = 0; // r_0 маска неинвертирована, r_1 инвертирование lsb
            int s_0 = 0; int s_0_inv = 0; int s_1 = 0; int s_1_inv = 0;
            for (int i=0; i<bytes.Length; i+=4)
            {
                byte[] group_orig = new byte[4];
                byte[] group_up = new byte[4];
                byte[] group_down = new byte[4];
                byte[] group_orig_lsb = new byte[4];
                byte[] group_orig_lsb_up = new byte[4];
                byte[] group_orig_lsb_down = new byte[4];
                for (int j=0; j<4; j++)
                {
                    group_orig[j] = bytes[i + j];
                    group_orig_lsb[j] = bytes[i + j];
                    if ((group_orig_lsb[j] % 2) == 0) { group_orig_lsb[j] += 1; }
                    else { group_orig_lsb[j] -= 1; }
                    group_up[j] = (byte)(((int)flip(group_orig[j], mask[j])+256)%256);
                    group_down[j] = (byte)(((int)flip(group_orig[j], -mask[j]) + 256) % 256);
                    group_orig_lsb_up[j] = (byte)(((int)flip(group_orig_lsb[j], mask[j]) + 256) % 256);
                    group_orig_lsb_down[j] = (byte)(((int)flip(group_orig_lsb[j], -mask[j]) + 256) % 256);
                }
                int orig = f_glad(group_orig);
                int up = f_glad(group_up);
                int down = f_glad(group_down);
                int orig_lsb= f_glad(group_orig_lsb);
                int orig_lsb_up = f_glad(group_orig_lsb_up);
                int orig_lsb_down = f_glad(group_orig_lsb_down);
                if (up > orig) { r_0 += 1; }
                if (up < orig) { s_0 += 1; }
                if (down > orig) { r_0_inv += 1; }
                if (down < orig) { s_0_inv += 1; }
                if (orig_lsb_up > orig_lsb) { r_1 += 1; }//orig_lsb
                if (orig_lsb_up < orig_lsb) { s_1 += 1; }
                if (orig_lsb_down > orig_lsb) { r_1_inv += 1; }
                if (orig_lsb_down < orig_lsb) { s_1_inv += 1; }
            }
            int d0 = 0; int d0_inv = 0; int d1 = 0; int d1_inv = 0;
            d0 = r_0 - s_0;
            d0_inv = r_0_inv - s_0_inv;
            d1 = r_1 - s_1;
            d1_inv = r_1_inv - s_1_inv;
            double a = 2 * (d1 + d0);
            double b = (d0_inv - d1_inv - d1 - 3 * d0);
            double c = d0 - d0_inv;
            double D = Math.Abs(b * b) - 4 * a * c;
            b *= -1;
            if (D < 0) { return 0; }
            if (D == 0) {return ((b/(2*a))/(b/(2*a)-0.5));}
            D = Math.Sqrt(D);
            double x1 = (b + D) / (2 * a);
            double x2 = (b - D) / (2 * a);
            double x = 0;
            if (Math.Abs(x1) > Math.Abs(x2)) { x = x2; }
            else { x = x1; }
            res = x/(x-0.5);
            ;
            return res;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            BitArray bitcont = new BitArray(cont);
            bool[] bits1 = new bool[bitcont.Length];
            bitcont.CopyTo(bits1, 0);
            inject =  ReadFile(inject);
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(inject);
            Bitmap bmp = new Bitmap(img);
            pictureBox2.Image = bmp;
            int[] mask = new int[4] { 1, 0, 0, 1 };
            BitArray bitinject = new BitArray(inject);
            bool[] bits2 = new bool[bitinject.Length];
            bitinject.CopyTo(bits2, 0);
            int shift = 54;
            bool[] code;
            if (checkBox1.Checked == true)
            {
                code = Hamming_coder(bits2);
            }
            else { code = new bool[bits2.Length]; bits2.CopyTo(code, 0); }
            source_length = code.Length*8;
            if (code.Length * 8 > bits1.Length)
            { MessageBox.Show("Невозможно полностью встроить файл"); return; }
            for (int i = 0; i < code.Length; i++)
            {
                bits1[(shift + i) * 8] = code[i];
            }
            source_length = code.Length;
            result = new byte[bits1.Length / 8];
            for (int i = 0; i < bits1.Length / 8; i++)
            {
                int temp = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (bits1[i * 8 + 7 - j] == true)
                    {
                        temp += (int)Math.Pow(2, 7 - j);
                    }
                }
                result[i] = Convert.ToByte(temp);
                temp = 0;
            }
            byte[] R_comp = new byte[(result.Length - shift) / 3];
            byte[] G_comp = new byte[(result.Length - shift) / 3];
            byte[] B_comp = new byte[(result.Length - shift) / 3];
            int k = 0;
            for (int i = shift; i < result.Length; i++) 
            { 
                if (i % 3 == 0) { R_comp[k] = result[i]; }
                if (i % 3 == 1) { G_comp[k] = result[i]; }
                if (i % 3 == 2) { B_comp[k] = result[i]; k++; }
            }
            double RED_RS = RS(R_comp, mask);
            double GREEN_RS = RS(G_comp, mask);
            double BLUE_RS = RS(B_comp, mask);
            double avg_x=0; double avg_y = 0; double disp_x = 0; double disp_y = 0; double cov_xy = 0; double k1 = 0.01; double k2 = 0.03;
            double L = Math.Pow(2, 24) - 1; double SSIM = 0; double Hi_R = 0; double Hi_G = 0; double Hi_B = 0;
            double c1 = Math.Pow(k1 * L, 2); double c2 = Math.Pow(k2 * L, 2);
            int[] h_R = new int[256]; int[] h_G = new int[256]; int[] h_B = new int[256];
            int[] s_R = new int[256]; int[] s_G = new int[256]; int[] s_B = new int[256];
            if (textBox20.Text.Length!=0) { source_length = source_length/ Convert.ToInt32(textBox20.Text); }
            for (int i=shift; i<source_length+shift; i++)// cont.Length
            {
                if (i % 3 == 0) { h_R[(int)result[i]] += 1; s_R[(int)cont[i]] += 1; }
                if (i % 3 == 1) { h_G[(int)result[i]] += 1; s_G[(int)cont[i]] += 1; }
                if (i % 3 == 2) { h_B[(int)result[i]] += 1; s_B[(int)cont[i]] += 1; }
                avg_x += (int)cont[i];
                avg_y += (int)result[i];
            }
            for (int i=0; i<256; i++)
            {
                if (h_R[i] == 0) { h_R[i] += 1; } if (s_R[i] == 0) { s_R[i] += 1; }
                if (h_G[i] == 0) { h_G[i] += 1; } if (s_G[i] == 0) { s_G[i] += 1; }
                if (h_B[i] == 0) { h_B[i] += 1; } if (s_B[i] == 0) { s_B[i] += 1; }
            }
            avg_x /= (cont.Length - shift); avg_y /= (result.Length - shift);
            for (int i = shift; i < cont.Length; i++)
            {
                disp_x += Math.Pow((int)cont[i]-avg_x,2);
                disp_y += Math.Pow((int)result[i]-avg_y,2);
                cov_xy += (int)cont[i]* (int)result[i]-avg_x*avg_y;
            }
            cov_xy /=(cont.Length-shift);
            disp_x/=(cont.Length-shift); disp_y /= (result.Length - shift);
            SSIM = ((2 * avg_x * avg_y + c1) * (2 * cov_xy + c2)) / ((Math.Pow(avg_x, 2) + Math.Pow(avg_y, 2) + c1) * (Math.Pow(disp_x, 2) + Math.Pow(disp_y, 2) + c2));
            double His_R = 0; double His_G = 0; double His_B = 0;
            for (int i=0; i<128; i++)
            {
                Hi_R += Math.Pow(h_R[2 * i] - ((h_R[2 * i] + h_R[2 * i + 1]) / 2), 2) / ((h_R[2 * i] + h_R[2 * i + 1]) / 2);
                Hi_G += Math.Pow(h_G[2 * i] - ((h_G[2 * i] + h_G[2 * i + 1]) / 2), 2) / ((h_G[2 * i] + h_G[2 * i + 1]) / 2);
                Hi_B += Math.Pow(h_B[2 * i] - ((h_B[2 * i] + h_B[2 * i + 1]) / 2), 2) / ((h_B[2 * i] + h_B[2 * i + 1]) / 2);
                His_R += Math.Pow(s_R[2 * i] - ((s_R[2 * i] + s_R[2 * i + 1]) / 2), 2) / ((s_R[2 * i] + s_R[2 * i + 1]) / 2);
                His_G += Math.Pow(s_G[2 * i] - ((s_G[2 * i] + s_G[2 * i + 1]) / 2), 2) / ((s_G[2 * i] + s_G[2 * i + 1]) / 2);
                His_B += Math.Pow(s_B[2 * i] - ((s_B[2 * i] + s_B[2 * i + 1]) / 2), 2) / ((s_B[2 * i] + s_B[2 * i + 1]) / 2);
            }
            double gamma = SpecialFunctions.Gamma(63);
            double pow = Math.Pow(2, 63);
            double composite_R = SimpsonRule.IntegrateComposite(x => Math.Exp(-x/2.0) * Math.Pow(x,62), 0.0, Hi_R, 10000);
            double composite_G = SimpsonRule.IntegrateComposite(x => Math.Exp(-x / 2.0) * Math.Pow(x, 62), 0.0, Hi_G, 10000);
            double composite_B = SimpsonRule.IntegrateComposite(x => Math.Exp(-x / 2.0) * Math.Pow(x, 62), 0.0, Hi_B, 10000);
            double composites_R= SimpsonRule.IntegrateComposite(x => Math.Exp(-x / 2.0) * Math.Pow(x, 62), 0.0, His_R, 10000);
            double composites_G = SimpsonRule.IntegrateComposite(x => Math.Exp(-x / 2.0) * Math.Pow(x, 62), 0.0, His_G, 10000);
            double composites_B = SimpsonRule.IntegrateComposite(x => Math.Exp(-x / 2.0) * Math.Pow(x, 62), 0.0, His_B, 10000);
            double P_R = 1 - (1.0 / (pow * gamma)) * composite_R;
            double P_G = 1 - (1.0 / (pow * gamma)) * composite_G;
            double P_B = 1 - (1.0 / (pow * gamma)) * composite_B;
            double Ps_R = 1 - (1.0 / (pow * gamma)) * composites_R;
            double Ps_G = 1 - (1.0 / (pow * gamma)) * composites_G;
            double Ps_B = 1 - (1.0 / (pow * gamma)) * composites_B;
            double MSE_R = 0, MSE_G = 0, MSE_B = 0, RMSE_R = 0, RMSE_G = 0, RMSE_B = 0, PSNR = 0;
            for (int i = 54; i < cont.Length; i++)
            {
                if (i % 3 == 0)
                {
                    MSE_R += Math.Pow(Convert.ToInt32(cont[i] - result[i]), 2);
                }
                if (i % 3 == 1)
                {
                    MSE_G += Math.Pow(Convert.ToInt32(cont[i] - result[i]), 2);
                }
                if (i % 3 == 2)
                {
                    MSE_B += Math.Pow(Convert.ToInt32(cont[i] - result[i]), 2);
                }
            }
            MSE_R = MSE_R / ((cont.Length - shift) / 3); RMSE_R = Math.Sqrt(MSE_R);
            MSE_G = MSE_G / ((cont.Length - shift) / 3); RMSE_G = Math.Sqrt(MSE_G);
            MSE_B = MSE_B / ((cont.Length - shift) / 3); RMSE_B = Math.Sqrt(MSE_B);
            double RMSE = (RMSE_R + RMSE_G + RMSE_B) / 3;
            double MSE = (MSE_R + MSE_G + MSE_B) / 3;
            PSNR = 20 * Math.Log10(255 / RMSE);
            textBox1.Text = MSE.ToString(); textBox2.Text = RMSE.ToString(); textBox3.Text = PSNR.ToString();
            textBox4.Text = SSIM.ToString();
            textBox5.Text = His_R.ToString(); textBox6.Text = His_G.ToString(); textBox7.Text = His_B.ToString();
            textBox8.Text = Math.Abs(Ps_R).ToString(); textBox9.Text = Math.Abs(Ps_G).ToString(); textBox10.Text = Math.Abs(Ps_B).ToString();
            textBox11.Text = Hi_R.ToString(); textBox12.Text = Hi_G.ToString(); textBox13.Text = Hi_B.ToString();
            textBox14.Text = Math.Abs(P_R).ToString(); textBox15.Text = Math.Abs(P_G).ToString(); textBox16.Text = Math.Abs(P_B).ToString();
            textBox17.Text = RED_RS.ToString(); textBox18.Text = GREEN_RS.ToString(); textBox19.Text = BLUE_RS.ToString();
            ;
        }
    }
}
