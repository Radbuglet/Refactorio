using System;
using System.Collections.Generic;
using Godot;
using Refactorio.helpers;

namespace Refactorio.game.scripting
{
	public class Runtime
	{
		public enum BinaryOperator
		{
			Add,
			Sub,
			Mul,
			Div,
			Mod,
			Gt,
			Lt,
			Eq,
			And,
			Or
		}

		public enum UnaryOperator
		{
			Abs,
			Minus,
			Not
		}

		public interface IExpression
		{
			int Evaluate(Runtime runtime);
		}

		public interface IReferentialExpression : IExpression
		{
			void Assign(Runtime runtime, int value);
		}

		public class LiteralExpression : IExpression
		{
			public int Value;

			public int Evaluate(Runtime runtime)
			{
				return Value;
			}
		}

		public class IndexExpression : IReferentialExpression
		{
			public IExpression Index;

			public int Evaluate(Runtime runtime)
			{
				var idx = Index.Evaluate(runtime);
				return runtime.GetMemory(idx);
			}

			public void Assign(Runtime runtime, int value)
			{
				var idx = Index.Evaluate(runtime);
				runtime.SetMemory(idx, value);
			}
		}

		public class VariableExpression : IReferentialExpression
		{
			public string Name;

			public int Evaluate(Runtime runtime)
			{
				return runtime.GetVariable(Name);
			}

			public void Assign(Runtime runtime, int value)
			{
				runtime.SetVariable(Name, value);
			}
		}

		public class UnaryExpression : IExpression
		{
			public IExpression Inner;
			public UnaryOperator Op;

			public int Evaluate(Runtime runtime)
			{
				var a = Inner.Evaluate(runtime);
				switch (Op)
				{
					case UnaryOperator.Abs:
						return Math.Abs(a);
					case UnaryOperator.Minus:
						return -a;
					case UnaryOperator.Not:
						return a == 0 ? 1 : 0;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public class BinaryExpression : IExpression
		{
			public IExpression Left;
			public BinaryOperator Op;
			public IExpression Right;

			public int Evaluate(Runtime runtime)
			{
				var a = Left.Evaluate(runtime);
				var b = Right.Evaluate(runtime);
				switch (Op)
				{
					case BinaryOperator.Add:
						return a + b;
					case BinaryOperator.Sub:
						return a - b;
					case BinaryOperator.Mul:
						return a * b;
					case BinaryOperator.Div:
						return b == 0 ? 0 : a / b;
					case BinaryOperator.Mod:
						return b == 0 ? 0 : a % b;
					case BinaryOperator.Gt:
						return a > b ? 1 : 0;
					case BinaryOperator.Lt:
						return a < b ? 1 : 0;
					case BinaryOperator.Eq:
						return a == b ? 1 : 0;
					case BinaryOperator.And:
						return a != 0 && b != 0 ? 1 : 0;
					case BinaryOperator.Or:
						return a != 0 || b != 0 ? 1 : 0;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public interface IInstruction
		{
			void Execute(Runtime runtime);
		}

		public class AssignmentInstruction : IInstruction
		{
			public IReferentialExpression Destination;
			public IExpression Source;

			public void Execute(Runtime runtime)
			{
				Destination.Assign(runtime, Source.Evaluate(runtime));
			}
		}

		public class EventInstruction : IInstruction
		{
			public string EventName;

			public void Execute(Runtime runtime)
			{
				runtime.RunEvent(EventName);
			}
		}

		public class ConditionalInstruction
		{
			public IExpression Condition;
			public IInstruction Instruction;

			public void Execute(Runtime runtime)
			{
				if (Condition == null || Condition.Evaluate(runtime) != 0) Instruction.Execute(runtime);
			}
		}
		
		private readonly List<string> _callStack = new List<string>();
		private readonly Dictionary<string, List<ConditionalInstruction>> _events;
		private readonly Dictionary<int, int> _memory = new Dictionary<int, int>();
		private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
		private readonly Dictionary<string, Action> _hooks = new Dictionary<string, Action>();

		public Runtime(Dictionary<string, List<ConditionalInstruction>> events)
		{
			_events = events;
		}

		public void RegisterHook(string name, Action behavior)
		{
			_hooks.Add(name, behavior);
			GD.Print("Registering hook " + name);
		}

		public int GetVariable(string name)
		{
			return DictUtils.GetFromDict(_variables, name, 0);
		}

		public void SetVariable(string name, int value)
		{
			_variables.Update(name, value);
		}

		private int GetMemory(int index)
		{
			return DictUtils.GetFromDict(_memory, index, 0);
		}

		private void SetMemory(int index, int value)
		{
			_memory.Update(index, value);
		}

		public void RunEvent(string eventName)
		{

			if (_hooks.TryGetValue(eventName, out var behavior))
			{
				GD.Print("Executing hook " + eventName);
				behavior();
				return;
			}
			if (!_events.TryGetValue(eventName, out var body)) return;
			// Don't permit recursion by forbidding calling events that are already
			// present in the call stack.
			if (_callStack.Contains(eventName)) return;
			_callStack.Add(eventName);
			foreach (var instruction in body) instruction.Execute(this);
			_callStack.RemoveAt(_callStack.Count - 1);
		}
	}
}
