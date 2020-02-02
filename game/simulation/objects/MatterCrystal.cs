using System;
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
				Color=Colors.Aqua,
				DropCount=1,
				Durability=100,
				Hardness=3
			}
		};
		
		public Color Color;
		public int DropCount;
		public int Durability;
		public int Hardness;
	}

	public class MatterCrystal : BaseObject
	{
		// Properties
		private short _crystalTypeIdx;
		private int _health;
		
		// Getters
		private CrystalType CrystalType => CrystalType.Types[_crystalTypeIdx];
		private Particles BreakParticles => GetNode<Particles>("./BreakParticles");
		private MeshInstance CrystalMesh => GetNode<MeshInstance>("./Mesh");  // "More like crystal meth, ha!"

		// Event handling
		public override void _Ready()
		{
			// Chose crystal type
			_crystalTypeIdx = (short) MathUtils.RandInt(0, CrystalType.Types.Length - 1);

			// Apply crystal type info
			{
				var crystalTypeCache = CrystalType;
				var crystalMeshCache = CrystalMesh;
				_health = crystalTypeCache.Durability;

				crystalMeshCache.MaterialOverride = new SpatialMaterial  // TODO: This strategy is painfully inefficient. Use a multi-mesh to optimize! 
				{
					AlbedoColor = crystalTypeCache.Color
				};
				crystalMeshCache.RotateY((float) GD.RandRange(0, Mathf.Tau));
			}
			
			RegisterGridPresence();
		}

		// Methods
		public int Damage(int pickLevel)
		{
			var crystalTypeCache = CrystalType;
			var finalDamage = Mathf.Max(pickLevel - crystalTypeCache.Hardness, 0);
			if (finalDamage == 0) return 0;
			_health -= finalDamage;
			if (_health == 0)
			{
				QueueFree();  // TODO: Will this break if it frees immediately?
				UnregisterGridPresence();
			}
			else
			{
				var component = (float) _health / crystalTypeCache.Durability;
				Scale = new Vector3(component, component, component);
			}

			{
				var breakParticlesCache = BreakParticles;
				breakParticlesCache.Restart();
				breakParticlesCache.Amount = finalDamage;
				breakParticlesCache.Emitting = true;
			}

			return finalDamage * crystalTypeCache.DropCount;
		}
	}
}
