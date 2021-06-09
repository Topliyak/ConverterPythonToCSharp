namespace ConverterApplication
{
	partial class MainMenu
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
			this.PythonModuleField = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.CSharpProjectPathField = new System.Windows.Forms.TextBox();
			this.ConvertButton = new System.Windows.Forms.Button();
			this.fileBrowserDialog = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.MessageAboutConvertingLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// PythonModuleField
			// 
			this.PythonModuleField.AccessibleName = "BoxForEntrancePythonModulePath";
			this.PythonModuleField.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.PythonModuleField.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PythonModuleField.Location = new System.Drawing.Point(121, 151);
			this.PythonModuleField.Name = "PythonModuleField";
			this.PythonModuleField.Size = new System.Drawing.Size(424, 33);
			this.PythonModuleField.TabIndex = 0;
			this.PythonModuleField.Text = "Path to entrance python module";
			this.PythonModuleField.TextChanged += new System.EventHandler(this.PythonModulePath_Changed);
			// 
			// button1
			// 
			this.button1.Font = new System.Drawing.Font("Microsoft YaHei UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.button1.Location = new System.Drawing.Point(566, 151);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(36, 33);
			this.button1.TabIndex = 1;
			this.button1.Text = "...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.ChooseDirectoryButton1_Click);
			// 
			// button2
			// 
			this.button2.Font = new System.Drawing.Font("Microsoft YaHei UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.button2.Location = new System.Drawing.Point(566, 208);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(36, 33);
			this.button2.TabIndex = 3;
			this.button2.Text = "...";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.ChooseDirectoryButton2_Click);
			// 
			// CSharpProjectPathField
			// 
			this.CSharpProjectPathField.AccessibleName = "BoxForCSharpProjectPath";
			this.CSharpProjectPathField.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.CSharpProjectPathField.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.CSharpProjectPathField.Location = new System.Drawing.Point(121, 208);
			this.CSharpProjectPathField.Name = "CSharpProjectPathField";
			this.CSharpProjectPathField.Size = new System.Drawing.Size(424, 33);
			this.CSharpProjectPathField.TabIndex = 2;
			this.CSharpProjectPathField.Text = "Path where C# project must be saved";
			this.CSharpProjectPathField.TextChanged += new System.EventHandler(this.CSharpProjectPath_Changed);
			// 
			// ConvertButton
			// 
			this.ConvertButton.AccessibleName = "ConvertButton";
			this.ConvertButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ConvertButton.Location = new System.Drawing.Point(277, 307);
			this.ConvertButton.Name = "ConvertButton";
			this.ConvertButton.Size = new System.Drawing.Size(198, 67);
			this.ConvertButton.TabIndex = 4;
			this.ConvertButton.Text = "Convert";
			this.ConvertButton.UseVisualStyleBackColor = true;
			this.ConvertButton.Click += new System.EventHandler(this.ConverterButton_Clicked);
			// 
			// fileBrowserDialog
			// 
			this.fileBrowserDialog.FileName = "openFileDialog1";
			// 
			// MessageAboutConvertingLabel
			// 
			this.MessageAboutConvertingLabel.AutoSize = true;
			this.MessageAboutConvertingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.MessageAboutConvertingLabel.Location = new System.Drawing.Point(116, 53);
			this.MessageAboutConvertingLabel.Name = "MessageAboutConvertingLabel";
			this.MessageAboutConvertingLabel.Size = new System.Drawing.Size(237, 29);
			this.MessageAboutConvertingLabel.TabIndex = 5;
			this.MessageAboutConvertingLabel.Text = "Convert Python to C#";
			// 
			// MainMenu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.MessageAboutConvertingLabel);
			this.Controls.Add(this.ConvertButton);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.CSharpProjectPathField);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.PythonModuleField);
			this.Name = "MainMenu";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox PythonModuleField;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox CSharpProjectPathField;
		private System.Windows.Forms.Button ConvertButton;
		private System.Windows.Forms.OpenFileDialog fileBrowserDialog;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label MessageAboutConvertingLabel;
	}
}

