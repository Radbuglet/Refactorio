using Godot;

namespace Refactorio.helpers
{
    public static class MathUtils
    {
        public static int RandSign()
        {
            return (GD.RandRange(0, 1) > 0.5) ? 1 : -1;
        }

        public static Vector2 Vec2FromXz(Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        public static int RandInt(int min, int max)
        {
            return Mathf.RoundToInt((float) GD.RandRange(min, max));
        }
        
        public static Vector2 RandDir()
        {
            var num = RandInt(0, 3);
            var sign = num > 1 ? 1 : -1;
            return new Vector2(num % 2 == 0 ? 1 : 0, num % 2 != 0 ? 1 : 0) * sign;
        }
    }
}