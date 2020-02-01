using Godot;

namespace Refactorio.game.simulation
{
    public abstract class MachineController : Spatial
    {
        private Vector3 _targetRenderPos;

        public override void _Ready()
        {
            _targetRenderPos = Translation;
        }

        public override void _Process(float delta)
        {
            var moveLerpWeight = delta * 10f;
            Translation = (Translation + (_targetRenderPos * moveLerpWeight)) / (1 + moveLerpWeight);
        }

        protected void Move(Vector2 relative)
        {
            _targetRenderPos += new Vector3(relative.x, 0, relative.y) * 2;
        }
    }
}