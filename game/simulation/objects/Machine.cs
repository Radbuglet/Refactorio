using System;
using Godot;
using Refactorio.helpers;

namespace Refactorio.game.simulation.objects
{
	public class Machine : BaseObject
	{
		// Machine properties
		public uint MachineId;
		private uint _energy;
		private float _currentActionTimer;
		
		// Action methods
		public void GrantEnergy(uint amount)  // TODO: indicator
		{
			_energy += amount;
			GameWorld.IncreaseScore(amount);
		}

		// Event handlers
		public override void _Ready()
		{
			RegisterGridPresence();
			MachineId = GameWorld.RequestMachineId();
			GameWorld.LogMachineMessage(this, "Hello world!");
		}

		public override void _ExitTree()
		{
			UnregisterGridPresence();
			GameWorld.LogMachineMessage(this, "Goodbye world!");
		}

		public override void _Process(float delta)
		{
			// Process AI
			if (_currentActionTimer - delta <= 0)
			{  // Perform next action
				Move(new Vector2(MathUtils.RandSign(), MathUtils.RandSign()));
				_currentActionTimer = 0.2f;
				GameWorld.IncreaseScore((uint) MathUtils.RandInt(0, 20));
			}
			else
			{  // Decrement timer
				_currentActionTimer = Math.Max(_currentActionTimer - delta, 0);
			}
			
			// Movement rendering
			{
				var moveLerpWeight = delta * 10f;
				Translation = (Translation + GridDisplayPos * moveLerpWeight) / (1 + moveLerpWeight);
				//if (GridDisplayPos.DistanceSquaredTo(Translation) > 0.1) LookAt(GridDisplayPos, Vector3.Up);  TODO	
			}
		}
	}
}
