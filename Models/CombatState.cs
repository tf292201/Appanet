using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Appanet.Scripts.Models.SpecialAbilities;
namespace Appanet.Scripts.Models
{
	
	public class CombatAction
	{
		public CombatParticipant Actor { get; set; }
		public CombatParticipant Target { get; set; }
		public string ActionType { get; set; }  // "Attack" or "UseItem"
		public string ItemName { get; set; }    // For UseItem actions
	}
	
	public class CombatState
	{
		// All combatants
		public List<CombatParticipant> PlayerTeam { get; private set; }
		public List<CombatParticipant> EnemyTeam { get; private set; }
		
		// Turn management
		public TurnPhase CurrentPhase { get; private set; }
		public int RoundNumber { get; private set; }
		
		// Action queue for player team
		public List<CombatAction> QueuedActions { get; private set; }
		public int ActionsNeeded { get; private set; }
		
		// Combat state
		public bool IsCombatOver { get; private set; }
		public Team WinningTeam { get; private set; }
		
		// Events for UI to listen to
		public event Action<TurnPhase> OnPhaseChange;
		public event Action<Team> OnCombatEnd;
		public event Action<string> OnCombatLog;
		
		private List<CombatParticipant> _actorsWhoActedThisTurn = new List<CombatParticipant>();
		
		public CombatState()
		{
			PlayerTeam = new List<CombatParticipant>();
			EnemyTeam = new List<CombatParticipant>();
			QueuedActions = new List<CombatAction>();
			CurrentPhase = TurnPhase.PlayerTeamActing;
			RoundNumber = 1;
			IsCombatOver = false;
		}
		
		// Setup methods
		public void AddPlayerCharacter(Player player)
		{
			var participant = new CombatParticipant(player, Team.Player, isAI: false);
			PlayerTeam.Add(participant);
		}
		
		public void AddAlly(Character ally, string allyID)
		{
			var participant = new CombatParticipant(ally, Team.Player, isAI: false, allyID);  // Changed to false - player controls allies
			PlayerTeam.Add(participant);
		}
		
		public void AddEnemy(Enemy enemy)
		{
			var participant = new CombatParticipant(enemy, Team.Enemy, isAI: true);
			EnemyTeam.Add(participant);
		}
		
		// Initialize combat
		public void StartCombat()
{
	CurrentPhase = TurnPhase.PlayerTeamActing;
	RoundNumber = 1;
	IsCombatOver = false;
	
	// NEW - Reset all special meters to 0
	foreach (var member in PlayerTeam)
	{
		member.Character.ResetSpecialMeter();
	}
	
	foreach (var enemy in EnemyTeam)
	{
		enemy.Character.ResetSpecialMeter();
	}
	
	Log("=== COMBAT START ===");
	Log($"Player Team: {PlayerTeam.Count} members");
	Log($"Enemy Team: {EnemyTeam.Count} enemies");
	Log("");
	
	StartPlayerTurn();
}
		
		private void StartPlayerTurn()
{
	// Clear defending status from last turn
	foreach (var member in PlayerTeam.Where(p => p.IsAlive))
	{
		if (member.Character.HasStatusEffect(StatusEffect.Defending))
		{
			member.Character.RemoveStatusEffect(StatusEffect.Defending);
		}
	}
	
	 // NEW - Process status effects for player team
	foreach (var member in PlayerTeam.Where(p => p.IsAlive))
	{
		member.Character.ProcessStatusEffects();
	}
	
	_actorsWhoActedThisTurn.Clear();  // Reset for new turn
	QueuedActions.Clear();
	ActionsNeeded = PlayerTeam.Count(p => p.IsAlive);
	
	Log($"--- ROUND {RoundNumber}: YOUR TEAM'S TURN ---");
	
	CurrentPhase = TurnPhase.PlayerTeamActing;
	OnPhaseChange?.Invoke(CurrentPhase);
}
		
	public bool ExecuteAttackImmediately(CombatParticipant attacker, CombatParticipant target, float damageMultiplier = 1.0f)
	{
	if (IsCombatOver) return false;
	
	if (target == null || !target.IsAlive)
	{
		Log($"⚠️ {attacker.GetDisplayName()}'s target is already defeated!");
		return false;
	}
	
	// Mark this actor as having acted
	if (!_actorsWhoActedThisTurn.Contains(attacker))
	{
		_actorsWhoActedThisTurn.Add(attacker);
	}
	attacker.Character.AddSpecialMeter(25);
	
	// Get attack result with all details
	AttackResult attackResult = attacker.Character.AttackWithResult();
	
	// Apply the timing multiplier
	int modifiedDamage = (int)(attackResult.Damage * damageMultiplier);
	
	// Get weapon info
	string weaponInfo = "";
	if (attacker.Character is Player player && player.EquippedWeapon != null)
	{
		weaponInfo = $" with {player.EquippedWeapon.Name}";
	}
	else if (attacker.Character is Ally ally && ally.EquippedWeapon != null)
	{
		weaponInfo = $" with {ally.EquippedWeapon.Name}";
	}
	
	// Apply damage with the modified amount
	DamageResult damageResult = target.Character.TakeDamageWithResult(
		modifiedDamage,
		attackResult.DamageType
	);
	
	if (!damageResult.WasDodged && target.IsAlive)
	{
	target.Character.AddSpecialMeter(15);
	}
	
	// Build combat log message
	if (damageResult.WasDodged)
	{
		Log($"💨 {target.GetDisplayName()} DODGED {attacker.GetDisplayName()}'s attack{weaponInfo}!");
	}
	else
	{
		string critText = attackResult.IsCritical ? " [CRITICAL HIT!]" : "";
		string timingText = damageMultiplier > 1.0f ? $" [x{damageMultiplier:F1}]" : "";
		string damageTypeText = attackResult.DamageType != DamageType.Physical 
			? $" ({attackResult.DamageType})" 
			: "";
		
		Log($"⚔️ {attacker.GetDisplayName()} attacks{weaponInfo} for {damageResult.DamageTaken} damage{damageTypeText}{timingText}{critText}! " +
			$"[{target.Character.Health}/{target.Character.MaxHealth} HP remaining]");
		
		if (!target.IsAlive)
		{
			Log($"💀 {target.GetDisplayName()} has been defeated!");
			
			if (target.Character is Enemy enemy)
			{
				int xpDrop = enemy.ExperienceReward;
				int moneyDrop = enemy.GetMoneyDrop();
				
				target.XPDropped = xpDrop;
				target.MoneyDropped = moneyDrop;
				
				Log($"   💰 Dropped: {xpDrop} XP, ${moneyDrop}");
			}
		}
	}
	
	// Check if combat ended
	CheckCombatEnd();
	
	return true;
}

public bool ExecuteUseItemImmediately(CombatParticipant actor, string itemName, CombatParticipant target)
{
	if (IsCombatOver) return false;
	
	// Mark this actor as having acted
	if (!_actorsWhoActedThisTurn.Contains(actor))
	{
		_actorsWhoActedThisTurn.Add(actor);
	}
	
	actor.Character.AddSpecialMeter(25);
	
	if (target == null)
		target = actor;
	
	int healthBefore = target.Character.Health;
	bool wasDefeated = !target.IsAlive;  // ← ADD THIS
	
	// Simple healing logic based on item name
	int healAmount = itemName switch
	{
		"Health Potion" => 30,
		"Mega Potion" => 60,
		"Gas Station Coffee" => 15,
		"Moon Pie & RC Cola" => 18,
		"Pepperoni Roll" => 22,
		"Mason Jar Mountain Dew" => 25,
		"Dial-Up Healing Tonic (56k BITTER)" => 40,
		"Coal Dust Candied Pecans" => 20,
		"VHS Comfort Blanket" => 70,
		"Terminal-Cured Jerky" => 30,
		_ => 20
	};
	
	target.Character.Heal(healAmount);
	int healthRestored = target.Character.Health - healthBefore;
	
	// Check if we just revived someone
	if (wasDefeated && target.IsAlive)  // ← ADD THIS BLOCK
	{
		if (target == actor)
		{
			Log($"✨ {actor.GetDisplayName()} uses {itemName} and is REVIVED with {healthRestored} HP! " +
				$"[{target.Character.Health}/{target.Character.MaxHealth} HP]");
		}
		else
		{
			Log($"✨ {actor.GetDisplayName()} uses {itemName} on {target.GetDisplayName()} and REVIVES them with {healthRestored} HP! " +
				$"[{target.Character.Health}/{target.Character.MaxHealth} HP]");
		}
	}
	else if (target == actor)
	{
		Log($"💊 {actor.GetDisplayName()} uses {itemName} and restores {healthRestored} HP! " +
			$"[{target.Character.Health}/{target.Character.MaxHealth} HP]");
	}
	else
	{
		Log($"💊 {actor.GetDisplayName()} uses {itemName} on {target.GetDisplayName()}, restoring {healthRestored} HP! " +
			$"[{target.Character.Health}/{target.Character.MaxHealth} HP]");
	}
	
	// Check if combat ended
	CheckCombatEnd();
	
	return true;
}

public bool ExecuteSpecialAbilityImmediately(CombatParticipant actor, SpecialAbility ability, CombatParticipant singleTarget = null)
{
	if (IsCombatOver) return false;
	
	// Check if can use
	if (!actor.Character.CanUseSpecial())
	{
		Log($"⚠️ {actor.GetDisplayName()} doesn't have enough Special Power!");
		return false;
	}
	
	// Mark this actor as having acted
	if (!_actorsWhoActedThisTurn.Contains(actor))
	{
		_actorsWhoActedThisTurn.Add(actor);
	}
	
	// Execute the ability
	List<CombatParticipant> targets = ability.TargetType switch
	{
		TargetType.Self => new List<CombatParticipant> { actor },
		TargetType.AllAllies => GetAliveAllies(),
		TargetType.AllEnemies => GetAliveEnemies(),
		TargetType.SingleEnemy => singleTarget != null ? new List<CombatParticipant> { singleTarget } : new List<CombatParticipant>(),
		TargetType.SingleAlly => singleTarget != null ? new List<CombatParticipant> { singleTarget } : new List<CombatParticipant>(),
		_ => new List<CombatParticipant>()
	};
	
	ability.Execute(actor, targets, this);
	
	// Spend the special meter
	actor.Character.UseSpecial();
	
	// Check if combat ended
	CheckCombatEnd();
	
	return true;
}



public bool AllPlayersHaveActed()
{
	return _actorsWhoActedThisTurn.Count >= PlayerTeam.Count(p => p.IsAlive);
}

public void StartEnemyTurn()
{
	if (IsCombatOver) return;
	
	foreach (var enemy in EnemyTeam.Where(e => e.IsAlive))
	{
		enemy.Character.ProcessStatusEffects();
	}
	
	CurrentPhase = TurnPhase.EnemyTeamExecuting;
	OnPhaseChange?.Invoke(CurrentPhase);
	
	Log("");
	Log("--- ENEMY TEAM'S TURN ---");
	
	var aliveEnemies = EnemyTeam.Where(e => e.IsAlive).ToList();
	var aliveAllies = PlayerTeam.Where(p => p.IsAlive).ToList();
	
	if (aliveAllies.Count == 0)
	{
		CheckCombatEnd();
		return;
	}
	
	foreach (var enemy in aliveEnemies)
	{
		aliveAllies = PlayerTeam.Where(p => p.IsAlive).ToList();
		if (aliveAllies.Count == 0) break;
		
		// NEW - Check if enemy is confused
		CombatParticipant target;
		bool isConfused = enemy.Character.HasStatusEffect(StatusEffect.Confused);
		
		if (isConfused)
		{
			// Confused enemies attack random targets (including other enemies!)
			var allPossibleTargets = new List<CombatParticipant>();
			allPossibleTargets.AddRange(aliveAllies);
			allPossibleTargets.AddRange(EnemyTeam.Where(e => e.IsAlive && e != enemy)); // Other enemies
			
			target = allPossibleTargets[GD.RandRange(0, allPossibleTargets.Count - 1)];
			Log($"🌀 {enemy.GetDisplayName()} is CONFUSED!");
		}
		else
		{
			// Normal behavior - attack player team
			target = aliveAllies[GD.RandRange(0, aliveAllies.Count - 1)];
		}
		
		AttackResult attackResult = enemy.Character.AttackWithResult();
		
		DamageResult damageResult = target.Character.TakeDamageWithResult(
			attackResult.Damage,
			attackResult.DamageType
		);
		
		// Gain meter for taking damage
		if (!damageResult.WasDodged && target.IsAlive)
		{
			target.Character.AddSpecialMeter(15);
		}
		
		if (damageResult.WasDodged)
		{
			Log($"💨 {target.GetDisplayName()} DODGED {enemy.GetDisplayName()}'s attack!");
		}
		else
		{
			string critText = attackResult.IsCritical ? " [CRITICAL HIT!]" : "";
			string damageTypeText = attackResult.DamageType != DamageType.Physical 
				? $" ({attackResult.DamageType})" 
				: "";
			
			Log($"💥 {enemy.GetDisplayName()} attacks {target.GetDisplayName()} for {damageResult.DamageTaken} damage{damageTypeText}{critText}! " +
				$"[{target.Character.Health}/{target.Character.MaxHealth} HP remaining]");
			
			if (!target.IsAlive)
			{
				Log($"💀 {target.GetDisplayName()} has been defeated!");
			}
		}
	}
	
	Log("");
	
	CheckCombatEnd();
	
	if (!IsCombatOver)
	{
		RoundNumber++;
		StartPlayerTurn();
	}
}

// Add this new method - execute defend immediately
public bool ExecuteDefendImmediately(CombatParticipant defender)
{
	if (IsCombatOver) return false;
	
	// Mark this actor as having acted
	if (!_actorsWhoActedThisTurn.Contains(defender))
	{
		_actorsWhoActedThisTurn.Add(defender);
	}
	defender.Character.AddSpecialMeter(25);
	
	// Apply Defending status effect (lasts 1 turn, defense boost handled by GetEffectiveDefense)
	defender.Character.ApplyStatusEffect(StatusEffect.Defending, 1);
	
	Log($"🛡️ {defender.GetDisplayName()} takes a defensive stance! Defense increased for this turn.");
	
	return true;
}


		
		// Queue an action for a team member
		public void QueueAction(CombatParticipant actor, CombatParticipant target, string actionType, string itemName = null)
		{
			var action = new CombatAction
			{
				Actor = actor,
				Target = target,
				ActionType = actionType,
				ItemName = itemName
			};
			
			QueuedActions.Add(action);
			
			Log($"[Queued] {actor.GetDisplayName()} will {actionType.ToLower()} {(target != null ? target.GetDisplayName() : "")}");
		}
		
		public bool AllActionsQueued()
		{
			return QueuedActions.Count >= ActionsNeeded;
		}
		
		public CombatParticipant GetNextActorNeedingAction()
		{
			  var aliveMembers = PlayerTeam.Where(p => p.IsAlive).ToList();
	
  			  return aliveMembers.FirstOrDefault(member => !_actorsWhoActedThisTurn.Contains(member));
		}
		
		public void ExecutePlayerActions()
{
	if (!AllActionsQueued())
	{
		Log("Not all actions queued yet!");
		return;
	}
	
	CurrentPhase = TurnPhase.PlayerTeamExecuting;
	OnPhaseChange?.Invoke(CurrentPhase);
	
	Log("");
	Log("=== EXECUTING YOUR TEAM'S ACTIONS ===");
	
	foreach (var action in QueuedActions)
	{
		if (!action.Actor.IsAlive)
		{
			Log($"[Skipped] {action.Actor.GetDisplayName()} is defeated");
			continue;
		}
		
		if (action.ActionType == "Attack")
		{
			ExecuteAttack(action.Actor, action.Target);
		}
		else if (action.ActionType == "UseItem")
		{
			ExecuteUseItem(action.Actor, action.ItemName, action.Target);  // ← Pass target here
		}
	}
	
	QueuedActions.Clear();
	
	CheckCombatEnd();
	
	if (!IsCombatOver)
	{
		Log("");
		ExecuteEnemyTurn();
	}
}
		
	  private void ExecuteAttack(CombatParticipant attacker, CombatParticipant target)
{
	if (target == null || !target.IsAlive)
	{
		Log($"⚠️ {attacker.GetDisplayName()}'s target is already defeated!");
		return;
	}
	
	// Get attack result with all details
	AttackResult attackResult = attacker.Character.AttackWithResult();
	
	// Get weapon info
	string weaponInfo = "";
	if (attacker.Character is Player player && player.EquippedWeapon != null)
	{
		weaponInfo = $" with {player.EquippedWeapon.Name}";
	}
	else if (attacker.Character is Ally ally && ally.EquippedWeapon != null)
	{
		weaponInfo = $" with {ally.EquippedWeapon.Name}";
	}
	
	// Apply damage and get result
	DamageResult damageResult = target.Character.TakeDamageWithResult(
		attackResult.Damage, 
		attackResult.DamageType
	);
	
	// Build combat log message
	if (damageResult.WasDodged)
	{
		Log($"💨 {target.GetDisplayName()} DODGED {attacker.GetDisplayName()}'s attack{weaponInfo}!");
	}
	else
	{
		string critText = attackResult.IsCritical ? " [CRITICAL HIT!]" : "";
		string damageTypeText = attackResult.DamageType != DamageType.Physical 
			? $" ({attackResult.DamageType})" 
			: "";
		
		Log($"⚔️ {attacker.GetDisplayName()} attacks{weaponInfo} for {damageResult.DamageTaken} damage{damageTypeText}{critText}! " +
			$"[{target.Character.Health}/{target.Character.MaxHealth} HP remaining]");
		
		if (!target.IsAlive)
		{
			Log($"💀 {target.GetDisplayName()} has been defeated!");
			
			if (target.Character is Enemy enemy)
			{
				int xpDrop = enemy.ExperienceReward;
				int moneyDrop = enemy.GetMoneyDrop();
				
				target.XPDropped = xpDrop;
				target.MoneyDropped = moneyDrop;
				
				Log($"   💰 Dropped: {xpDrop} XP, ${moneyDrop}");
			}
		}
	}
}
		
		private void ExecuteUseItem(CombatParticipant actor, string itemName, CombatParticipant target = null)
{
	// If no target specified, use self
	if (target == null)
		target = actor;
	
	// Get the consumable by name (we need to create it temporarily since it was already removed)
	// Actually, we need to store the consumable object itself in the action!
	// For now, let's just apply the healing based on known item stats
	
	int healthBefore = target.Character.Health;
	
	// Simple healing logic based on item name
	// This is a workaround since the item was already removed from inventory
	int healAmount = itemName switch
	{
		"Health Potion" => 30,
		"Mega Potion" => 60,
		"Gas Station Coffee" => 15,
		"Moon Pie & RC Cola" => 18,
		"Pepperoni Roll" => 22,
		"Mason Jar Mountain Dew" => 25,
		"Dial-Up Healing Tonic (56k BITTER)" => 40,
		"Coal Dust Candied Pecans" => 20,
		"VHS Comfort Blanket" => 70,
		"Terminal-Cured Jerky" => 30,
		_ => 20 // default healing
	};
	
	target.Character.Heal(healAmount);
	int healthRestored = target.Character.Health - healthBefore;
	
	if (target == actor)
	{
		Log($"💊 {actor.GetDisplayName()} uses {itemName} and restores {healthRestored} HP! " +
			$"[{target.Character.Health}/{target.Character.MaxHealth} HP]");
	}
	else
	{
		Log($"💊 {actor.GetDisplayName()} uses {itemName} on {target.GetDisplayName()}, restoring {healthRestored} HP! " +
			$"[{target.Character.Health}/{target.Character.MaxHealth} HP]");
	}
}
		
		// Execute enemy team turn
		// In CombatState.cs - ExecuteEnemyTurn
private void ExecuteEnemyTurn()
{
	CurrentPhase = TurnPhase.EnemyTeamExecuting;
	OnPhaseChange?.Invoke(CurrentPhase);
	
	Log("--- ENEMY TEAM'S TURN ---");
	
	var aliveEnemies = EnemyTeam.Where(e => e.IsAlive).ToList();
	var aliveAllies = PlayerTeam.Where(p => p.IsAlive).ToList();
	
	if (aliveAllies.Count == 0)
	{
		CheckCombatEnd();
		return;
	}
	
	foreach (var enemy in aliveEnemies)
	{
		aliveAllies = PlayerTeam.Where(p => p.IsAlive).ToList();
		if (aliveAllies.Count == 0) break;
		
		var target = aliveAllies[GD.RandRange(0, aliveAllies.Count - 1)];
		
		// Get attack result
		AttackResult attackResult = enemy.Character.AttackWithResult();
		
		// Apply damage
		DamageResult damageResult = target.Character.TakeDamageWithResult(
			attackResult.Damage,
			attackResult.DamageType
		);
		
		if (!damageResult.WasDodged && target.IsAlive)
	{
		target.Character.AddSpecialMeter(15);
	}
		
		// Log result
		if (damageResult.WasDodged)
		{
			Log($"💨 {target.GetDisplayName()} DODGED {enemy.GetDisplayName()}'s attack!");
		}
		else
		{
			string critText = attackResult.IsCritical ? " [CRITICAL HIT!]" : "";
			string damageTypeText = attackResult.DamageType != DamageType.Physical 
				? $" ({attackResult.DamageType})" 
				: "";
			
			Log($"💥 {enemy.GetDisplayName()} attacks for {damageResult.DamageTaken} damage{damageTypeText}{critText}! " +
				$"[{target.Character.Health}/{target.Character.MaxHealth} HP remaining]");
			
			if (!target.IsAlive)
			{
				Log($"💀 {target.GetDisplayName()} has been defeated!");
			}
		}
	}
	
	Log("");
	
	CheckCombatEnd();
	
	if (!IsCombatOver)
	{
		RoundNumber++;
		StartPlayerTurn();
	}
}
		
		// Target selection helpers
		public List<CombatParticipant> GetValidTargets()
		{
			return EnemyTeam.Where(e => e.IsAlive).ToList();
		}
		
		public List<CombatParticipant> GetAliveAllies()
		{
			return PlayerTeam.Where(p => p.IsAlive).ToList();
		}
		
		public List<CombatParticipant> GetAliveEnemies()
		{
			return EnemyTeam.Where(e => e.IsAlive).ToList();
		}
		
		// Combat end check
		private void CheckCombatEnd()
{
	bool playerTeamAlive = PlayerTeam.Any(p => p.IsAlive);
	bool enemyTeamAlive = EnemyTeam.Any(e => e.IsAlive);
	
	if (!playerTeamAlive)
	{
		IsCombatOver = true;
		WinningTeam = Team.Enemy;
		CurrentPhase = TurnPhase.CombatEnd;
		OnPhaseChange?.Invoke(CurrentPhase);
		Log("");
		Log("=== DEFEAT ===");
		OnCombatEnd?.Invoke(Team.Enemy);
	}
	else if (!enemyTeamAlive)
	{
		IsCombatOver = true;
		WinningTeam = Team.Player;
		CurrentPhase = TurnPhase.CombatEnd;
		OnPhaseChange?.Invoke(CurrentPhase);
		Log("");
		Log("=== VICTORY ===");
		
		// Award XP and money to player - use STORED values
		if (PlayerTeam.FirstOrDefault(p => p.AllyID == null)?.Character is Player player)
		{
			// Use the stored drops instead of calling GetMoneyDrop again
			int totalXP = EnemyTeam.Sum(e => e.XPDropped);      // ← CHANGED
			int totalMoney = EnemyTeam.Sum(e => e.MoneyDropped); // ← CHANGED
			
			player.GainExperience(totalXP);
			player.AddMoney(totalMoney);
			
			Log($"Gained {totalXP} XP and ${totalMoney}!");
		}
		
		OnCombatEnd?.Invoke(Team.Player);
	}
}
		
		public void Log(string message)
		{
			OnCombatLog?.Invoke(message);
			GD.Print(message);
		}
	}
}
