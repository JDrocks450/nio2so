namespace nio2so.CrashHandler
{
    partial class ErrorWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorWindow));
            label1 = new Label();
            ErrorMessageBox = new TextBox();
            label2 = new Label();
            StackTraceText = new TextBox();
            label3 = new Label();
            ButtonsTray = new FlowLayoutPanel();
            button1 = new Button();
            ErrorTime = new Label();
            ButtonsTray.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(319, 15);
            label1.TabIndex = 0;
            label1.Text = "The program has run into a problem. The details are below.";
            // 
            // ErrorMessageBox
            // 
            ErrorMessageBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ErrorMessageBox.Location = new Point(12, 54);
            ErrorMessageBox.Name = "ErrorMessageBox";
            ErrorMessageBox.PlaceholderText = "Error Details";
            ErrorMessageBox.ReadOnly = true;
            ErrorMessageBox.Size = new Size(510, 23);
            ErrorMessageBox.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 36);
            label2.Name = "label2";
            label2.Size = new Size(81, 15);
            label2.TabIndex = 2;
            label2.Text = "Error Message";
            // 
            // StackTraceText
            // 
            StackTraceText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            StackTraceText.Location = new Point(16, 109);
            StackTraceText.Multiline = true;
            StackTraceText.Name = "StackTraceText";
            StackTraceText.PlaceholderText = "Error Stacktrace";
            StackTraceText.ReadOnly = true;
            StackTraceText.Size = new Size(506, 295);
            StackTraceText.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(16, 91);
            label3.Name = "label3";
            label3.Size = new Size(64, 15);
            label3.TabIndex = 4;
            label3.Text = "Full Details";
            // 
            // ButtonsTray
            // 
            ButtonsTray.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ButtonsTray.Controls.Add(button1);
            ButtonsTray.Location = new Point(16, 417);
            ButtonsTray.Name = "ButtonsTray";
            ButtonsTray.Size = new Size(506, 32);
            ButtonsTray.TabIndex = 5;
            // 
            // button1
            // 
            button1.Location = new Point(3, 3);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // ErrorTime
            // 
            ErrorTime.AutoSize = true;
            ErrorTime.Location = new Point(131, 36);
            ErrorTime.Name = "ErrorTime";
            ErrorTime.Size = new Size(34, 15);
            ErrorTime.TabIndex = 6;
            ErrorTime.Text = "TIME";
            // 
            // ErrorWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(534, 461);
            Controls.Add(ErrorTime);
            Controls.Add(ButtonsTray);
            Controls.Add(label3);
            Controls.Add(StackTraceText);
            Controls.Add(label2);
            Controls.Add(ErrorMessageBox);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(350, 300);
            Name = "ErrorWindow";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Error Window";
            ButtonsTray.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox ErrorMessageBox;
        private Label label2;
        private TextBox StackTraceText;
        private Label label3;
        private FlowLayoutPanel ButtonsTray;
        private Button button1;
        private Label ErrorTime;
    }
}
