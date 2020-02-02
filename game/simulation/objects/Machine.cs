using System;
using Godot;
using Refactorio.helpers;

namespace Refactorio.game.simulation.objects
{
	public class Machine : BaseObject
	{
		// Machine properties
		private int _energy;
		private float _currentActionTimer;
		private Vector3 _lerpedTargetPos;
		
		// Action methods
		private void GrantEnergy(int amount)
		{
			_energy += amount;
			GameWorld.IncreaseScore(amount);
		}

		// Event handlers
		public override void _Ready()
		{
			GameWorld.OnNewMachine();
			RegisterGridPresence();
			_lerpedTargetPos = GridDisplayPos;
		}

		public override void _ExitTree()
		{
			GameWorld.OnMachineDied();
			UnregisterGridPresence();
		}

		public override void _Process(float delta)
		{
			// Process AI
			if (_currentActionTimer - delta <= 0)
			{
				// TODO: Sometimes, robots move out of the way in the same tick your move gets cancelled. Fix this.
				Move(MathUtils.RandDir(), out var hitNode);
				if (hitNode is MatterCrystal crystal)
				{
					GrantEnergy(crystal.Damage(4));
					_currentActionTimer = 0.5f;
				}
				else
				{
					_currentActionTimer = 0.2f;
				}
			}
			else
			{
				_currentActionTimer = Math.Max(_currentActionTimer - delta, 0);
			}
			
			// Movement rendering
			{
				var moveLerpWeight = delta * 10f;
				Translation = (Translation + GridDisplayPos * moveLerpWeight) / (1 + moveLerpWeight);
				
				var latentTargetLerpWeight = delta * 5f;
				_lerpedTargetPos = (_lerpedTargetPos + GridDisplayPos * latentTargetLerpWeight) / (1 + latentTargetLerpWeight);
				if (_lerpedTargetPos.DistanceSquaredTo(Translation) > 0.1) LookAt(_lerpedTargetPos, Vector3.Up);
			}
		}
	}
}
