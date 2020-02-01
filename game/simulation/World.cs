using Godot;

namespace Refactorio.game.simulation
{
    public class SimulationController : Node
    {
        // Properties
        private bool _isSimulating = false;
        
        // Signals
        [Signal]
        public delegate void OnSimulationTick();
        
        // Methods
        public void Log()
        {
            
        }
    }
}