using Godot;
using Appanet.Managers;
using System.Collections.Generic;

namespace Appanet.Scripts.World
{
	public partial class WorldManager : Node
	{
		public static WorldManager Instance { get; private set; }
		
		private PlayerController _player;
		private bool _isInMenu = false;
		
		public override void _Ready()
{
	Instance = this;
	
	GD.Print("===== WorldManager _Ready() =====");
	
	// Find PlayerCharacter
	_player = GetNodeOrNull<PlayerController>("../PlayerCharacter");
	
	if (_player == null)
	{
		var parent = GetParent();
		if (parent != null)
		{
			_player = parent.GetNodeOrNull<PlayerController>("PlayerCharacter");
		}
	}
	
	if (_player == null)
	{
		_player = GetTree().Root.GetNodeOrNull<PlayerController>("World/PlayerCharacter");
	}
	
	if (_player == null)
	{
		GD.PrintErr("❌ ERROR: Could not find PlayerCharacter!");
	}
	else
	{
		GD.Print($"✅ WorldManager found PlayerCharacter at: {_player.GetPath()}");
	}
	
	// Check if returning from combat (just for logging, enemies already marked)
	if (GetTree().Root.HasMeta("returning_from_combat"))
	{
		GD.Print("✅ Returned from combat - enemies already marked as defeated");
		GetTree().Root.RemoveMeta("returning_from_combat");
		
		// Clean up old metadata (no longer needed)
		if (GetTree().Root.HasMeta("combat_enemy_id"))
		{
			GetTree().Root.RemoveMeta("combat_enemy_id");
		}
		if (GetTree().Root.HasMeta("combat_enemy_type"))
		{
			GetTree().Root.RemoveMeta("combat_enemy_type");
		}
	}
	
	GD.Print("===== WorldManager _Ready() Complete =====");
}
		
		public override void _Input(InputEvent @event)
		{
			// Open inventory with I key
			if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.I)
			{
				if (!_isInMenu)
				{
					OpenInventory();
				}
			}
		}
		
		public void OpenInventory()
{
	_isInMenu = true;
	_player.SetInputEnabled(false);
	
	// Mark that we came from world (not from combat)
	GetTree().Root.SetMeta("from_world_scene", true);
	
	GetTree().ChangeSceneToFile("res://Scenes/UI/InventoryScene.tscn");
}
		
		public void StartCombatWithEnemy(string enemyType, string enemyID)
		{
			GD.Print($"Starting combat with: {enemyType} (ID: {enemyID})");
			
			// Store enemy info for CombatTestController
			GetTree().Root.SetMeta("combat_enemy_type", enemyType);
			GetTree().Root.SetMeta("combat_enemy_id", enemyID);
			
			GetTree().ChangeSceneToFile("res://Scenes/Combat/CombatTest.tscn");
		}
		
		// Forward to GameManager
		public void MarkEnemyDefeated(string enemyID)
		{
			GameManager.Instance?.MarkEnemyDefeated(enemyID);
		}
		
		public bool IsEnemyDefeated(string enemyID)
		{
			return GameManager.Instance?.IsEnemyDefeated(enemyID) ?? false;
		}
	}
}
