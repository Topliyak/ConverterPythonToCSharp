using System;
using System.IO;
using Converter;

namespace ConverterLogUI
{
	class Program
	{
		static void Main(string[] args)
		{
            string emptyProjectPath = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName + "\\SourceObjects";
            
            string newProjectPath = "C:\\Users\\amirm\\Desktop";
			try
            {
                CopyDirectory(emptyProjectPath, newProjectPath);
            }
			catch { }

            string pythonFilePath = "C:\\Users\\amirm\\Projects\\python\\tasks\\a.py";
            string cSharpFilePath = newProjectPath + "\\EmptyConsoleCSharpProject\\EmptyConsoleCSharpProject\\Program.cs";
            var entrancePythonFile = new Converter.Converter(pythonFilePath, cSharpFilePath);
        }

        private static void CopyDirectory(string strSource, string strDestination = "C:\\Users\\amirm\\Desktop")
        {
            if (!Directory.Exists(strDestination))
            {
                Directory.CreateDirectory(strDestination);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(strSource);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo tempfile in files)
            {
                tempfile.CopyTo(Path.Combine(strDestination, tempfile.Name));
            }

            DirectoryInfo[] directories = dirInfo.GetDirectories();
            foreach (DirectoryInfo tempdir in directories)
            {
                CopyDirectory(Path.Combine(strSource, tempdir.Name), Path.Combine(strDestination, tempdir.Name));
            }

        }
    }
}
