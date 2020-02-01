using Godot;

namespace Refactorio.game.simulation
{
    public class GameWorld : Node
    {
        // Machine tracking properties
        private uint _machineIdCounter;
        public readonly GridController<Machine> GridController = new GridController<Machine>(new Vector3(1, 2, 1), 2);

        // Machine registry methods
        public void LogMachineMessage(Machine machine, string message)
        {
            GD.Print("[Machine " + machine.MachineId + "] " + message); // TODO: In game logging
        }

        public uint RequestMachineId()
        {
            var newId = _machineIdCounter;
            _machineIdCounter++;
            return newId;
        }
    }
}