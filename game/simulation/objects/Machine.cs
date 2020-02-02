using Godot;
using Godot.Collections;
using Refactorio.game.scripting;

namespace Refactorio.game.simulation.objects
{
	public class Machine : BaseObject
	{
		// Machine properties
		private int _energy;
		private float _currentActionTimer;
		private Vector3 _lerpedTargetPos;
		public Runtime ScriptingRuntime;
		private Dictionary<string, bool> _movement_hooks = new Dictionary<string, bool>();
		
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
			}
			else
			{
				_currentActionTimer -= delta;
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
