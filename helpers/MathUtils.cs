using Godot;

namespace Refactorio.helpers
{
    public static class MathUtils
    {
        public static int RandSign()
        {
            return (GD.RandRange(0, 1) > 0.5) ? 1 : -1;
        }
    }
}