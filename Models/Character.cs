using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Appanet.Scripts.Models.SpecialAbilities;

namespace Appanet.Scripts.Models
{
	public abstract class Character
	{
		// Private fields - encapsulation
		private int _health;
		private int _maxHealth;
		private static Random _random = new Random();
		
		// Public properties
		public string Name { get; protected set; }
		public int MaxHealth 
		{ 
			get => _maxHealth;
			protected set => _maxHealth = value;
		}
		
		public int Health 
		{ 
			get => _health;
			protected set 
			{
				_health = Math.Max(0, Math.Min(value, _maxHealth));
			}
		}
		
		public int AttackPower { get; protected set; }
		public int Defense { get; protected set; }
		public bool IsAlive => Health > 0;
		
		// NEW - Combat system properties
		public DamageResistance Resistances { get; protected set; }
		public List<StatusEffectInstance> ActiveStatusEffects { get; protected set; }
		public float DodgeChance { get; protected set; }  // 0.0 to 1.0 (0% to 100%)
		
		public List<SpecialAbility> UnlockedAbilities { get; protected set; }
   		 public SpecialAbility SelectedAbility { get; protected set; }
 	  public int SpecialMeter { get; protected set; }
 	   public int MaxSpecialMeter { get; protected set; }
		
		// Constructor
		protected Character(string name, int maxHealth, int attackPower, int defense)
		{
			Name = name;
			MaxHealth = maxHealth;
			Health = maxHealth;
			AttackPower = attackPower;
			Defense = defense;
			
			// NEW - Initialize combat properties
			Resistances = new DamageResistance();
			ActiveStatusEffects = new List<StatusEffectInstance>();
			DodgeChance = 0f;
			
			UnlockedAbilities = new List<SpecialAbility>();
	   		SelectedAbility = null;
	   		SpecialMeter = 0;
	   		MaxSpecialMeter = 100;
		}
		
		// In Character.cs - add this property
		public Armor EquippedArmor { get; protected set; }

		// Add method to equip armor
		public virtual void EquipArmor(Armor armor)
		{
 		   EquippedArmor = armor;
		}
		
		
		public virtual AttackResult AttackWithResult()
	   {
			int damage = AttackPower;
			return new AttackResult(damage, false, DamageType.Physical);
	   }

		public virtual AttackResult AttackWithResult(DamageType damageType)
		{
  		  int damage = AttackPower;
  		  return new AttackResult(damage, false, damageType);
		}
		
		
		public virtual int Attack()
		{
   			 return AttackWithResult().Damage;
		}

		public virtual int Attack(DamageType damageType)
		{
   			 return AttackWithResult(damageType).Damage;
		}
		
		public virtual DamageResult TakeDamageWithResult(int damage, DamageType damageType = DamageType.Physical)
{
	int damageBeforeDefense = damage;
	
	// Apply defense
	int totalDefense = Defense + (EquippedArmor?.DefenseBonus ?? 0);  // Dynamic defense!
	int damageAfterDefense = Mathf.Max(1, damage - totalDefense);
	
	// Apply resistance
	float resistance = Resistances.GetResistance(damageType);
	int damageAfterResistance = (int)(damageAfterDefense * (1 - resistance));
	
	// Check for dodge
	Random rand = new Random();
	bool dodged = rand.NextDouble() < DodgeChance;
	
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
		
		public virtual void TakeDamage(int damage, DamageType damageType = DamageType.Physical)
	  {
  		  TakeDamageWithResult(damage, damageType);
		}
		
		// Old TakeDamage for backwards compatibility
		public virtual void TakeDamage(int damage)
		{
			TakeDamage(damage, DamageType.Physical);
		}
		
		// NEW - Dodge check
		protected bool CheckDodge()
		{
			if (DodgeChance <= 0) return false;
			
			float roll = (float)_random.NextDouble();
			return roll < DodgeChance;
		}
		
		// NEW - Status effect management
		public void ApplyStatusEffect(StatusEffect effect, int duration, int potency = 0)
		{
			// Remove existing effect of same type
			RemoveStatusEffect(effect);
			
			// Add new effect
			var statusInstance = new StatusEffectInstance(effect, duration, potency);
			ActiveStatusEffects.Add(statusInstance);
			
			Console.WriteLine($"{Name} is now {effect}!");
		}
		
		public void RemoveStatusEffect(StatusEffect effect)
		{
			ActiveStatusEffects.RemoveAll(se => se.Effect == effect);
		}
		
		public bool HasStatusEffect(StatusEffect effect)
		{
			return ActiveStatusEffects.Any(se => se.Effect == effect);
		}
		
		public StatusEffectInstance? GetStatusEffect(StatusEffect effect)
		{
			return ActiveStatusEffects.FirstOrDefault(se => se.Effect == effect);
		}
		
		// NEW - Process status effects at start of turn
		public void ProcessStatusEffects()
		{
			if (ActiveStatusEffects.Count == 0) return;
			
			Console.WriteLine($"\n--- {Name}'s Status Effects ---");
			
			var effectsToProcess = new List<StatusEffectInstance>(ActiveStatusEffects);
			
			foreach (var effect in effectsToProcess)
			{
				Console.WriteLine(effect.GetDescription());
				
				switch (effect.Effect)
				{
					case StatusEffect.Poisoned:
						Health -= effect.Potency;
						Console.WriteLine($"{Name} takes {effect.Potency} poison damage!");
						break;
						
					case StatusEffect.Cursed:
						// Cursed already reduces stats, just show message
						break;
						
					case StatusEffect.Blessed:
						// Blessed already increases stats
						break;
				}
				
				// Decrement duration
				effect.DecrementDuration();
				
				// Remove if expired
				if (effect.IsExpired)
				{
					ActiveStatusEffects.Remove(effect);
					Console.WriteLine($"{Name} is no longer {effect.Effect}!");
				}
			}
		}
		
		// NEW - Get effective stat with status effect modifiers
		public int GetEffectiveAttack()
		{
			int attack = AttackPower;
			
			if (HasStatusEffect(StatusEffect.Cursed))
			{
				var curse = GetStatusEffect(StatusEffect.Cursed);
				attack = attack * (100 - curse!.Potency) / 100;
			}
			
			if (HasStatusEffect(StatusEffect.Blessed))
			{
				var blessing = GetStatusEffect(StatusEffect.Blessed);
				attack = attack * (100 + blessing!.Potency) / 100;
			}
			
			return attack;
		}
		
		public int GetEffectiveDefense()
{
	int defense = Defense;
	
	if (HasStatusEffect(StatusEffect.Cursed))
	{
		var curse = GetStatusEffect(StatusEffect.Cursed);
		defense = defense * (100 - curse!.Potency) / 100;
	}
	
	if (HasStatusEffect(StatusEffect.Blessed))
	{
		var blessing = GetStatusEffect(StatusEffect.Blessed);
		defense = defense * (100 + blessing!.Potency) / 100;
	}
	
	if (HasStatusEffect(StatusEffect.Defending))
	{
		defense = (int)(defense * 1.5f);
	}
	
	// NEW - Handle DefenseBoost from Street Lights Coming On
	if (HasStatusEffect(StatusEffect.DefenseBoost))
	{
		var boost = GetStatusEffect(StatusEffect.DefenseBoost);
		defense = defense * (100 + boost!.Potency) / 100;
	}
	
	return defense;
}
		
		public void Heal(int amount)
		{
			int oldHealth = Health;
			Health += amount;
			int actualHealing = Health - oldHealth;
			Console.WriteLine($"{Name} heals for {actualHealing} HP! Health: {Health}/{MaxHealth}");
		}
		
		protected virtual void OnDeath()
		{
			Console.WriteLine($"{Name} has been defeated!");
		}
		
		public void DisplayStats()
		{
			Console.WriteLine($"\n--- {Name} ---");
			Console.WriteLine($"Health: {Health}/{MaxHealth}");
			Console.WriteLine($"Attack: {AttackPower}");
			Console.WriteLine($"Defense: {Defense}");
			
			if (DodgeChance > 0)
			{
				Console.WriteLine($"Dodge: {(int)(DodgeChance * 100)}%");
			}
			
			if (ActiveStatusEffects.Count > 0)
			{
				Console.WriteLine("\nActive Effects:");
				foreach (var effect in ActiveStatusEffects)
				{
					Console.WriteLine($"  • {effect.GetDescription()}");
				}
			}
		}
		
		public void UnlockAbility(SpecialAbility ability)
	{
		if (!UnlockedAbilities.Contains(ability))
		{
			UnlockedAbilities.Add(ability);
			
			// Auto-select first ability
			if (SelectedAbility == null)
			{
				SelectedAbility = ability;
			}
			
			GD.Print($"{Name} learned {ability.Name}!");
		}
	}
	
	public void SelectAbility(int index)
	{
		if (index >= 0 && index < UnlockedAbilities.Count)
		{
			SelectedAbility = UnlockedAbilities[index];
		}
	}
	
	public void AddSpecialMeter(int amount)
	{
		SpecialMeter = Mathf.Min(SpecialMeter + amount, MaxSpecialMeter);
	}
	
	public bool CanUseSpecial()
	{
		return SelectedAbility != null && SpecialMeter >= SelectedAbility.Cost;
	}
	
	public void UseSpecial()
	{
		if (SelectedAbility != null)
		{
			SpecialMeter = Mathf.Max(0, SpecialMeter - SelectedAbility.Cost);
		}
	}
	
	public void ResetSpecialMeter()
	{
		SpecialMeter = 0;
	}
	}
}
