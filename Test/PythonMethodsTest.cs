using System;
using System.Linq;
using System.Collections.Generic;

namespace Test
{
	class PythonMethods
	{
		private static Dictionary<char, int> SymbolNumberPairs = new Dictionary<char, int>
		{
			{'0', 0 },
			{'1', 1 }, {'2', 2 }, {'3', 3 },
			{'4', 4 }, {'5', 5 }, {'6', 6 },
			{'7', 7 }, {'8', 8 }, {'9', 9 },
		};

		public static IEnumerable<dynamic> Range(dynamic start, dynamic stop, dynamic step)
		{
			dynamic i = start;

			while ((i < stop && step >= 0) || (i > stop && step <= 0))
			{
				yield return i;

				i += step;
			}
		}

		public static IEnumerable<dynamic> Range(dynamic start, dynamic stop)
		{
			foreach (dynamic i in Range(start, stop, 1))
			{
				yield return i;
			}
		}

		public static IEnumerable<dynamic> Range(dynamic stop)
		{
			foreach (dynamic i in Range(0, stop))
			{
				yield return i;
			}
		}

		public static int Int(dynamic str)
		{
			return Convert.ToInt32(str);
		}

		public static float Float(dynamic str)
		{
			return float.Parse(str);
		}

		public static string Str(dynamic x)
		{
			return Convert.ToString(x);
		}

		public static float Round(dynamic number, dynamic numsAfterDot)
		{
			return (float) Math.Round(number, numsAfterDot);
		}

		public static float Round(dynamic number)
		{
			return Round(number, 0);
		}

		public static dynamic Abs(dynamic n)
		{
			if (n < 0)
			{
				return -n;
			}

			return n;
		}

		public static dynamic Pow(dynamic number, dynamic pow)
		{
			return Math.Pow(number, pow);
		}

		public static dynamic Div(dynamic left, dynamic right)
		{
			dynamic defaultDivide = left / right;

			return defaultDivide - defaultDivide % 1;
		}

		public static dynamic Mod(dynamic left, dynamic right)
		{
			if (left >= 0 && right > 0)
			{
				return left % right;
			}
			else if (left >= 0 && right < 0)
			{
				return right + left % -right;
			}
			else if (left <= 0 && right > 0)
			{
				return right - -left % right;
			}
			else if (left <= 0 && right < 0)
			{
				return -(-left % -right);
			}
			else
			{
				throw new Exception($"You can't divide by zero ({left} % {right})");
			}
		}

		public static void Print(dynamic sep, dynamic end, params dynamic[] output)
		{
			for (int i = 0; i < output.Length; i++)
			{
				Console.Write(output[i]);
				Console.Write(i == output.Length - 1 ? end : sep);
			}
		}

		public static dynamic Input(dynamic textDisplayedWhenInput)
		{
			Print("", "", textDisplayedWhenInput);

			return Console.ReadLine();
		}

		public static dynamic Input()
		{
			return Console.ReadLine();
		}

		public static List<dynamic> Map(string type, IEnumerable<dynamic> list)
		{
			List<dynamic> resultList = new List<dynamic>();

			foreach (dynamic i in list)
			{
				dynamic newItem = null;

				if (type == "int")
					newItem = Int(i);
				else if (type == "float")
					newItem = Float(i);
				else if (type == "string")
					newItem = Str(i);

				resultList.Add(newItem);
			}

			return resultList;
		}

		static void Main(string[] args)
		{
			//dynamic a = Int(Input("A: "));
			//dynamic b = Float(Input("B: "));

			//Print("", "\n", Pow(a, b));
			//Print("", "\n", "Rounded 1.4:", Round(1.4f));
			//Print("", "\n", "Rounded 1.66:", Round(1.66f));
			//Print("", "\n", "Rounded 1.66:", Round(1.66f, 1));

			//string aaa = Str(1.445636f);

			//Print("", "\n", aaa);

			Print("", "\n", 1.0f);
		}
	}
}