using Godot;
using Appanet.Scripts.Models.Characters;

namespace Appanet.Scripts.World
{
	public partial class EnemySprite : Area2D
	{
		[Export] public string EnemyType = "BackroadsGremmlin";
		[Export] public float MoveSpeed = 50f;
		[Export] public bool Roaming = true;
		
		private Sprite2D _sprite;
		private Timer _moveTimer;
		private Vector2 _moveDirection;
		private bool _isDefeated = false;
		private string _uniqueID;
		
		public override void _Ready()
{
	// Generate unique ID based on position and type
	_uniqueID = $"{EnemyType}_{Position.X}_{Position.Y}";
	
	// Check if this enemy was already defeated
	if (WorldManager.Instance?.IsEnemyDefeated(_uniqueID) ?? false)
	{
		QueueFree(); // Remove from world
		return;
	}
	
	// Setup sprite - handle both Sprite2D and ColorRect
	_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
	if (_sprite == null)
	{
		GD.PrintErr($"Warning: {Name} has no Sprite2D child!");
		// Create a temporary sprite so we don't crash
		_sprite = new Sprite2D();
		AddChild(_sprite);
	}
	
	// Setup collision detection with player
	BodyEntered += OnBodyEntered;
	
	// Setup roaming behavior
	if (Roaming)
	{
		_moveTimer = new Timer();
		_moveTimer.WaitTime = GD.RandRange(2.0, 4.0);
		_moveTimer.Timeout += ChangeDirection;
		AddChild(_moveTimer);
		_moveTimer.Start();
		
		ChangeDirection();
	}
}
		
		public override void _Process(double delta)
		{
			if (Roaming && !_isDefeated)
			{
				Position += _moveDirection * MoveSpeed * (float)delta;
			}
		}
		
		private void ChangeDirection()
{
	if (_isDefeated) return;
	
	// Random movement in 8 directions (or stand still)
	int choice = GD.RandRange(0, 8);
	
	_moveDirection = choice switch
	{
		0 => Vector2.Up,
		1 => Vector2.Down,
		2 => Vector2.Left,
		3 => Vector2.Right,
		4 => new Vector2(1, 1).Normalized(),
		5 => new Vector2(-1, 1).Normalized(),
		6 => new Vector2(1, -1).Normalized(),
		7 => new Vector2(-1, -1).Normalized(),
		_ => Vector2.Zero
	};
	
	// Flip sprite for left/right (only if sprite exists)
	if (_sprite != null && _moveDirection.X != 0)
	{
		_sprite.FlipH = _moveDirection.X < 0;
	}
}
		
		private void OnBodyEntered(Node2D body)
		{
			if (_isDefeated) return;
			
			if (body is PlayerController)
			{
				GD.Print($"Player collided with {EnemyType}!");
				TriggerCombat();
			}
		}
		
		private void TriggerCombat()
		{
			_isDefeated = true;
			
			// Stop moving
			_moveDirection = Vector2.Zero;
			if (_moveTimer != null)
			{
				_moveTimer.Stop();
			}
			
			// Pass enemy info to WorldManager
			WorldManager.Instance?.StartCombatWithEnemy(EnemyType, _uniqueID);
		}
	}
}
