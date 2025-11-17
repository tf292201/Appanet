using Godot;
using Appanet.Scripts.Models;
using Appanet.Scripts.Managers;
using System.Collections.Generic;
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
		
		// Check if this actor can use items
		if (_currentActorSelecting.Character is Player player)
		{
			// Enable item button if player has ANY consumables
			var hasConsumables = player.Inventory.GetAllItems()
				.Any(item => item is Consumable);
			_itemBtn.Disabled = !hasConsumables;
		}
		else
		{
			_itemBtn.Disabled = true;  // Allies can't use items
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
		
		private void OnAttackButtonPressed()
		{
			ShowTargetSelection();
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
			_combat.QueueAction(_currentActorSelecting, target, "Attack");
			_targetSelection.Visible = false;
			UpdateUI();
			PromptNextAction();
		}
		
		private void OnItemButtonPressed()
{
	if (_currentActorSelecting.Character is Player player)
	{
		ShowItemSelection(player);
	}
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
	
	// Add title
	var title = new Label();
	title.Text = "Select Item to Use:";
	title.HorizontalAlignment = HorizontalAlignment.Center;
	title.AddThemeColorOverride("font_color", new Color(0, 1, 1));
	_targetSelection.AddChild(title);
	
	// Get all consumables from inventory
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
			
			// Disable if at full health
			bool canUse = player.Health < player.MaxHealth;
			btn.Disabled = !canUse;
			
			if (canUse)
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

private void OnItemSelected(Consumable item)
{
	_selectedItem = item;
	ShowHealTargetSelection();
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
	title.Text = $"Who should use {_selectedItem.Name}?";
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
	
	// Add separator (ONLY ONCE)
	var separator = new HSeparator();
	_targetSelection.AddChild(separator);
	
	// Add cancel button (ONLY ONCE)
	var cancelBtn = new Button();
	cancelBtn.Text = "← Back";
	cancelBtn.CustomMinimumSize = new Vector2(250, 40);
	cancelBtn.Pressed += () =>
	{
		if (_currentActorSelecting.Character is Player player)
		{
			ShowItemSelection(player);
		}
	};
	_targetSelection.AddChild(cancelBtn);
}


private void OnHealTargetSelected(CombatParticipant target)
{
	_combat.QueueAction(_currentActorSelecting, target, "UseItem", _selectedItem.Name);
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
		
		
		
	}
}
