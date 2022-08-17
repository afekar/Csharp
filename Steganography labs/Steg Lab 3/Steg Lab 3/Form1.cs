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
using System.Media;

namespace Steg_Lab_3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        byte[] sound1;
        byte[] result;
        Bitmap image1;
        int source_length;
        private void button1_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                sound1 = File.ReadAllBytes(filePath);
            }
            int channels = (int)(sound1[22]+sound1[23] * Math.Pow(2, 8));
            int sample_rate = (int)(sound1[24] + sound1[25]*Math.Pow(2,8) + sound1[26] * Math.Pow(2, 16) + sound1[27] * Math.Pow(2, 24));
            int bits_per_sample = (int)(sound1[34] + sound1[35] * Math.Pow(2, 8));
            textBox1.Text = channels.ToString();
            textBox2.Text = sample_rate.ToString();
            textBox3.Text = bits_per_sample.ToString();
        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
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
        private void button2_Click(object sender, EventArgs e)
        {
            BitArray bitsound = new BitArray(sound1);
            bool[] bits1 = new bool[bitsound.Length];
            bitsound.CopyTo(bits1, 0);
            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog1.FileName;
                image1 = new Bitmap(filePath);
                pictureBox1.Image = image1;
            }
            byte[] bytecont2 = ImageToByte(image1);

            BitArray bitimage1 = new BitArray(bytecont2);
            bool[] bits2 = new bool[bitimage1.Length];
            bitimage1.CopyTo(bits2, 0);
            int shift = 44;
            bool[] code = Hamming_coder(bits2);
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

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog1.FileName, result);
            }
            double MSE = 0; double RMSE = 0; double PSNR = 0;
            for (int i = shift; i < sound1.Length; i++)
            {
                MSE += Math.Pow(Convert.ToInt32(sound1[i] - result[i]), 2);
            }
            MSE = MSE / (sound1.Length - shift); RMSE = Math.Sqrt(MSE);
            PSNR = 20 * Math.Log10(255 / RMSE);
            textBox4.Text = Convert.ToString(MSE);
            textBox5.Text = Convert.ToString(RMSE);
            textBox6.Text = Convert.ToString(PSNR);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        SoundPlayer sp = new SoundPlayer();
        private void button3_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                sp.SoundLocation = filePath;
            }
            sp.Play();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sp.Stop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BitArray bitsound = new BitArray(result);
            bool[] bits = new bool[bitsound.Length];
            bitsound.CopyTo(bits, 0);
            int shift = 44;
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
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(extr);
            Bitmap extracted = new Bitmap(img);
            saveFileDialog1.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                img.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }
    }
}
