using Godot;
using Refactorio.helpers;

namespace Refactorio.game.simulation.objects
{
    // Tier definitions
    public class CrystalType
    {
        public static readonly CrystalType[] Types = {
            new CrystalType
            {
                Color=Colors.Gray,
                DropCount=1,
                Durability=20,
                Hardness=0
            },
            new CrystalType
            {
                Color=Colors.Beige,
                DropCount=1,
                Durability=100,
                Hardness=10
            }
        };
        
        public Color Color;
        public uint DropCount;
        public uint Durability;
        public uint Hardness;
    }

    public class MatterCrystal : BaseObject
    {
        private short _crystalTypeIdx;
        private CrystalType CrystalType => CrystalType.Types[_crystalTypeIdx];

        // Event handling
        public override void _Ready()
        {
            _crystalTypeIdx = (short) MathUtils.RandInt(0, CrystalType.Types.Length - 1);
            RegisterGridPresence();
        }

        public override void _ExitTree()
        {
            UnregisterGridPresence();
        }
    }
}