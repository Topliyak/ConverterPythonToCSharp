using System;
using System.Collections.Generic;

namespace Converter
{
	public class Converter
	{
		private System.IO.StreamReader pythonFile;
		private System.IO.StreamWriter cSharpFile;

		private string cSharpCode;

		private List<string> variables = new List<string>();

		private delegate string ProcessAct(string left, string right, string act, in List<string> variables);

		private static string[][] prioritisedActs = {
									new string[] { "=", "+=", "-=", "*=", "/=", "//=", "%=" }, 
									new string[] { "or" },
									new string[] { "and" },
									new string[] { "not" },
									new string[] { "<", "<=", ">", ">=", "!=", "==" },
									new string[] { "+", "-" }, 
									new string[] { "*", "/", "//", "%" }, 
									new string[] { "**" },
								  };
		private static ProcessAct[][] ProcessActs = {
												new ProcessAct[] {
																  Assignment, // =
																  ActWithAssignment, // +=
																  ActWithAssignment, // -=
																  ActWithAssignment, // *=
																  ActWithAssignment, // /=
																  ActWithAssignment, // //=
																  ActWithAssignment, // %=
																 },
												new ProcessAct[] {
																  Disjuction, // or
																 },
												new ProcessAct[] {
																  Conjuction, // and
																 },
												new ProcessAct[] {
																  Inversion, // not
																 },
												new ProcessAct[] {
																  JoinPartsByActWhichSameInPythonAndCS, // <
																  JoinPartsByActWhichSameInPythonAndCS, // <=
																  JoinPartsByActWhichSameInPythonAndCS, // >
																  JoinPartsByActWhichSameInPythonAndCS, // >=
																  JoinPartsByActWhichSameInPythonAndCS, // !=
																  JoinPartsByActWhichSameInPythonAndCS, // ==
																 },
												new ProcessAct[] {
																  JoinPartsByActWhichSameInPythonAndCS, // +
																  JoinPartsByActWhichSameInPythonAndCS, // -
																 },
												new ProcessAct[] {
																  JoinPartsByActWhichSameInPythonAndCS, // *
																  JoinPartsByActWhichSameInPythonAndCS, // /
																  Div, // //
																  Mod, // %
																 },
												new ProcessAct[] {
																  Pow, // **
																 },
											};

		private string ProcessLine(in string line)
		{
			if (line == null)
			{
				return "";
			}

			string rightPart = "";
			string leftPart = "";

			for (int actGroupIndex = 0; actGroupIndex < prioritisedActs.Length; actGroupIndex++)
			{
				int indexOfActFromCurrentActsGroupInLine = -1;
				string actFromCurrentGroup = string.Empty;
				int indexOfActInGroup = -1;

				for (int actIndex = 0; actIndex < prioritisedActs[actGroupIndex].Length; actIndex++)
				{
					string act = prioritisedActs[actGroupIndex][actIndex];

					int[] _intArray = FindIndexOfActStartIgnoringBrackets(in line, act);
					int indexOfCurrentAct = (_intArray.Length == 0) ? -1 : _intArray[_intArray.Length - 1];

					if (indexOfCurrentAct == -1)
					{
						continue;
					}

					if (indexOfCurrentAct > indexOfActFromCurrentActsGroupInLine)
					{
						indexOfActFromCurrentActsGroupInLine = indexOfCurrentAct;
						actFromCurrentGroup = act;
						indexOfActInGroup = actIndex;
					}
				}

				if (indexOfActFromCurrentActsGroupInLine == -1) // Line doesn't contain act from current act group
				{
					continue;
				}

				for (int i = 0; i < indexOfActFromCurrentActsGroupInLine; i++)
				{
					if (line[i] == ' ')
					{
						continue;
					}

					leftPart += line[i];
				}

				for (int i = indexOfActFromCurrentActsGroupInLine + actFromCurrentGroup.Length; i < line.Length; i++)
				{
					if (line[i] == ' ')
					{
						continue;
					}

					rightPart += line[i];
				}

				return ProcessActs[actGroupIndex][indexOfActInGroup](ProcessLine(leftPart), ProcessLine(rightPart), 
																				actFromCurrentGroup, in variables);
			}

			string _lineDuplicate = line.Trim();

			if (_lineDuplicate.Length > 0 && _lineDuplicate[0] == '(' && _lineDuplicate[_lineDuplicate.Length - 1] == ')')
			{
				_lineDuplicate = string.Empty;
				List<char> _charList = new List<char>(line.Trim().ToCharArray());

				for (int i = 1; i < _charList.Count - 1; i++)
				{
					_lineDuplicate += _charList[i];
				}

				return "(" + ProcessLine(_lineDuplicate) + ")";
			}

			foreach (char i in line)
			{
				if (i != ' ')
				{
					leftPart += i;
				}
			}

			return leftPart;
		}

		public Converter(string pathToPythonModule, string pathToCSharpFile)
		{
			pythonFile = new System.IO.StreamReader(pathToPythonModule);
			cSharpFile = new System.IO.StreamWriter(pathToCSharpFile);

			string line;

			do
			{
				line = pythonFile.ReadLine();
				cSharpCode += ProcessLine(in line) + "\n";

			} while (line != null);

			Console.Write(cSharpCode);

			pythonFile.Close();
			cSharpFile.Close();
		}

		private void TestMethod() // For Debug methods
		{
			string s = "\\=llpfkqkf kfkfkowqfk ok\\\\=fwqflpqlfplf\\\\=";

			for (int i = 0; i < s.Length; i++)
			{
				Console.WriteLine($"{i}) {s[i]}");
			}

			for (int i = 0; i < s.Length; i++)
			{
				if (IsOverlapEndOfAct(i, in s, "\\\\="))
					Console.Write(i + " ");
			}
		}

		/// <summary>
		/// Find act's first letter index in line 
		/// </summary>
		/// <returns>Returns array of indexes in order like in line</returns>
		private int[] FindIndexOfActStartIgnoringBrackets(in string line, string act)
		{
			List<int> indexesOfActStart = new List<int>();

			int howManyBracketsOpened = 0;
			int howManySkip = 0;

			for (int i = 0; i < line.Length; i++)
			{
				if (howManySkip > 0)
				{
					howManySkip--;
					continue;
				}

				if (line[i] == '(')
				{
					howManyBracketsOpened++;
				}
				else if (line[i] == ')')
				{
					howManyBracketsOpened--;
					continue;
				}

				if (howManyBracketsOpened > 0)
				{
					continue;
				}

				if (IsOverlapAct(i, in line, act))
				{
					bool canAddIndex = true;

					foreach (string[] actGroup in prioritisedActs)
					{
						foreach (string j in actGroup)
						{
							if (j != act && j.Length > act.Length && IsOverlapAct(i, in line, j))
							{
								canAddIndex = false;
							}
						}
					}

					if (canAddIndex)
					{
						howManySkip = act.Length - 1;
						indexesOfActStart.Add(i);
					}
				}
			}

			return indexesOfActStart.ToArray();
		}

		/// <summary>
		/// Check is current index overlap act in line
		/// </summary>
		private static bool IsOverlapAct(int currentIndex, in string line, string compareWithThisAct)
		{
			for (int offsetFromActStart = 0; offsetFromActStart < compareWithThisAct.Length; offsetFromActStart++)
			{
				if (currentIndex - offsetFromActStart < 0)
				{
					continue;
				}

				bool isEqual = true;

				for (int j = 0; j < compareWithThisAct.Length; j++)
				{
					if (currentIndex - offsetFromActStart + j >= line.Length)
					{
						isEqual = false;
						break;
					}

					if (line[currentIndex - offsetFromActStart + j] != compareWithThisAct[j])
					{
						isEqual = false;
						break;
					}
				}

				if (isEqual)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Check is current index overlap end of act in line
		/// </summary>
		private static bool IsOverlapEndOfAct(int currentIndex, in string line, string compareWithThisAct)
		{
			int offsetFromActStart = compareWithThisAct.Length - 1;

			if (currentIndex - offsetFromActStart < 0)
			{
				return false;
			}

			bool isEqual = true;

			for (int j = 0; j < compareWithThisAct.Length; j++)
			{
				if (currentIndex - offsetFromActStart + j >= line.Length)
				{
					isEqual = false;
					break;
				}

				if (line[currentIndex - offsetFromActStart + j] != compareWithThisAct[j])
				{
					isEqual = false;
					break;
				}
			}

			return isEqual;
		}

		private static string ActWithAssignment(string left, string right, string actWithAssignment, in List<string> variables)
		{
			string actWithoutAssignment = string.Empty;

			for (int i = 0; i < actWithAssignment.Length - 1; i++)
			{
				actWithoutAssignment += actWithAssignment[i];
			}

			int groupOfActIndex = -1;
			int actInGroupIndex = -1;

			for (int i = 0; i < prioritisedActs.Length; i++)
			{
				for (int j = 0; j < prioritisedActs[i].Length; j++)
				{
					if (prioritisedActs[i][j] == actWithoutAssignment)
					{
						groupOfActIndex = i;
						actInGroupIndex = j;
						
						break;
					}
				}

				if (groupOfActIndex != -1)
				{
					break;
				}
			}

			return Assignment(left, ProcessActs[groupOfActIndex][actInGroupIndex](left, right, actWithoutAssignment, variables), 
																								actWithoutAssignment, variables);
		}

		private static string Assignment(string left, string right, string _, in List<string> variables)
		{
			if (!variables.Contains(left))
			{
				variables.Add(left);
			}

			return $"{left} = {right}";
		}

		private static string Conjuction(string left, string right, string _, in List<string> __)
		{
			return $"{left} && {right}";
		}

		private static string Disjuction(string left, string right, string _, in List<string> __)
		{
			return $"{left} || {right}";
		}

		private static string Inversion(string _, string right, string __, in List<string> ___)
		{
			return $"!{right}";
		}

		private static string Div(string left, string right, string _a, in List<string> _b)
		{
			return $"PythonMethods.Div({left}, {right})";
		}

		private static string Mod(string left, string right, string _a, in List<string> _b)
		{
			return $"PythonMethods.Mod({left}, {right})";
		}

		private static string Pow(string left, string right, string _a, in List<string> _b)
		{
			return $"Math.Pow({left}, {right})";
		}

		private static string JoinPartsByActWhichSameInPythonAndCS(string left, string right, string act, in List<string> _)
		{
			return $"{left} {act} {right}";
		}

		private static string StringFromCharList(List<char> charList)
		{
			string result = "";

			foreach (char i in charList)
			{
				result += i;
			}

			return result;
		}
	}
}