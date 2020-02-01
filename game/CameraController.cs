using Godot;
using Refactorio.helpers;

namespace Refactorio.game
{
    public class CameraController : Camera
    {
        public override void _Process(float delta)
        {
            var direction = new Vector3();
            if (Input.IsActionPressed(ActionNames.MoveCameraLeftAction)) direction.x += -1;
            if (Input.IsActionPressed(ActionNames.MoveCameraRightAction)) direction.x += 1;
            if (Input.IsActionPressed(ActionNames.MoveCameraUpAction)) direction.z += -1;
            if (Input.IsActionPressed(ActionNames.MoveCameraDownAction)) direction.z += 1;
            Translation += direction * delta * 10;
        }
    }
}