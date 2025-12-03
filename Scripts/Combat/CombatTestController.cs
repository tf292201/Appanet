using Godot;
using Appanet.Scripts.Models.Characters;      // ← CHANGED: For Player, Ally, Enemy, Character
using Appanet.Scripts.Models.Items;           // ← ADD: For Weapon, Armor, Consumable
using Appanet.Scripts.Models.Combat;          // ← ADD: For CombatState, CombatParticipant, etc.
using Appanet.Managers;               // ← KEEP: For GameManager
using System.Collections.Generic;
using System.Linq;

namespace Appanet.Scripts.Combat
{
	public partial class CombatTestController : Node
	{
		private CombatState _combat;
		private CombatParticipant _currentActorSelecting;
		private string _enemyTypeOverride = null;
		private bool IsCombatOver => _combat?.IsCombatOver ?? false;
		
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
	var _fleeBtn = GetNode<Button>("../ActionButtons/FleeBtn"); 
	
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
	_fleeBtn.Pressed += OnFleeButtonPressed;
	
	// ✅ Disable equipment management during combat
	_equipBtn.Disabled = true;
	_equipBtn.TooltipText = "Cannot change equipment during combat";
	
	// Fresh combat - initialize normally
	InitializeCombat();
}




		
private void OnManageEquipmentPressed()
{
	GD.Print("📦 Opening inventory from combat...");
	
	// Mark that we have an active combat to return to
	GetTree().Root.SetMeta("active_combat_state", true);
	
	// Keep enemy metadata so we know which combat to return to
	// (combat_enemy_type and combat_enemy_id should already be set)
	
	GetTree().ChangeSceneToFile("res://Scenes/UI/InventoryScene.tscn");
}
		
		
		
		
		
	// In CombatTestController.cs - InitializeCombat()
private void InitializeCombat()
{
	_combat = new CombatState();
	
	// ✅ Store in GameManager so it persists
	GameManager.Instance.ActiveCombat = _combat;
	
	// Add player
	var player = GameManager.Instance.Player;
	_combat.AddPlayerCharacter(player);
	
	// Add all party members
	foreach (var ally in GameManager.Instance.PartyMembers)
	{
		var allyId = ally is Ally a ? a.AllyID : "unknown";
		_combat.AddAlly(ally, allyId);
	}
	
	// ✅ CHECK FOR OVERWORLD ENEMY
	if (GetTree().Root.HasMeta("combat_enemy_type"))
	{
		_enemyTypeOverride = (string)GetTree().Root.GetMeta("combat_enemy_type");
		GetTree().Root.RemoveMeta("combat_enemy_type");
		GD.Print($"🎯 Loading specific enemy from overworld: {_enemyTypeOverride}");
	}
	
	// ✅ ADD ENEMY BASED ON OVERRIDE OR DEFAULT
	if (_enemyTypeOverride != null)
	{
		GD.Print($"Adding enemy: {_enemyTypeOverride}");
		AddEnemyByType(_enemyTypeOverride);
	}
	else
	{
		GD.Print("No enemy override - loading default test enemies");
		// Default test enemies
		_combat.AddEnemy(Enemy.CreateBackroadsGremmlin());
		_combat.AddEnemy(Enemy.CreateBarnWirePossum());
		_combat.AddEnemy(Enemy.CreateOffGridScavver());
	}
	
	// Subscribe to events
	_combat.OnPhaseChange += OnPhaseChange;
	_combat.OnCombatEnd += OnCombatEnd;
	_combat.OnCombatLog += Log;
	
	_combat.StartCombat();
	
	UpdateUI();
	PromptNextAction();
}
private void AddEnemyByType(string enemyType)
{
	Enemy enemy = enemyType switch
	{
		"BackroadsGremmlin" => Enemy.CreateBackroadsGremmlin(),
		"BarnWirePossum" => Enemy.CreateBarnWirePossum(),
		"OffGridScavver" => Enemy.CreateOffGridScavver(),
		"SkeletonKeyer" => Enemy.CreateSkeletonKeyer(),
		"RidgeRunnerHowler" => Enemy.CreateRidgeRunnerHowler(),
		"BunkerBrute" => Enemy.CreateBunkerBrute(),
		"BridgeTroll" => Enemy.CreateBridgeTroll(),
		"NightDialer" => Enemy.CreateNightDialer(),
		"HornedServerman" => Enemy.CreateHornedServerman(),
		"BlackBadgeEnforcer" => Enemy.CreateBlackBadgeEnforcer(),
		"ThunderHollowWyrm" => Enemy.CreateThunderHollowWyrm(),
		"ArchivistOfEchoVault" => Enemy.CreateArchivistOfEchoVault(),
		"DirectorOfBeneathNet" => Enemy.CreateDirectorOfBeneathNet(),
		_ => Enemy.CreateBackroadsGremmlin() // Default fallback
	};
	
	_combat.AddEnemy(enemy);
	GD.Print($"✅ Added enemy to combat: {enemyType}");
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
		
	private void OnFleeButtonPressed()
{
	if (_combat == null || _combat.IsCombatOver) return;
	
	GD.Print("🏃 Player attempting to flee...");
	
	// Attempt flee (enemies get revenge attacks)
	bool fleeSuccessful = _combat.AttemptFlee();
	
	// Update UI to show any damage taken
	UpdateUI();
	
	if (fleeSuccessful)
	{
		GD.Print("✅ Fled successfully! Returning to world in 3 seconds...");
		
		// Add to combat log
		_combatLog.AppendText("\n[color=yellow]You fled from combat![/color]\n");
		GameManager.Instance.ActiveCombat = null; 
		// Mark enemy as defeated in GameManager BEFORE returning
		if (GetTree().Root.HasMeta("combat_enemy_id"))
		{
			string enemyID = (string)GetTree().Root.GetMeta("combat_enemy_id");
			Appanet.Managers.GameManager.Instance?.MarkEnemyDefeated(enemyID);
			GD.Print($"🏃 Enemy marked as defeated immediately: {enemyID}");
		}
		
		// Clear active combat state
		if (GetTree().Root.HasMeta("active_combat_state"))
		{
			GetTree().Root.RemoveMeta("active_combat_state");
		}
		
		GetTree().Root.SetMeta("returning_from_combat", true);
		
		// Wait a moment then return to world
		GetTree().CreateTimer(3.0).Timeout += () =>
		{
			GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
		};
	}
	else
	{
		// Party was wiped out during flee attempt
		GD.Print("💀 Party defeated while fleeing!");
		_combatLog.AppendText("\n[color=red]Your party was defeated while trying to escape![/color]\n");
		GameManager.Instance.ActiveCombat = null;
		// Clear active combat state
		if (GetTree().Root.HasMeta("active_combat_state"))
		{
			GetTree().Root.RemoveMeta("active_combat_state");
		}
		
		GetTree().CreateTimer(3.0).Timeout += () =>
		{
			// Return to world or game over screen
			GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
		};
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
		
	private void OnCombatEnd(Team winningTeam)
{
	GD.Print($"=== Combat has ended! Winner: {winningTeam} ===");
	
	// ✅ Mark enemy as defeated in GameManager BEFORE returning
	if (GetTree().Root.HasMeta("combat_enemy_id"))
	{
		string enemyID = (string)GetTree().Root.GetMeta("combat_enemy_id");
		Appanet.Managers.GameManager.Instance?.MarkEnemyDefeated(enemyID);
		GD.Print($"✅ Enemy marked as defeated immediately: {enemyID}");
	}
	
	// Clear active combat state
	GameManager.Instance.ActiveCombat = null;  // ← CLEAR COMBAT
	
	if (GetTree().Root.HasMeta("active_combat_state"))
	{
		GetTree().Root.RemoveMeta("active_combat_state");
	}
	
	// Set metadata for WorldManager to know we're returning from combat
	GetTree().Root.SetMeta("returning_from_combat", true);
	
	// Delay returning to world so player can see final state
	GetTree().CreateTimer(3.0).Timeout += () =>
	{
		GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
	};
}
		
		private void UpdateUI()
{
	UpdateTeamDisplay(_playerTeamUI, _combat.PlayerTeam, true);  // Show ALL allies (alive and defeated)
	UpdateTeamDisplay(_enemyTeamUI, _combat.GetAliveEnemies(), false);   // Only show ALIVE enemies
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
		
		// Make panel transparent
		var panelStyle = new StyleBoxFlat();
		panelStyle.BgColor = new Color(0, 0, 0, 0); // Fully transparent
		panel.AddThemeStyleboxOverride("panel", panelStyle);
		
		// Determine layout based on whether we have a sprite
		Container mainContainer;
		string iconPath = "";
		bool hasSprite = false;
		
		// Check for enemy sprite
		if (!isPlayerTeam && member.Character is Enemy enemy && !string.IsNullOrEmpty(enemy.IconPath))
		{
			iconPath = enemy.IconPath;
			hasSprite = true;
		}
		// Check for player sprite
		else if (isPlayerTeam && member.Character is Player player && !string.IsNullOrEmpty(player.IconPath))
		{
			iconPath = player.IconPath;
			hasSprite = true;
		}
		// Check for ally sprite
		else if (isPlayerTeam && member.Character is Ally ally && !string.IsNullOrEmpty(ally.IconPath))
		{
			iconPath = ally.IconPath;
			hasSprite = true;
		}
		
		// If we have a sprite, use horizontal layout with sprite on left
		if (hasSprite)
		{
			mainContainer = new HBoxContainer();
			((HBoxContainer)mainContainer).AddThemeConstantOverride("separation", 10);
			
			// Add sprite on the left
			var iconTexture = GD.Load<Texture2D>(iconPath);
			if (iconTexture != null)
			{
				var textureRect = new TextureRect();
				textureRect.Texture = iconTexture;
				textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
				textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
				textureRect.CustomMinimumSize = new Vector2(128, 128);
				
				// Gray out if defeated
				if (!member.IsAlive)
				{
					textureRect.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.6f);
				}
				
				mainContainer.AddChild(textureRect);
			}
		}
		else
		{
			// No sprite - use vertical layout
			mainContainer = new VBoxContainer();
			
			// Constrain width for player team without sprites
			if (isPlayerTeam)
			{
				panel.CustomMinimumSize = new Vector2(320, 0);
				panel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
			}
		}
		
		// Stats VBox (goes beside sprite, or is the main container if no sprite)
		var statsVbox = new VBoxContainer();
		
		// Add defeated indicator if not alive
		if (!member.IsAlive)
		{
			var defeatedLabel = new Label();
			defeatedLabel.Text = "💀 DEFEATED";
			defeatedLabel.AddThemeFontSizeOverride("font_size", 12);
			defeatedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.2f, 0.2f));
			statsVbox.AddChild(defeatedLabel);
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
		statsVbox.AddChild(nameLabel);
		
		// HP Progress Bar with fixed width
		var hpBar = new ProgressBar();
		hpBar.MinValue = 0;
		hpBar.MaxValue = member.Character.MaxHealth;
		hpBar.Value = member.Character.Health;
		hpBar.CustomMinimumSize = new Vector2(150, 20);
		hpBar.ShowPercentage = false;
		hpBar.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
		
		// Color the HP bar based on health percentage
		float hpPercent = (float)member.Character.Health / member.Character.MaxHealth;
		Color hpColor;
		if (hpPercent > 0.5f)
			hpColor = new Color(0, 1, 0); // Green
		else if (hpPercent > 0.25f)
			hpColor = new Color(1, 1, 0); // Yellow
		else
			hpColor = new Color(1, 0, 0); // Red

		// Gray out if defeated
		if (!member.IsAlive)
		{
			hpColor = new Color(0.5f, 0.5f, 0.5f);
		}

		var hpBarStyle = new StyleBoxFlat();
		hpBarStyle.BgColor = hpColor;
		hpBar.AddThemeStyleboxOverride("fill", hpBarStyle);

		// Add a dark background to the unfilled portion
		var hpBarBg = new StyleBoxFlat();
		hpBarBg.BgColor = new Color(0.2f, 0.2f, 0.2f, 1);
		hpBar.AddThemeStyleboxOverride("background", hpBarBg);

		statsVbox.AddChild(hpBar);
		
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
		
		statsVbox.AddChild(statsLabel);
		
		// Weapon label (if equipped)
		if (!string.IsNullOrEmpty(weaponName))
		{
			var weaponLabel = new Label();
			weaponLabel.Text = $"⚔️ {weaponName}";
			weaponLabel.AddThemeFontSizeOverride("font_size", 10);
			weaponLabel.AddThemeColorOverride("font_color", 
				member.IsAlive ? new Color(0.8f, 0.8f, 0.5f) : new Color(0.5f, 0.5f, 0.5f));
			statsVbox.AddChild(weaponLabel);
		}
		
		// Armor label (if equipped)
		if (!string.IsNullOrEmpty(armorName))
		{
			var armorLabel = new Label();
			armorLabel.Text = $"🛡️ {armorName}";
			armorLabel.AddThemeFontSizeOverride("font_size", 10);
			armorLabel.AddThemeColorOverride("font_color", 
				member.IsAlive ? new Color(0.5f, 0.7f, 0.9f) : new Color(0.5f, 0.5f, 0.5f));
			statsVbox.AddChild(armorLabel);
		}
		
		// Show defending status (only if alive)
		if (member.IsAlive && member.Character.HasStatusEffect(StatusEffect.Defending))
		{
			var defendingLabel = new Label();
			defendingLabel.Text = "🛡️ DEFENDING";
			defendingLabel.AddThemeFontSizeOverride("font_size", 10);
			defendingLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
			statsVbox.AddChild(defendingLabel);
		}
		
		// Add statsVbox to main container
		mainContainer.AddChild(statsVbox);
		
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
		
		panel.AddChild(mainContainer);
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

private void OnSpecialButtonPressed()
{
	// Show ability selection menu
	ShowSpecialAbilitySelection();
}

private void ShowSpecialAbilitySelection()
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
	title.Text = $"{_currentActorSelecting.GetDisplayName()}: Select Special Ability";
	title.HorizontalAlignment = HorizontalAlignment.Center;
	title.AddThemeColorOverride("font_color", new Color(1, 0.5f, 1));
	title.AddThemeFontSizeOverride("font_size", 18);
	_targetSelection.AddChild(title);
	
	// Get all unlocked abilities
	var abilities = _currentActorSelecting.Character.UnlockedAbilities;
	
	if (abilities.Count == 0)
	{
		var noAbilitiesLabel = new Label();
		noAbilitiesLabel.Text = "No special abilities unlocked!";
		noAbilitiesLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_targetSelection.AddChild(noAbilitiesLabel);
	}
	else
	{
		foreach (var ability in abilities)
		{
			var btn = new Button();
			
			// Check if can afford
			bool canAfford = _currentActorSelecting.Character.SpecialMeter >= ability.Cost;
			
			// Build button text
			string buttonText = $"{ability.AbilityIcon} {ability.Name}\n";
			buttonText += $"Cost: {ability.Cost} SP (Have: {_currentActorSelecting.Character.SpecialMeter})\n";
			buttonText += $"{ability.Description}";
			
			btn.Text = buttonText;
			btn.CustomMinimumSize = new Vector2(300, 80);
			btn.Disabled = !canAfford;
			
			// Add visual indicator if it's the selected/equipped ability
			if (ability == _currentActorSelecting.Character.SelectedAbility)
			{
				btn.AddThemeColorOverride("font_color", new Color(1, 1, 0));  // Yellow for equipped
			}
			
			if (canAfford)
			{
				var abilityToUse = ability;
				btn.Pressed += () => OnSpecialAbilitySelected(abilityToUse);
			}
			else
			{
				// Grey out if can't afford
				btn.Modulate = new Color(0.5f, 0.5f, 0.5f);
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

private void OnSpecialAbilitySelected(Appanet.Scripts.Models.SpecialAbilities.SpecialAbility ability)
{
	// Check if character can use special (just to be safe)
	if (!_currentActorSelecting.Character.CanUseSpecial())
	{
		Log("⚠️ Not enough Special Power!");
		return;
	}
	
	// Different execution based on target type
	switch (ability.TargetType)
	{
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.Self:
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.AllAllies:
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.AllEnemies:
			// Execute immediately (no target selection needed)
			_targetSelection.Visible = false;
			ExecuteSpecialAbility(ability);
			break;
			
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.SingleAlly:
			// Show ally selection (implement if needed)
			ShowSingleAllyTargetSelection(ability);
			break;
			
		case Appanet.Scripts.Models.SpecialAbilities.TargetType.SingleEnemy:
			// Show enemy selection (implement if needed)
			ShowSingleEnemyTargetSelection(ability);
			break;
	}
}

// For future single-target abilities
private void ShowSingleEnemyTargetSelection(Appanet.Scripts.Models.SpecialAbilities.SpecialAbility ability)
{
	_targetSelection.Visible = true;
	
	// Clear previous buttons
	foreach (Node child in _targetSelection.GetChildren())
	{
		child.QueueFree();
	}
	
	// Add title
	var title = new Label();
	title.Text = $"{ability.Name}: Select Target";
	title.HorizontalAlignment = HorizontalAlignment.Center;
	title.AddThemeColorOverride("font_color", new Color(1, 0.5f, 1));
	_targetSelection.AddChild(title);
	
	// Show all alive enemies
	var enemies = _combat.GetAliveEnemies();
	foreach (var enemy in enemies)
	{
		var btn = new Button();
		btn.Text = $"{enemy.Character.Name}\nHP: {enemy.Character.Health}/{enemy.Character.MaxHealth}";
		btn.CustomMinimumSize = new Vector2(250, 60);
		
		var targetEnemy = enemy;
		var abilityToUse = ability;
		btn.Pressed += () => OnSingleTargetAbilityConfirmed(abilityToUse, targetEnemy);
		
		_targetSelection.AddChild(btn);
	}
	
	// Add cancel button
	var cancelBtn = new Button();
	cancelBtn.Text = "← Back";
	cancelBtn.CustomMinimumSize = new Vector2(250, 40);
	cancelBtn.Pressed += () => ShowSpecialAbilitySelection();
	_targetSelection.AddChild(cancelBtn);
}

private void ShowSingleAllyTargetSelection(Appanet.Scripts.Models.SpecialAbilities.SpecialAbility ability)
{
	_targetSelection.Visible = true;
	
	// Clear previous buttons
	foreach (Node child in _targetSelection.GetChildren())
	{
		child.QueueFree();
	}
	
	// Add title
	var title = new Label();
	title.Text = $"{ability.Name}: Select Ally";
	title.HorizontalAlignment = HorizontalAlignment.Center;
	title.AddThemeColorOverride("font_color", new Color(1, 0.5f, 1));
	_targetSelection.AddChild(title);
	
	// Show all alive allies
	var allies = _combat.GetAliveAllies();
	foreach (var ally in allies)
	{
		var btn = new Button();
		btn.Text = $"{ally.GetDisplayName()}\nHP: {ally.Character.Health}/{ally.Character.MaxHealth}";
		btn.CustomMinimumSize = new Vector2(250, 60);
		
		var targetAlly = ally;
		var abilityToUse = ability;
		btn.Pressed += () => OnSingleTargetAbilityConfirmed(abilityToUse, targetAlly);
		
		_targetSelection.AddChild(btn);
	}
	
	// Add cancel button
	var cancelBtn = new Button();
	cancelBtn.Text = "← Back";
	cancelBtn.CustomMinimumSize = new Vector2(250, 40);
	cancelBtn.Pressed += () => ShowSpecialAbilitySelection();
	_targetSelection.AddChild(cancelBtn);
}

private void OnSingleTargetAbilityConfirmed(Appanet.Scripts.Models.SpecialAbilities.SpecialAbility ability, CombatParticipant target)
{
	_targetSelection.Visible = false;
	
	// Execute the ability with the selected target
	bool actionExecuted = _combat.ExecuteSpecialAbilityImmediately(_currentActorSelecting, ability, target);
	
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
		HidePlayerActions();
		_combat.StartEnemyTurn();
	}
	else
	{
		PromptNextAction();
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
	var minigameScene = GD.Load<PackedScene>("res://Scenes/Combat/AttackTimingMinigame.tscn");  // ← NEW PATH
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
