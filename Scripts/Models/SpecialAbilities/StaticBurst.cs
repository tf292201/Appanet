using System;
using System.Collections.Generic;
using Godot;
using Appanet.Scripts.Models.Combat; 
using Appanet.Scripts.Models.Characters;  

namespace Appanet.Scripts.Models.SpecialAbilities
{
	public class StaticBurst : SpecialAbility
	{
		public StaticBurst() 
			: base(
				"Static Burst",
				"Channel electromagnetic energy into a devastating single-target attack. Deals 2x damage.",
				75,  // Lower cost since it's single target
				1,
				TargetType.SingleEnemy,
                "⚡"
			)
		{
		}
		
		public override void Execute(CombatParticipant user, List<CombatParticipant> targets, CombatState combat)
		{
			if (targets == null || targets.Count == 0)
			{
				combat.Log("⚠️ No target selected!");
				return;
			}
			
			var target = targets[0];  // Single target
			
			// Calculate damage (2x user's attack power)
			int baseDamage = (int)(user.Character.AttackPower * 2.0f);
			
			// Add weapon bonus if equipped
			if (user.Character is Player player && player.EquippedWeapon != null)
			{
				baseDamage += player.EquippedWeapon.AttackBonus;
			}
			
			combat.Log($"⚡ {user.GetDisplayName()} uses {Name}!");
			combat.Log("Crackling electricity surges toward the target...");
			
			var damageResult = target.Character.TakeDamageWithResult(baseDamage, DamageType.Electric);
			
			if (damageResult.WasDodged)
			{
				combat.Log($"   {target.GetDisplayName()} dodged the Static Burst!");
			}
			else
			{
				combat.Log($"   {target.GetDisplayName()} takes {damageResult.DamageTaken} electric damage! [{target.Character.Health}/{target.Character.MaxHealth} HP]");
				
				if (!target.IsAlive)
				{
					combat.Log($"   💀 {target.GetDisplayName()} has been defeated!");
					
					if (target.Character is Enemy enemy)
					{
						int xpDrop = enemy.ExperienceReward;
						int moneyDrop = enemy.GetMoneyDrop();
						
						target.XPDropped = xpDrop;
						target.MoneyDropped = moneyDrop;
						
						combat.Log($"   💰 Dropped: {xpDrop} XP, ${moneyDrop}");
					}
				}
			}
		}
	}
}
