using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public enum ConsumableRarity
	{
		Common,
		Uncommon,
		Rare,
		Legendary
	}
	
	public class Consumable : Item
	{
		public int HealAmount { get; private set; }
		public ConsumableRarity Rarity { get; private set; }
		public string Look { get; private set; }
		public string Lore { get; private set; }
		
		public Consumable(string name, string description, int healAmount, ConsumableRarity rarity, 
						  string look, string lore, int value = 10,  string iconPath = "")
			: base(name, description, value, iconPath)
		{
			HealAmount = healAmount;
			Rarity = rarity;
			Look = look;
			Lore = lore;
		}
		
		public override void Use(Character character)
		{
			if (character.Health >= character.MaxHealth)
			{
				Console.WriteLine($"{character.Name} is already at full health!");
				return;
			}
			
			character.Heal(HealAmount);
			
			// Only remove from inventory if it's a Player
			if (character is Player player)
			{
				player.Inventory.RemoveItem(this);
			}
			
			Console.WriteLine($"Used {Name}!");
		}
		
		public override void Examine()
		{
			Console.WriteLine($"\n╔{'═', 50}╗");
			Console.WriteLine($"  {Name}");
			Console.WriteLine($"╚{'═', 50}╝");
			Console.WriteLine($"Rarity: {GetRarityColor()}{Rarity}\u001b[0m");
			Console.WriteLine($"Restores: {HealAmount} HP");
			Console.WriteLine($"\n{Description}");
			Console.WriteLine($"\n\u001b[33mLook:\u001b[0m {Look}");
			Console.WriteLine($"\n\u001b[36mLore:\u001b[0m {Lore}");
			Console.WriteLine($"\nValue: {Value} dollars");
		}
		
		private string GetRarityColor()
		{
			return Rarity switch
			{
				ConsumableRarity.Common => "\u001b[37m",      // White
				ConsumableRarity.Uncommon => "\u001b[32m",    // Green
				ConsumableRarity.Rare => "\u001b[34m",        // Blue
				ConsumableRarity.Legendary => "\u001b[35m",   // Magenta
				_ => "\u001b[37m"
			};
		}
		
		// ===== APPALACHIAN TECH/FOLKLORE CONSUMABLES =====
		
		public static Consumable CreateMasonJarMountainDew()
		{
			return new Consumable(
				"Mason Jar Mountain Dew",
				"A mysterious carbonated healing brew that somehow works.",
				25,
				ConsumableRarity.Uncommon,
				"A green-tinted mason jar labeled with a floppy-disk logo and hand-drawn circuit diagrams.",
				"Brewed by a mysterious sysop who swears that soda carbonation is \"just trapped healing packets.\" Nobody knows if he's a genius or insane—but it works.",
				20,
				"res://icons/consumables/Mason_Jar_dew.png"
			);
		}
		
		public static Consumable CreateDialUpHealingTonic()
		{
			return new Consumable(
				"Dial-Up Healing Tonic (56k BITTER)",
				"Shake it and hear the screech of connection. Your body remembers how to sync.",
				40,
				ConsumableRarity.Rare,
				"Amber bottle labeled \"56k BITTER\" with a phone jack symbol etched in the glass.",
				"Shake it and it makes the dial-up handshake noise. Somehow—over many years—the tonic learned to \"connect\" your body back together. BBS operators swear by it.",
				45,
				"res://icons/consumables/56k_tonic.png"
			);
		}
		
		public static Consumable CreateCoalDustCandiedPecans()
		{
			return new Consumable(
				"Coal Dust Candied Pecans",
				"Sweet, charred pecans that miners swore could sustain a man through the darkest shifts.",
				20,
				ConsumableRarity.Common,
				"Dark, crystallized pecans in a wax paper bag. They smell like caramelized sugar and coal smoke.",
				"Miners said these sweet, charred pecans \"put life back in your bones\" during long shifts. The recipe came from the company store—back when it was still open.",
				12,
				"res://icons/consumables/Coaldust_pecans.png"
			);
		}
		
		public static Consumable CreateVHSComfortBlanket()
		{
			return new Consumable(
				"VHS Comfort Blanket",
				"A single-use blanket woven from memories and magnetic tape. Heals deeply, but unravels after.",
				70,
				ConsumableRarity.Legendary,
				"A blanket woven from magnetic VHS tape; soft, warm, slightly glitchy. When you look closely, you can see sitcom faces flickering in the weave.",
				"Wrap yourself in years of cherished memories, wholesome static, and stray sitcom laughter. It only works once—after that, the memories fade to white noise.",
				100,
				"res://icons/consumables/VHS_blanket.png"
			);
		}
		
		public static Consumable CreateTerminalCuredJerky()
		{
			return new Consumable(
				"Terminal-Cured Jerky",
				"Meat dried on old computer hardware. Toughens you up in ways that don't make medical sense.",
				30,
				ConsumableRarity.Uncommon,
				"Strips of jerky dried on the metal frame of an old IBM terminal. The meat has a faint metallic tang and smells like ozone.",
				"The warmth from aging power supplies and CRT static was said to \"cure the meat with wisdom.\" Miners and tinkerers swore it toughened both jaw and resolve.",
				25,
				"res://icons/consumables/Terminal_cured_jerky.png"
			);
		}
		
		// ===== MUNDANE/COMMON CONSUMABLES =====
		
		public static Consumable CreateHealthPotion()
		{
			return new Consumable(
				"Health Potion",
				"A standard healing potion. Nothing fancy, but it works.",
				30,
				ConsumableRarity.Common,
				"A small red vial with a cork stopper.",
				"Generic healing potion from the corner store. Tastes like cherry cough syrup.",
				15,
				""
			);
		}
		
		public static Consumable CreateMegaPotion()
		{
			return new Consumable(
				"Mega Potion",
				"A powerful healing potion for serious injuries.",
				60,
				ConsumableRarity.Rare,
				"A large blue vial with an ornate label.",
				"High-grade healing formula. Expensive, but worth it when you're bleeding out.",
				35,
				""
			);
		}
		
		public static Consumable CreateGasolineCoffee()
		{
			return new Consumable(
				"Gas Station Coffee",
				"It's been sitting there since morning, but caffeine is caffeine.",
				15,
				ConsumableRarity.Common,
				"Styrofoam cup with burnt coffee that's been on the burner for 6+ hours.",
				"From the 24-hour Exxon off Route 52. The clerk says it \"puts hair on your chest.\" You're not sure that's a good thing.",
				5,
				"res://icons/consumables/Gas_Station_Coffee.png"
			);
		}
		
		public static Consumable CreateMoonPie()
		{
			return new Consumable(
				"Moon Pie & RC Cola",
				"The working man's lunch. Sweet, simple, nostalgic.",
				18,
				ConsumableRarity.Common,
				"A marshmallow-chocolate sandwich cookie and an ice-cold Royal Crown Cola.",
				"Every general store in three counties sells these. Your grandpa lived on them during the mine strikes.",
				8,
				"res://icons/consumables/Pie_Cola.png"
			);
		}
		
		public static Consumable CreatePepperoniRoll()
		{
			return new Consumable(
				"Pepperoni Roll",
				"West Virginia's greatest contribution to world cuisine. Portable, filling, perfect.",
				22,
				ConsumableRarity.Common,
				"Fresh-baked bread rolled around sticks of pepperoni. Still warm from the Italian bakery.",
				"Invented by Giuseppe Argiro in 1927 for coal miners. Simple, genius, and still the best lunch you can get for two bucks.",
				10,
				"res://icons/consumables/Pepperoni_roll.png"
			);
		}
	}
}
