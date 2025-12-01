using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Appanet.Scripts.Models.Characters;    
using Appanet.Scripts.Models.Combat; 

namespace Appanet.Scripts.Models.Items
{
	public enum WeaponRarity
	{
		Common,
		Uncommon,
		Rare,
		Legendary
	}
	
	public class Weapon : Item
	{
		public int AttackBonus { get; private set; }
		public WeaponRarity Rarity { get; private set; }
		public string SpecialEffect { get; private set; }
		public DamageType DamageType { get; private set; }  // NEW
		public float BonusDamageVsType { get; private set; }  // NEW - multiplier vs specific enemy types
		public EnemyType EffectiveAgainst { get; private set; }  // NEW - which enemy type
		
		public Weapon(string name, string description, int attackBonus, WeaponRarity rarity, 
					  string specialEffect = "", int value = 50, 
					  DamageType damageType = DamageType.Physical,  // NEW
					  EnemyType effectiveAgainst = EnemyType.Normal,  // NEW
					  float bonusDamageVsType = 0f,
					  string iconPath = "")  // NEW
			: base(name, description, value, iconPath)
		{
			AttackBonus = attackBonus;
			Rarity = rarity;
			SpecialEffect = specialEffect;
			DamageType = damageType;
			EffectiveAgainst = effectiveAgainst;
			BonusDamageVsType = bonusDamageVsType;
		}
		
		public override void Use(Character character)
		{
			if (character is Player player)
			{
				player.EquipWeapon(this);
			}
			else
			{
				Console.WriteLine($"{character.Name} cannot equip weapons.");
			}
		}
		
		// NEW - Calculate damage against specific enemy
		public int CalculateDamage(int baseDamage, Enemy enemy)
		{
			float damageMultiplier = 1.0f;
			
			// Check if weapon is effective against this enemy type
			if (enemy.EnemyType == EffectiveAgainst && BonusDamageVsType > 0)
			{
				damageMultiplier += BonusDamageVsType;
				Console.WriteLine($"[{Name} is effective against {enemy.EnemyType} enemies!]");
			}
			
			return (int)(baseDamage * damageMultiplier);
		}
		
		public override void Examine()
		{
			Console.WriteLine($"\n╔{new string('═', 40)}╗");
			Console.WriteLine($"  {Name}");
			Console.WriteLine($"╚{new string('═', 40)}╝");
			Console.WriteLine($"Rarity: {GetRarityColor()}{Rarity}\u001b[0m");
			Console.WriteLine($"Attack Bonus: +{AttackBonus}");
			Console.WriteLine($"Damage Type: {DamageType}");
			
			if (EffectiveAgainst != EnemyType.Normal && BonusDamageVsType > 0)
			{
				Console.WriteLine($"Bonus vs {EffectiveAgainst}: +{(int)(BonusDamageVsType * 100)}%");
			}
			
			Console.WriteLine($"\n{Description}");
			
			if (!string.IsNullOrEmpty(SpecialEffect))
			{
				Console.WriteLine($"\n\u001b[36m★ Special: {SpecialEffect}\u001b[0m");
			}
			
			Console.WriteLine($"\nValue: {Value} dollars");
		}
		
		private string GetRarityColor()
		{
			return Rarity switch
			{
				WeaponRarity.Common => "\u001b[37m",
				WeaponRarity.Uncommon => "\u001b[32m",
				WeaponRarity.Rare => "\u001b[34m",
				WeaponRarity.Legendary => "\u001b[35m",
				_ => "\u001b[37m"
			};
		}
		
		// ===== APPALACHIAN TECH WEAPONS =====
		
		public static Weapon CreateCRTBlaster()
		{
			return new Weapon(
				"CRT Blaster",
				"A portable cathode-ray tube scavenged from an old Zenith TV. When powered on, it floods the area with electromagnetic static that makes paranormal entities flicker like bad reception.",
				8,
				WeaponRarity.Uncommon,
				"Deals bonus damage to spectral enemies",
				75,
				DamageType.Electric,
				EnemyType.Spectral,
				0.5f,  // 50% bonus damage vs spectral
				"res://Assets/Icons/weapons/CRTblaster.png"
			);
		}
		
		public static Weapon CreateAcousticCouplerGrenade()
		{
			return new Weapon(
				"Acoustic Coupler Grenade",
				"A modified 300-baud modem handset rigged to loop its screaming dial-up handshake at max volume. The psychic noise scrambles supernatural signals within a 20-foot radius.",
				12,
				WeaponRarity.Rare,
				"Disrupts psychic-type enemies, chance to confuse",
				150,
				DamageType.Psychic,
				EnemyType.Psychic,
				0.6f,  // 60% bonus vs psychic
				"res://Assets/Icons/weapons/Acoustic_grenade.png"
				
			);
		}
		
		public static Weapon CreateFloppySigils()
		{
			return new Weapon(
				"Floppy Sigils (3.5\" Disk)",
				"A collection of 3.5\" diskettes covered in hand-drawn ASCII sigils and BASIC code fragments. When slotted into any drive—or just held up like a ward—they seem to crash reality's boot sequence.",
				6,
				WeaponRarity.Common,
				"Can dispel minor curses and hexes",
				40,
				DamageType.Curse,
				EnemyType.Normal,
				0f,
				"res://Assets/Icons/weapons/Floppy_Sigil.png"
			);
		}
		
		public static Weapon CreateHamRadioTuningRod()
		{
			return new Weapon(
				"Ham Radio Tuning Rod",
				"A telescoping CB antenna from a '91 F-150, modified with copper wire wraps and quartz crystals. When extended and swept through the air, it picks up \"false signals\"—the electromagnetic signatures of things that shouldn't be there.",
				10,
				WeaponRarity.Uncommon,
				"Reveals hidden enemies, +2 attack vs illusions",
				90,
				DamageType.Electric,
				EnemyType.Illusion,
				0.4f,  // 40% bonus vs illusions
				"res://Assets/Icons/weapons/Ham_radio_tuning_rod.png"
			);
		}
		
	
		
		public static Weapon CreateCodebookHaintbreaker()
		{
			return new Weapon(
				"Codebook \"Haintbreaker\"",
				"A weathered punch-card cipher book from the old coal company's accounting office. Someone—maybe a clerk, maybe something else—added annotations in the margins: folklore protections encoded in COBOL. Reading the right sequence aloud makes old haints forget why they're angry.",
				15,
				WeaponRarity.Legendary,
				"Extra damage vs folklore creatures, can banish spirits",
				300,
				DamageType.Curse,
				EnemyType.Folklore,
				0.8f,  // 80% bonus vs folklore!
				"res://Assets/Icons/weapons/HaintBreaker.png"
			);
		}
		
		// ===== MUNDANE WEAPONS =====
		
		public static Weapon CreateBaseballBat()
		{
			return new Weapon(
				"Aluminum Baseball Bat",
				"A Louisville Slugger from the high school team. Dented, reliable, and good for more than just home runs.",
				4,
				WeaponRarity.Common,
				"",
				20,
				DamageType.Physical,
				EnemyType.Normal,              // ✅ Position 8 - effectiveAgainst
				0f,                            // ✅ Position 9 - bonusDamageVsType
				"res://Assets/Icons/weapons/Bat.png"
			);
		}
		
		public static Weapon CreateMagliteFlashlight()
		{
			return new Weapon(
				"Maglite Flashlight",
				"A heavy-duty D-cell Maglite. Makes a decent club in a pinch, and the beam is bright enough to blind.",
				3,
				WeaponRarity.Common,
				"Can illuminate dark areas",
				15,
				DamageType.Physical,
				EnemyType.Normal,     // ADD THIS - effectiveAgainst
				 0f,                   // ADD THIS - bonusDamageVsType
				"res://Assets/Icons/weapons/Flashlight.png"
			);
		}
		
		public static Weapon CreatePocketKnife()
		{
			return new Weapon(
				"Swiss Army Knife",
				"Your dad's old pocket knife. Not much for fighting, but it's got sentimental value and a dozen tools you probably won't need.",
				2,
				WeaponRarity.Common,
				"Utility tool, can solve certain puzzles",
				10,
				DamageType.Physical,
				EnemyType.Normal,     // ADD THIS - effectiveAgainst
	  		  0f,                   // ADD THIS - bonusDamageVsType
				"res://Assets/Icons/weapons/knife.png"
			);
		}
		
		public static Weapon CreateTireIron()
		{
			return new Weapon(
				"Tire Iron",
				"Pulled from the trunk of a '89 Oldsmobile. Rusty but effective.",
				5,
				WeaponRarity.Common,
				"",
				15,
				DamageType.Physical,
				EnemyType.Normal,     // ADD THIS - effectiveAgainst
				0f,                   // ADD THIS - bonusDamageVsType
				"res://Assets/Icons/weapons/tireiron.png"
			);
		}
	}
}
