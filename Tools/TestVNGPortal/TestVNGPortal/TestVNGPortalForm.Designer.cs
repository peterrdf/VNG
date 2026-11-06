namespace TestVNGPortal
{
    partial class TestVNGPortalForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _buttonRun = new Button();
            groupBox1 = new GroupBox();
            _textBoxLog = new TextBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // _buttonRun
            // 
            _buttonRun.Location = new Point(620, 402);
            _buttonRun.Name = "_buttonRun";
            _buttonRun.Size = new Size(75, 23);
            _buttonRun.TabIndex = 0;
            _buttonRun.Text = "Run";
            _buttonRun.UseVisualStyleBackColor = true;
            _buttonRun.Click += _buttonRun_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(_textBoxLog);
            groupBox1.Location = new Point(12, 127);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(681, 268);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Log";
            // 
            // _textBoxLog
            // 
            _textBoxLog.Location = new Point(6, 19);
            _textBoxLog.Multiline = true;
            _textBoxLog.Name = "_textBoxLog";
            _textBoxLog.ReadOnly = true;
            _textBoxLog.ScrollBars = ScrollBars.Both;
            _textBoxLog.Size = new Size(669, 237);
            _textBoxLog.TabIndex = 0;
            // 
            // TestVNGProtalForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(705, 428);
            Controls.Add(groupBox1);
            Controls.Add(_buttonRun);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TestVNGProtalForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Test VNG Portal Tool";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button _buttonRun;
        private GroupBox groupBox1;
        private TextBox _textBoxLog;
    }
}
