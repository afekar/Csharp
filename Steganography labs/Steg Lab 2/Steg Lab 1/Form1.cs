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
        byte[] header = new byte[54];
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

        private bool[] Hamming_coder(bool[] bits)
        {
            bool[] code = new bool[bits.Length*12/8];
            int j = 0;
            for (int i=0; i<bits.Length; i+=8)
            {
                code[j] = bits[i]^bits[i+1]^bits[i+3]^bits[i+4]^bits[i+6]; //x1+x2+x4+x5+x7
                code[j + 1] = bits[i]^bits[i + 2]^bits[i + 3]^bits[i + 5]^bits[i + 6]; //x1+x3+x4+x6+x7
                code[j + 3] = bits[i+1]^bits[i + 2]^bits[i + 3]^bits[i + 7]; //x2+x3+x4+x8
                code[j + 7] = bits[i + 4] ^ bits[i + 5] ^ bits[i + 6] ^ bits[i + 7]; //x5+x6+x7+x8
                code[j + 2] = bits[i]; code[j + 4] = bits[i+1]; code[j + 5] = bits[i+2]; code[j + 6] = bits[i+3];
                code[j + 8] = bits[i+4]; code[j + 9] = bits[i + 5]; code[j + 10] = bits[i + 6]; code[j + 11] = bits[i + 7];
                j += 12;
            }
            return code;
        }
        private bool[] error_gen(bool[] code)
        {
            Random rnd = new Random();
            int value = 0;

            for (int i=0; i<code.Length;i+=12)
            {
                value = rnd.Next(0, 12);
                code[i + value] = !code[i + value];
            }
            return code;
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
            if (checkBox2.Checked == true)
            {
                for (int i = 0; i < 54; i++)
                {
                    header[i] = bytecont2[i];
                }
            }
            BitArray bitimage2 = new BitArray(bytecont2);
            bool[] bits2 = new bool[bitimage2.Length];
            bitimage2.CopyTo(bits2, 0);
            int shift = 54;
            bool[] code = Hamming_coder(bits2);
            if (code.Length * 8 > bits1.Length) 
            { MessageBox.Show("Невозможно полностью встроить изображение"); return; }
            if (checkBox1.Checked == true)
            {
                code = error_gen(code);
            }
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
            Form2 newForm = new Form2(bytecont1,result);
            newForm.Show();
        }
        private bool[] Hamming_decoder(bool[] bits)
        {
            bool[] decode = new bool[bits.Length * 8/12];
            int j = 0;
            bool r1, r2, r3, r4;
            for (int i = 0; i < bits.Length; i += 12)
            {
                int temp = 0;
                r1 = bits[i+2] ^ bits[i + 4] ^ bits[i + 6] ^ bits[i + 8] ^ bits[i + 10]; //x1+x2+x4+x5+x7
                r2 = bits[i+2] ^ bits[i + 5] ^ bits[i + 6] ^ bits[i + 9] ^ bits[i + 10]; //x1+x3+x4+x6+x7
                r3 = bits[i + 4] ^ bits[i + 5] ^ bits[i + 6] ^ bits[i + 11]; //x2+x3+x4+x8
                r4 = bits[i + 8] ^ bits[i + 9] ^ bits[i + 10] ^ bits[i + 11]; //x5+x6+x7+x8
                if (r1 != bits[i]) {temp += 1;} if (r2 != bits[i+1]) {temp+=2;} if (r3 != bits[i + 3]) {temp+=4;} if (r4 != bits[i + 7]) {temp+=8;}
                if (temp != 0)
                {
                    bits[i+temp - 1] = !bits[i+temp - 1];
                }
                decode[j] = bits[i+2]; decode[j + 1] = bits[i + 4]; decode[j + 2] = bits[i + 5]; decode[j + 3] = bits[i + 6];
                decode[j+4] = bits[i + 8]; decode[j + 5] = bits[i + 9]; decode[j + 6] = bits[i + 10]; decode[j + 7] = bits[i + 11];
                j += 8;
            }
            return decode;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                Bitmap image = new Bitmap(filePath);
                result = ImageToByte(image);
            }
            BitArray bitimage3 = new BitArray(result);
            bool[] bits = new bool[bitimage3.Length];
            bitimage3.CopyTo(bits, 0);
            int shift = 54;
            bool[] res_bits = new bool[source_length];
            for (int i = 0; i < source_length; i++)
            {
                res_bits[i] = bits[(shift + i) * 8];
            }
            bool[] decode = Hamming_decoder(res_bits);
            byte[] extr = new byte[decode.Length / 8];
            for (int i = 0; i < decode.Length / 8; i++)
            {
                int temp = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (decode[i * 8 + 7 - j] == true)
                    {
                        temp += (int)Math.Pow(2, 7 - j);
                    }
                }
                extr[i] = Convert.ToByte(temp);
            }
            if (checkBox2.Checked==true)
            {
                for (int i=0; i<shift;i++)
                {
                    extr[i] = header[i];
                }
            }
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(extr);
            Bitmap extracted = new Bitmap(img);
            pictureBox4.Image = extracted;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveFileDialog1.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    img.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
            label4.Visible = true;
        }

        private void panel1_Scroll(object sender, ScrollEventArgs e)
        {
            panel3.VerticalScroll.Value = panel1.VerticalScroll.Value;
            panel3.HorizontalScroll.Value = panel1.HorizontalScroll.Value;
        }
    }
}
