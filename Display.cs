using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Zernike_coefficients_AI
{
    internal class Display
    {
        public void DisplayPicture(string pathImage, PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                DisplayClear(new List<PictureBox> { pictureBox });
                FileStream fileStream = new System.IO.FileStream(pathImage, FileMode.Open, FileAccess.Read);
                pictureBox.Image = System.Drawing.Image.FromStream(fileStream);
                fileStream.Close();
            }
            else
            {
                FileStream fileStream = new System.IO.FileStream(pathImage, FileMode.Open, FileAccess.Read);
                pictureBox.Image = System.Drawing.Image.FromStream(fileStream);
                fileStream.Close();
            }
        }

        public void DisplayClear(List<PictureBox> pictureBoxes)
        {
            foreach (PictureBox pb in pictureBoxes)
            {
                if (pb.Image != null)
                {
                    pb.Image.Dispose();
                    pb.Image = null;
                }
                else 
                {
                    return;
                }
            }
        }

        public void RadioButonEnabled(List<RadioButton> radioButtons, bool trueFalse)
        {
            foreach (RadioButton rb in radioButtons)
            {
                rb.Enabled = trueFalse;
            }
        }

        public void RadioButonChecked(List<RadioButton> radioButtons, bool trueFalse)
        {
            foreach (RadioButton rb in radioButtons)
            {
                rb.Checked = trueFalse;
            }
        }

        public void Magnifying(PictureBox pictureBoxOriginal, PictureBox pictureBoxZoom, int coordrectMouseX, int coordrectMouseY)
        {
            if (pictureBoxOriginal.Image == null)
            {
                return;
            }
            else
            {
                Bitmap tempImage = new Bitmap(160, 160, PixelFormat.Format24bppRgb);
                Graphics bitmapGraphics = Graphics.FromImage(tempImage);
                bitmapGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bitmapGraphics.DrawImage(pictureBoxOriginal.Image,
                        new Rectangle(0, 0, 160, 160),
                        new Rectangle(coordrectMouseX - 40, coordrectMouseY - 40,80, 80), 
                        GraphicsUnit.Pixel);
                pictureBoxZoom.Image = tempImage;
            }
        }
    }
}
