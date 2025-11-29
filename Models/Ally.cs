using System;
using Godot;
using Appanet.Scripts.Models.SpecialAbilities;
namespace Appanet.Scripts.Models
{
	public class Ally : Character
	{
		public string AllyID { get; private set; }
		public Inventory Inventory { get; private set; }
		public Weapon EquippedWeapon { get; private set; }
		public string IconPath { get; private set; }
		
		
		public Ally(string id, string name, int maxHealth, int attackPower, int defense, string iconPath = "")
			: base(name, maxHealth, attackPower, defense)
		{
			AllyID = id;
			Inventory = new Inventory(10);
			IconPath = iconPath;
		}
		
		// EQUIPMENT METHODS
		public void EquipWeapon(Weapon weapon)
		{
			if (EquippedWeapon != null)
			{
				Inventory.AddItem(EquippedWeapon);
			}
			
			EquippedWeapon = weapon;
			if (weapon != null)
			{
				Inventory.RemoveItem(weapon);
			}
		}
		
		public void UnequipWeapon()
		{
			if (EquippedWeapon == null)
			{
				return;
			}
			
			Inventory.AddItem(EquippedWeapon);
			EquippedWeapon = null;
		}
		
		public void UnequipArmor()
		{
			if (EquippedArmor == null)
			{
				return;
			}
			
			Inventory.AddItem(EquippedArmor);
			EquippedArmor = null;
		}
		
		public override AttackResult AttackWithResult()
		{
  		  int baseAttack = AttackPower;
  		  int weaponBonus = EquippedWeapon?.AttackBonus ?? 0;
  		  int totalAttack = baseAttack + weaponBonus;
	
  		  int minDamage = totalAttack * 80 / 100;
  		  int maxDamage = totalAttack * 120 / 100;
  		  Random rand = new Random();
  		  int varianceDamage = rand.Next(minDamage, maxDamage + 1);
	
  		  bool isCrit = rand.Next(1, 101) <= 10;
	
  		  int finalDamage = isCrit ? (int)(varianceDamage * 1.5) : varianceDamage;
	
  		  DamageType damageType = EquippedWeapon?.DamageType ?? DamageType.Physical;
	
  		  return new AttackResult(finalDamage, isCrit, damageType);
		}
		
		public override int Attack()
		{
 		   return AttackWithResult().Damage;
		}
		
		// Factory methods
		public static Ally CreateMichaelWebb()
		{
			var michael = new Ally(
				"michael",
				"Michael Webb",
				80,
				12,
				4,
				"res://icons/party/MichaelWeb.png" 
			);
			
			michael.UnlockAbility(new MKUltraMemoryScramble());
			
			return michael;
		}
		
		public static Ally CreateDima()
		{
			var dima = new Ally(
				"dima",
				"Dima Volkov",
				70,
				8,
				6,
				"res://icons/party/Dima.png" 
			);
			return dima;
		}
		
		public static Ally CreateCase()
		{
			var case_ally = new Ally(
				"cass",
				"Cassie Whitmore",
				90,
				14,
				3,
				"res://icons/party/Cassie.png" 
			);
			case_ally.DodgeChance = 0.15f;
			
			case_ally.UnlockAbility(new HaintWind());
			
			return case_ally;
		}
		
		public override void EquipArmor(Armor armor)
		{
			// Remove old armor bonuses
			if (EquippedArmor != null)
			{
				Inventory.AddItem(EquippedArmor);
				
				// Remove old resistance bonuses
				foreach (var kvp in EquippedArmor.ResistanceBonuses)
				{
					Resistances.AddResistance(kvp.Key, -kvp.Value);
				}
				
				// Remove dodge bonus
				DodgeChance -= EquippedArmor.DodgeBonus;
			}
			
			// Equip new armor
			EquippedArmor = armor;
			if (armor != null)
			{
				Inventory.RemoveItem(armor);
				
				// Apply new resistance bonuses
				foreach (var kvp in armor.ResistanceBonuses)
				{
					Resistances.AddResistance(kvp.Key, kvp.Value);
				}
				
				// Apply dodge bonus
				DodgeChance += armor.DodgeBonus;
			}
		}
	}
}
