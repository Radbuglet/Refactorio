using System.Collections.Generic;
using Godot;
using Refactorio.game.scripting;
using Refactorio.game.simulation;
using Refactorio.helpers;

namespace Refactorio.game
 {
	 public class GameController : Node
	 {
		 // Properties
		 public enum GameState
		 {
			 Scripting,
			 Simulating
		 }
		 public GameState State = GameState.Scripting;
		 private Control CodingUi => GetNode<Control>("CodingUi");
		 
		 private const string SimulationNodeName = "SimulationWorld"; 
		 private GameWorld SimulationWorld => GetNode<GameWorld>(SimulationNodeName);
		 private PackedScene _simulationWorldScene;
		 
		 // Simulation methods
		 public void StartSimulation()
		 {
			 // Parse code
			 Dictionary<string, List<Runtime.ConditionalInstruction>> parseResult; 
			 try
			 {
				 var code = GetNode<TextEdit>("./CodingUi/Editor/TextEdit").Text;
				 parseResult = Parser.Parse(code);
			 }
			 catch (Parser.ParseError e)
			 {
				 OS.Alert(e.Message, "Parse error");
				 return;
			 }
			 
			 // Spawn simulation
			 var simulationRoot = (GameWorld) _simulationWorldScene.Instance();
			 simulationRoot.Name = SimulationNodeName;
			 simulationRoot.MachineTemplateProgram = parseResult;
			 simulationRoot.Connect(nameof(GameWorld.SimulationStopped), this, nameof(StopSimulation));
			 AddChild(simulationRoot);
			 
			 // Set states
			 State = GameState.Simulating;
			 CodingUi.Visible = false;
		 }
		 
		 public void StopSimulation()
		 {
			 // Undo potential global changes
			 GetTree().Paused = false;
			 Input.SetMouseMode(Input.MouseMode.Visible);
			 
			 // Delete simulation
			 var world = SimulationWorld;
			 RemoveChild(world);
			 world.QueueFree();
			 
			 // Set states
			 State = GameState.Scripting;
			 CodingUi.Visible = true;
		 }

		 // Event Handlers
		 public override void _Ready()
		 {
			 _simulationWorldScene = GD.Load<PackedScene>(Constants.PathToSimulationWorldScene);
		 }
	 }
 }
