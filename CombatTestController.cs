using Godot;
using Appanet.Scripts.Models;
using Appanet.Scripts.Managers;
using System.Collections.Generic;
using Appanet.Scripts.Combat;
using System.Linq;

namespace Appanet.Scripts.Tests
{
	public partial class CombatTestController : Node
	{
		private CombatState _combat;
		private CombatParticipant _currentActorSelecting;
		
		// UI references
		private VBoxContainer _playerTeamUI;
		private VBoxContainer _enemyTeamUI;
		private RichTextLabel _combatLog;
		private HBoxContainer _actionButtons;
		private Button _attackBtn;
		private Button _itemBtn;
		private VBoxContainer _targetSelection;
		private Label _turnIndicator;
		private Consumable _selectedItem;
		private CombatParticipant _pendingAttackTarget;  
		private float _currentAttackMultiplier = 1.0f;   
		private Button _defendBtn; 
		private Button _specialBtn; 
		private Button _equipBtn;
		
		public override void _Ready()
		{
			// Get UI references
			_playerTeamUI = GetNode<VBoxContainer>("../MainLayout/PlayerTeamPanel/PlayerTeam");
			_enemyTeamUI = GetNode<VBoxContainer>("../MainLayout/EnemyTeamPanel/EnemyTeam");
			_combatLog = GetNode<RichTextLabel>("../CombatLog");
			_actionButtons = GetNode<HBoxContainer>("../ActionButtons");
			_attackBtn = GetNode<Button>("../ActionButtons/AttackBtn");
			_itemBtn = GetNode<Button>("../ActionButtons/ItemBtn");
			_defendBtn = GetNode<Button>("../ActionButtons/DefendBtn"); 
			_specialBtn = GetNode<Button>("../ActionButtons/SpecialBtn");
			_targetSelection = GetNode<VBoxContainer>("../TargetSelection");
			_equipBtn = GetNode<Button>("../ActionButtons/EquipBtn");
			
			
			// Create turn indicator label
			_turnIndicator = new Label();
			_turnIndicator.Position = new Vector2(400, 30);
			_turnIndicator.AddThemeFontSizeOverride("font_size", 20);
			GetNode("..").AddChild(_turnIndicator);
			
			
		
			
			// Connect button signals
			_attackBtn.Pressed += OnAttackButtonPressed;
			_itemBtn.Pressed += OnItemButtonPressed;
			_defendBtn.Pressed += OnDefendButtonPressed; 
			_specialBtn.Pressed += OnSpecialButtonPressed; 
			_equipBtn.Pressed += OnManageEquipmentPressed;
			
			
			InitializeCombat();
		}
		
private void OnManageEquipmentPressed()
{
	GetTree().ChangeSceneToFile("res://InventoryScene.tscn");
	}
		
		
		
		
		
	// In CombatTestController.cs - InitializeCombat()
private void InitializeCombat()
{
	_combat = new CombatState();
	
	// Add player with armor
	var player = GameManager.Instance.Player;
	var playerArmor = Armor.CreateLeatherJacket();  // Starting armor
	player.Inventory.AddItem(playerArmor);
	player.EquipArmor(playerArmor);
	_combat.AddPlayerCharacter(player);
	
	// Michael with weapon and armor
	var michael = Ally.CreateMichaelWebb();
	var michaelWeapon = Weapon.CreateMagliteFlashlight();
	var michaelArmor = Armor.CreateDenimJacket();
	michael.Inventory.AddItem(michaelWeapon);
	michael.Inventory.AddItem(michaelArmor);
	michael.EquipWeapon(michaelWeapon);
	michael.EquipArmor(michaelArmor);
	_combat.AddAlly(michael, "michael");
	GameManager.Instance.AddAllyToParty(michael);
	
	// Casey with weapon and armor
	var casey = Ally.CreateCase();
	var caseyWeapon = Weapon.CreateTireIron();
	var caseyArmor = Armor.CreateFlannel();
	casey.Inventory.AddItem(caseyWeapon);
	casey.Inventory.AddItem(caseyArmor);
	casey.EquipWeapon(caseyWeapon);
	casey.EquipArmor(caseyArmor);
	_combat.AddAlly(casey, "case");
	GameManager.Instance.AddAllyToParty(casey);
	
	// Add enemies
	_combat.AddEnemy(Enemy.CreateBackroadsGremmlin());
	_combat.AddEnemy(Enemy.CreateBarnWirePossum());
	_combat.AddEnemy(Enemy.CreateOffGridScavver());
	
	// Subscribe to events
	_combat.OnPhaseChange += OnPhaseChange;
	_combat.OnCombatEnd += OnCombatEnd;
	_combat.OnCombatLog += Log;
	
	
	_combat.StartCombat();
	
	UpdateUI();
	PromptNextAction();
}
		
		private void OnPhaseChange(TurnPhase phase)
		{
			UpdateUI();
			
			if (phase == TurnPhase.PlayerTeamActing)
			{
				PromptNextAction();
			}
		}
		private void PromptNextAction()
{
	_currentActorSelecting = _combat.GetNextActorNeedingAction();
	
	if (_currentActorSelecting == null)
	{
		HidePlayerActions();
		_combat.ExecutePlayerActions();
	}
	else
	{
		ShowPlayerActions();
		UpdateUI();
		_turnIndicator.Text = $"Choose action for: {_currentActorSelecting.GetDisplayName()}";
		
		// Get player to check party inventory
		var player = _combat.PlayerTeam.FirstOrDefault(p => p.Character is Player)?.Character as Player;
		
		// Enable item button if party has ANY consumables
		if (player != null)
		{
			var hasConsumables = player.Inventory.GetAllItems().Any(item => item is Consumable);
			_itemBtn.Disabled = !hasConsumables;
		}
		else
		{
			_itemBtn.Disabled = true;
		}
		
		// NEW - Enable/disable special button based on meter
		_specialBtn.Disabled = !_currentActorSelecting.Character.CanUseSpecial();
	}
}
		
		
		private void ShowPlayerActions()
		{
			_actionButtons.Visible = true;
			_targetSelection.Visible = false;
		}
		
		private void HidePlayerActions()
		{
			_actionButtons.Visible = false;
			_targetSelection.Visible = false;
			_turnIndicator.Text = "";
		}
		

		
		private void ShowTargetSelection()
		{
			_targetSelection.Visible = true;
			_actionButtons.Visible = false;
			
			// Clear previous buttons
			foreach (Node child in _targetSelection.GetChildren())
			{
				child.QueueFree();
			}
			
			// Add title
			var title = new Label();
			title.Text = $"{_currentActorSelecting.GetDisplayName()}: Select Target";
			title.HorizontalAlignment = HorizontalAlignment.Center;
			title.AddThemeColorOverride("font_color", new Color(1, 1, 0));
			_targetSelection.AddChild(title);
			
			// Create button for each enemy
			var targets = _combat.GetValidTargets();
			foreach (var target in targets)
			{
				var btn = new Button();
				btn.Text = $"{target.Character.Name}\nHP: {target.Character.Health}/{target.Character.MaxHealth}";
				btn.CustomMinimumSize = new Vector2(250, 50);
				btn.Pressed += () => OnTargetSelected(target);
				_targetSelection.AddChild(btn);
			}
			
			// Add separator
			var separator = new HSeparator();
			_targetSelection.AddChild(separator);
			
			// Add cancel button
			var cancelBtn = new Button();
			cancelBtn.Text = "← Back";
			cancelBtn.CustomMinimumSize = new Vector2(250, 40);
			cancelBtn.Pressed += () =>
			{
				_targetSelection.Visible = false;
				_actionButtons.Visible = true;
			};
			_targetSelection.AddChild(cancelBtn);
		}
		
		private void OnTargetSelected(CombatParticipant target)
{
	// Store the target for after the minigame
	_pendingAttackTarget = target;
	
	// Hide target selection
	_targetSelection.Visible = false;
	
	// Launch the timing minigame
	LaunchTimingMinigame();
}

private void ShowHealTargetSelection()
{
	_targetSelection.Visible = true;
	
	// Clear previous buttons
	foreach (Node child in _targetSelection.GetChildren())
	{
		child.QueueFree();
	}
	
	// Add title
	var title = new Label();
	title.Text = $"{_currentActorSelecting.GetDisplayName()}: Who should use {_selectedItem.Name}?";
	title.HorizontalAlignment = HorizontalAlignment.Center;
	title.AddThemeColorOverride("font_color", new Color(0, 1, 0.5f));
	_targetSelection.AddChild(title);
	
	// Show all alive team members as potential targets
	var allAllies = _combat.PlayerTeam;
	
	foreach (var ally in allAllies)
{
	var btn = new Button();
	
	if (!ally.IsAlive)
	{
		// Defeated ally - can be revived
		btn.Text = $"💀 {ally.GetDisplayName()} [DEFEATED]\n" +
				   $"Will revive with {_selectedItem.HealAmount} HP";
		btn.CustomMinimumSize = new Vector2(250, 70);
	}
	else
	{
		// Living ally - normal healing
		int potentialHealing = Mathf.Min(_selectedItem.HealAmount, 
										 ally.Character.MaxHealth - ally.Character.Health);
		
		btn.Text = $"{ally.GetDisplayName()}\nHP: {ally.Character.Health}/{ally.Character.MaxHealth}\n" +
				   $"(+{potentialHealing} HP)";
		btn.CustomMinimumSize = new Vector2(250, 70);
		
		// Disable if target is at full health
		if (ally.Character.Health >= ally.Character.MaxHealth)
		{
			btn.Disabled = true;
			btn.Text += "\n(Full HP)";
		}
	}
	
	if (!btn.Disabled)
	{
		var targetAlly = ally;
		btn.Pressed += () => OnHealTargetSelected(targetAlly);
	}
	
	_targetSelection.AddChild(btn);
}
	
	// Add separator
	var separator = new HSeparator();
	_targetSelection.AddChild(separator);
	
	// Add cancel button
	var cancelBtn = new Button();
	cancelBtn.Text = "← Back";
	cancelBtn.CustomMinimumSize = new Vector2(250, 40);
	cancelBtn.Pressed += () =>
	{
		// Get the player to pass to ShowItemSelection
		var player = _combat.PlayerTeam.FirstOrDefault(p => p.Character is Player)?.Character as Player;
		if (player != null)
		{
			ShowItemSelection(player);
		}
	};
	_targetSelection.AddChild(cancelBtn);
}

private void OnItemSelected(Consumable item)
{
	_selectedItem = item;
	ShowHealTargetSelection();
}
		
private void ShowItemSelection(Player player)
{
	_targetSelection.Visible = true;
	_actionButtons.Visible = false;
	
	// Clear previous buttons
	foreach (Node child in _targetSelection.GetChildren())
	{
		child.QueueFree();
	}
	
	// Add title - show WHO is using the item
	var title = new Label();
	title.Text = $"{_currentActorSelecting.GetDisplayName()}: Select Item to Use";  // ← Changed
	title.HorizontalAlignment = HorizontalAlignment.Center;
	title.AddThemeColorOverride("font_color", new Color(0, 1, 1));
	_targetSelection.AddChild(title);
	
	// Get all consumables from PARTY inventory (player's inventory)
	var consumables = player.Inventory.GetAllItems()
		.Where(item => item is Consumable)
		.Cast<Consumable>()
		.ToList();
	
	if (consumables.Count == 0)
	{
		var noItemsLabel = new Label();
		noItemsLabel.Text = "No consumables available!";
		noItemsLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_targetSelection.AddChild(noItemsLabel);
	}
	else
	{
		// Group by item name to show quantities
		var groupedItems = consumables
			.GroupBy(c => c.Name)
			.Select(g => new { Item = g.First(), Count = g.Count() });
		
		foreach (var group in groupedItems)
		{
			var btn = new Button();
			btn.Text = $"{group.Item.Name} ({group.Count}x)\nHeals: {group.Item.HealAmount} HP";
			btn.CustomMinimumSize = new Vector2(250, 60);
			
			// Enable if ANY team member can be healed
			bool anyoneNeedsHealing = _combat.GetAliveAllies().Any(ally => ally.Character.Health < ally.Character.MaxHealth);
			btn.Disabled = !anyoneNeedsHealing;
			
			if (anyoneNeedsHealing)
			{
				var itemToUse = group.Item;
				btn.Pressed += () => OnItemSelected(itemToUse);
			}
			
			_targetSelection.AddChild(btn);
		}
	}
	
	// Add separator
	var separator = new HSeparator();
	_targetSelection.AddChild(separator);
	
	// Add cancel button
	var cancelBtn = new Button();
	cancelBtn.Text = "← Back";
	cancelBtn.CustomMinimumSize = new Vector2(250, 40);
	cancelBtn.Pressed += () =>
	{
		_targetSelection.Visible = false;
		_actionButtons.Visible = true;
	};
	_targetSelection.AddChild(cancelBtn);
}


private void OnItemButtonPressed()
{
	// Get the player's inventory (shared party inventory)
	var player = _combat.PlayerTeam.FirstOrDefault(p => p.Character is Player)?.Character as Player;
	
	if (player != null)
	{
		ShowItemSelection(player);
	}
}




private void OnHealTargetSelected(CombatParticipant target)
{
	// Get the player's inventory (party inventory)
	var player = _combat.PlayerTeam.FirstOrDefault(p => p.Character is Player)?.Character as Player;
	
	if (player != null && _selectedItem != null)
	{
		// Remove the item from inventory NOW
		player.Inventory.RemoveItem(_selectedItem);
		
		// Execute healing immediately
		bool actionExecuted = _combat.ExecuteUseItemImmediately(_currentActorSelecting, _selectedItem.Name, target);
		
		if (!actionExecuted)
		{
			return;
		}
	}
	
	_targetSelection.Visible = false;
	_selectedItem = null;
	
	// Update UI to show healing
	UpdateUI();
	
	// Check if combat is over
	if (_combat.IsCombatOver)
	{
		return;
	}
	
	// Check if all players have acted
	if (_combat.AllPlayersHaveActed())
	{
		// All players acted, start enemy turn
		HidePlayerActions();
		_combat.StartEnemyTurn();
	}
	else
	{
		// Continue to next actor
		PromptNextAction();
	}
}
		
		private void OnCombatEnd(Team winner)
		{
			if (winner == Team.Player)
			{
				_turnIndicator.Text = "VICTORY!";
				_turnIndicator.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			}
			else
			{
				_turnIndicator.Text = "DEFEAT";
				_turnIndicator.AddThemeColorOverride("font_color", new Color(1, 0, 0));
			}
			
			HidePlayerActions();
			
			// Return to test menu after 3 seconds
			GetTree().CreateTimer(3.0).Timeout += () =>
			{
				GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn");
			};
		}
		
		private void UpdateUI()
		{
 		   UpdateTeamDisplay(_playerTeamUI, _combat.PlayerTeam, true);  // ← Changed to PlayerTeam (all members)
 		   UpdateTeamDisplay(_enemyTeamUI, _combat.EnemyTeam, false);   // ← Changed to EnemyTeam (all enemies)
		}
		
private void UpdateTeamDisplay(VBoxContainer container, List<CombatParticipant> team, bool isPlayerTeam)
{
	foreach (Node child in container.GetChildren())
	{
		child.QueueFree();
	}
	
	// Show ALL team members (alive and defeated)
	foreach (var member in team)
	{
		var panel = new PanelContainer();
		var vbox = new VBoxContainer();
		
		// Add defeated indicator if not alive
		if (!member.IsAlive)
		{
			var defeatedLabel = new Label();
			defeatedLabel.Text = "💀 DEFEATED";
			defeatedLabel.AddThemeFontSizeOverride("font_size", 12);
			defeatedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.2f, 0.2f));
			vbox.AddChild(defeatedLabel);
		}
		
		// Name label
		var nameLabel = new Label();
		nameLabel.Text = member.GetDisplayName();
		
		// Gray out name if defeated, otherwise use team color
		if (!member.IsAlive)
		{
			nameLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
		}
		else
		{
			nameLabel.AddThemeColorOverride("font_color", 
				isPlayerTeam ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f));
		}
		vbox.AddChild(nameLabel);
		
		// HP label
		var hpLabel = new Label();
		float hpPercent = (float)member.Character.Health / member.Character.MaxHealth;
		Color hpColor = hpPercent > 0.5f ? new Color(0, 1, 0) : 
					   hpPercent > 0.25f ? new Color(1, 1, 0) : 
					   new Color(1, 0, 0);
		
		// Gray out HP if defeated
		if (!member.IsAlive)
		{
			hpColor = new Color(0.5f, 0.5f, 0.5f);
		}
		
		hpLabel.Text = $"HP: {member.Character.Health}/{member.Character.MaxHealth}";
		hpLabel.AddThemeColorOverride("font_color", hpColor);
		vbox.AddChild(hpLabel);
		
		// Get equipment info
		// Get equipment info
int baseAtk = member.Character.AttackPower;
int baseDef = member.Character.Defense;
int weaponBonus = 0;
int armorBonus = 0;
string weaponName = "";
string armorName = "";

// Check if this is a Player
if (member.Character is Player)
{
	Player player = (Player)member.Character;
	if (player.EquippedWeapon != null)
	{
		weaponBonus = player.EquippedWeapon.AttackBonus;
		weaponName = player.EquippedWeapon.Name;
	}
	if (player.EquippedArmor != null)
	{
		armorBonus = player.EquippedArmor.DefenseBonus;
		armorName = player.EquippedArmor.Name;
	}
}
else if (member.Character is Ally)
{
	Ally ally = (Ally)member.Character;
	if (ally.EquippedWeapon != null)
	{
		weaponBonus = ally.EquippedWeapon.AttackBonus;
		weaponName = ally.EquippedWeapon.Name;
	}
	if (ally.EquippedArmor != null)
	{
		armorBonus = ally.EquippedArmor.DefenseBonus;
		armorName = ally.EquippedArmor.Name;
	}
}
		
		// Stats label with dynamic attack and defense
		var statsLabel = new Label();
		string atkText = weaponBonus > 0 
			? $"ATK: {baseAtk}+{weaponBonus}={baseAtk + weaponBonus}" 
			: $"ATK: {baseAtk}";
		string defText = armorBonus > 0 
			? $"DEF: {baseDef}+{armorBonus}={baseDef + armorBonus}" 
			: $"DEF: {baseDef}";
		
		statsLabel.Text = $"{atkText} | {defText}";
		statsLabel.AddThemeFontSizeOverride("font_size", 12);
		
		// Gray out stats if defeated
		if (!member.IsAlive)
		{
			statsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
		}
		
		vbox.AddChild(statsLabel);
		
		// Weapon label (if equipped)
		if (!string.IsNullOrEmpty(weaponName))
		{
			var weaponLabel = new Label();
			weaponLabel.Text = $"⚔️ {weaponName}";
			weaponLabel.AddThemeFontSizeOverride("font_size", 10);
			weaponLabel.AddThemeColorOverride("font_color", 
				member.IsAlive ? new Color(0.8f, 0.8f, 0.5f) : new Color(0.5f, 0.5f, 0.5f));
			vbox.AddChild(weaponLabel);
		}
		
		// Armor label (if equipped)
		if (!string.IsNullOrEmpty(armorName))
		{
			var armorLabel = new Label();
			armorLabel.Text = $"🛡️ {armorName}";
			armorLabel.AddThemeFontSizeOverride("font_size", 10);
			armorLabel.AddThemeColorOverride("font_color", 
				member.IsAlive ? new Color(0.5f, 0.7f, 0.9f) : new Color(0.5f, 0.5f, 0.5f));
			vbox.AddChild(armorLabel);
		}
		
		// Show defending status (only if alive)
		if (member.IsAlive && member.Character.HasStatusEffect(StatusEffect.Defending))
		{
			var defendingLabel = new Label();
			defendingLabel.Text = "🛡️ DEFENDING";
			defendingLabel.AddThemeFontSizeOverride("font_size", 10);
			defendingLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
			vbox.AddChild(defendingLabel);
		}
		
		// Highlight if this actor is selecting (only if alive)
		if (member == _currentActorSelecting && member.IsAlive)
		{
			panel.AddThemeStyleboxOverride("panel", CreateHighlightStyle());
		}
		
		// Dim the entire panel if defeated
		if (!member.IsAlive)
		{
			panel.Modulate = new Color(0.6f, 0.6f, 0.6f, 0.8f);
		}
		
		panel.AddChild(vbox);
		container.AddChild(panel);
	}
}
		
		private StyleBoxFlat CreateHighlightStyle()
		{
			var style = new StyleBoxFlat();
			style.BgColor = new Color(1, 1, 0, 0.3f);
			style.BorderColor = new Color(1, 1, 0, 1);
			style.BorderWidthLeft = 2;
			style.BorderWidthRight = 2;
			style.BorderWidthTop = 2;
			style.BorderWidthBottom = 2;
			return style;
		}
		
		private void Log(string message)
		{
			_combatLog.AppendText(message + "\n");
			_combatLog.GetVScrollBar().Value = _combatLog.GetVScrollBar().MaxValue;
		}
		
private void OnDefendButtonPressed()
{
	// Execute defend action immediately
	bool actionExecuted = _combat.ExecuteDefendImmediately(_currentActorSelecting);

	if (!actionExecuted)
	{
		return;
	}
	
	// Update UI to show defending status
	UpdateUI();
	
	// Check if combat is over (shouldn't happen from defending)
	if (_combat.IsCombatOver)
	{
		return;
	}
	
	// Check if all players have acted
	if (_combat.AllPlayersHaveActed())
	{
		// All players acted, start enemy turn
		HidePlayerActions();
		_combat.StartEnemyTurn();
	}
	else
	{
		// Continue to next actor
		PromptNextAction();
	}
} 

private void OnSpecialButtonPressed()  // ← Now it's a separate method
{
	// Check if character can use special
	if (!_currentActorSelecting.Character.CanUseSpecial())
	{
		Log("⚠️ Not enough Special Power!");
		return;
	}
	
	// If character has multiple abilities, show selection menu
	// For now, they only have 1, so execute directly
	var ability = _currentActorSelecting.Character.SelectedAbility;
	
	if (ability == null)
	{
		Log("⚠️ No special ability equipped!");
		return;
	}
	
	// Different execution based on target type
	switch (ability.TargetType)
	{
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.Self:
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.AllAllies:
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.AllEnemies:
			// Execute immediately (no target selection needed)
			ExecuteSpecialAbility(ability);
			break;
			
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.SingleAlly:
			// Show ally selection (implement later if needed)
			break;
			
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.SingleEnemy:
			// Show enemy selection (implement later if needed)
			break;
	}
}

private void ExecuteSpecialAbility(Appanet.Scripts.Models.SpecialAbilities.SpecialAbility ability)
{
	// Execute the ability
	bool actionExecuted = _combat.ExecuteSpecialAbilityImmediately(_currentActorSelecting, ability);
	
	if (!actionExecuted)
	{
		return;
	}
	
	// Update UI
	UpdateUI();
	
	// Check if combat is over
	if (_combat.IsCombatOver)
	{
		return;
	}
	
	// Check if all players have acted
	if (_combat.AllPlayersHaveActed())
	{
		// All players acted, start enemy turn
		HidePlayerActions();
		_combat.StartEnemyTurn();
	}
	else
	{
		// Continue to next actor
		PromptNextAction();
	}
} 
		
		private void OnAttackButtonPressed()
		{
			// Launch the timing minigame
			ShowTargetSelection();
		}

private void LaunchTimingMinigame()
{
	// Load and instantiate the minigame
	var minigameScene = GD.Load<PackedScene>("res://AttackTimingMinigame.tscn");
	var minigame = minigameScene.Instantiate<Appanet.Scripts.Combat.AttackTimingMinigame>();
	
	// Add to scene tree (above other UI)
	GetNode("..").AddChild(minigame);
	
	// Connect to the signal
	minigame.TimingComplete += OnTimingComplete;
}

private void OnTimingComplete(float multiplier)
{
	// Show feedback
	string feedback = multiplier switch
	{
		>= 2.0f => "⚡ PERFECT! Critical Hit!",
		>= 1.5f => "✨ Great timing!",
		>= 1.0f => "👍 Good hit",
		>= 0.75f => "😐 Okay...",
		_ => "💢 Weak hit..."
	};
	
	Log($"{feedback} (x{multiplier:F1} damage)");
	
	// Execute attack immediately
	bool actionExecuted = _combat.ExecuteAttackImmediately(_currentActorSelecting, _pendingAttackTarget, multiplier);
	
	if (!actionExecuted)
	{
		// Combat ended or target invalid
		return;
	}
	
	// Update UI to show damage
	UpdateUI();
	
	// Check if combat is over
	if (_combat.IsCombatOver)
	{
		return; // Combat end handler will take over
	}
	
	// Check if all players have acted
	if (_combat.AllPlayersHaveActed())
	{
		// All players acted, start enemy turn
		HidePlayerActions();
		_combat.StartEnemyTurn();
	}
	else
	{
		// Continue to next actor
		PromptNextAction();
	}
}
		
		
		
	}
}
