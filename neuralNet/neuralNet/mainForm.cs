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
        ///either an output of 1 (goodFiles) or 0 (badFiles), and 
        ///</summary>
        private double trainingError(net netIn, List<string> goodFiles, List<string> badFiles)
        {
            //count how many the net get right, wrong, or uncertain
            //print the wrong and uncertain ones
            double error = 0;
            int right = 0;
            int uncertain = 0;
            int wrong = 0;
            //All the files that should output 1
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
            //all the files that should output 0
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

        ///<summary>
        ///Train a neural network on a bunch of images to distinguish between two categories of images
        ///(currently EU4 screenshots versus everything else, as that's what I had images of available)
        ///with a random selection of images as testing images. The network doesn't train with these, and so
        ///they provide a good measure of the accuracy of the network
        ///</summary>
        public void pictureNetTrain()
        {
            int testCountPerCat = 30;
            Random rnd = new Random();
            //Grab all the good (output = 1) and bad (output = 0) images and put them in a list in random order
            List<string> goodFilesIn = System.IO.Directory.GetFiles("D:\\Libraries\\Desktop\\images\\good").ToList();
            List<string> goodFiles = new List<string>(goodFilesIn.OrderBy(x => rnd.Next()).ToArray());
            //The last testCountPerCat images in the goodFiles list will be used as test images; not trained against
            int goodFileCount = goodFiles.Count() - testCountPerCat;
            List<string> goodFilesTrain = goodFiles.GetRange(0, goodFileCount);
            List<string> goodFilesTest = goodFiles.GetRange(goodFileCount, testCountPerCat);
            List<string> badFilesIn = System.IO.Directory.GetFiles("D:\\Libraries\\Desktop\\images\\bad").ToList();
            List<string> badFiles = new List<string>(badFilesIn.OrderBy(x => rnd.Next()).ToArray());
            //same idea as for good files
            int badFileCount = badFiles.Count() - testCountPerCat;
            List<string> badFilesTrain = badFiles.GetRange(0, badFileCount);
            List<string> badFilesTest = badFiles.GetRange(badFileCount, testCountPerCat);
            //Make a list of the inputs and outputs, excluding the testing images
            List<Vector<double>> inputs = new List<Vector<double>>();
            List<Vector<double>> outputs = new List<Vector<double>>();
            for (int j = 0; j < goodFileCount; j++) //only goes up to goodFileCount - therefore excludes test images
            {
                string file;
                Vector<double> output = Vector<double>.Build.Dense(1);
                file = goodFiles[j];
                output[0] = 1;

                Vector<double> input = fileToInput(file);
                inputs.Add(input);
                outputs.Add(output);
            }

            for (int j = 0; j < badFileCount; j++)
            {
                string file;
                Vector<double> output = Vector<double>.Build.Dense(1);
                file = badFiles[j];
                output[0] = 0;

                Vector<double> input = fileToInput(file);
                inputs.Add(input);
                outputs.Add(output);
            }

            //a list of locations in the input/output lists, will be randomized to draw from to train
            int[] intRange = Enumerable.Range(0, goodFileCount + badFileCount).ToArray();

            //Load in an old net or create a new one
            net net = new net("D:\\Libraries\\Documents\\tempTest\\net4.net");
            //net net = new net(4*pictureWidth*pictureHeight, 1, 2, 30);

            //We'll only save the net to file when it's error on the test images decreases
            double priorError = 1;
            int trainNum = 0;
            //Until the user closes the training thing
            while (true)
            {
                //randomly grab 10 inputs and their corresponding outputs
                int[] imageID = intRange.OrderBy(x => rnd.Next()).ToArray();
                List<Vector<double>> inputsForRound = new List<Vector<double>>();
                List<Vector<double>> outputsForRound = new List<Vector<double>>();
                for (int j = 0; j < 10; j++)
                {
                    inputsForRound.Add(inputs[imageID[j]]);
                    outputsForRound.Add(outputs[imageID[j]]);
                }
                //train the net with the selected inputs and outputs
                net.train(inputsForRound, outputsForRound, 1);
                //every once in a while, check how we're doing, record it on the graph, and save if we're improving
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

        ///<summary>
        ///Adds a point to the error chart, supports multithreading so
        ///we can add a point to the chart without lag when the training is 
        ///fully using a CPU core
        ///</summary>
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

        //Update the chart every so often
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
