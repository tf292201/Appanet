using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Appanet.Scripts.Models.Characters;      
using Appanet.Scripts.Models.Combat;          

namespace Appanet.Scripts.Models.Items
{
	public enum ArmorRarity
	{
		Common,
		Uncommon,
		Rare,
		Legendary
	}
	
	public class Armor : Item
	{
		public int DefenseBonus { get; private set; }
		public ArmorRarity Rarity { get; private set; }
		public string SpecialEffect { get; private set; }
		public string Era { get; private set; }
		public string Look { get; private set; }
		public string Lore { get; private set; }
		public float DodgeBonus { get; private set; }  // NEW
		public Dictionary<DamageType, float> ResistanceBonuses { get; private set; }  // NEW
		
		public Armor(string name, string description, int defenseBonus, ArmorRarity rarity, 
					 string era, string look, string lore, string specialEffect = "", int value = 75,
					 float dodgeBonus = 0f, string iconPath = "")  // NEW
			: base(name, description, value, iconPath)
		{
			DefenseBonus = defenseBonus;
			Rarity = rarity;
			Era = era;
			Look = look;
			Lore = lore;
			SpecialEffect = specialEffect;
			DodgeBonus = dodgeBonus;
			ResistanceBonuses = new Dictionary<DamageType, float>();  // NEW
		}
		
		// NEW - Add resistance bonus
		public void AddResistanceBonus(DamageType type, float amount)
		{
			ResistanceBonuses[type] = amount;
		}
		
		public override void Use(Character character)
		{
			if (character is Player player)
			{
				player.EquipArmor(this);
			}
			else
			{
				Console.WriteLine($"{character.Name} cannot equip armor.");
			}
		}
		
		public override void Examine()
		{
			Console.WriteLine($"\n╔{new string('═', 50)}╗");
			Console.WriteLine($"  {Name}");
			Console.WriteLine($"╚{new string('═', 50)}╝");
			Console.WriteLine($"Rarity: {GetRarityColor()}{Rarity}\u001b[0m");
			Console.WriteLine($"Defense Bonus: +{DefenseBonus}");
			
			if (DodgeBonus > 0)
			{
				Console.WriteLine($"Dodge Bonus: +{(int)(DodgeBonus * 100)}%");
			}
			
			if (ResistanceBonuses.Count > 0)
			{
				Console.WriteLine("\nResistances:");
				foreach (var kvp in ResistanceBonuses)
				{
					Console.WriteLine($"  {kvp.Key}: +{(int)(kvp.Value * 100)}%");
				}
			}
			
			Console.WriteLine($"Era: {Era}");
			Console.WriteLine($"\n{Description}");
			Console.WriteLine($"\n\u001b[33mLook:\u001b[0m {Look}");
			Console.WriteLine($"\n\u001b[36mLore:\u001b[0m {Lore}");
			
			if (!string.IsNullOrEmpty(SpecialEffect))
			{
				Console.WriteLine($"\n\u001b[35m★ Special: {SpecialEffect}\u001b[0m");
			}
			
			Console.WriteLine($"\nValue: {Value} dollars");
		}
		
		private string GetRarityColor()
		{
			return Rarity switch
			{
				ArmorRarity.Common => "\u001b[37m",
				ArmorRarity.Uncommon => "\u001b[32m",
				ArmorRarity.Rare => "\u001b[34m",
				ArmorRarity.Legendary => "\u001b[35m",
				_ => "\u001b[37m"
			};
		}
		
		// ===== APPALACHIAN TECH/FOLKLORE ARMOR =====
		
		public static Armor CreateVHSPhantomPoncho()
		{
			var armor = new Armor(
				"VHS Phantom Poncho",
				"A translucent protection that exists slightly out of sync with reality.",
				6,
				ArmorRarity.Rare,
				"1990–1993 home movie era",
				"A translucent poncho that flickers like degraded VHS tape, with little sprocket holes burned into the edges.",
				"Born from the idea that memory itself warps with time — the poncho plays back flickering clips of the wearer a few seconds out of sync.",
				"Dodge chance +15%, enemies see afterimages",
				180,
				0.15f,  // 15% dodge bonus
				"res://Assets/Icons/armor/VHS_Phantom_Poncho.png"
			);
			return armor;
		}
		
		public static Armor CreateAppalachianSysOpDuster()
		{
			var armor = new Armor(
				"Appalachian SysOp Duster",
				"The legendary coat of the rural BBS keepers.",
				10,
				ArmorRarity.Legendary,
				"BBS SysOps era (1985-1995)",
				"A long brown canvas coat lined with login passwords, ANSI art sigils, and pocketfuls of mystical tools (jumper cables, soldering wand, floppy holster).",
				"The coat that every rural BBS sysop swore they had but no one ever saw them wear in public. Passed down between \"keepers of the node.\"",
				"Grants +2 to all tech weapon attacks, inventory +5 slots",
				350,
				0.03f, 
				"res://Assets/Icons/armor/Sysop_Duster.png"
			);
			// Note: Tech weapon bonus handled in Player.cs
			return armor;
		}
		
		public static Armor CreateCRTCathodeCarapace()
		{
			var armor = new Armor(
				"CRT Cathode Carapace",
				"Armor that bends light and static fields through salvaged monitor frames.",
				8,
				ArmorRarity.Rare,
				"1991–1996 beige CRT monitor age",
				"Armor built from shattered CRT frames, fused in weird hexagonal plates that shimmer green in the dark.",
				"Rumored to protect the wearer by bending light and static fields. Some say you can see old screensavers drifting across it at night.",
				"Reduces damage from energy/electric attacks by 50%",
				200,
				0.03f, 
				"res://Assets/Icons/armor/CRT_Carapace.png"
			);
			armor.AddResistanceBonus(DamageType.Electric, 0.5f);  // 50% electric resistance
			return armor;
		}
		
		public static Armor CreateFloppyPlateJerkin()
		{
			var armor = new Armor(
				"Floppy-Plate Jerkin",
				"A vest reinforced with diskettes containing protective code fragments.",
				7,
				ArmorRarity.Uncommon,
				"1990–1994 PC hobbyist culture",
				"A leather vest reinforced with actual 3.5\" floppy disks, each labeled with odd names like \"HAINT_BACKUP_01.EXE\" or \"PROPHETIC_SAVEFILE\".",
				"Every floppy holds a protective spell encoded in archaic file formats. Spirits from the valley can't read the old storage tech, and it confuses them.",
				"Resistance to psychic/curse effects",
				120,
				0.03f, 
				"res://Assets/Icons/armor/Floppy_Jerkin.png"
			);
			armor.AddResistanceBonus(DamageType.Psychic, 0.3f);  // 30% psychic resistance
			armor.AddResistanceBonus(DamageType.Curse, 0.3f);    // 30% curse resistance
			return armor;
		}
		
		public static Armor CreateMountainModemMail()
		{
			var armor = new Armor(
				"Mountain Modem Mail",
				"Patchwork armor woven from telephone cords and rural fabric.",
				9,
				ArmorRarity.Rare,
				"1992 BBS / Dial-up",
				"A patched-together suit of denim, quilted cotton, and dangling telephone cords.",
				"Forged by the old phone-line repairmen who swore they once heard \"voices in the dial tone\" out in the hollers. The braided cords act like protective charms against both spirits and static.",
				"Immunity to possession, +1 to spirit detection",
				220,
				0.03f, 
				"res://Assets/Icons/armor/Mountain_Modem_Mail.png"
			);
			armor.AddResistanceBonus(DamageType.Spectral, 0.4f);  // 40% spectral resistance
			armor.AddResistanceBonus(DamageType.Curse, 0.4f);     // 40% curse resistance (possession)
			return armor;
		}
		
		// ===== MUNDANE/COMMON ARMOR =====
		
		
		
		
		public static Armor CreateConstructionVest()
		{
			return new Armor(
				"Construction Safety Vest",
				"High-visibility orange vest. Won't stop claws, but might save you from getting hit by a coal truck in the dark.",
				4,
				ArmorRarity.Common,
				"1990s safety regulation",
				"Neon orange with reflective strips. Has a company logo for a mine that closed in '88.",
				"Your uncle left it in the truck. He said you might need it. He was right, but not for the reasons he thought.",
				"Easier to spot in darkness",
				30,
				0.03f, 
				"res://Assets/Icons/armor/Work_Safety_Vest.png"
				
			);
		}
		
		public static Armor CreateCarhartJacket()
		{
			return new Armor(
				"Carhartt Work Jacket",
				"Heavy canvas work jacket. Built for coal dust and mountain winters, not otherworldly threats.",
				5,
				ArmorRarity.Common,
				"Working class standard issue",
				"Brown duck canvas, stiff with age and honest labor. The pockets still have receipts from the hardware store.",
				"Every working man in three counties owns one. Yours has seen the inside of more barns than most.",
				"",
				50,
				0.03f,
				"res://Assets/Icons/armor/Workwear.png"
			);
		}
		
		public static Armor CreateFlannel()
{
	var flannel = new Armor(
		"Flannel Shirt",
		"A classic Appalachian flannel. Comfortable, breathable, lets you move freely.",
		2,  // Defense bonus
		ArmorRarity.Common,
		"Timeless mountain wear",
		"Red and black plaid flannel, soft from years of washing. Missing one button.",
		"Every person in the hollers has three of these. This one's yours. It's seen you through a lot of cold mornings.",
		"Lightweight and flexible",
		25,  // Value
		0.03f,  // 3% dodge bonus
		"res://Assets/Icons/armor/flannel_shirt.png"
	);
	return flannel;
}
		
		public static Armor CreateLeatherJacket()
		{
			return new Armor(
				"Leather Motorcycle Jacket",
				"Black leather with worn elbows. Makes you look tougher than you probably are.",
				6,
				ArmorRarity.Uncommon,
				"Rebel without a cause aesthetic",
				"Scuffed black leather with a broken zipper. There's a Harley Davidson logo on the back, but you've never owned a bike.",
				"Bought it off a guy at the flea market who said it belonged to someone who 'saw things.' You didn't ask what things.",
				"",
				80,
				0.03f, 
				"res://Assets/Icons/armor/leather_jacket.png"
			);
		}
	}
}
