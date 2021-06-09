using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConverterApplication
{
	public partial class MainMenu : Form
	{
		ConverterLaunchData ConverterLaunchData = new ConverterLaunchData();

		public MainMenu()
		{
			InitializeComponent();
		}

		private void PythonModulePath_Changed(object sender, EventArgs e)
		{
			ConverterLaunchData.EntrancePythonModulePath = PythonModuleField.Text;
		}

		private void CSharpProjectPath_Changed(object sender, EventArgs e)
		{
			ConverterLaunchData.WhereCSharpProjectMustBeSavedPath = CSharpProjectPathField.Text;
		}

		private void ConverterButton_Clicked(object sender, EventArgs e)
		{
			string convertingTryResult = ConverterLaunchData.TryStartConverting();
			MessageAboutConvertingLabel.Text = convertingTryResult;
		}

		private void ChooseDirectoryButton1_Click(object sender, EventArgs e)
		{
			PythonModuleField.Text = ChooseFile();
		}

		private void ChooseDirectoryButton2_Click(object sender, EventArgs e)
		{
			CSharpProjectPathField.Text = ChooseFolder();
		}

		public string ChooseFile()
		{
			if (fileBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				return fileBrowserDialog.FileName;
			}

			return string.Empty;
		}

		public string ChooseFolder()
		{
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				return folderBrowserDialog.SelectedPath;
			}

			return string.Empty;
		}
	}
}
