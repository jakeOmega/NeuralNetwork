namespace neuralNet
{
    partial class selectorForm
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
            this.trainButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // trainButton
            // 
            this.trainButton.Location = new System.Drawing.Point(12, 12);
            this.trainButton.Name = "trainButton";
            this.trainButton.Size = new System.Drawing.Size(125, 237);
            this.trainButton.TabIndex = 0;
            this.trainButton.Text = "Train Neural Net";
            this.trainButton.UseVisualStyleBackColor = true;
            this.trainButton.Click += new System.EventHandler(this.trainButton_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(145, 12);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(125, 237);
            this.runButton.TabIndex = 1;
            this.runButton.Text = "Run Neural Net";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // selectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.trainButton);
            this.Name = "selectorForm";
            this.Text = "selectorForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button trainButton;
        private System.Windows.Forms.Button runButton;
    }
}