using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using System.Drawing.Drawing2D;
using System.Net;

namespace neuralNet
{
    public partial class screenshotDetector : Form
    {
        int width = 160;
        int height = 90;
        net eu4detector;
        Vector<double> lastInput;

        public screenshotDetector()
        {
            InitializeComponent();
            System.Diagnostics.Debug.Write(File.Exists("../../../../eu4ScreenshotDetector.net"));
            eu4detector = new net("../../../../eu4ScreenshotDetector.net");
        }

        public Vector<double> bitmapToInput(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bmp.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height + 1;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes); bmp.UnlockBits(bmpData);
            List<double> imList = new List<double>();
            for (int j = 1; j < rgbValues.Count(); j++)
            {
                imList.Add(rgbValues[j] / 255.0);
            }
            Vector<double> input = Vector<double>.Build.DenseOfEnumerable(imList);
            return input;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            Bitmap bitmap;

            var request = WebRequest.Create(urlBox.Text);

            urlBox.Text = "";

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                Image im = Bitmap.FromStream(stream);
                bitmap = ResizeImage(im, width, height);
            }
            imageBox.Image = bitmap;

            Vector<double> input = bitmapToInput(bitmap);
            lastInput = input;
            double output = eu4detector.forward(input)[0];
            if (output > 0.667)
            {
                outputBox.Text = "EU4 screenshot!";
                outputBox.BackColor = Color.LightGreen;
            }
            else if (output > 0.333)
            {
                outputBox.Text = "Uncertain...";
                outputBox.BackColor = Color.LightGray;
            }
            else
            {
                outputBox.Text = "Not EU4!";
                outputBox.BackColor = Color.Pink;
            }

        }
    }
}
