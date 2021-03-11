using System;

namespace Test
{
	class Program
	{
		public static dynamic Abs(dynamic n)
		{
			if (n < 0)
			{
				return -n;
			}

			return n;
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

		public static void Print(string sep = " ", string end = "\n", params dynamic[] output)
		{
			for (int i = 0; i < output.Length; i++)
			{
				Console.Write(output[i] + (i == output.Length - 1 ? end : sep));
			}
		}

		static void Main(string[] args)
		{
			Print(", ", "\n", 1, 2, 3, 4);
			Print(", ", "\n", 1, 2, 3, 4);
			Print("; ", "\n", Abs(5.5d), Abs(-5.5f));
		}
	}
}
