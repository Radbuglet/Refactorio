using Godot;

namespace Refactorio.game
{
    public class CameraController : Spatial
    {
        private bool _isDragging;
        private int _zoom;

        public override void _Process(float delta)
        {
            var isMouseDown = Input.IsMouseButtonPressed((int) ButtonList.Left);
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
                    Translation += new Vector3(relative.x, 0, relative.y) * 0.1f;
                    break;
                }
                case InputEventMouseButton eventClick:
                {
                    int relative;
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
                    _zoom += relative;
                    GetNode<Spatial>("./Camera").Translation = new Vector3(0, _zoom, _zoom);
                    break;
                }
            }
        }
    }
}