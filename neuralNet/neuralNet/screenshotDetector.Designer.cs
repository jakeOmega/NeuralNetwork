namespace neuralNet
{
    partial class screenshotDetector
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.urlBox = new System.Windows.Forms.TextBox();
            this.runButton = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.imageBox = new System.Windows.Forms.PictureBox();
            this.outputBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // urlBox
            // 
            this.urlBox.Location = new System.Drawing.Point(110, 14);
            this.urlBox.Name = "urlBox";
            this.urlBox.Size = new System.Drawing.Size(593, 20);
            this.urlBox.TabIndex = 0;
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(720, 12);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 23);
            this.runButton.TabIndex = 1;
            this.runButton.Text = "Load URL";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(44, 17);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(60, 13);
            this.textBox2.TabIndex = 2;
            this.textBox2.Text = "Image URL";
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(44, 40);
            this.imageBox.MinimumSize = new System.Drawing.Size(800, 450);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(800, 450);
            this.imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageBox.TabIndex = 3;
            this.imageBox.TabStop = false;
            // 
            // outputBox
            // 
            this.outputBox.Location = new System.Drawing.Point(44, 528);
            this.outputBox.Name = "outputBox";
            this.outputBox.Size = new System.Drawing.Size(206, 20);
            this.outputBox.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 594);
            this.Controls.Add(this.outputBox);
            this.Controls.Add(this.imageBox);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.urlBox);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox urlBox;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.PictureBox imageBox;
        private System.Windows.Forms.TextBox outputBox;
    }
}