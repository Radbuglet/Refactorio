using Godot;
using Godot.Collections;
using Refactorio.helpers;

namespace Refactorio.game.simulation
{
    public class GridController<TContainedNode> where TContainedNode : Object, IGridSpatial
    {
        // Properties
        private readonly Vector3 _offset;
        private readonly uint _scale;
        private readonly Dictionary<Vector2, TContainedNode> _posToObjMap = new Dictionary<Vector2, TContainedNode>();

        public GridController(Vector3 offset, uint scale)
        {
            _offset = offset;
            _scale = scale;
        }
        
        // Rendering API
        public Vector2 RealToGridPos(Vector3 pos)
        {
            return (MathUtils.Vec2FromXz(pos - _offset) / _scale).Floor();
        }
        
        public Vector3 GridToRealPos(Vector2 grid)
        {
            return new Vector3(grid.x, 0, grid.y) * _scale + _offset;
        }

        private Vector3 GridDeltaToRealDelta(Vector2 delta)
        {
            return new Vector3(delta.x, 0, delta.y) * _scale;
        }
        
        // Containment API
        public void AddObject(TContainedNode obj)
        {
            var gridPos = RealToGridPos(obj.GridDisplayPos);
            obj.GridDisplayPos = GridToRealPos(gridPos);
            obj.GridPos = gridPos;
            _posToObjMap.Add(obj.GridPos, obj);
        }
        
        public void RemoveObject(TContainedNode obj)
        {
            _posToObjMap.Remove(obj.GridPos);
        }

        public bool MoveObject(TContainedNode obj, Vector2 relative, out TContainedNode hitNode)
        {
            hitNode = GetObjectAt(obj.GridPos + relative);
            if (hitNode != null) return false;
            
            _posToObjMap.Remove(obj.GridPos);
            obj.GridDisplayPos += GridDeltaToRealDelta(relative);
            obj.GridPos += relative;
            _posToObjMap.Add(obj.GridPos, obj);
            return true;
        }

        public bool MoveObject(TContainedNode obj, Vector2 relative)
        {
            return MoveObject(obj, relative, out _);
        }

        public TContainedNode GetObjectAt(Vector2 pos)
        {
            return DictUtils.GetFromDict(_posToObjMap, pos, null);
        }
    }

    public interface IGridSpatial
    {
        Vector3 GridDisplayPos { set; get; }
        Vector2 GridPos { set; get; }
    }
}