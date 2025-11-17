#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public class Player : Character
	{
		public Inventory Inventory { get; private set; }
		public int Experience { get; private set; }
		public int Level { get; private set; }
		public Weapon EquippedWeapon { get; private set; }
		public Armor EquippedArmor { get; private set; }
		public int Money { get; private set; }
		
		public Player(string name, int maxHealth = 100, int attackPower = 10, int defense = 2)
			: base(name, maxHealth, attackPower, defense)
		{
			Inventory = new Inventory(20);
			Experience = 0;
			Level = 1;
			EquippedWeapon = null;
			EquippedArmor = null;
			Money = 50;
		}
		
		// Money management methods
		public void AddMoney(int amount)
		{
			Money += amount;
			GD.Print($"💵 Earned ${amount}! Total: ${Money}");
		}
		
		public bool SpendMoney(int amount)
		{
			if (Money >= amount)
			{
				Money -= amount;
				GD.Print($"💸 Spent ${amount}. Remaining: ${Money}");
				return true;
			}
			else
			{
				GD.Print($"Not enough money! Need ${amount}, but only have ${Money}.");
				return false;
			}
		}
		
		// Selling items
		public void SellItem(Item item)
		{
			if (!Inventory.GetAllItems().Contains(item))
			{
				GD.Print("You don't have that item.");
				return;
			}
			
			if (item == EquippedWeapon)
			{
				GD.Print("Unequip the weapon first!");
				return;
			}
			
			if (item == EquippedArmor)
			{
				GD.Print("Unequip the armor first!");
				return;
			}
			
			int sellPrice = item.Value / 2;
			Inventory.RemoveItem(item);
			AddMoney(sellPrice);
			GD.Print($"Sold {item.Name} for ${sellPrice}.");
		}
		
		// Attack with detailed result
		public override AttackResult AttackWithResult()
		{
			int baseAttack = AttackPower;
			int weaponBonus = EquippedWeapon?.AttackBonus ?? 0;
			int totalAttack = baseAttack + weaponBonus;
			
			// Damage variance (80% - 120%)
			int minDamage = totalAttack * 80 / 100;
			int maxDamage = totalAttack * 120 / 100;
			Random rand = new Random();
			int varianceDamage = rand.Next(minDamage, maxDamage + 1);
			
			// Critical hit check (10% chance)
			bool isCrit = rand.Next(1, 101) <= 10;
			
			int finalDamage = isCrit ? (int)(varianceDamage * 1.5) : varianceDamage;
			
			// Get damage type from weapon, default to Physical
			DamageType damageType = EquippedWeapon?.DamageType ?? DamageType.Physical;
			
			return new AttackResult(finalDamage, isCrit, damageType);
		}
		
		// Simple attack (backward compatibility)
		public override int Attack()
		{
			return AttackWithResult().Damage;
		}
		
		// Get total defense including armor
		public int GetTotalDefense()
		{
			int baseDefense = Defense;
			int armorBonus = EquippedArmor?.DefenseBonus ?? 0;
			return baseDefense + armorBonus;
		}
		
		// Override TakeDamage to use armor and detailed result tracking
		public override DamageResult TakeDamageWithResult(int damage, DamageType damageType = DamageType.Physical)
		{
			int damageBeforeDefense = damage;
			
			// Apply defense (base + armor)
			int totalDefense = GetTotalDefense();
			int damageAfterDefense = Mathf.Max(1, damage - totalDefense);
			
			// Apply resistance
			float resistance = Resistances.GetResistance(damageType);
			int damageAfterResistance = (int)(damageAfterDefense * (1 - resistance));
			
			// Check for dodge (base + armor dodge bonus)
			float totalDodge = DodgeChance + (EquippedArmor?.DodgeBonus ?? 0f);
			Random rand = new Random();
			bool dodged = rand.NextDouble() < totalDodge;
			
			if (dodged)
			{
				return new DamageResult(0, true, damageBeforeDefense, totalDefense);
			}
			
			// Apply damage
			Health -= damageAfterResistance;
			
			if (Health <= 0)
			{
				Health = 0;
				OnDeath();
			}
			
			return new DamageResult(damageAfterResistance, false, damageBeforeDefense, totalDefense);
		}
		
		// Simple TakeDamage (backward compatibility)
		public override void TakeDamage(int damage, DamageType damageType = DamageType.Physical)
		{
			TakeDamageWithResult(damage, damageType);
		}
		
		// Equipment methods
		public void EquipWeapon(Weapon weapon)
		{
			if (EquippedWeapon != null)
			{
				Inventory.AddItem(EquippedWeapon);
			}
			
			EquippedWeapon = weapon;
			Inventory.RemoveItem(weapon);
			
			GD.Print($"⚔️ Equipped: {weapon.Name}");
			GD.Print($"   Attack Power: {AttackPower} + {weapon.AttackBonus} = {AttackPower + weapon.AttackBonus}");
			GD.Print($"   Damage Type: {weapon.DamageType}");
			
			if (!string.IsNullOrEmpty(weapon.SpecialEffect))
			{
				GD.Print($"   ★ {weapon.SpecialEffect}");
			}
		}
		
		public void UnequipWeapon()
		{
			if (EquippedWeapon == null)
			{
				GD.Print("No weapon equipped.");
				return;
			}
			
			Inventory.AddItem(EquippedWeapon);
			GD.Print($"Unequipped {EquippedWeapon.Name}.");
			EquippedWeapon = null;
		}
		
		public void EquipArmor(Armor armor)
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
			Inventory.RemoveItem(armor);
			
			GD.Print($"🛡️ Equipped: {armor.Name}");
			GD.Print($"   Defense: {Defense} + {armor.DefenseBonus} = {GetTotalDefense()}");
			
			// Apply new resistance bonuses
			foreach (var kvp in armor.ResistanceBonuses)
			{
				Resistances.AddResistance(kvp.Key, kvp.Value);
				GD.Print($"   +{(int)(kvp.Value * 100)}% {kvp.Key} Resistance");
			}
			
			// Apply dodge bonus
			if (armor.DodgeBonus > 0)
			{
				DodgeChance += armor.DodgeBonus;
				GD.Print($"   +{(int)(armor.DodgeBonus * 100)}% Dodge Chance");
			}
			
			if (!string.IsNullOrEmpty(armor.SpecialEffect))
			{
				GD.Print($"   ★ {armor.SpecialEffect}");
			}
		}
		
		public void UnequipArmor()
		{
			if (EquippedArmor == null)
			{
				GD.Print("No armor equipped.");
				return;
			}
			
			// Remove resistance bonuses
			foreach (var kvp in EquippedArmor.ResistanceBonuses)
			{
				Resistances.AddResistance(kvp.Key, -kvp.Value);
			}
			
			// Remove dodge bonus
			DodgeChance -= EquippedArmor.DodgeBonus;
			
			Inventory.AddItem(EquippedArmor);
			GD.Print($"Unequipped {EquippedArmor.Name}.");
			EquippedArmor = null;
		}
		
		// Experience and leveling
		public void GainExperience(int amount)
		{
			Experience += amount;
			GD.Print($"{Name} gains {amount} XP! Total: {Experience}");
			
			int requiredXP = Level * 100;
			if (Experience >= requiredXP)
			{
				LevelUp();
			}
		}
		
		private void LevelUp()
		{
			Level++;
			MaxHealth += 10;
			Health = MaxHealth;
			AttackPower += 2;
			Defense += 1;
			
			GD.Print($"\n*** LEVEL UP! {Name} is now level {Level}! ***");
			GD.Print($"Max Health: +10");
			GD.Print($"Attack: +2");
			GD.Print($"Defense: +1");
		}
		
		// Item usage
		public bool UseItem(string itemName)
		{
			Item item = Inventory.GetItem(itemName);
			
			if (item == null)
			{
				GD.Print($"You don't have a {itemName}.");
				return false;
			}
			
			item.Use(this);
			return true;
		}
		
		// Display methods
		public void DisplayFullStatus()
		{
			GD.Print("\n╔════════════════════════════╗");
			GD.Print($"  {Name} - Level {Level}");
			GD.Print("╚════════════════════════════╝");
			GD.Print($"Health: {Health}/{MaxHealth}");
			GD.Print($"Money: ${Money}");
			
			if (EquippedWeapon != null)
			{
				GD.Print($"Attack: {AttackPower} (+{EquippedWeapon.AttackBonus} from {EquippedWeapon.Name}) = {AttackPower + EquippedWeapon.AttackBonus}");
				GD.Print($"Damage Type: {EquippedWeapon.DamageType}");
			}
			else
			{
				GD.Print($"Attack: {AttackPower} (No weapon equipped)");
			}
			
			if (EquippedArmor != null)
			{
				GD.Print($"Defense: {Defense} (+{EquippedArmor.DefenseBonus} from {EquippedArmor.Name}) = {GetTotalDefense()}");
			}
			else
			{
				GD.Print($"Defense: {Defense} (No armor equipped)");
			}
			
			// Show dodge chance
			float totalDodge = DodgeChance + (EquippedArmor?.DodgeBonus ?? 0f);
			if (totalDodge > 0)
			{
				GD.Print($"Dodge Chance: {(int)(totalDodge * 100)}%");
			}
			
			GD.Print($"Experience: {Experience}/{Level * 100}");
			
			// Show equipped gear
			if (EquippedWeapon != null)
			{
				GD.Print($"\n⚔️ Equipped Weapon: {EquippedWeapon.Name}");
			}
			
			if (EquippedArmor != null)
			{
				GD.Print($"🛡️ Equipped Armor: {EquippedArmor.Name}");
			}
			
			// Show active resistances
			bool hasResistances = false;
			foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
			{
				float resistance = Resistances.GetResistance(type);
				if (resistance != 0)
				{
					if (!hasResistances)
					{
						GD.Print("\n--- Resistances ---");
						hasResistances = true;
					}
					GD.Print($"{type}: {(int)(resistance * 100)}%");
				}
			}
			
			// Show active status effects
			if (ActiveStatusEffects.Count > 0)
			{
				GD.Print("\n--- Active Effects ---");
				foreach (var effect in ActiveStatusEffects)
				{
					GD.Print($"• {effect.GetDescription()}");
				}
			}
			
			Inventory.DisplayInventory();
		}
		
		protected override void OnDeath()
		{
			GD.Print($"\n{Name} has fallen...");
			GD.Print("GAME OVER");
		}
	}
}
