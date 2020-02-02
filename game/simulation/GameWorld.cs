using System.Collections.Generic;
using Godot;
using Refactorio.game.scripting;
using Refactorio.game.simulation.objects;
using Refactorio.helpers;

namespace Refactorio.game.simulation
{
	public class GameWorld : Node
	{
		// Signals
		[Signal]
		public delegate void SimulationStopped();
		
		// Game properties
		public readonly GridController<BaseObject> GridController = new GridController<BaseObject>(new Vector3(0, 0, 0), 3);
		private int _score;
		private int _machineCount;
		private float _animatedScore;
		
		// Machine construction properties
		public Dictionary<string, List<Runtime.ConditionalInstruction>> MachineTemplateProgram;
		private PackedScene _machineScene;

		// Getters
		public Node GameObjectsContainer => GetNode("GameObjects");

		// Score methods
		public void IncreaseScore(int points)
		{
			_score += points;
		}

		public void OnNewMachine()
		{
			_machineCount++;
		}
		
		public void OnMachineDied()
		{
			_machineCount--;
		}
		
		// Interaction API
		public void SpawnMachine(Vector2 position)
		{
			if (GridController.GetObjectAt(position) != null)
			{
				GD.PushWarning("Failed to spawn machine at " + position + ".");
				return;
			}

			var machine = (Machine) _machineScene.Instance();
			machine.ScriptingRuntime = new Runtime(MachineTemplateProgram);
			GameObjectsContainer.AddChild(machine);
		}

		// Event handlers
		public override void _Ready()
		{
			// Spawn machine
			// It is important to spawn the machine before the crystals so its spawn doesn't get obstructed
			_machineScene = GD.Load<PackedScene>(Constants.PathToMachineScene);
			SpawnMachine(Vector2.Zero);
			
			// Spawn crystals
			var matterCrystalScene = GD.Load<PackedScene>(Constants.PathToCrystalScene);
			for (var x = 0; x < 1000; x++)
			{
				var gridPos = new Vector2();
				var firstIteration = true;
				while (firstIteration || GridController.GetObjectAt(gridPos) != null)
				{
					gridPos = new Vector2(MathUtils.RandInt(-100, 100), MathUtils.RandInt(-100, 100));
					firstIteration = false;
				}

				var crystal = (MatterCrystal)matterCrystalScene.Instance();
				crystal.Translation = GridController.GridToRealPos(gridPos);
				GameObjectsContainer.AddChild(crystal);
			}
		}

		public override void _Process(float delta)
		{
			var hudContainerRoot = GetNode("PlayUi/HUD/Container");

			var weight = delta * 20f;
			_animatedScore = (_animatedScore + _score * weight) / (1 + weight);
			hudContainerRoot.GetNode<Label>("Score").Text = "Score:\n" + Mathf.RoundToInt(_animatedScore);
			hudContainerRoot.GetNode<Label>("Population").Text = "Population:\n" + _machineCount;
		}

		public void OnStopSimulationPressed()
		{
			EmitSignal(nameof(SimulationStopped));
		}
	}
}
