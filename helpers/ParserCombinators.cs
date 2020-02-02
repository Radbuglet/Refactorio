/*
 * A flexible, generic parser combinator library.
 */

using System;
using Godot.Collections;

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
        Array<Tuple<T, ParseState>> Parse(ParseState state);
    }

    public class Pop : IParser<char>
    {
        public Array<Tuple<char, ParseState>> Parse(ParseState state)
        {
            var result = state.Pop();
            return result == null
                ? new Array<Tuple<char, ParseState>>()
                : new Array<Tuple<char, ParseState>> {result};
        }
    }

    public class Eof : IParser<bool>
    {
        public Array<Tuple<bool, ParseState>> Parse(ParseState state)
        {
            return state.Pop() == null
                ? new Array<Tuple<bool, ParseState>> {new Tuple<bool, ParseState>(true, state)}
                : new Array<Tuple<bool, ParseState>>();
        }
    }
    
    public class Fail<T> : IParser<T>
    {
        public Array<Tuple<T, ParseState>> Parse(ParseState state)
        {
            return new Array<Tuple<T, ParseState>>();
        }
    }
    
    public class Pure<T> : IParser<T>
    {
        public T ReturnVal;
        public Array<Tuple<T, ParseState>> Parse(ParseState state)
        {
            return new Array<Tuple<T, ParseState>> { new Tuple<T, ParseState>(ReturnVal, state) };
        }
    }

    public class AndThen<T, TU> : IParser<T>
    {
        public IParser<TU> Init;
        public Func<TU, IParser<T>> Handler;
        public Array<Tuple<T, ParseState>> Parse(ParseState state)
        {
            var initCases = Init.Parse(state);
            var results = new Array<Tuple<T, ParseState>>();
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

        public Array<Tuple<T, ParseState>> Parse(ParseState state)
        {
            var firstCases = First.Parse(state);
            var secondCases = Second.Parse(state);
            foreach (var secondCase in secondCases)
            {
                firstCases.Add(secondCase);
            }

            return secondCases;
        }
    }

    public class Delay<T> : IParser<T>
    {
        public Func<IParser<T>> Thunk;

        public Array<Tuple<T, ParseState>> Parse(ParseState state)
        {
            return Thunk().Parse(state);
        }
    }

    public static class Combinators
    {
        public static IParser<char> Pop()
        {
            return new Pop { };
        }

        public static IParser<bool> Eof()
        {
            return new Eof { };
        }
        
        public static IParser<T> Fail<T>()
        {
            return new Fail<T> { };
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

        private static IParser<string> expect(string expected, int pos)
        {
            return pos >= expected.Length
                ? Pure(expected)
                : Pop().AndThen(c =>
                    c == expected[pos]
                        ? expect(expected, pos + 1)
                        : Fail<string>());
        }
        
        public static IParser<string> Expect(string expected)
        {
            return expect(expected, 0);
        }

        public static IParser<T> SurroundedBy<T, B, A>(
            this IParser<T> inner,
            IParser<B> before,
            IParser<A> after)
        {
            return before.IgnoreAnd(inner).AndIgnore(after);
        }
        
        public static IParser<T> Or<T>(this IParser<T> first, IParser<T> second)
        {
            return new Or<T> {First = first, Second = second};
        }

        public static IParser<Tuple<T, U>> And<T, U>(this IParser<T> first, IParser<U> second)
        {
            return first.AndThen(a => second.Map(b => new Tuple<T, U>(a, b)));
        }

        public static IParser<T> AndIgnore<T, U>(this IParser<T> first, IParser<U> second)
        {
            return first.AndThen(a => second.Map(_ => a));
        }
        
        public static IParser<U> IgnoreAnd<T, U>(this IParser<T> first, IParser<U> second)
        {
            return first.AndThen(_ => second);
        }

        public static IParser<U> Map<T, U>(this IParser<T> init, Func<T, U> transform)
        {
            return init.AndThen(i => Pure(transform(i)));
        }

        private class LinkedList<T>
        {
            public T Head;
            public LinkedList<T> Tail;
        }

        private static Array<T> convertLinkedListToArray<T>(LinkedList<T> list)
        {
            var results = new Array<T>();
            while (list != null)
            {
                results.Add(list.Head);
                list = list.Tail;
            }

            return results;
        }

        private static IParser<LinkedList<T>> repeat<T>(IParser<T> first)
        {
            return
                first.AndThen(h =>
                        repeat(first).Map(t => new LinkedList<T> {Head = h, Tail = t}))
                    .Or(Pure<LinkedList<T>>(null));
        }

        public static IParser<Array<T>> Repeat1<T>(this IParser<T> first)
        {
            return first.And(repeat(first)).Map(results =>
            {
                var list = new LinkedList<T> {Head = results.Item1, Tail = results.Item2};
                return convertLinkedListToArray(list);
            });
        }

        public static IParser<Array<T>> Repeat<T>(this IParser<T> first)
        {
            return repeat(first).Map(convertLinkedListToArray);
        }

        public static IParser<T> Delay<T>(Func<IParser<T>> thunk)
        {
            return new Delay<T> {Thunk = thunk};
        }

        public static IParser<char> Brainfuck()
        {
            return
                Expect(c => c == '+').Or(
                Expect(c => c == '-')).Or(
                Expect(c => c == '<')).Or(
                Expect(c => c == '>')).Or(
                Expect(c => c == '.')).Or(
                Expect(c => c == ',')).Or(
                Expect(c => c == '[').AndThen(_ => Brainfuck()).AndThen(_ => Expect(c => c == ']')))
                    .Repeat().Map(_ => '_');
        }
    }
}