/*
 * A flexible, generic parser combinator library.
 */

using System;
using System.Collections.Generic;

namespace Refactorio.helpers
{
	public class ParseState
	{
		private readonly string _input;
		private readonly int _position;

		public ParseState(string input, int position)
		{
			_input = input;
			_position = position;
		}

		public Tuple<char, ParseState> Pop()
		{
			if (_position >= _input.Length)
			{
				return null;
			}
			else
			{
				var newState = new ParseState(_input, _position + 1);
				return new Tuple<char, ParseState>(_input[_position], newState); 
			}
		}
	}

	public interface IParser<T>
	{
		List<Tuple<T, ParseState>> Parse(ParseState state);
	}

	public class Pop : IParser<char>
	{
		public List<Tuple<char, ParseState>> Parse(ParseState state)
		{
			var result = state.Pop();
			return result == null
				? new List<Tuple<char, ParseState>>()
				: new List<Tuple<char, ParseState>> {result};
		}
	}

	public class Eof : IParser<bool>
	{
		public List<Tuple<bool, ParseState>> Parse(ParseState state)
		{
			return state.Pop() == null
				? new List<Tuple<bool, ParseState>> {new Tuple<bool, ParseState>(true, state)}
				: new List<Tuple<bool, ParseState>>();
		}
	}
	
	public class Fail<T> : IParser<T>
	{
		public List<Tuple<T, ParseState>> Parse(ParseState state)
		{
			return new List<Tuple<T, ParseState>>();
		}
	}
	
	public class Pure<T> : IParser<T>
	{
		public T ReturnVal;
		public List<Tuple<T, ParseState>> Parse(ParseState state)
		{
			return new List<Tuple<T, ParseState>> { new Tuple<T, ParseState>(ReturnVal, state) };
		}
	}

	public class AndThen<T, TU> : IParser<T>
	{
		public IParser<TU> Init;
		public Func<TU, IParser<T>> Handler;
		public List<Tuple<T, ParseState>> Parse(ParseState state)
		{
			var initCases = Init.Parse(state);
			var results = new List<Tuple<T, ParseState>>();
			foreach (var (u, caseState) in initCases)
			{
				var handledCases = Handler(u).Parse(caseState);
				foreach (var handledCase in handledCases)
				{
					results.Add(handledCase);
				}
			}

			return results;
		}
	}

	public class Or<T> : IParser<T>
	{
		public IParser<T> First;
		public IParser<T> Second;

		public List<Tuple<T, ParseState>> Parse(ParseState state)
		{
			var firstCases = First.Parse(state);
			var secondCases = Second.Parse(state);
			firstCases.AddRange(secondCases);

			return firstCases;
		}
	}

	public class Delay<T> : IParser<T>
	{
		public Func<IParser<T>> Thunk;

		public List<Tuple<T, ParseState>> Parse(ParseState state)
		{
			return Thunk().Parse(state);
		}
	}

	public static class Combinators
	{
		public static IParser<char> Pop()
		{
			return new Pop();
		}

		public static IParser<bool> Eof()
		{
			return new Eof();
		}
		
		public static IParser<T> Fail<T>()
		{
			return new Fail<T>();
		}
		
		public static IParser<T> Pure<T>(T returnVal)
		{
			return new Pure<T> {ReturnVal = returnVal};
		}

		public static IParser<T> AndThen<T, U>(this IParser<U> init, Func<U, IParser<T>> handler)
		{
			return new AndThen<T, U> {Init = init, Handler = handler};
		}
		
		public static IParser<char> Expect(Func<char, bool> isValid)
		{
			return Pop().AndThen(c =>
				isValid(c) ? Pure(c) : Fail<char>());
		}

		private static IParser<string> ExpectPos(string expected, int pos)
		{
			return pos >= expected.Length
				? Pure(expected)
				: Pop().AndThen(c =>
					c == expected[pos]
						? ExpectPos(expected, pos + 1)
						: Fail<string>());
		}
		
		public static IParser<string> Expect(string expected)
		{
			return ExpectPos(expected, 0);
		}

		public static IParser<T> SurroundedBy<T, TB, TA>(
			this IParser<T> inner,
			IParser<TB> before,
			IParser<TA> after)
		{
			return before.IgnoreAnd(inner).AndIgnore(after);
		}

		public static IParser<T> Or<T>(this IParser<T> first, IParser<T> second)
		{
			return new Or<T> {First = first, Second = second};
		}

		public static IParser<Tuple<T, TU>> And<T, TU>(this IParser<T> first, IParser<TU> second)
		{
			return first.AndThen(a => second.Map(b => new Tuple<T, TU>(a, b)));
		}

		public static IParser<T> AndIgnore<T, TU>(this IParser<T> first, IParser<TU> second)
		{
			return first.AndThen(a => second.Map(_ => a));
		}
		
		public static IParser<TU> IgnoreAnd<T, TU>(this IParser<T> first, IParser<TU> second)
		{
			return first.AndThen(_ => second);
		}

		public static IParser<TU> Map<T, TU>(this IParser<T> init, Func<T, TU> transform)
		{
			return init.AndThen(i => Pure(transform(i)));
		}

		private class LinkedList<T>
		{
			public T Head;
			public LinkedList<T> Tail;
		}

		private static List<T> ConvertLinkedListToList<T>(LinkedList<T> list)
		{
			var results = new List<T>();
			while (list != null)
			{
				results.Add(list.Head);
				list = list.Tail;
			}

			return results;
		}

		private static IParser<LinkedList<T>> RepeatToLinkedList<T>(IParser<T> first)
		{
			return
				first.AndThen(h =>
						RepeatToLinkedList(first).Map(t => new LinkedList<T> {Head = h, Tail = t}))
					.Or(Pure<LinkedList<T>>(null));
		}

		public static IParser<List<T>> Repeat1<T>(this IParser<T> first)
		{
			return first.And(RepeatToLinkedList(first)).Map(results =>
			{
				var (head, tail) = results;
				var list = new LinkedList<T> {Head = head, Tail = tail};
				return ConvertLinkedListToList(list);
			});
		}

		public static IParser<List<T>> Repeat<T>(this IParser<T> first)
		{
			return RepeatToLinkedList(first).Map(ConvertLinkedListToList);
		}

		public static IParser<List<T>> RepeatedSeparatedBy<T, TS>(this IParser<T> first, IParser<TS> sep)
		{
			return first.And(sep.IgnoreAnd(RepeatToLinkedList(first))).Map(results =>
			{
				var (head, tail) = results;
				var list = new LinkedList<T> {Head = head, Tail = tail};
				return ConvertLinkedListToList(list);
			});
		}

		public static IParser<T> Delay<T>(Func<IParser<T>> thunk)
		{
			return new Delay<T> {Thunk = thunk};
		}
	}
}
