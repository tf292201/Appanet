using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public enum Team
	{
		Player,
		Enemy
	}
	
	public enum TurnPhase
	{
		PlayerTeamActing,
		PlayerTeamExecuting,
		EnemyTeamExecuting,
		CombatEnd
	}
	// Damage types that weapons can deal
	public enum DamageType
	{
		Physical,      // Normal attacks
		Electric,      // Tech-based electrical damage
		Psychic,       // Mind/mental attacks
		Spectral,      // Ghost/spirit attacks
		Curse          // Hex/curse damage
	}
	
	// Enemy classification types
	public enum EnemyType
	{
		Normal,        // Regular physical enemies
		Spectral,      // Ghosts, spirits, ethereal beings
		Psychic,       // Mind-based entities
		Folklore,      // Appalachian folklore creatures (haints, etc.)
		Tech,          // Technology-based enemies
		Illusion       // Fake/illusory enemies
	}
	
	// Status effects that can be applied
	public enum StatusEffect
	{
		None,
		Poisoned,      // Damage over time
		Stunned,       // Skip next turn
		Confused,      // Attacks randomly/misses
		Cursed,        // Stat penalties
		Possessed,     // Lose control
		Blessed,       // Stat bonuses
		Defending,      // Increased defense temporarily
		DefenseBoost 
	}
}
