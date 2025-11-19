using System;
using System.Collections.Generic;
using Godot;

namespace Appanet.Scripts.Models.SpecialAbilities
{
	public class HaintWind : SpecialAbility
	{
		public HaintWind() 
			: base(
				"Haint Wind",
				"Summons a spectral wind that sweeps the battlefield. Hits all enemies for 1.5x damage.",
				100,
				1,
				TargetType.AllEnemies,
                "💨"
			)
		{
		}
		
		public override void Execute(CombatParticipant user, List<CombatParticipant> targets, CombatState combat)
		{
			// Calculate damage (1.5x user's attack power)
			int baseDamage = (int)(user.Character.AttackPower * 1.5f);
			
			// Add weapon bonus if equipped
			if (user.Character is Ally ally && ally.EquippedWeapon != null)
			{
				baseDamage += ally.EquippedWeapon.AttackBonus;
			}
			
			combat.Log($"💨 {user.GetDisplayName()} uses {Name}!");
			combat.Log("A cold wind rises from the hollers...");
			
			foreach (var enemy in combat.GetAliveEnemies())
			{
				var damageResult = enemy.Character.TakeDamageWithResult(baseDamage, DamageType.Spectral);
				
				if (damageResult.WasDodged)
				{
					combat.Log($"   {enemy.GetDisplayName()} dodged the Haint Wind!");
				}
				else
				{
					combat.Log($"   {enemy.GetDisplayName()} takes {damageResult.DamageTaken} spectral damage! [{enemy.Character.Health}/{enemy.Character.MaxHealth} HP]");
					
					if (!enemy.IsAlive)
					{
						combat.Log($"   💀 {enemy.GetDisplayName()} has been defeated!");
					}
				}
			}
		}
	}
}
