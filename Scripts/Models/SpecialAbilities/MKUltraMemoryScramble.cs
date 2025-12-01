using System;
using System.Collections.Generic;
using Godot;
using Appanet.Scripts.Models.Combat;   

namespace Appanet.Scripts.Models.SpecialAbilities
{
	public class MKUltraMemoryScramble : SpecialAbility
	{
		public MKUltraMemoryScramble() 
			: base(
				"MK-Ultra Memory Scramble",
				"Scrambles enemy minds with psychic static. All enemies become confused for 2 turns.",
				100,
				1,
				TargetType.AllEnemies,
                "🌀"
			)
		{
		}
		
		public override void Execute(CombatParticipant user, List<CombatParticipant> targets, CombatState combat)
		{
			foreach (var enemy in combat.GetAliveEnemies())
			{
				enemy.Character.ApplyStatusEffect(StatusEffect.Confused, 2);
			}
			
			combat.Log($"🌀 {user.GetDisplayName()} uses {Name}!");
			combat.Log("Psychic static floods the battlefield...");
			combat.Log("All enemies are CONFUSED! They may attack each other!");
		}
	}
}
