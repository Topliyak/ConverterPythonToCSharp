using System;
using System.Collections.Generic;
using System.Text;

namespace Converter
{
	class Stack<T>
	{
		private Exception emptyStackExeption = new Exception("You try get element from empty stack");

		private List<T> stack = new List<T>();

		public void Add(T newElement)
		{
			stack.Add(newElement);
		}

		public T Get()
		{
			if (stack.Count == 0)
			{
				throw emptyStackExeption;
			}

			return stack[stack.Count - 1];
		}

		public T Delete()
		{
			if (stack.Count == 0)
			{
				throw emptyStackExeption;
			}

			T lastElement = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);

			return lastElement;
		}

		public int Count
		{
			get {
				return stack.Count;
			}
		}
	}
}
