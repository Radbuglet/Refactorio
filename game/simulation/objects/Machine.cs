using System;
using Godot;
using Refactorio.helpers;

namespace Refactorio.game.simulation.objects
{
	public class Machine : BaseObject
	{
		// Properties
		private float _currentActionTimer;
		private float _lastMoveAngle;
		private float _lerpedRotationAnim;
		
		// Machine stat properties
		private int _energy;
		
		// Utility methods
		private void GrantEnergy(int amount)
		{
			_energy += amount;
			GameWorld.IncreaseScore(amount);
		}

		protected override bool Move(Vector2 relative, out BaseObject hitNode)
		{
			_lastMoveAngle = relative.Angle();
			return base.Move(relative, out hitNode);
		}

		// Event handlers
		public override void _Ready()
		{
			GameWorld.OnNewMachine();
			RegisterGridPresence();
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
				
				var rotLerpWeight = delta * 5f;
				_lerpedRotationAnim = (_lerpedRotationAnim + _lastMoveAngle * rotLerpWeight) / (1 + rotLerpWeight);
				// Transform = Transform.Rotated(Vector3.Up, _lerpedRotationAnim);  TODO
			}
		}
	}
}
