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
		if (str.Length == 0)
		{
			throw new Exception("Try get integer from empty string");
		}

		int result = 0;

		List<char> strAsList = new List<char>(str.ToCharArray());
		int tenDegree = 0;

		while (strAsList.Count > 0)
		{
			char lastSymbol = strAsList[strAsList.Count - 1];
			result += SymbolNumberPairs[lastSymbol] * Pow(10, tenDegree);

			tenDegree += 1;
			strAsList.RemoveAt(strAsList.Count - 1);
		}

		return result;
	}

	public static float Float(dynamic str)
	{
		if (str.Length == 0)
		{
			throw new Exception("Try get float from empty string");
		}

		string[] strParts = str.Split('.', ',');

		string _strWholePart = strParts[0];
		string _strNonWholePart = strParts.Length > 1 ? strParts[1] : string.Empty;

		float result = 0;

		result += Convert.ToInt32(_strWholePart);

		if (_strNonWholePart != string.Empty)
		{
			_strNonWholePart = "0," + _strNonWholePart;
			result += float.Parse(_strNonWholePart);
		}

		return result;
	}

	public static string Str(dynamic x)
	{
		return Convert.ToString(x);
	}

	public static float Round(dynamic number, dynamic numsAfterDot)
	{
		return (float)Math.Round(number, numsAfterDot);
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
}