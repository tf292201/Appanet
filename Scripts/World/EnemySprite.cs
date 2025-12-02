using Godot;
using Appanet.Scripts.World;

namespace Appanet.Scripts.World
{
	public partial class EnemySprite : Area2D
	{
		[Export] public string EnemyType = "BackroadsGremmlin";
		[Export] public float MoveSpeed = 50f;
		[Export] public bool Roaming = true;
		
		private Sprite2D _sprite;
		private Timer _moveTimer;
		private Vector2 _moveDirection = Vector2.Zero;
		private string _uniqueID;
		private bool _isActive = true; // ← NEW: Track if enemy is active
		
		public override void _Ready()
		{
			// Generate unique ID based on position and type
			_uniqueID = $"{EnemyType}_{Position.X}_{Position.Y}";
			
			// Check if this enemy was already defeated
			if (Appanet.Managers.GameManager.Instance?.IsEnemyDefeated(_uniqueID) ?? false)
			{
				GD.Print($"🗑️ Removing defeated enemy: {_uniqueID}");
				
				// Disable immediately to prevent interactions
				_isActive = false;
				Visible = false;
				SetPhysicsProcess(false);
				SetProcess(false);
				
				// Remove safely on next frame
				CallDeferred(Node.MethodName.QueueFree);
				return;
			}
			
			// Setup sprite - handle both Sprite2D and ColorRect
			_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (_sprite == null)
			{
				GD.PrintErr($"Warning: {Name} has no Sprite2D child!");
			}
			
			// Setup collision detection with player
			BodyEntered += OnBodyEntered;
			
			// Setup roaming behavior
			if (Roaming)
			{
				// Create timer immediately but add as child deferred
				_moveTimer = new Timer();
				_moveTimer.WaitTime = GD.RandRange(2.0, 4.0);
				_moveTimer.Timeout += ChangeDirection;
				_moveTimer.Autostart = true;
				
				CallDeferred(MethodName.AddTimerToScene);
				CallDeferred(MethodName.ChangeDirection);
			}
		}
		
		// NEW - Add timer safely
		private void AddTimerToScene()
		{
			if (_moveTimer != null && !_moveTimer.IsInsideTree())
			{
				AddChild(_moveTimer);
			}
		}
		
		public override void _PhysicsProcess(double delta)
		{
			if (!_isActive) return; // ← Don't move if inactive
			
			if (Roaming && _moveDirection != Vector2.Zero)
			{
				Position += _moveDirection * MoveSpeed * (float)delta;
			}
		}
		
		private void ChangeDirection()
		{
			if (!_isActive) return; // ← Don't change direction if inactive
			
			if (_sprite == null)
			{
				_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
				if (_sprite == null) return;
			}
			
			// Random 8-directional movement
			float[] directions = { -1f, 0f, 1f };
			float x = directions[GD.RandRange(0, 2)];
			float y = directions[GD.RandRange(0, 2)];
			
			_moveDirection = new Vector2(x, y).Normalized();
			
			// Flip sprite if moving left
			if (x < 0)
			{
				_sprite.FlipH = true;
			}
			else if (x > 0)
			{
				_sprite.FlipH = false;
			}
		}
		
		private void OnBodyEntered(Node2D body)
		{
			if (!_isActive) return; // ← Ignore collisions if inactive
			
			if (body is PlayerController)
			{
				GD.Print($"💥 Player collided with {EnemyType} (ID: {_uniqueID})");
				
				// Disable this enemy immediately to prevent double-triggers
				_isActive = false;
				
				// Start combat (deferred to avoid physics callback issues)
				CallDeferred(MethodName.TriggerCombat);
			}
		}
		
		// NEW - Trigger combat safely
		private void TriggerCombat()
		{
			WorldManager.Instance?.StartCombatWithEnemy(EnemyType, _uniqueID);
		}
	}
}
