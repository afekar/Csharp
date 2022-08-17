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

namespace Steg_Lab_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        Bitmap image1;
        Bitmap image2;
        Bitmap res_bmp;
        int source_length;
        byte[] result;
        private void button1_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;

                image1 = new Bitmap(filePath);
                pictureBox1.Image = image1;
            }
            label1.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] bytecont1 = ImageToByte(image1);
            BitArray bitimage1 = new BitArray(bytecont1);
            bool[] bits1 = new bool[bitimage1.Length];
            bitimage1.CopyTo(bits1, 0);
            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog1.FileName;
                image2 = new Bitmap(filePath);
                pictureBox2.Image = image2;
            }
            byte[] bytecont2 = ImageToByte(image2);
            BitArray bitimage2 = new BitArray(bytecont2);
            bool[] bits2 = new bool[bitimage2.Length];
            bitimage2.CopyTo(bits2, 0);
            int shift = 54;
            if (bits2.Length * 8 > bits1.Length) 
            { MessageBox.Show("Невозможно полностью встроить изображение"); return; }
            for (int i = 0; i < bits2.Length; i++)
            {
                bits1[(shift + i) * 8] = bits2[i];
            }
            source_length = bits2.Length;
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
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(result);
            res_bmp = new Bitmap(img);
            pictureBox3.Image = res_bmp;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveFileDialog1.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    img.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
            label2.Visible = true;
            label3.Visible = true;
            double MSE_R = 0, MSE_G = 0, MSE_B = 0, RMSE_R = 0, RMSE_G = 0, RMSE_B = 0, PSNR = 0;
            for (int i = 54; i < bytecont1.Length; i++)
            {
                if (i % 3 == 0)
                {
                    MSE_R += Math.Pow(Convert.ToInt32(bytecont1[i] - result[i]),2);
                }
                if (i % 3 == 1)
                {
                    MSE_G += Math.Pow(Convert.ToInt32(bytecont1[i] - result[i]),2);
                }
                if (i % 3 == 2)
                {
                    MSE_B += Math.Pow(Convert.ToInt32(bytecont1[i] - result[i]),2);
                }
            }
            MSE_R = MSE_R / ((bytecont1.Length-shift) / 3); RMSE_R = Math.Sqrt(MSE_R);
            MSE_G = MSE_G / ((bytecont1.Length-shift) / 3); RMSE_G = Math.Sqrt(MSE_G);
            MSE_B = MSE_B / ((bytecont1.Length-shift) / 3); RMSE_B = Math.Sqrt(MSE_B);
            double RMSE = (RMSE_R + RMSE_G + RMSE_B) / 3;
            double MSE = (MSE_R+MSE_G+MSE_B)/3;
            PSNR = 20 * Math.Log10(255 / RMSE);
            textBox1.Text = Convert.ToString(MSE_R);
            textBox2.Text = Convert.ToString(MSE_G);
            textBox3.Text = Convert.ToString(MSE_B);
            textBox4.Text = Convert.ToString(RMSE_R);
            textBox5.Text = Convert.ToString(RMSE_G);
            textBox6.Text = Convert.ToString(RMSE_B);
            textBox7.Text = Convert.ToString(MSE);
            textBox8.Text = Convert.ToString(RMSE);
            textBox9.Text = Convert.ToString(PSNR);
            Form2 newForm = new Form2(bytecont1,result);
            newForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BitArray bitimage3 = new BitArray(result);
            bool[] bits = new bool[bitimage3.Length];
            bitimage3.CopyTo(bits, 0);
            int shift = 54;
            bool[] res_bits = new bool[source_length];
            for (int i = 0; i < source_length; i++)
            {
                res_bits[i] = bits[(shift + i) * 8];
            }
            byte[] extr = new byte[res_bits.Length / 8];
            for (int i = 0; i < res_bits.Length / 8; i++)
            {
                int temp = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (res_bits[i * 8 + 7 - j] == true)
                    {
                        temp += (int)Math.Pow(2, 7 - j);
                    }
                }
                extr[i] = Convert.ToByte(temp);
                temp = 0;
            }
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(extr);
            Bitmap extracted = new Bitmap(img);
            pictureBox4.Image = extracted;
            label4.Visible = true;
        }

        private void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            panel3.VerticalScroll.Value = panel1.VerticalScroll.Value;
            panel3.HorizontalScroll.Value = panel1.HorizontalScroll.Value;
        }
    }
}
