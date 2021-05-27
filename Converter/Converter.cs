using System;
using System.Linq;
using System.Collections.Generic;

namespace Converter
{
	public enum ConstructionType
	{
		Loop,
		Condition,
		Function,
		Class,
		Default,
	}

	public class Converter
	{
		private System.IO.StreamReader pythonFile;
		private System.IO.StreamWriter cSharpFile;

		int _lineNumber;

		private static string lastConvertedLine = string.Empty;

		private static int minSpacesAtStartLine = 4;
		private static Stack<Body> bodyStack = new Stack<Body>();

		private static List<string> classes = new List<string>();
		private static List<string> functions = new List<string>();

		private static Body namespaceBody = new Body("Program", "namespace Program", ConstructionType.Default, 0);
		private static Body mainClassBody = new Body("Program", "public class Program", ConstructionType.Class, 0, 4);
		private static Body entranceFunctionBody = new Body("Main", "static void Main(string[] args)", ConstructionType.Function, 0, 4);

		private static List<string> libraries = new List<string>
		{
			"System",
			"System.Linq",
			"System.Collections.Generic",
		};

		private ConstructionType typeOfLastConstruction;
		private string nameOfLastConstruction;

		private static Dictionary<ConstructionType, List<string>> ConstructionTypesAndTheirKeywords = new Dictionary<ConstructionType, List<string>>
		{
			{ConstructionType.Class, new List<string> { "class" } },
			{ConstructionType.Condition, new List<string> { "if" } },
			{ConstructionType.Function, new List<string> { "def" } },
			{ConstructionType.Loop, new List<string> { "while", "for" } },
		};

		private static Dictionary<ConstructionType, GetConstructionName> namedConstructionTypes = new Dictionary<ConstructionType, GetConstructionName>
		{
			{ConstructionType.Class, GetClassName },
			{ConstructionType.Function, GetFunctionName },
		};

		private static Dictionary<string, string> analogsOfPythonKeywordInCSharp = new Dictionary<string, string>
		{
			{"True", "true" },
			{"False", "false" },
			{"None", "null" },
			{"self", "this" },
		};

		private delegate string ProcessAct(string left, string right, string act, in List<string> variables);
		private delegate string ProcessKeyword(string left, string right, in List<string> variables);
		private delegate string GetConstructionName(in string line);

		private static List<char> whiteSpaces = new List<char> { ' ', '\n', '\t', };

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

		private static Dictionary<string, ProcessAct> functionsWhichProcessArithmeticAndLogicalActs = new Dictionary<string, ProcessAct>
		{
			{"=", Assignment },
			{"+=", ActWithAssignment },
			{"-=", ActWithAssignment },
			{"*=", ActWithAssignment },
			{"/=", ActWithAssignment },
			{"//=", ActWithAssignment },
			{"%=", ActWithAssignment },
			{"or", Disjuction },
			{"and", Conjuction },
			{"not", Inversion },
			{"<", JoinPartsByActWhichSameInPythonAndCS },
			{"<=", JoinPartsByActWhichSameInPythonAndCS },
			{">", JoinPartsByActWhichSameInPythonAndCS },
			{">=", JoinPartsByActWhichSameInPythonAndCS },
			{"!=", JoinPartsByActWhichSameInPythonAndCS },
			{"==", JoinPartsByActWhichSameInPythonAndCS },
			{"+", JoinPartsByActWhichSameInPythonAndCS },
			{"-", JoinPartsByActWhichSameInPythonAndCS },
			{"*", JoinPartsByActWhichSameInPythonAndCS },
			{"/", JoinPartsByActWhichSameInPythonAndCS },
			{"//", Div },
			{"%", Mod },
			{"**", Pow },
		};

		private static string[] cSharpKeywordsWithWhichLineMustntBeWrited = { "if", "for", "while", "class", "def" };

		private static Dictionary<string, ProcessKeyword> functionsWhichProcessKeywords = new Dictionary<string, ProcessKeyword>
		{
			{"class", ProcessClass },
			{"def", ProcessDef },
			{"for", ProcessFor },
			{"if", ProcessIf },
			{"while", ProcessWhile },
			{"in", ProcessIn },
		};

		private static Dictionary<char, char> OpenCloseBracketsPairs = new Dictionary<char, char>
		{
			{ '(', ')' },
			{ '[', ']' },
			{ '{', '}' },
		};

		private static string ProcessLine(in string pythonLine)
		{
			if (pythonLine == null)
			{
				return "";
			}

			string line = pythonLine.Trim(' ', ':');

			line = ChangePythonKeywordsToCSharpKeywords(line);
			line = ProcessKeywords(line);
			line = ProcessArithmeticAndLogicalActs(line);
			line = ProcessDots(line);
			line = ProcessFunctionCalling(line);
			line = ProcessSquareBrackets(line);
			line = ProcessAroundBrackets(line);

			return line.Trim();
		}

		public Converter(string pathToPythonModule, string pathToCSharpFile)
		{
			pythonFile = new System.IO.StreamReader(pathToPythonModule);
			cSharpFile = new System.IO.StreamWriter(pathToCSharpFile);

			bodyStack.Add(namespaceBody);
			bodyStack.Add(mainClassBody);
			bodyStack.Add(entranceFunctionBody);
			mainClassBody.parentBody = namespaceBody;
			entranceFunctionBody.parentBody = mainClassBody;

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
				nameOfLastConstruction = DefineConstructionName(line, typeOfLastConstruction);

				ClassesListUpdate();
				FunctionsListUpdate();

				lastConvertedLine = ProcessLine(line);
				PutLineToBody(line);

			} while (newLine != null);

			while (bodyStack.Count > 1)
			{
				CloseBody();
			}

			string finallCode = GetResultProgram();

			cSharpFile.Write(finallCode);

			pythonFile.Close();
			cSharpFile.Close();
		}

		private void TestMethod() // For Debug methods
		{
			string s = @"dakod, aksd, askf(dadk, aokfs, dad'('dado, asfk), kad(')'), kafassa";

			var d = GetOpenCloseBracketsAndQuotesPairs(s);

			foreach (var i in d.Keys)
			{
				Console.WriteLine(i + " " + d[i]);
			}
		}

		private void ClassesListUpdate()
		{
			if (typeOfLastConstruction == ConstructionType.Class && !classes.Contains(nameOfLastConstruction))
			{
				classes.Add(nameOfLastConstruction);
			}
		}

		private void FunctionsListUpdate()
		{
			if (typeOfLastConstruction == ConstructionType.Function &&
				bodyStack.Get().parentBody == mainClassBody)
			{
				functions.Add(nameOfLastConstruction);
			}
		}

		private static string GetResultProgram()
		{
			string result = bodyStack.Get().GetBodyText(0, minSpacesAtStartLine);

			result = "\n" + result;

			for (int i = libraries.Count - 1; i >= 0; i--)
			{
				result = $"using {libraries[i]};\n" + result;
			}

			return result;
		}

		private static void PutLineToBody(in string pythonLine)
		{
			if (lastConvertedLine.Trim() == string.Empty)
			{
				bodyStack.Get().AddCode("\n");
				return;
			}

			foreach (string keyword in cSharpKeywordsWithWhichLineMustntBeWrited)
			{
				if (FindIndexOfActStartIgnoringBrackets(pythonLine, keyword).Length != 0)
				{
					bodyStack.Get().AddCode(string.Empty);
					return;
				}
			}

			bodyStack.Get().AddCode(lastConvertedLine.TrimEnd('\n') + ";\n");
			return;
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

		private static string ChangePythonKeywordsToCSharpKeywords(in string line)
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

		private static string ProcessKeywords(in string line)
		{
			int indexOfKeywordInLine = -1;
			string keyword = string.Empty;

			foreach (string currentKeyword in functionsWhichProcessKeywords.Keys)
			{
				int[] _intArray = FindIndexOfActStartIgnoringBrackets(line, currentKeyword);
				int indexOfCurrentKeyword = (_intArray.Length == 0) ? -1 : _intArray[_intArray.Length - 1];

				if (indexOfCurrentKeyword == -1)
				{
					continue;
				}

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

			return functionsWhichProcessKeywords[keyword](ProcessLine(left), ProcessLine(right), bodyStack.Get().variables);
		}

		private static string ProcessAroundBrackets(in string line)
		{
			string _lineDuplicate = line.Trim();

			Dictionary<int, int> openCloseBrackketsPairs = GetOpenCloseBracketsAndQuotesPairs(_lineDuplicate);

			if (line.Length > 0 && _lineDuplicate[0] == '[' 
				&& openCloseBrackketsPairs.ContainsKey(0) && openCloseBrackketsPairs[0] == _lineDuplicate.Length - 1)
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

		private static string ProcessSquareBrackets(in string line)
		{
			string _trimmedLine = line.Trim();

			Dictionary<int, int> openCloseBracketsPairs = GetOpenCloseBracketsAndQuotesPairs(_trimmedLine);

			if (!(line.Length != 0 && line[0] == '[' && openCloseBracketsPairs.ContainsKey(0) && openCloseBracketsPairs[0] == _trimmedLine.Length - 1))
			{
				return _trimmedLine;
			}

			string codeInBrackets = GetCodeFromBrackets(line);

			if (FindIndexOfActStartIgnoringBrackets(codeInBrackets, ",").Length > 0
				&& FindIndexOfActStartIgnoringBrackets(codeInBrackets, "for").Length == 0)
			{
				return CreateArray(codeInBrackets);
			}

			return WrapCodeInSquareBrackets(ProcessLine(codeInBrackets));
		}

		private static string CreateArray(in string line)
		{
			string result = string.Empty;

			foreach (string arrayElement in SplitIgnoringBrackets(line, ','))
			{
				result += ProcessLine(arrayElement) + ", ";
			}

			return "new List<dynamic> { " + result + "}";
		}

		private static Dictionary<int, int> GetOpenCloseBracketsAndQuotesPairs(in string line)
		{
			Dictionary<int, int> openCloseBracketsPairs = new Dictionary<int, int>();
			Stack<int> openBracketsIndexesStack = new Stack<int>();

			bool insideString = false;
			bool insideChar = false;

			for (int i = 0; i < line.Length; i++)
			{
				char letter = line[i];

				if (letter == '\"' && !insideString && !insideChar)
				{
					insideString = true;
					openBracketsIndexesStack.Add(i);
				}
				else if (letter == '\"' && insideString && (i - 1 >= 0 && line[i - 1] != '\\' || i - 1 < 0))
				{
					insideString = false;
					openCloseBracketsPairs.Add(openBracketsIndexesStack.Get(), i);
					openBracketsIndexesStack.Delete();
				}
				else if (letter == '\'' && !insideString && !insideChar)
				{
					insideChar = true;
					openBracketsIndexesStack.Add(i);
				}
				else if (letter == '\'' && insideChar && (i - 1 >= 0 && line[i - 1] != '\\' || i - 1 < 0))
				{
					insideChar = false;
					openCloseBracketsPairs.Add(openBracketsIndexesStack.Get(), i);
					openBracketsIndexesStack.Delete();
				}
				else if (!insideChar && !insideString && OpenCloseBracketsPairs.ContainsKey(letter))
				{
					openBracketsIndexesStack.Add(i);
				}
				else if (!insideChar && !insideString && OpenCloseBracketsPairs.ContainsValue(letter))
				{
					openCloseBracketsPairs.Add(openBracketsIndexesStack.Get(), i);
					openBracketsIndexesStack.Delete();
				}
			}

			return openCloseBracketsPairs;
		}

		private static bool IsInsideBrackets(int index, in string line, Dictionary<int, int> openCloseBracketsPairs = null)
		{
			if (openCloseBracketsPairs == null)
			{
				openCloseBracketsPairs = GetOpenCloseBracketsAndQuotesPairs(line);
			}

			foreach (int i in openCloseBracketsPairs.Keys)
			{
				if (index > i && index < openCloseBracketsPairs[i])
				{
					return true;
				}
			}

			return false;
		}

		private static List<string> SplitIgnoringBrackets(in string line, char separator)
		{
			string _duplicateLine = line + separator;

			Dictionary<int, int> openCloseBracketsPairs = GetOpenCloseBracketsAndQuotesPairs(line);

			List<string> result = new List<string>();

			string element = string.Empty;

			for (int i = 0; i < _duplicateLine.Length; i++)
			{
				if (_duplicateLine[i] == separator && element.Length != 0 && !IsInsideBrackets(i, _duplicateLine, openCloseBracketsPairs))
				{
					result.Add(element);
					element = string.Empty;
					continue;
				}

				element += _duplicateLine[i];
			}

			return result;
		}

		private static string GetCodeFromBrackets(char openBracket, char closeBracket, in string line)
		{
			string contentInsideBrackets = line.Trim();

			if (contentInsideBrackets.Length >= 2
				&& contentInsideBrackets[0] == openBracket
				&& contentInsideBrackets[contentInsideBrackets.Length - 1] == closeBracket)
			{
				contentInsideBrackets = string.Empty;
				string _trimmedLine = line.Trim();

				for (int i = 1; i < _trimmedLine.Length - 1; i++)
				{
					contentInsideBrackets += _trimmedLine[i];
				}

				return contentInsideBrackets;
			}

			return line;
		}

		private static string GetCodeFromBrackets(in string line)
		{
			string contentInsideBrackets = line.Trim();

			if (contentInsideBrackets.Length >= 2
				&& OpenCloseBracketsPairs.ContainsKey(contentInsideBrackets[0])
				&& contentInsideBrackets[contentInsideBrackets.Length - 1] == OpenCloseBracketsPairs[contentInsideBrackets[0]])
			{
				contentInsideBrackets = string.Empty;
				string _trimmedLine = line.Trim();

				for (int i = 1; i < _trimmedLine.Length - 1; i++)
				{
					contentInsideBrackets += _trimmedLine[i];
				}

				return contentInsideBrackets;
			}

			return line;
		}

		private static string WrapCodeInSquareBrackets(in string line)
		{
			if (FindIndexOfActStartIgnoringBrackets(line, "select").Length != 0)
			{
				return "(" + line + ").ToList()";
			}

			return "[" + line + "]";
		}

		private static string ProcessArithmeticAndLogicalActs(in string line)
		{
			string rightPart = "";
			string leftPart = "";

			for (int actGroupIndex = 0; actGroupIndex < prioritisedActs.Length; actGroupIndex++)
			{
				int indexOfActInLine = -1;
				string actFromCurrentGroup = string.Empty;
				int indexOfActInGroup = -1;

				for (int actIndex = 0; actIndex < prioritisedActs[actGroupIndex].Length; actIndex++)
				{
					string act = prioritisedActs[actGroupIndex][actIndex];

					int[] _intArray = FindIndexOfActStartIgnoringBrackets(in line, act);
					int indexOfCurrentActInLine = (_intArray.Length == 0) ? -1 : _intArray[_intArray.Length - 1];

					if (indexOfCurrentActInLine == -1)
					{
						continue;
					}

					if (indexOfCurrentActInLine > indexOfActInLine)
					{
						indexOfActInLine = indexOfCurrentActInLine;
						actFromCurrentGroup = act;
						indexOfActInGroup = actIndex;
					}
				}

				if (indexOfActInLine == -1) // Line doesn't contain act from current act group
				{
					continue;
				}

				for (int i = 0; i < indexOfActInLine; i++)
				{
					leftPart += line[i];
				}

				for (int i = indexOfActInLine + actFromCurrentGroup.Length; i < line.Length; i++)
				{
					rightPart += line[i];
				}

				leftPart = leftPart.Trim();
				rightPart = rightPart.Trim();

				return functionsWhichProcessArithmeticAndLogicalActs[actFromCurrentGroup](ProcessLine(leftPart), ProcessLine(rightPart),
																				actFromCurrentGroup, in bodyStack.Get().variables);
			}

			return line;
		}

		public static ConstructionType DefineWhichTypeOfConstructionInLine(in string line)
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

		private static string DefineConstructionName(in string line, ConstructionType constructionType)
		{
			if (!namedConstructionTypes.ContainsKey(constructionType))
			{
				return string.Empty;
			}

			return namedConstructionTypes[constructionType](line);
		}

		private static string GetClassName(in string line)
		{
			int[] _intArray = FindIndexOfActStartIgnoringBrackets(line, "class");
			int indexOfClassKeywordFirstLetter = _intArray.Length > 0 ? _intArray[0] : -1;

			List<char> lineAsList = line.ToList();

			if (indexOfClassKeywordFirstLetter != -1)
			{
				for (int i = 0; i <= indexOfClassKeywordFirstLetter + "class".Length - 1; i++)
				{
					lineAsList.RemoveAt(0);
				}
			}

			lineAsList = StringFromCharList(lineAsList).Trim().ToList();

			string className = string.Empty;

			while (lineAsList.Count > 0 && lineAsList[0] != '(' && lineAsList[0] != ' ' && lineAsList[0] != ':')
			{
				className += lineAsList[0];
				lineAsList.RemoveAt(0);
			}

			return className;
		}

		private static string GetFunctionName(in string line)
		{
			int[] _intArray = FindIndexOfActStartIgnoringBrackets(line, "def");
			int indexOfDefKeywordFirstLetter = _intArray.Length > 0 ? _intArray[0] : -1;

			List<char> lineAsList = line.ToList();

			if (indexOfDefKeywordFirstLetter != -1)
			{
				for (int i = 0; i <= indexOfDefKeywordFirstLetter + "def".Length - 1; i++)
				{
					lineAsList.RemoveAt(0);
				}
			}

			lineAsList = StringFromCharList(lineAsList).Trim().ToList();

			string functionName = string.Empty;

			while (lineAsList.Count > 0 && lineAsList[0] != '(' && lineAsList[0] != ' ')
			{
				functionName += lineAsList[0];
				lineAsList.RemoveAt(0);
			}

			return functionName;
		}

		private void UpdateBodyStack(in string line)
		{
			if (line == null || line.Trim() == string.Empty)
			{
				return;
			}

			int howManySpacesInCurrentLine = GetNumberOfSpacesInLineStart(line);

			if (howManySpacesInCurrentLine > bodyStack.Get().howManySpaces)
			{
				Body newBody;

				if (typeOfLastConstruction == ConstructionType.Condition || typeOfLastConstruction == ConstructionType.Loop)
				{
					newBody = new Body(null, lastConvertedLine, typeOfLastConstruction, howManySpacesInCurrentLine, bodyStack.Get().variables);
				}
				else
				{
					newBody = new Body(nameOfLastConstruction, lastConvertedLine, typeOfLastConstruction, howManySpacesInCurrentLine);
				}

				LinkNewBodyWithParent(newBody);
				bodyStack.Add(newBody);
			}

			while (howManySpacesInCurrentLine < bodyStack.Get().howManySpaces)
			{
				CloseBody();
			}
		}

		private static int GetNumberOfSpacesInLineStart(in string line)
		{
			int result = 0;

			foreach (char i in line)
			{
				if (i == ' ')
				{
					result++;
				}
				else if (i == '\t')
				{
					result += 4;
				}
				else
				{
					break;
				}
			}

			return result;
		}

		private void LinkNewBodyWithParent(Body newBody)
		{
			if (newBody.constructionType == ConstructionType.Class)
			{
				newBody.parentBody = namespaceBody;
			}
			else if (newBody.constructionType == ConstructionType.Function)
			{
				if (bodyStack.Get().constructionType == ConstructionType.Class)
				{
					newBody.parentBody = bodyStack.Get();
				}
				else
				{
					newBody.parentBody = mainClassBody;
				}
			}
			else
			{
				newBody.parentBody = bodyStack.Get();
			}
		}

		private void CloseBody()
		{
			bodyStack.Delete().ImportCodeToParent();

			//Body previous = bodyStack.Delete();

			//currentBody.AddCode(previous.GetBodyText(currentBody.howManySpaces, bodyStack.Count == 0 ? minSpacesAtStartLine : 0));
		}

		/// <summary>
		/// Find act's first letter index in line 
		/// </summary>
		/// <returns>Returns array of indexes in order like in line</returns>
		public static int[] FindIndexOfActStartIgnoringBrackets(in string line, string act)
		{
			bool findOnlySeparatedByWhiteSpace = false;

			Dictionary<int, int> openCloseBracketsPairs = GetOpenCloseBracketsAndQuotesPairs(line);

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

				if (IsInsideBrackets(i, line, openCloseBracketsPairs))
				{
					continue;
				}

				if (IsOverlapAct(i, in line, act))
				{
					bool canAddIndex = true;

					foreach (string[] actGroup in prioritisedActs)
					{
						foreach (string otherAct in actGroup)
						{
							if (otherAct != act && otherAct.Length > act.Length && IsOverlapAct(i, in line, otherAct))
							{
								howManySkip = otherAct.Length - 1;
								canAddIndex = false;
							}
						}
					}

					if (canAddIndex && findOnlySeparatedByWhiteSpace)
					{
						int firstLetterIndex = i;
						int lastLetterIndex = i + act.Length - 1;

						if ((firstLetterIndex != 0 && !whiteSpaces.Contains(line[firstLetterIndex - 1]))
							|| (lastLetterIndex != line.Length - 1 && !whiteSpaces.Contains(line[lastLetterIndex + 1])))
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

		private static string ProcessDots(in string line)
		{
			return line;
		}

		private static string ProcessFunctionCalling(in string line)
		{
			Dictionary<int, int> openCloseBracketsAndQuotesPairs = GetOpenCloseBracketsAndQuotesPairs(line);

			int openBracketIndex = 0;

			if (!TryGetKeyByValue(openCloseBracketsAndQuotesPairs, line.Length - 1, ref openBracketIndex))
			{
				return line;
			}

			string functionName = line.Substring(0, line.Length - 2);

			if (CheckDoesWordHaveSpecialSymbol(functionName))
			{
				return line;
			}

			if (functions.Contains(functionName))
			{
				return $"{mainClassBody.name}.{functionName}()";
			}

			if (classes.Contains(functionName))
			{
				return $"new {functionName}()";
			}

			return $"new UNFAMOUS_CLASS() /* REQUARED HAND EDITING !!! WAS CODE: {line} */";
		}

		private static string CreateFunctionParameters(in string line)
		{
			string result = string.Empty;

			List<string> parameters = SplitIgnoringBrackets(line, ',');

			for (int i = 0; i < parameters.Count; i++)
			{
				if (FindIndexOfActStartIgnoringBrackets(parameters[i], "self").Length > 0)
				{
					continue;
				}

				result += "dynamic " + ProcessLine(parameters[i]);

				if (i != parameters.Count - 1)
				{
					result += ", ";
				}
			}

			return result;
		}

		private static string ProcessDef(string _a, string line, in List<string> _b)
		{
			List<char> lineAsList = new List<char>(line.Trim().ToCharArray());
			string functionName = GetFunctionName(line);

			while (lineAsList[0] != '(')
			{
				lineAsList.RemoveAt(0);
			}

			line = StringFromCharList(lineAsList).Trim();

			string parameters = GetCodeFromBrackets(line);
			parameters = "(" + CreateFunctionParameters(parameters) + ")";

			if (functionName == "__init__")
			{
				return "public " + bodyStack.Get().name + parameters;
			}

			return "public dynamic " + functionName + parameters;
		}

		private static string ProcessClass(string _a, string line, in List<string> _b)
		{
			List<char> lineAsList = new List<char>(line.Trim().ToCharArray());
			
			string className = GetClassName(line);

			while (lineAsList.Count > 0 && lineAsList[0] != '(')
			{
				lineAsList.RemoveAt(0);
			}

			if (lineAsList.Count == 0)
			{
				return "public class " + className;
			}

			string parentClassName = GetCodeFromBrackets(StringFromCharList(lineAsList));

			return "public class " + className + ": " + parentClassName;
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

			return Assignment(left, functionsWhichProcessArithmeticAndLogicalActs[actWithoutAssignment](left, right, actWithoutAssignment, variables),
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

		private static bool TryGetKeyByValue(Dictionary<int, int> dictionary, int value, ref int result)
		{
			foreach (int key in dictionary.Keys)
			{
				if (dictionary[key] == value)
				{
					result = key;
					return true;
				}
			}

			return false;
		}

		private static bool CheckDoesWordHaveSpecialSymbol(string s)
		{
			foreach (char symbol in s)
			{
				bool currentSymbolIsDefaultLetter = symbol >= 'a' && symbol <= 'z' || symbol >= 'A' && symbol <= 'Z' || symbol == '_';

				if (!currentSymbolIsDefaultLetter)
				{
					return true;
				}
			}

			return false;
		}
	}
}