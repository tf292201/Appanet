using System;
using System.Collections.Generic;
using Godot;
using Appanet.Scripts.Models.Characters;    
using Appanet.Scripts.Models.Combat; 


namespace Appanet.Scripts.Models.SpecialAbilities
{
	public enum TargetType
	{
		Self,
		SingleAlly,
		AllAllies,
		SingleEnemy,
		AllEnemies
	}
	
	public abstract class SpecialAbility
	{
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public int Cost { get; protected set; }
		public int RequiredLevel { get; protected set; }
		public TargetType TargetType { get; protected set; }
		public string AbilityIcon { get; protected set; }
		
		protected SpecialAbility(string name, string description, int cost, int requiredLevel, TargetType targetType, string icon)
		{
			Name = name;
			Description = description;
			Cost = cost;
			RequiredLevel = requiredLevel;
			TargetType = targetType;
			AbilityIcon = icon;
		}
		
		// Abstract method - each ability implements its own effect
		public abstract void Execute(CombatParticipant user, List<CombatParticipant> targets, CombatState combat);
		
		// Check if user can use this ability
		public virtual bool CanUse(Character user)
		{
			return user.SpecialMeter >= Cost;
		}
		
		// Get description for UI
		public virtual string GetFullDescription()
		{
			return $"{AbilityIcon} {Name}\n{Description}\nCost: {Cost} SP";
		}
	}
}
