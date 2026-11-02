namespace GenerateTestData
{
    partial class GenerateTestDataForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._buttonSelectIFCModel = new System.Windows.Forms.Button();
            this._textBoxIFCModel = new System.Windows.Forms.TextBox();
            this._buttonRun = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this._textBoxBBoxSize = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this._textBoxLabel = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._textBoxLog = new System.Windows.Forms.TextBox();
            this._buttonSelectLocationFile = new System.Windows.Forms.Button();
            this._textBoxLocationsFile = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._buttonSelectIFCModel);
            this.groupBox1.Controls.Add(this._textBoxIFCModel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(545, 50);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IFC Model";
            // 
            // _buttonSelectIFCModel
            // 
            this._buttonSelectIFCModel.Location = new System.Drawing.Point(501, 17);
            this._buttonSelectIFCModel.Name = "_buttonSelectIFCModel";
            this._buttonSelectIFCModel.Size = new System.Drawing.Size(37, 23);
            this._buttonSelectIFCModel.TabIndex = 1;
            this._buttonSelectIFCModel.Text = "...";
            this._buttonSelectIFCModel.UseVisualStyleBackColor = true;
            this._buttonSelectIFCModel.Click += new System.EventHandler(this._buttonSelectIFCModel_Click);
            // 
            // _textBoxIFCModel
            // 
            this._textBoxIFCModel.Location = new System.Drawing.Point(8, 18);
            this._textBoxIFCModel.Name = "_textBoxIFCModel";
            this._textBoxIFCModel.ReadOnly = true;
            this._textBoxIFCModel.Size = new System.Drawing.Size(486, 20);
            this._textBoxIFCModel.TabIndex = 0;
            // 
            // _buttonRun
            // 
            this._buttonRun.Location = new System.Drawing.Point(482, 395);
            this._buttonRun.Name = "_buttonRun";
            this._buttonRun.Size = new System.Drawing.Size(75, 23);
            this._buttonRun.TabIndex = 2;
            this._buttonRun.Text = "Run";
            this._buttonRun.UseVisualStyleBackColor = true;
            this._buttonRun.Click += new System.EventHandler(this._buttonRun_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._buttonSelectLocationFile);
            this.groupBox2.Controls.Add(this._textBoxLocationsFile);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this._textBoxLabel);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this._textBoxBBoxSize);
            this.groupBox2.Location = new System.Drawing.Point(12, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(545, 81);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Input";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "BBox Size:";
            // 
            // _textBoxBBoxSize
            // 
            this._textBoxBBoxSize.Location = new System.Drawing.Point(67, 48);
            this._textBoxBBoxSize.Name = "_textBoxBBoxSize";
            this._textBoxBBoxSize.Size = new System.Drawing.Size(100, 20);
            this._textBoxBBoxSize.TabIndex = 4;
            this._textBoxBBoxSize.Text = "400";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(182, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(145, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Randomly selected locations:";
            // 
            // _textBoxLabel
            // 
            this._textBoxLabel.Location = new System.Drawing.Point(329, 48);
            this._textBoxLabel.Name = "_textBoxLabel";
            this._textBoxLabel.Size = new System.Drawing.Size(163, 20);
            this._textBoxLabel.TabIndex = 6;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._textBoxLog);
            this.groupBox3.Location = new System.Drawing.Point(12, 170);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(545, 218);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Log";
            // 
            // _textBoxLog
            // 
            this._textBoxLog.Location = new System.Drawing.Point(9, 17);
            this._textBoxLog.Multiline = true;
            this._textBoxLog.Name = "_textBoxLog";
            this._textBoxLog.ReadOnly = true;
            this._textBoxLog.Size = new System.Drawing.Size(529, 190);
            this._textBoxLog.TabIndex = 0;
            // 
            // _buttonSelectLocationFile
            // 
            this._buttonSelectLocationFile.Location = new System.Drawing.Point(499, 18);
            this._buttonSelectLocationFile.Name = "_buttonSelectLocationFile";
            this._buttonSelectLocationFile.Size = new System.Drawing.Size(37, 23);
            this._buttonSelectLocationFile.TabIndex = 9;
            this._buttonSelectLocationFile.Text = "...";
            this._buttonSelectLocationFile.UseVisualStyleBackColor = true;
            this._buttonSelectLocationFile.Click += new System.EventHandler(this._buttonSelectLocationFile_Click);
            // 
            // _textBoxLocationsFile
            // 
            this._textBoxLocationsFile.Location = new System.Drawing.Point(6, 19);
            this._textBoxLocationsFile.Name = "_textBoxLocationsFile";
            this._textBoxLocationsFile.ReadOnly = true;
            this._textBoxLocationsFile.Size = new System.Drawing.Size(486, 20);
            this._textBoxLocationsFile.TabIndex = 8;
            // 
            // GenerateTestDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 424);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._buttonRun);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateTestDataForm";
            this.Text = "Generate Test Data";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button _buttonSelectIFCModel;
        private System.Windows.Forms.TextBox _textBoxIFCModel;
        private System.Windows.Forms.Button _buttonRun;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _textBoxBBoxSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _textBoxLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox _textBoxLog;
        private System.Windows.Forms.Button _buttonSelectLocationFile;
        private System.Windows.Forms.TextBox _textBoxLocationsFile;
    }
}

