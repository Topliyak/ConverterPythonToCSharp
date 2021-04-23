using System;
using System.Collections.Generic;

namespace Converter
{
	public class Converter
	{
		private System.IO.StreamReader pythonFile;
		private System.IO.StreamWriter cSharpFile;

		int _lineNumber;

		private int minSpacesAtStartLine = 4;
		private Body currentBody;
		private Stack<Body> bodyStack = new Stack<Body>();

		private enum ConstructionType
		{
			Loop,
			Condition,
			Function,
			Class,
			Default,
		}

		private ConstructionType typeOfLastConstruction;

		private static Dictionary<ConstructionType, List<string>> ConstructionTypesAndTheirKeywords = new Dictionary<ConstructionType, List<string>>
		{
			{ConstructionType.Class, new List<string> { "class" } },
			{ConstructionType.Condition, new List<string> { "if" } },
			{ConstructionType.Function, new List<string> { "def" } },
			{ConstructionType.Loop, new List<string> { "while", "for" } },
		};
		private static Dictionary<string, string> analogsOfPythonKeywordInCSharp = new Dictionary<string, string>
		{
			{"True", "true" },
			{"False", "false" },
			{"None", "null" },
		};

		private delegate string ProcessAct(string left, string right, string act, in List<string> variables);
		private delegate string ProcessKeyword(string left, string right, in List<string> variables);

		private static List<char> whiteSpaces = new List<char>(new char[] { ' ', '\n', '\t' });

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

		private static string[] cSharpKeywordsWithWhichLineMustntBeClosed = { "if", "foreach", "while", };

		private static string[] keywords = { "for", "if", "while", "in", };
		private static ProcessKeyword[] processKeywords = { ProcessFor, ProcessIf, ProcessWhile, ProcessIn, };

		private string ProcessLine(in string pythonLine)
		{
			if (pythonLine == null)
			{
				return "";
			}

			string line = pythonLine.Trim(' ', ':');

			line = ChangePythonKeywordsToCSharpKeywords(line);
			line = ProcessKeywords(line);
			line = ProcessArithmeticAndLogicalActs(line);
			line = ProcessSquareBrackets(line);
			line = ProcessAroundBrackets(line);

			return line.Trim();
		}

		public Converter(string pathToPythonModule, string pathToCSharpFile)
		{
			pythonFile = new System.IO.StreamReader(pathToPythonModule);
			cSharpFile = new System.IO.StreamWriter(pathToCSharpFile);

			bodyStack.Add(new Body(0));

			string line;
			string newLine;
			_lineNumber = 0;

			do
			{
				line = string.Empty;
				bool allBracketsClosed;

				do
				{
					_lineNumber++;
					newLine = pythonFile.ReadLine();
					line = line.TrimEnd('\n') + newLine;
					allBracketsClosed = CheckAllBracketsClosed(line);

				} while (!allBracketsClosed && newLine != null);

				UpdateBodyStack(line);
				typeOfLastConstruction = DefineWhichTypeOfConstructionInLine(line);

				string _convertedLine = ProcessLine(line);

				currentBody = bodyStack.Get();
				currentBody.AddCode(CloseLine(_convertedLine));


			} while (newLine != null);

			while (bodyStack.Count > 1)
			{
				CloseBody();
			}

			currentBody = bodyStack.Get();
			Console.Write(currentBody.GetBodyText(0, minSpacesAtStartLine));

			pythonFile.Close();
			cSharpFile.Close();
		}

		private void TestMethod() // For Debug methods
		{
			string s = "\\=llpfkqkf kfkfkorqfk or ok\\\\=fwqflpqlfplf\\\\=";

			for (int i = 0; i < s.Length; i++)
			{
				Console.WriteLine($"{i}) {s[i]}");
			}

			foreach (var i in FindIndexOfActStartIgnoringBrackets(s, "\\\\="))
			{
				Console.Write(i + " ");
			}
		}

		private static string CloseLine(in string line)
		{
			if (line.Trim() == string.Empty)
			{
				return "\n";
			}

			foreach (string keyword in cSharpKeywordsWithWhichLineMustntBeClosed)
			{
				if (FindIndexOfActStartIgnoringBrackets(line, keyword).Length != 0)
				{
					return line.TrimEnd('\n') + "\n";
				}
			}

			return line.TrimEnd('\n') + ";\n";
		}

		private bool CheckAllBracketsClosed(in string line)
		{
			int aroundBracketsOpened = 0;
			int squareBracketsOpened = 0;
			int figureBracketsOpened = 0;

			foreach (char letter in line)
			{
				switch (letter)
				{
					case '(':
						{
							aroundBracketsOpened++;
							break;
						}

					case ')':
						{
							aroundBracketsOpened--;
							break;
						}

					case '[':
						{
							squareBracketsOpened++;
							break;
						}

					case ']':
						{
							squareBracketsOpened--;
							break;
						}

					case '{':
						{
							figureBracketsOpened++;
							break;
						}

					case '}':
						{
							figureBracketsOpened--;
							break;
						}

					default:
						break;
				}
			}

			if (aroundBracketsOpened == 0 && squareBracketsOpened == 0 && figureBracketsOpened == 0)
			{
				return true;
			}

			return false;
		}

		private string ChangePythonKeywordsToCSharpKeywords(in string line)
		{
			string resultLine = line;

			foreach (string keyword in analogsOfPythonKeywordInCSharp.Keys)
			{
				List<int> _intArray = new List<int>(FindIndexOfActStartIgnoringBrackets(in resultLine, keyword));
				int _skipIterations = 0;
				string _newLine = string.Empty;

				for (int i = 0; i < resultLine.Length; i++)
				{
					if (_skipIterations > 0)
					{
						_skipIterations--;
						continue;
					}

					if (!_intArray.Contains(i))
					{
						_newLine += resultLine[i];
						continue;
					}

					_skipIterations = keyword.Length - 1;

					_newLine += analogsOfPythonKeywordInCSharp[keyword];
				}

				resultLine = _newLine;
			}

			return resultLine;
		}

		private string ProcessKeywords(in string line)
		{
			int indexOfKeywordInKeywordsArray = -1;
			int indexOfKeywordInLine = -1;
			string keyword = string.Empty;

			for (int i = 0; i < keywords.Length; i++)
			{
				string currentKeyword = keywords[i];

				int[] _intArray = FindIndexOfActStartIgnoringBrackets(line, currentKeyword);
				int indexOfCurrentKeyword = (_intArray.Length == 0) ? -1 : _intArray[_intArray.Length - 1];

				if (indexOfCurrentKeyword == -1)
				{
					continue;
				}

				indexOfKeywordInKeywordsArray = i;
				indexOfKeywordInLine = indexOfCurrentKeyword;
				keyword = currentKeyword;

				break;
			}

			if (indexOfKeywordInLine == -1)
			{
				return line;
			}

			string left = string.Empty;
			string right = string.Empty;

			for (int i = 0; i < indexOfKeywordInLine; i++)
			{
				left += line[i];
			}

			for (int i = indexOfKeywordInLine + keyword.Length; i < line.Length; i++)
			{
				right += line[i];
			}

			left = left.Trim();
			right = right.Trim();

			return processKeywords[indexOfKeywordInKeywordsArray](ProcessLine(left), ProcessLine(right), bodyStack.Get().variables);
		}

		private string ProcessAroundBrackets(in string line)
		{
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

			return line.Trim();
		}

		private string ProcessSquareBrackets(in string line)
		{
			string _lineDuplicate = line.Trim();

			if (_lineDuplicate.Length > 0 && _lineDuplicate[0] == '[' && _lineDuplicate[_lineDuplicate.Length - 1] == ']')
			{
				_lineDuplicate = string.Empty;
				List<char> _charList = new List<char>(line.Trim().ToCharArray());

				for (int i = 1; i < _charList.Count - 1; i++)
				{
					_lineDuplicate += _charList[i];
				}

				return WrapCodeInSquareBrackets(ProcessLine(_lineDuplicate));
			}

			return line.Trim();
		}

		private static string WrapCodeInSquareBrackets(in string line)
		{
			if (FindIndexOfActStartIgnoringBrackets(line, "select").Length != 0)
			{
				return "(" + line + ").ToArray()";
			}

			return "[" + line + "]";
		}

		private string ProcessArithmeticAndLogicalActs(in string line)
		{
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
					leftPart += line[i];
				}

				for (int i = indexOfActFromCurrentActsGroupInLine + actFromCurrentGroup.Length; i < line.Length; i++)
				{
					rightPart += line[i];
				}

				leftPart = leftPart.Trim();
				rightPart = rightPart.Trim();

				return ProcessActs[actGroupIndex][indexOfActInGroup](ProcessLine(leftPart), ProcessLine(rightPart),
																				actFromCurrentGroup, in bodyStack.Get().variables);
			}

			return line;
		}

		private static ConstructionType DefineWhichTypeOfConstructionInLine(in string line)
		{
			foreach (ConstructionType constructionType in ConstructionTypesAndTheirKeywords.Keys)
			{
				foreach (string keyword in ConstructionTypesAndTheirKeywords[constructionType])
				{
					if (FindIndexOfActStartIgnoringBrackets(line, keyword).Length > 0)
					{
						return constructionType;
					}
				}
			}

			return ConstructionType.Default;
		}

		private void UpdateBodyStack(in string line)
		{
			if (line == null || line.Trim() == string.Empty)
			{
				return;
			}

			int howManySpacesInCurrentLine = 0;
			
			foreach (char i in line)
			{
				if (i == ' ')
				{
					howManySpacesInCurrentLine++;
				}
				else if (i == '\t')
				{
					howManySpacesInCurrentLine += 4;
				}
				else
				{
					break;
				}
			}

			if (howManySpacesInCurrentLine > bodyStack.Get().howManySpaces)
			{
				if (typeOfLastConstruction == ConstructionType.Condition || typeOfLastConstruction == ConstructionType.Loop)
				{
					bodyStack.Add(new Body(howManySpacesInCurrentLine, bodyStack.Get().variables));
				}
				else
				{
					bodyStack.Add(new Body(howManySpacesInCurrentLine));
				}
			}

			while (howManySpacesInCurrentLine < bodyStack.Get().howManySpaces)
			{
				CloseBody();
			}
		}

		private void CloseBody()
		{
			Body previous = bodyStack.Delete();
			Body currentBody = bodyStack.Get();

			currentBody.AddCode(previous.GetBodyText(currentBody.howManySpaces, bodyStack.Count == 0 ? minSpacesAtStartLine : 0));
		}

		/// <summary>
		/// Find act's first letter index in line 
		/// </summary>
		/// <returns>Returns array of indexes in order like in line</returns>
		private static int[] FindIndexOfActStartIgnoringBrackets(in string line, string act)
		{
			Dictionary<char, char> openBracketByCloseBracket = new Dictionary<char, char>();
			openBracketByCloseBracket.Add(')', '(');
			openBracketByCloseBracket.Add(']', '[');
			openBracketByCloseBracket.Add('}', '{');

			Dictionary<char, int> howManyBracketsOpened = new Dictionary<char, int>();
			howManyBracketsOpened.Add('(', 0);
			howManyBracketsOpened.Add('[', 0);
			howManyBracketsOpened.Add('{', 0);

			bool findOnlySeparatedByWhiteSpace = false;

			foreach (char i in act)
			{
				if ((i >= 'a' && i <= 'z') || (i >= 'A' && i <= 'Z') || i == '_')
				{
					findOnlySeparatedByWhiteSpace = true;
					break;
				}
			}

			List<int> indexesOfActStart = new List<int>();

			int howManySkip = 0;

			for (int i = 0; i < line.Length; i++)
			{
				if (howManySkip > 0)
				{
					howManySkip--;
					continue;
				}

				if (howManyBracketsOpened.ContainsKey(line[i]))
				{
					howManyBracketsOpened[line[i]] += 1;
					continue;
				}

				if (openBracketByCloseBracket.ContainsKey(line[i]))
				{
					howManyBracketsOpened[openBracketByCloseBracket[line[i]]] -= 1;
					continue;
				}

				bool anyBracketIsNotClosed = false;

				foreach (char bracket in howManyBracketsOpened.Keys)
				{
					if (howManyBracketsOpened[bracket] > 0)
					{
						anyBracketIsNotClosed = true;
						break;
					}
				}

				if (anyBracketIsNotClosed)
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
								howManySkip = j.Length - 1;
								canAddIndex = false;
							}
						}
					}

					if (canAddIndex && findOnlySeparatedByWhiteSpace)
					{
						if ((i != 0 && !whiteSpaces.Contains(line[i - 1])) 
							|| (i + act.Length < line.Length - 1 && !whiteSpaces.Contains(line[i + act.Length])))
						{
							howManySkip = act.Length - 1;
							canAddIndex = false;
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

		private static string ProcessIn(string left, string right, in List<string> variables)
		{
			if (left.Split(' ').Length > 1)
			{
				if (!variables.Contains(left) && FindIndexOfActStartIgnoringBrackets(left, "from").Length == 0)
				{
					variables.Add(left);
				}
			}

			return $"{left} in {right}";
		}

		private static string ProcessIf(string left, string right, in List<string> _)
		{
			if (FindIndexOfActStartIgnoringBrackets(left, "in").Length != 0)
			{
				return ProcessIfAfterFor(left, right);
			}

			return $"if ({right})";
		}

		private static string ProcessIfAfterFor(string left, string right)
		{
			return $"{left} where ({right})";
		}

		private static string ProcessFor(string left, string right, in List<string> _)
		{
			if (left.Trim() != string.Empty)
			{
				int[] _intArr = FindIndexOfActStartIgnoringBrackets(left, "select");

				if (_intArr.Length != 0)
				{
					string resultCode = string.Empty;

					for (int i = 0; i < _intArr[0]; i++)
					{
						resultCode += left[i];
					}

					resultCode = resultCode.TrimEnd();
					resultCode += "\n\t" + $"from {right}" + "\n\t";

					for (int i = _intArr[0]; i < left.Length; i++)
					{
						resultCode += left[i];
					}

					return resultCode;
				}

				return $"from {right} select {left}";
			}

			return $"foreach ({right})";
		}

		private static string ProcessWhile(string left, string right, in List<string> _)
		{
			return $"while ({right})";
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
			if (left.Trim() == string.Empty)
			{
				return $"{act}{right}";
			}

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