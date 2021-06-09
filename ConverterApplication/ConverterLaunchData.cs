using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConverterApplication.Properties;

namespace ConverterApplication
{
	class ConverterLaunchData
	{
		public string EntrancePythonModulePath = string.Empty;
		public string WhereCSharpProjectMustBeSavedPath = string.Empty;

		public string TryStartConverting()
		{
			bool canStartConverting = true;

			string exception = string.Empty;

			if (!File.Exists(EntrancePythonModulePath))
			{
				canStartConverting = false;

				exception = "Path to .py module uncorrect";
			}

			if (!Directory.Exists(WhereCSharpProjectMustBeSavedPath))
			{
				canStartConverting = false;

				exception += exception.Length == 0 ? string.Empty : "\n";
				exception += "Path where you want save C# project uncorrect";
			}

			if (!canStartConverting)
			{
				return exception;
			}

			Convert(EntrancePythonModulePath, WhereCSharpProjectMustBeSavedPath);

			return "Success";
		}

		private void Convert(string pythonModulePath, string cSharpProjectPath)
		{
			cSharpProjectPath += @"\Program";
			Directory.CreateDirectory(cSharpProjectPath);

			File.WriteAllText(cSharpProjectPath + @"\Program.csproj", Resources.Program);
			File.WriteAllBytes(cSharpProjectPath + @"\Program.sln", Resources.Program1);
			File.WriteAllText(cSharpProjectPath + @"\Program.cs", Resources.Program2);

			Converter.Converter converter = new Converter.Converter(pythonModulePath, cSharpProjectPath + @"\Program.cs");
		}
	}
}
