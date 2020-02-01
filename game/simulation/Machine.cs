using Godot;
using Refactorio.helpers;

namespace Refactorio.game.simulation
{
	public class Machine : Spatial, IGridSpatial
	{
		// Machine properties
		public uint MachineId;
		
		// Grid properties
		public Vector3 GridDisplayPos { set; get; }
		public Vector2 GridPos { set; get; }
		

		// Event handlers
		public override void _Ready()
		{
			var world = GetGameWorld();
			MachineId = world.RequestMachineId();

			// Track it!
			GridDisplayPos = Translation;
			world.GridController.AddObject(this);
			world.LogMachineMessage(this, "Hello world!");
		}

		public override void _ExitTree()
		{
			var world = GetGameWorld(); 
			world.LogMachineMessage(this, "Goodbye world!");
			world.GridController.RemoveObject(this);
		}

		public override void _Process(float delta)
		{
			var moveLerpWeight = delta * 10f;
			Translation = (Translation + GridDisplayPos * moveLerpWeight) / (1 + moveLerpWeight);
			Move(new Vector2(MathUtils.RandSign(), MathUtils.RandSign()));
			LookAt(GridDisplayPos, Vector3.Up);
		}
		
		// Util methods
		private GameWorld GetGameWorld()
		{
			return GetParent<GameWorld>();
		}
		
		private bool Move(Vector2 relative)
		{
			return GetGameWorld().GridController.MoveObject(this, relative);
		}
	}
}
