using Godot;
using Appanet.Scripts.Models;
using Appanet.Scripts.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Appanet.Scripts.UI
{
	public partial class InventorySceneController : Control
	{
		// Character selection
		private List<Character> _partyMembers;
		private int _currentCharacterIndex = 0;
		private Character _currentCharacter;
		
		// UI References
		private Label _characterNameLabel;
		private Label _characterStatsLabel;
		private Label _expMoneyLabel;
		private Panel _weaponSlotPanel;
		private Panel _armorSlotPanel;
		private Label _weaponSlotLabel;
		private Label _armorSlotLabel;
		private GridContainer _inventoryGrid;
		private Panel _itemDetailsPanel;
		private Label _itemDetailsLabel;
		private Button _equipButton;
		private Button _unequipButton;
		private HBoxContainer _characterTabs;
		private Label _abilitiesLabel;
		
		// State
		private Item _selectedItem;
		
	public override void _Ready()
{
	GD.Print("=== InventoryScene _Ready() START ===");
	
	try {
		// Get UI references - CORRECTED PATHS
		_characterNameLabel = GetNode<Label>("MainContainer/LeftPanel/CharacterPanel/VBox/NameLabel");
		_characterStatsLabel = GetNode<Label>("MainContainer/LeftPanel/CharacterPanel/VBox/StatsLabel");
		_expMoneyLabel = GetNode<Label>("TopBar/ExpMoneyLabel");
		
		_weaponSlotPanel = GetNode<Panel>("MainContainer/LeftPanel/EquipmentPanel/HBox/WeaponSlot");
		_armorSlotPanel = GetNode<Panel>("MainContainer/LeftPanel/EquipmentPanel/HBox/ArmorSlot");
		_weaponSlotLabel = GetNode<Label>("MainContainer/LeftPanel/EquipmentPanel/HBox/WeaponSlot/VBox/ItemLabel");
		_armorSlotLabel = GetNode<Label>("MainContainer/LeftPanel/EquipmentPanel/HBox/ArmorSlot/VBox/ItemLabel");
		
		_inventoryGrid = GetNode<GridContainer>("MainContainer/InventoryPanel/ScrollContainer/InventoryGrid");
		_itemDetailsPanel = GetNode<Panel>("MainContainer/ItemDetailsPanel");
		_itemDetailsLabel = GetNode<Label>("MainContainer/ItemDetailsPanel/VBox/DetailsLabel");
		_equipButton = GetNode<Button>("MainContainer/ItemDetailsPanel/VBox/ButtonBox/EquipButton");
		_unequipButton = GetNode<Button>("MainContainer/ItemDetailsPanel/VBox/ButtonBox/UnequipButton");
		_abilitiesLabel = GetNode<Label>("MainContainer/LeftPanel/AbilitiesPanel/VBox/ScrollContainer/AbilitiesLabel");
		_characterTabs = GetNode<HBoxContainer>("MainContainer/LeftPanel/CharacterPanel/VBox/TabsPanel/CharacterTabs");
		
		GD.Print($"✓ All UI nodes found! Tabs has {_characterTabs.GetChildCount()} children");
		
		// CLEAR TABS IMMEDIATELY
		while (_characterTabs.GetChildCount() > 0)
		{
			var child = _characterTabs.GetChild(0);
			_characterTabs.RemoveChild(child);
			child.QueueFree();
		}
		GD.Print($"Cleared tabs, now has {_characterTabs.GetChildCount()} children");
		
		// Connect signals
		GD.Print("Connecting equipButton...");
		_equipButton.Pressed += OnEquipPressed;
		GD.Print("✓ equipButton connected!");
		
		GD.Print("Connecting unequipButton...");
		_unequipButton.Pressed += OnUnequipPressed;
		GD.Print("✓ unequipButton connected!");
		
		GD.Print("Getting BackButton...");
		var backButton = GetNode<Button>("TopBar/BackButton");
		GD.Print("Connecting BackButton...");
		backButton.Pressed += OnBackPressed;
		GD.Print("✓ Back button connected!");
		
		// Setup weapon/armor slot click detection
		GD.Print("Creating weapon button...");
		var weaponButton = new Button();
		weaponButton.CustomMinimumSize = new Vector2(150, 150);
		weaponButton.Theme = new Theme();
		weaponButton.Text = "";
		weaponButton.Flat = true;
		weaponButton.Pressed += OnWeaponSlotPressed;
		_weaponSlotPanel.AddChild(weaponButton);
		_weaponSlotPanel.MoveChild(weaponButton, 0);
		GD.Print("✓ Weapon button created!");
		
		GD.Print("Creating armor button...");
		var armorButton = new Button();
		armorButton.CustomMinimumSize = new Vector2(150, 150);
		armorButton.Theme = new Theme();
		armorButton.Text = "";
		armorButton.Flat = true;
		armorButton.Pressed += OnArmorSlotPressed;
		_armorSlotPanel.AddChild(armorButton);
		_armorSlotPanel.MoveChild(armorButton, 0);
		GD.Print("✓ Equipment slot buttons created!");
		
		GD.Print("Checking GameManager...");
		if (GameManager.Instance == null)
		{
			GD.PrintErr("ERROR: GameManager.Instance is NULL!");
			return;
		}
		GD.Print("✓ GameManager found!");
		
		GD.Print("Checking Player...");
		if (GameManager.Instance.Player == null)
		{
			GD.PrintErr("ERROR: GameManager.Instance.Player is NULL!");
			return;
		}
		GD.Print($"✓ Player found: {GameManager.Instance.Player.Name}");
		
		GD.Print("Calling InitializeParty...");
		InitializeParty();
		
		GD.Print("Calling UpdateUI...");
		UpdateUI();
		
		GD.Print("=== InventoryScene _Ready() COMPLETE ===");
	}
	catch (System.Exception e)
	{
		GD.PrintErr($"ERROR in _Ready(): {e.Message}");
		GD.PrintErr($"Stack trace: {e.StackTrace}");
	}
	}
		
		private void InitializeParty()
{
	GD.Print($"--- InitializeParty() START --- (Tabs currently has {_characterTabs.GetChildCount()} children)");
	
	_partyMembers = new List<Character>();
	
	// Get party from GameManager
	_partyMembers = GameManager.Instance.GetActiveParty();
	
	GD.Print($"Party members found: {_partyMembers.Count}");
	foreach (var member in _partyMembers)
	{
		GD.Print($"  - {member.Name}");
	}
	
	if (_partyMembers.Count == 0)
	{
		GD.PrintErr("ERROR: No party members found!");
		return;
	}
	
	_currentCharacter = _partyMembers[0];
	GD.Print($"Current character set to: {_currentCharacter.Name}");
	
	// Create character tabs
	for (int i = 0; i < _partyMembers.Count; i++)
	{
		int index = i;
		var tabButton = new Button();
		tabButton.Text = _partyMembers[i].Name;
		
		// Lock the button size to prevent expansion
		tabButton.CustomMinimumSize = new Vector2(110, 40);
		tabButton.Size = new Vector2(110, 40);
		
		// Clip text that doesn't fit
		tabButton.ClipText = true;
		tabButton.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
		
		// Prevent button from expanding
		tabButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
		
		// Load custom font
		var customFont = GD.Load<Font>("res://fonts/RETROTECH.ttf");
		if (customFont != null)
		{
			tabButton.AddThemeFontOverride("font", customFont);
			tabButton.AddThemeFontSizeOverride("font_size", 18);  // Reduced from 20 to fit better
		}
		
		tabButton.Pressed += () => SwitchCharacter(index);
		_characterTabs.AddChild(tabButton);
		GD.Print($"  Created tab for: {_partyMembers[i].Name} (Total tabs now: {_characterTabs.GetChildCount()})");
	}
	
	UpdateTabHighlight();
	GD.Print("--- InitializeParty() COMPLETE ---");
}	
		private void SwitchCharacter(int index)
		{
			if (index < 0 || index >= _partyMembers.Count) return;
			
			_currentCharacterIndex = index;
			_currentCharacter = _partyMembers[index];
			_selectedItem = null;
			
			UpdateTabHighlight();
			UpdateUI();
		}
		
		private void UpdateTabHighlight()
		{
			var tabs = _characterTabs.GetChildren();
			for (int i = 0; i < tabs.Count; i++)
			{
				if (tabs[i] is Button btn)
				{
					if (i == _currentCharacterIndex)
					{
						btn.AddThemeColorOverride("font_color", new Color(1, 1, 0));
						btn.Modulate = new Color(1.2f, 1.2f, 1.0f);
					}
					else
					{
						btn.RemoveThemeColorOverride("font_color");
						btn.Modulate = new Color(1, 1, 1);
					}
				}
			}
		}
		
		private void UpdateUI()
		{
			UpdateCharacterDisplay();
			UpdateEquipmentSlots();
			UpdateInventoryGrid();
			UpdateItemDetails();
			UpdateExpMoney();
		}
		
		private void UpdateCharacterDisplay()
{
	_characterNameLabel.Text = _currentCharacter.Name;
	
	// Get current stats
	int baseAtk = _currentCharacter.AttackPower;
	int baseDef = _currentCharacter.Defense;
	int weaponBonus = 0;
	int armorBonus = 0;
	
	if (_currentCharacter is Player player)
	{
		weaponBonus = player.EquippedWeapon?.AttackBonus ?? 0;
		armorBonus = player.EquippedArmor?.DefenseBonus ?? 0;
	}
	else if (_currentCharacter is Ally ally)
	{
		weaponBonus = ally.EquippedWeapon?.AttackBonus ?? 0;
		armorBonus = ally.EquippedArmor?.DefenseBonus ?? 0;
	}
	
	int totalAtk = baseAtk + weaponBonus;
	int totalDef = baseDef + armorBonus;
	
	_characterStatsLabel.Text = $"HP: {_currentCharacter.Health}/{_currentCharacter.MaxHealth}\n";
	
	if (weaponBonus > 0)
		_characterStatsLabel.Text += $"ATK: {baseAtk} + {weaponBonus} = {totalAtk}\n";
	else
		_characterStatsLabel.Text += $"ATK: {baseAtk}\n";
		
	if (armorBonus > 0)
		_characterStatsLabel.Text += $"DEF: {baseDef} + {armorBonus} = {totalDef}";
	else
		_characterStatsLabel.Text += $"DEF: {baseDef}";
	
	// Update abilities in separate panel
	UpdateAbilitiesDisplay();
}

private void UpdateAbilitiesDisplay()
{
	if (_currentCharacter.UnlockedAbilities.Count == 0)
	{
		_abilitiesLabel.Text = "(No special abilities unlocked)";
		return;
	}
	
	string text = "";
	
	foreach (var ability in _currentCharacter.UnlockedAbilities)
	{
		string selectedMark = ability == _currentCharacter.SelectedAbility ? "★ " : "  ";
		text += $"{selectedMark}{ability.AbilityIcon} {ability.Name}\n";
		text += $"   Cost: {ability.Cost} SP\n";
		text += $"   {ability.Description}\n\n";
	}
	
	// Show current special meter
	text += $"Special Power: {_currentCharacter.SpecialMeter}/{_currentCharacter.MaxSpecialMeter}";
	
	_abilitiesLabel.Text = text;
}
		
		private void UpdateEquipmentSlots()
		{
			// Weapon slot
			if (_currentCharacter is Player player)
			{
				if (player.EquippedWeapon != null)
				{
					_weaponSlotLabel.Text = $"⚔️ {player.EquippedWeapon.Name}\n+{player.EquippedWeapon.AttackBonus} ATK";
				}
				else
				{
					_weaponSlotLabel.Text = "⚔️ No Weapon\n(Click to equip)";
				}
				
				if (player.EquippedArmor != null)
				{
					_armorSlotLabel.Text = $"🛡️ {player.EquippedArmor.Name}\n+{player.EquippedArmor.DefenseBonus} DEF";
				}
				else
				{
					_armorSlotLabel.Text = "🛡️ No Armor\n(Click to equip)";
				}
			}
			else if (_currentCharacter is Ally ally)
			{
				if (ally.EquippedWeapon != null)
				{
					_weaponSlotLabel.Text = $"⚔️ {ally.EquippedWeapon.Name}\n+{ally.EquippedWeapon.AttackBonus} ATK";
				}
				else
				{
					_weaponSlotLabel.Text = "⚔️ No Weapon\n(Click to equip)";
				}
				
				if (ally.EquippedArmor != null)
				{
					_armorSlotLabel.Text = $"🛡️ {ally.EquippedArmor.Name}\n+{ally.EquippedArmor.DefenseBonus} DEF";
				}
				else
				{
					_armorSlotLabel.Text = "🛡️ No Armor\n(Click to equip)";
				}
			}
		}
		
		private void UpdateInventoryGrid()
		{
			// Clear existing items
			foreach (Node child in _inventoryGrid.GetChildren())
			{
				child.QueueFree();
			}
			
			// Get all weapons and armor from player's inventory
			var player = GameManager.Instance.Player;
			var items = player.Inventory.GetAllItems()
				.Where(item => item is Weapon || item is Armor)
				.ToList();
			
			foreach (var item in items)
			{
				var itemButton = new Button();
				itemButton.CustomMinimumSize = new Vector2(120, 80);
				
				string itemText = "";
				if (item is Weapon weapon)
				{
					itemText = $"⚔️ {weapon.Name}\n+{weapon.AttackBonus} ATK\n[{weapon.Rarity}]";
				}
				else if (item is Armor armor)
				{
					itemText = $"🛡️ {armor.Name}\n+{armor.DefenseBonus} DEF\n[{armor.Rarity}]";
				}
				
				itemButton.Text = itemText;
				itemButton.Pressed += () => OnItemSelected(item);
				
				_inventoryGrid.AddChild(itemButton);
			}
		}
		
		private void OnItemSelected(Item item)
		{
			_selectedItem = item;
			UpdateItemDetails();
		}
		
		private void UpdateItemDetails()
		{
			if (_selectedItem == null)
			{
				_itemDetailsLabel.Text = "Select an item to view details";
				_equipButton.Disabled = true;
				_unequipButton.Visible = false;
				return;
			}
			
			string details = "";
			bool canEquip = false;
			
			if (_selectedItem is Weapon weapon)
			{
				details = $"⚔️ {weapon.Name}\n\n";
				details += $"Rarity: {weapon.Rarity}\n";
				details += $"Attack Bonus: +{weapon.AttackBonus}\n";
				details += $"Damage Type: {weapon.DamageType}\n\n";
				details += $"{weapon.Description}\n\n";
				
				if (!string.IsNullOrEmpty(weapon.SpecialEffect))
				{
					details += $"★ {weapon.SpecialEffect}\n\n";
				}
				
				// Show stat comparison
				int currentAtk = _currentCharacter.AttackPower;
				if (_currentCharacter is Player p && p.EquippedWeapon != null)
					currentAtk += p.EquippedWeapon.AttackBonus;
				else if (_currentCharacter is Ally a && a.EquippedWeapon != null)
					currentAtk += a.EquippedWeapon.AttackBonus;
				
				int newAtk = _currentCharacter.AttackPower + weapon.AttackBonus;
				int diff = newAtk - currentAtk;
				
				if (diff > 0)
					details += $"ATK: {currentAtk} → {newAtk} (+{diff})";
				else if (diff < 0)
					details += $"ATK: {currentAtk} → {newAtk} ({diff})";
				else
					details += $"ATK: {currentAtk} (no change)";
				
				canEquip = true;
			}
			else if (_selectedItem is Armor armor)
			{
				details = $"🛡️ {armor.Name}\n\n";
				details += $"Rarity: {armor.Rarity}\n";
				details += $"Defense Bonus: +{armor.DefenseBonus}\n";
				
				if (armor.DodgeBonus > 0)
				{
					details += $"Dodge Bonus: +{(int)(armor.DodgeBonus * 100)}%\n";
				}
				
				details += $"\n{armor.Description}\n\n";
				
				if (!string.IsNullOrEmpty(armor.SpecialEffect))
				{
					details += $"★ {armor.SpecialEffect}\n\n";
				}
				
				// Show stat comparison
				int currentDef = _currentCharacter.Defense;
				if (_currentCharacter is Player p && p.EquippedArmor != null)
					currentDef += p.EquippedArmor.DefenseBonus;
				else if (_currentCharacter is Ally a && a.EquippedArmor != null)
					currentDef += a.EquippedArmor.DefenseBonus;
				
				int newDef = _currentCharacter.Defense + armor.DefenseBonus;
				int diff = newDef - currentDef;
				
				if (diff > 0)
					details += $"DEF: {currentDef} → {newDef} (+{diff})";
				else if (diff < 0)
					details += $"DEF: {currentDef} → {newDef} ({diff})";
				else
					details += $"DEF: {currentDef} (no change)";
				
				canEquip = true;
			}
			
			_itemDetailsLabel.Text = details;
			_equipButton.Disabled = !canEquip;
			_unequipButton.Visible = false;
		}
		
		private void OnEquipPressed()
		{
			if (_selectedItem == null) return;
			
			if (_selectedItem is Weapon weapon)
			{
				if (_currentCharacter is Player player)
				{
					player.EquipWeapon(weapon);
				}
				else if (_currentCharacter is Ally ally)
				{
					ally.EquipWeapon(weapon);
				}
			}
			else if (_selectedItem is Armor armor)
			{
				if (_currentCharacter is Player player)
				{
					player.EquipArmor(armor);
				}
				else if (_currentCharacter is Ally ally)
				{
					ally.EquipArmor(armor);
				}
			}
			
			_selectedItem = null;
			UpdateUI();
		}
		
		private void OnUnequipPressed()
		{
			// We'll implement this when equipment slots are clicked
		}
		
		private void OnWeaponSlotPressed()
		{
			if (_currentCharacter is Player player && player.EquippedWeapon != null)
			{
				player.UnequipWeapon();
			}
			else if (_currentCharacter is Ally ally && ally.EquippedWeapon != null)
			{
				ally.UnequipWeapon();
				
				// Move to player inventory for sharing
				var weapon = ally.Inventory.GetAllItems().FirstOrDefault(i => i is Weapon) as Weapon;
				if (weapon != null)
				{
					ally.Inventory.RemoveItem(weapon);
					GameManager.Instance.Player.Inventory.AddItem(weapon);
				}
			}
			
			UpdateUI();
		}
		
		private void OnArmorSlotPressed()
		{
			if (_currentCharacter is Player player && player.EquippedArmor != null)
			{
				player.UnequipArmor();
			}
			else if (_currentCharacter is Ally ally && ally.EquippedArmor != null)
			{
				ally.UnequipArmor();
				
				// Move to player inventory for sharing
				var armor = ally.Inventory.GetAllItems().FirstOrDefault(i => i is Armor) as Armor;
				if (armor != null)
				{
					ally.Inventory.RemoveItem(armor);
					GameManager.Instance.Player.Inventory.AddItem(armor);
				}
			}
			
			UpdateUI();
		}
		
		private void UpdateExpMoney()
		{
			var player = GameManager.Instance.Player;
			int xpNeeded = player.Level * 100;
			_expMoneyLabel.Text = $"Level {player.Level} | XP: {player.Experience}/{xpNeeded} | ${player.Money}";
		}
		
		private void OnBackPressed()
		{
			// Return to previous scene (combat or main menu)
			GetTree().ChangeSceneToFile("res://CombatTest.tscn");
		}
	}
}
