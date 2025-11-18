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
		private CombatParticipant _pendingAttackTarget;  // ← ADD THIS
		private float _currentAttackMultiplier = 1.0f;   
		
		public override void _Ready()
		{
			// Get UI references
			_playerTeamUI = GetNode<VBoxContainer>("../MainLayout/PlayerTeamPanel/PlayerTeam");
			_enemyTeamUI = GetNode<VBoxContainer>("../MainLayout/EnemyTeamPanel/EnemyTeam");
			_combatLog = GetNode<RichTextLabel>("../CombatLog");
			_actionButtons = GetNode<HBoxContainer>("../ActionButtons");
			_attackBtn = GetNode<Button>("../ActionButtons/AttackBtn");
			_itemBtn = GetNode<Button>("../ActionButtons/ItemBtn");
			_targetSelection = GetNode<VBoxContainer>("../TargetSelection");
			
			// Create turn indicator label
			_turnIndicator = new Label();
			_turnIndicator.Position = new Vector2(400, 30);
			_turnIndicator.AddThemeFontSizeOverride("font_size", 20);
			GetNode("..").AddChild(_turnIndicator);
			
			// Connect button signals
			_attackBtn.Pressed += OnAttackButtonPressed;
			_itemBtn.Pressed += OnItemButtonPressed;
			
			InitializeCombat();
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
	
	// Casey with weapon and armor
	var casey = Ally.CreateCase();
	var caseyWeapon = Weapon.CreateTireIron();
	var caseyArmor = Armor.CreateFlannel();
	casey.Inventory.AddItem(caseyWeapon);
	casey.Inventory.AddItem(caseyArmor);
	casey.EquipWeapon(caseyWeapon);
	casey.EquipArmor(caseyArmor);
	_combat.AddAlly(casey, "case");
	
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
	var aliveAllies = _combat.GetAliveAllies();
	
	foreach (var ally in aliveAllies)
	{
		var btn = new Button();
		
		int potentialHealing = Mathf.Min(_selectedItem.HealAmount, 
										 ally.Character.MaxHealth - ally.Character.Health);
		
		btn.Text = $"{ally.GetDisplayName()}\nHP: {ally.Character.Health}/{ally.Character.MaxHealth}\n" +
				   $"(+{potentialHealing} HP)";
		btn.CustomMinimumSize = new Vector2(250, 70);
		
		// Disable if target is at full health
		bool canHeal = ally.Character.Health < ally.Character.MaxHealth;
		btn.Disabled = !canHeal;
		
		if (canHeal)
		{
			var targetAlly = ally;
			btn.Pressed += () => OnHealTargetSelected(targetAlly);
		}
		else
		{
			btn.Text += "\n(Full HP)";
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
		// Remove the item from inventory NOW (when queuing, not when executing)
		player.Inventory.RemoveItem(_selectedItem);
		
		// Queue the action with the item name
		_combat.QueueAction(_currentActorSelecting, target, "UseItem", _selectedItem.Name);
		
		Log($"[Reserved] {_currentActorSelecting.GetDisplayName()} will use {_selectedItem.Name} on {target.GetDisplayName()}");
	}
	
	_targetSelection.Visible = false;
	_selectedItem = null;
	PromptNextAction();
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
			UpdateTeamDisplay(_playerTeamUI, _combat.GetAliveAllies(), true);
			UpdateTeamDisplay(_enemyTeamUI, _combat.GetAliveEnemies(), false);
		}
		
private void UpdateTeamDisplay(VBoxContainer container, List<CombatParticipant> team, bool isPlayerTeam)
{
	foreach (Node child in container.GetChildren())
	{
		child.QueueFree();
	}
	
	foreach (var member in team)
	{
		var panel = new PanelContainer();
		var vbox = new VBoxContainer();
		
		// Name label
		var nameLabel = new Label();
		nameLabel.Text = member.GetDisplayName();
		nameLabel.AddThemeColorOverride("font_color", 
			isPlayerTeam ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f));
		vbox.AddChild(nameLabel);
		
		// HP label
		var hpLabel = new Label();
		float hpPercent = (float)member.Character.Health / member.Character.MaxHealth;
		Color hpColor = hpPercent > 0.5f ? new Color(0, 1, 0) : 
					   hpPercent > 0.25f ? new Color(1, 1, 0) : 
					   new Color(1, 0, 0);
		hpLabel.Text = $"HP: {member.Character.Health}/{member.Character.MaxHealth}";
		hpLabel.AddThemeColorOverride("font_color", hpColor);
		vbox.AddChild(hpLabel);
		
		// Get equipment info
		int baseAtk = member.Character.AttackPower;
		int baseDef = member.Character.Defense;
		int weaponBonus = 0;
		int armorBonus = 0;
		string weaponName = "";
		string armorName = "";
		
		if (member.Character is Player player)
		{
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
		else if (member.Character is Ally ally)
		{
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
		vbox.AddChild(statsLabel);
		
		// Weapon label (if equipped)
		if (!string.IsNullOrEmpty(weaponName))
		{
			var weaponLabel = new Label();
			weaponLabel.Text = $"⚔️ {weaponName}";
			weaponLabel.AddThemeFontSizeOverride("font_size", 10);
			weaponLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.5f));
			vbox.AddChild(weaponLabel);
		}
		
		// Armor label (if equipped)
		if (!string.IsNullOrEmpty(armorName))
		{
			var armorLabel = new Label();
			armorLabel.Text = $"🛡️ {armorName}";
			armorLabel.AddThemeFontSizeOverride("font_size", 10);
			armorLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.7f, 0.9f));
			vbox.AddChild(armorLabel);
		}
		
		// Highlight if this actor is selecting
		if (member == _currentActorSelecting)
		{
			panel.AddThemeStyleboxOverride("panel", CreateHighlightStyle());
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
	
	// Queue the attack with the multiplier
	_combat.QueueAction(_currentActorSelecting, _pendingAttackTarget, "Attack");
	
	// Store the multiplier for this specific action
	_currentAttackMultiplier = multiplier;
	
	// Continue to next actor
	UpdateUI();
	PromptNextAction();
}
		
		
		
	}
}
