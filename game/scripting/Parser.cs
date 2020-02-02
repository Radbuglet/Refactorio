using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Policy;
using Godot;
using Refactorio.helpers;

using C = Refactorio.helpers.Combinators;
using Rt = Refactorio.game.scripting.Runtime;

namespace Refactorio.game.scripting
{
	public static class Parser
	{
		public class ParseError : Exception
		{
			public ParseError(string msg) : base(msg) { }
		}

		private const string IdentChars =
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@_'";
		
		private static readonly IParser<bool> Whitespace =
			C.Expect(c => c == ' ' || c == '\t')
				.Repeat()
				.Map(_ => true);

		private static IParser<Rt.IExpression> Unary(
			string name,
			Runtime.UnaryOperator op,
			IParser<Rt.IExpression> next)
		{
			return C.Expect(name)
				.And(Whitespace)
				.IgnoreAnd(next)
				.Map(inner => (Rt.IExpression) new Rt.UnaryExpression
				{
					Op = op,
					Inner = inner
				});
		}

		private static IParser<Rt.IExpression> Binary(
			string name,
			Runtime.BinaryOperator op,
			IParser<Rt.IExpression> innerL,
			IParser<Rt.IExpression> innerR)
		{
			return innerL
				.AndIgnore(C.Expect(name))
				.AndIgnore(Whitespace)
				.And(innerR)
				.Map(args => (Rt.IExpression) new Rt.BinaryExpression
				{
					Op = op,
					Left = args.Item1,
					Right = args.Item2,
				});
		}
		
		public static Dictionary<string, List<Rt.ConditionalInstruction>> Parse(string code)
		{

			IParser<Rt.IExpression>
				expr0 = null,
				expr1 = null,
				expr2 = null,
				expr3,
				expr4 = null,
				expr5 = null;

			var ident = C.Expect(IdentChars.Contains)
				.Repeat1()
				.Map(chars => new string(chars.ToArray()))
				.AndIgnore(Whitespace);

			var variableExpr = ident.Map(s => (Rt.IReferentialExpression) new Rt.VariableExpression { Name = s });
			
			var indexExpr =
				C.Delay(() => expr5)
					.Map(e => (Rt.IReferentialExpression) new Rt.IndexExpression {Index = e})
					.SurroundedBy(
						C.Expect("[").And(Whitespace),
						C.Expect("]").And(Whitespace));

			var referentialExpr = indexExpr.Or(variableExpr);

			var literalExpr =
				C.Expect(char.IsDigit)
					.Repeat1()
					.Map(chars =>
					{
						var s = new string(chars.ToArray());
						return (Rt.IExpression) new Rt.LiteralExpression
						{
							Value = int.TryParse(s, out var r) ? r : 0
						};
					}).AndIgnore(Whitespace);

			var parenExpr =
				C.Delay(() => expr5)
					.SurroundedBy(
						C.Expect("(").And(Whitespace),
						C.Expect(")").And(Whitespace));
			
			// Operator precedence:
			// 0:   [] x () | - ! 8
			// 1:   * / %
			// 2:   + -
			// 3:   > < ==
			// 4:   &&
			// 5:   ||
			
			expr0 = referentialExpr.Map(e => (Rt.IExpression) e)
				.Or(Unary("|", Rt.UnaryOperator.Abs, C.Delay(() => expr0)))
				.Or(Unary("-", Rt.UnaryOperator.Minus, C.Delay(() => expr0)))
				.Or(Unary("!", Rt.UnaryOperator.Not, C.Delay(() => expr0)))
				.Or(literalExpr)
				.Or(parenExpr);

			expr1 = expr0
				.Or(Binary("*", Rt.BinaryOperator.Mul, expr0, C.Delay(() => expr1)))
				.Or(Binary("/", Rt.BinaryOperator.Div, expr0, C.Delay(() => expr1)))
				.Or(Binary("%", Rt.BinaryOperator.Mod, expr0, C.Delay(() => expr1)));

			expr2 = expr1
				.Or(Binary("+", Rt.BinaryOperator.Add, expr1, C.Delay(() => expr2)))
				.Or(Binary("-", Rt.BinaryOperator.Sub, expr1, C.Delay(() => expr2)));
			
			// Don't permit chained comparisons like `a == b == c`, since they probably
			// don't do what the user expects them to.
			expr3 = expr2
				.Or(Binary(">", Rt.BinaryOperator.Gt, expr2, expr2))
				.Or(Binary("<", Rt.BinaryOperator.Lt, expr2, expr2))
				.Or(Binary("==", Rt.BinaryOperator.Eq, expr2, expr2));

			expr4 = expr3.Or(Binary(",", Rt.BinaryOperator.And, expr3, C.Delay(() => expr4)));
			
			expr5 = expr4.Or(Binary("|", Rt.BinaryOperator.And, expr4, C.Delay(() => expr5)));

			var assignmentInstruction = referentialExpr
				.AndIgnore(C.Expect("=").And(Whitespace))
				.And(expr5)
				.Map(v => (Rt.IInstruction) new Rt.AssignmentInstruction
				{
					Source = v.Item2,
					Destination = v.Item1,
				});

			var eventInstruction = ident.Map(s => (Rt.IInstruction) new Rt.EventInstruction {EventName = s});

			var instruction = assignmentInstruction.Or(eventInstruction);

			var conditional = expr5
				.SurroundedBy(
					C.Expect("(").And(Whitespace),
					C.Expect(")").And(Whitespace))
				.Or(C.Pure<Rt.IExpression>(null));

			var conditionalInstruction = Whitespace
				.IgnoreAnd(conditional)
				.And(instruction)
				.AndIgnore(C.Eof())
				.Map(v => new Rt.ConditionalInstruction
				{
					Condition = v.Item1,
					Instruction = v.Item2,
				});

			var eventDeclaration = C.Expect(":")
				.And(Whitespace)
				.IgnoreAnd(ident)
				.AndIgnore(C.Eof());

			var programLines = code.Split("\n", false);

			var lineNumber = 0;

			var done = false;
			var events = new Dictionary<string, List<Runtime.ConditionalInstruction>>();
			
			while (!done)
			{
				var line = programLines[lineNumber];
				GD.Print("line is " + line);
				var nameCases = eventDeclaration.Parse(new ParseState(line, 0));
				if (nameCases.Count != 1)
				{
					throw new ParseError($"Line {lineNumber + 1} is invalid.");
				}

				var eventName = nameCases[0].Item1;
				var eventBody = new List<Rt.ConditionalInstruction>();

				while (true)
				{
					lineNumber++;
					if (lineNumber >= programLines.Length)
					{
						done = true;
						break;
					}
					var insLine = programLines[lineNumber];
					if (insLine == null)
					{
						done = true;
						break;
					}
					GD.Print("insline is " + insLine);
					var insCases = conditionalInstruction.Parse(new ParseState(insLine, 0));
					if (insCases.Count != 1)
					{
						break;
					}

					eventBody.Add(insCases[0].Item1);
				}
				
				events.Add(eventName, eventBody);
			}
			
			return events;
		}
	}
}
