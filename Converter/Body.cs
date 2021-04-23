using System;
using System.Collections.Generic;
using System.Text;

namespace Converter
{
	public class Body
	{
		public List<string> variables = new List<string>();
		private bool hasOwnVariables = true;
		public int howManySpaces;
		private string code = string.Empty;

		public Body(int howManySpaces)
		{
			this.howManySpaces = howManySpaces;
		}

		public Body(int howManySpaces, List<string> variables)
		{
			this.howManySpaces = howManySpaces;
			this.variables = variables;
			hasOwnVariables = false;
		}

		public string GetBodyText(int howManySpacesInOutsideBody = 0, int additionalSpaces = 0)
		{
			string spacesForCodeLine = string.Empty;

			for (int i = 0; i < howManySpaces - howManySpacesInOutsideBody + additionalSpaces; i++)
			{
				spacesForCodeLine += ' ';
			}

			List<string> _resultWithWhiteSpaces = new List<string>(this.code.Split('\n'));

			while (_resultWithWhiteSpaces[0].Trim() == string.Empty)
			{
				_resultWithWhiteSpaces.RemoveAt(0);
			}

			while (_resultWithWhiteSpaces[_resultWithWhiteSpaces.Count - 1].Trim() == string.Empty)
			{
				_resultWithWhiteSpaces.RemoveAt(_resultWithWhiteSpaces.Count - 1);
			}

			string result = string.Empty;

			if (hasOwnVariables && variables.Count != 0)
			{
				foreach(string variable in variables)
				{
					result += spacesForCodeLine + $"dynamic {variable};\n";
				}

				result += "\n";
			}

			foreach (string codeLine in _resultWithWhiteSpaces)
			{
				result += spacesForCodeLine + codeLine + '\n';
			}

			result = "{\n" + result + "}\n\n";

			return result;
		}

		public void AddCode(string newCode)
		{
			code += newCode;
		}
	}
}
