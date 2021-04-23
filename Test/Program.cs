using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
	class Program
	{
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
			foreach(dynamic i in Range(start, stop, 1))
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

		public static void Print(dynamic sep, dynamic end, params dynamic[] output)
		{
			for (int i = 0; i < output.Length; i++)
			{
				Console.Write(output[i] + (i == output.Length - 1 ? end : sep));
			}
		}

		public static void Print(params dynamic[] output)
		{
			Print(" ", "\n", output);
		}

		static void Main(string[] args)
		{
			char[] a = { 'A', 'B', 'C' };
			char[] b = { 'x', 'y', 'z' };

			var c = (from i in Range(5) where i != 3
					from j in Range(5) where j < 4
					select new int[] { i, j }).ToArray();

			foreach (var x in c)
					Console.Write(x[0] + " " + x[1] + "\n");
		}
	}
}
