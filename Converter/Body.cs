using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Converter
{
	public class Body
	{
		public string name = string.Empty;
		public string header = string.Empty;
		public ConstructionType constructionType;
		public Body parentBody;
		public List<string> variables = new List<string>();
		private bool hasOwnVariables = true;
		public int howManySpaces;
		private string code = string.Empty;
		private int additionalSpaces = 0;

		public Body(string name, string header, ConstructionType constructionType, int howManySpaces, int additionalSpaces = 0)
		{
			this.name = name;
			this.header = header;
			this.howManySpaces = howManySpaces;
			this.constructionType = constructionType;
			this.additionalSpaces = additionalSpaces;
		}

		public Body(string name, string header, ConstructionType constructionType, int howManySpaces, List<string> variables): this(name, header, constructionType, howManySpaces)
		{
			this.variables = variables;
			hasOwnVariables = false;
		}

		public Body(string name, string header, 
			ConstructionType constructionType, 
			int howManySpaces, List<string> variables, 
			int additionalSpaces) : this(name, header, constructionType, howManySpaces, additionalSpaces)
		{
			this.variables = variables;
			hasOwnVariables = false;
		}

		public void ImportCodeToParent()
		{
			if (parentBody == null)
			{
				return;
			}

			parentBody.AddCode(GetBodyText(parentBody.howManySpaces, additionalSpaces));
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

			PutVariablesToCode(ref result, spacesForCodeLine);

			foreach (string codeLine in _resultWithWhiteSpaces)
			{
				result += spacesForCodeLine + codeLine + '\n';
			}

			if (constructionType == ConstructionType.Function && Converter.FindIndexOfActStartIgnoringBrackets(header, "dynamic").Length != 0)
			{
				string[] _resultAsLinesList = result.Split('\n');
				
				if (_resultAsLinesList.Length >= 2 && 
					Converter.FindIndexOfActStartIgnoringBrackets(_resultAsLinesList[_resultAsLinesList.Length - 2], "return").Length == 0)
				{
					result += "\n" + spacesForCodeLine + "return null;" + "\n";
				}
			}

			result = "{\n" + result + "}\n\n";

			if (header != null)
			{
				header = header.TrimEnd(';', '\n');
				result = header + "\n" + result;
			}

			return result;
		}

		public void AddCode(string newCode)
		{
			code += newCode;
		}

		private void PutVariablesToCode(ref string code, string spaces)
		{
			if (hasOwnVariables == false)
			{
				return;
			}

			DeleteSameVariables();
			ProcessVariablesWithThis();

			foreach (string variable in variables)
			{
				code += spaces + $"dynamic {variable};\n";
			}

			if (variables.Count > 0)
			{
				code += "\n";
			}
		}

		private void DeleteSameVariables()
		{
			if (variables.Count < 2)
			{
				return;
			}

			List<int> indexesWhichMustBeDeleted = new List<int>();

			for (int i = 1; i < variables.Count; i++)
			{
				for (int j = 0; j < i; j++)
				{
					if (variables[i] == variables[j] && indexesWhichMustBeDeleted.Contains(j) == false)
					{
						indexesWhichMustBeDeleted.Add(j);
					}
				}
			}

			for (int i = indexesWhichMustBeDeleted.Count - 1; i >= 0; i--)
			{
				int index = indexesWhichMustBeDeleted[i];
				variables.RemoveAt(index);
			}
		}

		private void ProcessVariablesWithThis()
		{
			for (int i = variables.Count - 1; i >= 0; i--)
			{
				if (IsLocalVariable(variables[i]))
				{
					continue;
				}

				if (constructionType != ConstructionType.Class)
				{
					parentBody.variables.Add(variables[i]);
					variables.RemoveAt(i);
				}
				else
				{
					string variableWithoutThis = GetVariableWithoutThis(variables[i]);
					variables.Add(variableWithoutThis);
					variables.RemoveAt(i);
				}
			}
		}

		private static bool IsLocalVariable(in string variable)
		{
			string[] varParts = variable.Split('.');

			if (varParts[0] == "this")
			{
				return false;
			}

			return true;
		}

		private static string GetVariableWithoutThis(string variable)
		{
			string[] varParts = variable.Split('.');

			string result = string.Empty;

			for (int i = 1; i < varParts.Length; i++)
				result += varParts[i];

			return result;
		}
	}
}
