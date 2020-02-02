using Godot;

namespace Refactorio.game.simulation.objects
{
	public abstract class BaseObject : Spatial, IGridSpatial
	{
		// Properties
		public Vector3 GridDisplayPos { set; get; }
		public Vector2 GridPos { set; get; }

		// Utils
		protected GameWorld GameWorld => GetNode<GameWorld>("../../");

		protected virtual bool Move(Vector2 relative, out BaseObject hitNode)
		{
			var couldMove = GameWorld.GridController.MoveObject(this, relative, out var theHitNode);
			hitNode = theHitNode;
			return couldMove;
		}

		// Event handlers
		protected void RegisterGridPresence()
		{
			GridDisplayPos = Translation;
			GameWorld.GridController.AddObject(this);
		}

		protected void UnregisterGridPresence()
		{
			GameWorld.GridController.RemoveObject(this);
		}
	}
}
