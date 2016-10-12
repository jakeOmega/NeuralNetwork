using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace neuralNet
{
    /// <summary>
    /// Form allowing user to select between training the neural net and runing it on images from the internet
    /// </summary>
    public partial class selectorForm : Form
    {
        public selectorForm()
        {
            InitializeComponent();
        }

        private void trainButton_Click(object sender, EventArgs e)
        {
            mainForm form = new mainForm();
            form.Show();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            screenshotDetector detector = new screenshotDetector();
            detector.Show();
        }
    }
}
