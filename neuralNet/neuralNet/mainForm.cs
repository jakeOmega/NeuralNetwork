using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using System.Threading;
using System.Drawing.Drawing2D;

namespace neuralNet
{
    public partial class mainForm : Form
    {
        public System.Windows.Forms.Timer t;
        delegate void AddPointCallback(int index, double error, int series);
        int pictureWidth = 160;
        int pictureHeight = 90;

        ///<summary>
        ///Takes a file location for an image and translates it into a vector of 
        ///length 4*height*width (in pixels) that can be input into a neural network
        ///</summary>
        public Vector<double> fileToInput(string file)
        {
            //Resize image to have appropriate number of inputs for neural net
            Bitmap bmp = ResizeImage(Image.FromFile(file), pictureWidth, pictureHeight);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bmp.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height + 1;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);
            List<double> imList = new List<double>();
            for (int j = 1; j < rgbValues.Count(); j++)
            {
                imList.Add(rgbValues[j] / 255.0); //rescale for use in neural net
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
                //Want a high quality resize
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

        ///<summary>
        ///For a given neural network with one output and a list of files that should be given
        ///either an output of 1 (goodFiles) or -1 (badFiles)
        ///</summary>
        private double trainingError(net netIn, List<string> goodFiles, List<string> badFiles)
        {
            double error = 0;
            int right = 0;
            int uncertain = 0;
            int wrong = 0;
            for (int i = 0; i < goodFiles.Count; i++)
            {
                Vector<double> input = fileToInput(goodFiles[i]);
                Vector<double> output = Vector<double>.Build.Dense(1);
                output[0] = 1;
                double result = netIn.forward(input)[0];
                if (result > 0.9)
                {
                    right += 1;
                }
                else if (result > 0.1)
                {
                    uncertain += 1;
                    System.Diagnostics.Debug.WriteLine(result.ToString() + ", " + goodFiles[i]);
                }
                else
                {
                    wrong += 1;
                    System.Diagnostics.Debug.WriteLine(result.ToString() + ", " + goodFiles[i]);
                }
                error += netIn.error(input, output);
            }
            for (int i = 0; i < badFiles.Count; i++)
            {
                Vector<double> input = fileToInput(badFiles[i]);
                Vector<double> output = Vector<double>.Build.Dense(1);
                output[0] = 0;
                double result = netIn.forward(input)[0];
                if (result < 0.1)
                {
                    right += 1;
                }
                else if (result < 0.9)
                {
                    uncertain += 1;
                    System.Diagnostics.Debug.WriteLine(result.ToString() + ", " + badFiles[i]);
                }
                else
                {
                    wrong += 1;
                    System.Diagnostics.Debug.WriteLine(result.ToString() + ", " + badFiles[i]);
                }
                error += netIn.error(input, output);
            }
            System.Diagnostics.Debug.WriteLine(right.ToString() + ", " + uncertain.ToString() + ", " + wrong.ToString());
            return error / (goodFiles.Count + badFiles.Count);
        }

        public void pictureNetTrain()
        {
            int testCountPerCat = 30;
            Random rnd = new Random();
            List<string> goodFilesIn = System.IO.Directory.GetFiles("D:\\Libraries\\Desktop\\images\\good").ToList();
            List<string> goodFiles = new List<string>(goodFilesIn.OrderBy(x => rnd.Next()).ToArray());
            int goodFileCount = goodFiles.Count() - testCountPerCat;
            List<string> goodFilesTrain = goodFiles.GetRange(0, goodFileCount);
            List<string> goodFilesTest = goodFiles.GetRange(goodFileCount, testCountPerCat);
            List<string> badFilesIn = System.IO.Directory.GetFiles("D:\\Libraries\\Desktop\\images\\bad").ToList();
            List<string> badFiles = new List<string>(badFilesIn.OrderBy(x => rnd.Next()).ToArray());
            int badFileCount = badFiles.Count() - testCountPerCat;
            List<string> badFilesTrain = badFiles.GetRange(0, badFileCount);
            List<string> badFilesTest = badFiles.GetRange(badFileCount, testCountPerCat);
            List<Vector<double>> inputs = new List<Vector<double>>();
            List<Vector<double>> outputs = new List<Vector<double>>();
            for (int j = 0; j < goodFileCount + badFileCount; j++)
            {
                string file;
                Vector<double> output = Vector<double>.Build.Dense(1);
                if (j >= goodFileCount)
                {
                    file = badFiles[j - goodFileCount];
                    output[0] = 0;
                }
                else
                {
                    file = goodFiles[j];
                    output[0] = 1;
                }

                Vector<double> input = fileToInput(file);
                inputs.Add(input);
                outputs.Add(output);
            }

            int[] intRange = Enumerable.Range(0, goodFileCount + badFileCount).ToArray();

            //net net = new net("D:\\Libraries\\Documents\\tempTest\\net4.net");
            net net = new net(4*pictureWidth*pictureHeight, 1, 2, 30);


            double priorError = 1000;
            int trainNum = 0;
            while (true)
            {
                int[] imageID = intRange.OrderBy(x => rnd.Next()).ToArray();
                List<Vector<double>> inputsForRound = new List<Vector<double>>();
                List<Vector<double>> outputsForRound = new List<Vector<double>>();
                for (int j = 0; j < 10; j++)
                {
                    inputsForRound.Add(inputs[imageID[j]]);
                    outputsForRound.Add(outputs[imageID[j]]);
                }
                net.train(inputsForRound, outputsForRound, 1);
                if (trainNum % 100 == 0)
                {
                    double error = trainingError(net, goodFilesTest, badFilesTest);
                    addPoint(trainNum, error, 1);
                    addPoint(trainNum, net.error(inputsForRound, outputsForRound), 0);
                    if ( error < priorError )
                    {
                        priorError = error;
                        net.serialize("D:\\Libraries\\Documents\\tempTest\\net4.net");
                    }
                    System.Diagnostics.Debug.WriteLine(error);
                }
                trainNum++;
            }
        }

        private void addPoint(int index, double error, int series)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.errorChart.InvokeRequired)
            {
                AddPointCallback d = new AddPointCallback(addPoint);
                this.Invoke(d, new object[] { index, error, series });
            }
            else
            {
                errorChart.Series[series].Points.AddXY(index, error);
            }
        }

        public void updateChart(Object o, EventArgs e)
        {
            errorChart.Update();
        }

        public mainForm()
        {
            InitializeComponent();
            t = new System.Windows.Forms.Timer();
            t.Enabled = true;
            t.Interval = 100;
            t.Tick += updateChart;
            Thread thread = new Thread(pictureNetTrain);
            thread.Start();
        }
        
    }
}
