using Godot;
using Godot.Collections;
using Refactorio.game.scripting;
using Refactorio.helpers;

namespace Refactorio.game.simulation.objects
{
	public class Machine : BaseObject
	{
		// Properties
		private float _currentActionTimer;
		private Vector3 _lerpedTargetPos;
		public Runtime ScriptingRuntime;
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
			_lastMoveAngle = new Vector2(relative.x, -relative.y).Angle();
			return base.Move(relative, out hitNode);
		}

		// Event handlers
		public override void _Ready()
		{
			// Setup machine in game engine
			GameWorld.OnNewMachine();
			RegisterGridPresence();
			_lerpedTargetPos = GridDisplayPos;
			
			// Setup scripting runtime
			ScriptingRuntime.RunEvent("init");
			ScriptingRuntime.RegisterHook("up", () => { Move(Vector2.Down); });
			ScriptingRuntime.RegisterHook("down", () => { Move(Vector2.Up); });
			ScriptingRuntime.RegisterHook("left", () => { Move(Vector2.Right); });
			ScriptingRuntime.RegisterHook("right", () => { Move(Vector2.Left); });
			ScriptingRuntime.RegisterHook("ping", () => { GD.Print("pong; a = " + ScriptingRuntime.GetVariable("a")); });
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
				ScriptingRuntime.RunEvent("tick");
				_currentActionTimer = 0.2f;
			}
			else
			{
				_currentActionTimer -= delta;
			}
			
			// Movement rendering
			{
				// Animate position
				var moveLerpWeight = delta * 10f;
				Translation = (Translation + GridDisplayPos * moveLerpWeight) / (1 + moveLerpWeight);
				
				// Animate rotation
				var rotLerpWeight = delta * 20f;
				_lerpedRotationAnim = (_lerpedRotationAnim + _lastMoveAngle * rotLerpWeight) / (1 + rotLerpWeight);
				var newRotation = Rotation;
				newRotation.y = _lerpedRotationAnim;
				Rotation = newRotation;
			}
		}
	}
}
