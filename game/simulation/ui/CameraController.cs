using Godot;

namespace Refactorio.game.simulation.ui
{
    public class CameraController : Spatial
    {
        private bool _isDragging;
        private float _zoom = 30;

        private void UpdateZoom()
        {
            GetNode<Spatial>("./Camera").Translation = new Vector3(0, _zoom, -_zoom);
        }

        public override void _Ready()
        {
            UpdateZoom();
        }

        public override void _Process(float delta)
        {
            var isMouseDown = Input.IsMouseButtonPressed((int) ButtonList.Right);
            if (isMouseDown == _isDragging) return;
            Input.SetMouseMode(isMouseDown ? Input.MouseMode.Captured : Input.MouseMode.Visible);
            _isDragging = isMouseDown;
        }

        public override void _Input(InputEvent @event)
        {
            switch (@event)
            {
                case InputEventMouseMotion eventMotion when _isDragging:
                {
                    var relative = eventMotion.Relative;
                    Translation += new Vector3(relative.x, 0, relative.y) * 0.1f;  // TODO: Adaptive move speed (scroll doesn't work on some machines)
                    break;
                }
                case InputEventPanGesture eventPan:
                {
                    _zoom = Mathf.Clamp(_zoom + eventPan.Delta.y, 0, 100f);
                    UpdateZoom();
                    break;
                }
            }
        }
    }
}