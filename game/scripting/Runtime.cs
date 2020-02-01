using System.Linq;
using Godot;
using Godot.Collections;
using Refactorio.helpers;

namespace Refactorio.game.scripting
{
    public class Runtime
    {
        private interface Expression
        {
            int Evaluate(Runtime runtime);
        }
        private interface IReferentialExpression : Expression
        {
            void Assign(Runtime runtime, int value);
        }
        private interface IInstruction
        {
            void Execute(Runtime runtime);
        }
        private class AssignmentInstruction : IInstruction
        {
            private Expression source;
            private IReferentialExpression destination;

            public void Execute(Runtime runtime)
            {
                destination.Assign(runtime, source.Evaluate(runtime));
            }
        }

        private class EventInstruction : IInstruction
        {
            private string eventName;
            public void Execute(Runtime runtime)
            {
                
            }
        }
        private class Condition
        {
            private bool checksZero;
            private string variable;

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
        private class ConditionalInstruction
        {
            private Array<Condition> conditions;
            private IInstruction instruction;

            public void Execute(Runtime runtime)
            {
                if (conditions.All(condition => condition.IsMet(runtime)))
                {
                    instruction.Execute(runtime);
                }
            }
        }

        private Dictionary<string, int> variables;
        private Dictionary<int, int> memory;
        private Dictionary<string, Array<ConditionalInstruction>> events;
        private Array<string> callStack;

        private int GetVariable(string name)
        {
            return DictUtils.GetFromDict(variables, name, 0);
        }

        public void RunEvent(string eventName)
        {
            if (!events.TryGetValue(eventName, out var body)) return;
            callStack.Add(eventName);
            foreach (var instruction in body)
            {
                instruction.Execute(this);
            }
        }
    }
}