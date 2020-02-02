using Godot;

namespace Refactorio.game
{
    public class CameraController : Spatial
    {
        private bool _isDragging;
        private float _zoom = 10;

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
                    Translation += new Vector3(relative.x, 0, relative.y) * 0.1f;  // TODO: Adaptive move speed
                    break;
                }
                case InputEventMouseButton eventClick:
                {
                    float relative;
                    switch (eventClick.ButtonIndex)
                    {
                        case (int) ButtonList.WheelDown:
                            relative = 1;
                            break;
                        case (int) ButtonList.WheelUp:
                            relative = -1;
                            break;
                        default:
                            return;
                    }
                    _zoom = Mathf.Clamp(_zoom + relative, 0, 100f);
                    UpdateZoom();
                    break;
                }
            }
        }
    }
}