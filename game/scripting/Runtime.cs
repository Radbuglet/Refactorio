using System;
using System.Linq;
using Godot.Collections;
using Refactorio.helpers;

namespace Refactorio.game.scripting
{
    public class Runtime
    {
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
            public int value;

            public int Evaluate(Runtime runtime)
            {
                return value;
            }
        }

        public class IndexExpression : IReferentialExpression
        {
            public IExpression index;
            public int Evaluate(Runtime runtime)
            {
                var idx = index.Evaluate(runtime);
                return runtime.GetMemory(idx);
            }

            public void Assign(Runtime runtime, int value)
            {
                var idx = index.Evaluate(runtime);
                runtime.SetMemory(idx, value);
            }
        }

        public class VariableExpression : IReferentialExpression
        {
            public string name;
            
            public int Evaluate(Runtime runtime)
            {
                return runtime.GetVariable(name);
            }

            public void Assign(Runtime runtime, int value)
            {
                runtime.SetVariable(name, value);
            }
        }

        public enum UnaryOperator
        {
            Abs,
            Minus,
            Not,
        }

        public class UnaryExpression : IExpression
        {
            public IExpression inner;
            public UnaryOperator op;

            public int Evaluate(Runtime runtime)
            {
                var a = inner.Evaluate(runtime);
                switch (op)
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

        public enum BinaryOperator
        {
            Plus,
            Minus,
            Times,
            Div,
            Mod,
            Eq,
            And,
            Or,
        }

        public class BinaryExpression : IExpression
        {
            public IExpression left;
            public IExpression right;
            public BinaryOperator op;

            public int Evaluate(Runtime runtime)
            {
                var a = left.Evaluate(runtime);
                var b = right.Evaluate(runtime);
                switch (op)
                {
                    case BinaryOperator.Plus:
                        return a + b;
                    case BinaryOperator.Minus:
                        return a - b;
                    case BinaryOperator.Times:
                        return a * b;
                    case BinaryOperator.Div:
                        return b == 0 ? 0 : a / b;
                    case BinaryOperator.Mod:
                        return b == 0 ? 0 : a % b;
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
            private IExpression source;
            private IReferentialExpression destination;

            public void Execute(Runtime runtime)
            {
                destination.Assign(runtime, source.Evaluate(runtime));
            }
        }

        public class EventInstruction : IInstruction
        {
            public string eventName;
            public void Execute(Runtime runtime)
            {
                runtime.RunEvent(eventName);
            }
        }
        public class Condition
        {
            public bool checksZero;
            public string variable;

            public bool IsMet(Runtime runtime)
            {
                if (checksZero)
                {
                    return runtime.GetVariable(variable) == 0;
                }
                else
                {
                    return runtime.GetVariable(variable) != 0;
                }
            }
        }
        public class ConditionalInstruction
        {
            public Array<Condition> conditions;
            public IInstruction instruction;

            public void Execute(Runtime runtime)
            {
                if (conditions.All(condition => condition.IsMet(runtime)))
                {
                    instruction.Execute(runtime);
                }
            }
        }
        
        public Dictionary<string, int> variables;
        public Dictionary<int, int> memory;
        public Dictionary<string, Array<ConditionalInstruction>> events;
        public Dictionary<string, bool> flags;
        public Array<string> callStack;

        private int GetVariable(string name)
        {
            return DictUtils.GetFromDict(variables, name, 0);
        }

        private void SetVariable(string name, int value)
        {
            variables.Add(name, value);
        }

        private int GetMemory(int index)
        {
            return DictUtils.GetFromDict(memory, index, 0);
        }

        private void SetMemory(int index, int value)
        {
            memory.Add(index, value);
        }

        public void RunEvent(string eventName)
        {
            if (flags.ContainsKey(eventName))
            {
                flags.Add(eventName, true);
                return;
            }
            if (!events.TryGetValue(eventName, out var body)) return;
            // Don't permit recursion by forbidding calling events that are already
            // present in the call stack.
            if (callStack.Contains(eventName)) return;
            callStack.Add(eventName);
            foreach (var instruction in body)
            {
                instruction.Execute(this);
            }
            callStack.Resize(callStack.Count - 1);
        }
    }
}