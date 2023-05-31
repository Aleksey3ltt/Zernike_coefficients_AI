using System.Diagnostics.Metrics;
using System.Drawing;
using System;
using System.Diagnostics;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Diagnostics.Tracing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Zernike_coefficients_AI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            display.RadioButonEnabled(new List<RadioButton> { radioButton3, radioButton4, radioButton5, radioButton6 }, false);
        }

        int expNumber;
        Display display = new Display();
        Point locationXY;

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCount = comboBox2.SelectedItem.ToString();
            expNumber = Convert.ToInt32(selectedCount);
        }

        private void button1_Click(object sender, EventArgs e)  //zernike coeficients
        {
            string path = Directory.GetCurrentDirectory();
            if (File.Exists(path + "\\zernike.txt"))
            {
                File.Delete(path + "\\zernike.txt");
            }
            StreamWriter zernikeText = new StreamWriter(path + "\\zernike.txt", true);
            double[] array = new double[21];
            double[] array2 = new double[21];
            if (radioButton1.Checked == true)
            {
                var rand = new Random();
                for (int i = 0; i < array.Length; i++)
                {
                    if (i <= 2)
                        array[i] = 0;
                    else
                    {
                        array[i] = Math.Round((Convert.ToDouble((rand.Next(-80, 80))) / 1000) / expNumber, 3);
                        array2[i] = array[i] * array[i];
                    }
                    zernikeText.WriteLine(Convert.ToString(array[i]).Replace(',', '.'));
                    dataGridView1.Rows[0].Cells[i].Value = array[i];
                }
            }
            else if (radioButton2.Checked == true)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (comboBox1.SelectedIndex == 0) array[3] = Math.Round(0.5 / expNumber, 3);        //defocus
                    else if (comboBox1.SelectedIndex == 1) array[4] = Math.Round(0.5 / expNumber, 3);   //actigmatism 45
                    else if (comboBox1.SelectedIndex == 2) array[5] = Math.Round(0.5 / expNumber, 3);   //actigmatism 0
                    else if (comboBox1.SelectedIndex == 3) array[6] = Math.Round(0.5 / expNumber, 3);   //coma y
                    else if (comboBox1.SelectedIndex == 4) array[7] = Math.Round(0.5 / expNumber, 3);   //coma x
                    else array[i] = 0;
                    array2[i] = array[i] * array[i];
                    zernikeText.WriteLine(Convert.ToString(array[i]).Replace(',', '.'));
                    dataGridView1.Rows[0].Cells[i].Value = array[i];
                }
            }
            zernikeText.Close();
            double rms = Math.Round(Math.Pow(array2.Sum(), 0.5), 3);
            textBox1.Text = Convert.ToString(rms);
            button2.Enabled = true;
            display.DisplayClear(new List<PictureBox> { pictureBox1, pictureBox2 });
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                button1.Enabled = true;
                comboBox1.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                button1.Enabled = true;
                comboBox1.Enabled = true;
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e) //Hartmann
        {
            string path = Directory.GetCurrentDirectory();
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/K C:\\ProgramData\\Anaconda3\\envs\\3ltt\\python.exe " + path + "\\pyScript_hartmann.py & exit";
            //process.StartInfo.Arguments = "/K python " + path + "\\pyScript_hartmann.py & exit";
            //process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            string image1Path = path + "\\hart.png";
            string image2Path = path + "\\Phi2.png";
            display.DisplayPicture(image1Path, pictureBox1);
            display.DisplayPicture(image2Path, pictureBox2);
            comboBox3.Enabled = true;
            button3.Enabled = true;
            display.RadioButonEnabled(new List<RadioButton> { radioButton3, radioButton4, radioButton5, radioButton6 }, false);
            display.RadioButonChecked(new List<RadioButton> { radioButton3, radioButton4, radioButton5, radioButton6 }, false);
            display.DisplayClear(new List<PictureBox> { pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8, pictureBox9, pictureBox10 });
        }

        private void button3_Click(object sender, EventArgs e)  //test_object
        {
            string path = Directory.GetCurrentDirectory();
            string pathTestObj = path + "\\test_objects\\" + comboBox3.Text + ".png";
            display.DisplayPicture(pathTestObj, pictureBox3);

            if (File.Exists(path + "\\test_object.txt"))
            {
                File.Delete(path + "\\test_object.txt");
            }
            StreamWriter testObjectText = new StreamWriter(path + "\\test_object.txt", true);
            testObjectText.WriteLine(Convert.ToString(pathTestObj.ToString()).Replace("\\", "/").Replace(@"\r\n?|\n", ""));
            testObjectText.Close();
            comboBox4.Enabled = true;
            button4.Enabled = true;
            display.DisplayClear(new List<PictureBox> { pictureBox7, pictureBox8, pictureBox9, pictureBox10 });
        }

        private void button4_Click(object sender, EventArgs e) //calculate AI zernike
        {
            display.DisplayClear(new List<PictureBox> { pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8, pictureBox9, pictureBox10 });
            string path = Directory.GetCurrentDirectory();
            string pathCNN = path + "\\networks\\" + comboBox4.Text;
            if (File.Exists(path + "\\networks\\cnn.txt"))
            {
                File.Delete(path + "\\networks\\cnn.txt");
            }
            StreamWriter CnnText = new StreamWriter(path + "\\networks\\cnn.txt", true);
            CnnText.WriteLine(Convert.ToString(pathCNN.ToString()).Replace("\\", "/").Replace(@"\r\n?|\n", ""));
            CnnText.Close();
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/K C:\\ProgramData\\Anaconda3\\envs\\3ltt\\python.exe " + path + "\\pyScript_zernike_cnn_inference.py & exit";
            //process.StartInfo.Arguments = "/K python " + path + "\\pyScript_zernike_cnn_inference.py & exit";
            //process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            string image4Path = path + "\\Phi4.png";
            string image5Path = path + "\\deltaPhi.png";
            string image6Path = path + "\\Zernike.png";
            display.DisplayPicture(image4Path, pictureBox4);
            display.DisplayPicture(image5Path, pictureBox5);
            display.DisplayPicture(image6Path, pictureBox6);
            display.RadioButonEnabled(new List<RadioButton> { radioButton3, radioButton4, radioButton5, radioButton6 }, true);
            display.RadioButonChecked(new List<RadioButton> { radioButton3, radioButton4, radioButton5, radioButton6 }, true);
            display.RadioButonChecked(new List<RadioButton> { radioButton3, radioButton5 }, true);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            string image8Path, image9Path;

            if (radioButton3.Checked == true & radioButton5.Checked == true)
            {
                image8Path = path + "\\image_without_correction.png";
                image9Path = path + "\\image_dm_correction.png";
                display.DisplayPicture(image8Path, pictureBox8);
                display.DisplayPicture(image9Path, pictureBox9);
            }
            if (radioButton3.Checked == true & radioButton6.Checked == true)
            {
                image8Path = path + "\\deconvolution_without_correction.png";
                image9Path = path + "\\deconvolution_dm.png";
                display.DisplayPicture(image8Path, pictureBox8);
                display.DisplayPicture(image9Path, pictureBox9);
            }
            radioButton5.Enabled = true;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            string image9Path;

            if (radioButton4.Checked == true & radioButton6.Checked == true)
            {
                image9Path = path + "\\deconvolution_ai.png";
                display.DisplayPicture(image9Path, pictureBox9);
            }
            radioButton5.Enabled = false;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            string image7Path, image8Path, image9Path;

            if (radioButton3.Checked == true & radioButton5.Checked == true)
            {
                image8Path = path + "\\image_without_correction.png";
                image9Path = path + "\\image_dm_correction.png";
                display.DisplayPicture(image8Path, pictureBox8);
                display.DisplayPicture(image9Path, pictureBox9);
            }
            image7Path = path + "\\image_0.png";
            display.DisplayPicture(image7Path, pictureBox7);
            radioButton4.Enabled = false;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            string image7Path, image8Path, image9Path;

            if (radioButton3.Checked == true & radioButton6.Checked == true)
            {
                image8Path = path + "\\deconvolution_without_correction.png";
                image9Path = path + "\\deconvolution_dm.png";
                display.DisplayPicture(image8Path, pictureBox8);
                display.DisplayPicture(image9Path, pictureBox9);
            }
            image7Path = path + "\\deconvolution_0.png";
            display.DisplayPicture(image7Path, pictureBox7);
            radioButton4.Enabled = true;
        }

        private void pictureBox7_MouseMove(object sender, MouseEventArgs e)
        {
            locationXY = e.Location;
            display.Magnifying(pictureBox7, pictureBox10, locationXY.X, locationXY.Y);
            label12.Text = "Coordinates: " + locationXY.X.ToString() + ", " + locationXY.Y.ToString();
        }

        private void pictureBox8_MouseMove(object sender, MouseEventArgs e)
        {
            locationXY = e.Location;
            display.Magnifying(pictureBox8, pictureBox10, locationXY.X, locationXY.Y);
            label12.Text = "Coordinates: " + locationXY.X.ToString() + ", " + locationXY.Y.ToString();
        }

        private void pictureBox9_MouseMove(object sender, MouseEventArgs e)
        {
            locationXY = e.Location;
            display.Magnifying(pictureBox9, pictureBox10, locationXY.X, locationXY.Y);
            label12.Text = "Coordinates: " + locationXY.X.ToString() + ", " + locationXY.Y.ToString();
        }
    }
}