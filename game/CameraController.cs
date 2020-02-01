using Godot;

namespace Refactorio.game
{
    public class CameraController : Camera
    {
        private bool _isDragging;

        public override void _Process(float delta)
        {
            var isMouseDown = Input.IsMouseButtonPressed(2);
            if (isMouseDown == _isDragging) return;
            Input.SetMouseMode(isMouseDown ? Input.MouseMode.Captured : Input.MouseMode.Visible);
            _isDragging = isMouseDown;
        }

        public override void _Input(InputEvent @event)
        {
            if (!(@event is InputEventMouseMotion eventMotion) || !_isDragging) return;
            var relative = eventMotion.Relative;
            
            Translation += new Vector3(relative.x, 0, relative.y) * -0.1f;
        }
    }
}