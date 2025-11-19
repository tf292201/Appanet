using System;
using System.Collections.Generic;
using Godot;

namespace Appanet.Scripts.Models.SpecialAbilities
{
	public class StreetLightsComingOn : SpecialAbility
	{
		public StreetLightsComingOn() 
			: base(
				"Street Lights Coming On",
				"The street lights flicker on. All allies gain +50% defense for 2 turns.",
				100,
				1,
				TargetType.AllAllies,
                "💡"
			)
		{
		}
		
		public override void Execute(CombatParticipant user, List<CombatParticipant> targets, CombatState combat)
		{
			foreach (var ally in combat.GetAliveAllies())
			{
				// Apply a defense boost (we'll use a new status effect)
				ally.Character.ApplyStatusEffect(StatusEffect.DefenseBoost, 2, 50); // 50% boost for 2 turns
			}
			
			combat.Log($"💡 {user.GetDisplayName()} uses {Name}!");
			combat.Log("The street lights flicker on... it's time to come home safe.");
			combat.Log("All allies feel protected! (+50% DEF for 2 turns)");
		}
	}
}
