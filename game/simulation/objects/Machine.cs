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
		private Dictionary<string, bool> _movement_hooks = new Dictionary<string, bool>();
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
			_lerpedTargetPos = GridDisplayPos;

			var scriptFile = new File();
			scriptFile.Open("assets/script.txt", File.ModeFlags.Read);
			ScriptingRuntime = new Runtime(Parser.Parse(scriptFile.GetAsText()));
			scriptFile.Close();
			
			ScriptingRuntime.RunEvent("init");
			
			GD.Print("Is this even running?");
			
			// Register event hooks.
			ScriptingRuntime.RegisterHook("up", () => { Move(Vector2.Up); });
			ScriptingRuntime.RegisterHook("down", () => { Move(Vector2.Down); });
			ScriptingRuntime.RegisterHook("left", () => { Move(Vector2.Left); });
			ScriptingRuntime.RegisterHook("right", () => { Move(Vector2.Right); });
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
				_currentActionTimer -= delta;
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
