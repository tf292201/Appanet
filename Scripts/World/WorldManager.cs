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
		
		// Track defeated enemies (cleared on scene reload or save)
		private HashSet<string> _defeatedEnemies = new HashSet<string>();
		
		// Store pending combat info
		private string _pendingEnemyType;
		private string _pendingEnemyID;
		
		public override void _Ready()
{
	Instance = this;
	
	// Find PlayerCharacter - try different paths
	_player = GetNodeOrNull<PlayerController>("../PlayerCharacter");
	
	if (_player == null)
	{
		// Try alternative path
		_player = GetTree().Root.GetNodeOrNull<PlayerController>("World/PlayerCharacter");
	}
	
	if (_player == null)
	{
		GD.PrintErr("ERROR: Could not find PlayerCharacter!");
	}
	else
	{
		GD.Print($"✅ WorldManager found PlayerCharacter at: {_player.GetPath()}");
	}
	
	// Check if returning from combat
	if (GetTree().Root.HasMeta("returning_from_combat"))
	{
		bool playerWon = (bool)GetTree().Root.GetMeta("returning_from_combat");
		
		if (playerWon && GetTree().Root.HasMeta("defeated_enemy_id"))
		{
			string enemyID = (string)GetTree().Root.GetMeta("defeated_enemy_id");
			MarkEnemyDefeated(enemyID);
			GetTree().Root.RemoveMeta("defeated_enemy_id");
		}
		
		GetTree().Root.RemoveMeta("returning_from_combat");
	}
}
		
		public override void _Input(InputEvent @event)
		{
			// Open inventory with Tab or I
			if (@event.IsActionPressed("ui_cancel") || Input.IsKeyPressed(Key.I))
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
			GetTree().ChangeSceneToFile("res://Scenes/UI/InventoryScene.tscn");
		}
		
		public void StartCombatWithEnemy(string enemyType, string enemyID)
		{
			GD.Print($"Starting combat with: {enemyType} (ID: {enemyID})");
			
			// Store enemy info in root for CombatTestController to access
			GetTree().Root.SetMeta("combat_enemy_type", enemyType);
			GetTree().Root.SetMeta("combat_enemy_id", enemyID);
			
			GetTree().ChangeSceneToFile("res://Scenes/Combat/CombatTest.tscn");
		}
		
		public void MarkEnemyDefeated(string enemyID)
		{
			_defeatedEnemies.Add(enemyID);
			GD.Print($"Enemy defeated: {enemyID} (Total defeated: {_defeatedEnemies.Count})");
		}
		
		public bool IsEnemyDefeated(string enemyID)
		{
			return _defeatedEnemies.Contains(enemyID);
		}
		
		public void ReturnToWorld()
		{
			_isInMenu = false;
			if (_player != null)
				_player.SetInputEnabled(true);
		}
	}
}
