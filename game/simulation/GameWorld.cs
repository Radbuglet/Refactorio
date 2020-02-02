using Godot;
using Refactorio.game.simulation.objects;
using Refactorio.helpers;

namespace Refactorio.game.simulation
{
	public class GameWorld : Node
	{
		// Properties
		public readonly GridController<BaseObject> GridController = new GridController<BaseObject>(new Vector3(2, 0, 2), 4);
		private uint _score;
		private float _animatedScore;
		private uint _machineIdCounter;

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
		
		// Score methods
		public void IncreaseScore(uint points)
		{
			_score += points;
		}
		
		// Event handlers
		public override void _Process(float delta)
		{
			var hudContainerRoot = GetNode("PlayUi/HUD/Container");

			var weight = delta * 20f;
			_animatedScore = (_animatedScore + _score * weight) / (1 + weight);
			hudContainerRoot.GetNode<Label>("Score").Text = "Score:\n" + Mathf.Floor(_animatedScore);
		}
	}
}
