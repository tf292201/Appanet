using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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
			
			Log("=== COMBAT START ===");
			Log($"Player Team: {PlayerTeam.Count} members");
			Log($"Enemy Team: {EnemyTeam.Count} enemies");
			Log("");
			
			StartPlayerTurn();
		}
		
		private void StartPlayerTurn()
		{
			QueuedActions.Clear();
			ActionsNeeded = PlayerTeam.Count(p => p.IsAlive);
			
			Log($"--- ROUND {RoundNumber}: YOUR TEAM'S TURN ---");
			Log($"Choose actions for all {ActionsNeeded} team members");
			
			CurrentPhase = TurnPhase.PlayerTeamActing;
			OnPhaseChange?.Invoke(CurrentPhase);
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
			var actorsWhoActed = QueuedActions.Select(a => a.Actor).ToList();
			
			return aliveMembers.FirstOrDefault(member => !actorsWhoActed.Contains(member));
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
		
		private void Log(string message)
		{
			OnCombatLog?.Invoke(message);
			GD.Print(message);
		}
	}
}
