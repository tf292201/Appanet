using Godot;
using Appanet.Managers;

namespace Appanet.Scripts.World
{
	public partial class PlayerController : CharacterBody2D
	{
		[Export] public float Speed = 200f;
		
		private Sprite2D _sprite;
		private Vector2 _lastDirection = Vector2.Down;
		
		public override void _Ready()
		{
			GD.Print("===== PlayerController _Ready() CALLED! =====");
			GD.Print($"Player position: {Position}");
			
			_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (_sprite == null)
			{
				GD.PrintErr("ERROR: PlayerCharacter has no Sprite2D child!");
			}
			
			// FORCE top-down mode
			MotionMode = MotionModeEnum.Floating;
			
			GD.Print($"Motion Mode set to: {MotionMode}");
			GD.Print("===== PlayerController Ready Complete! =====");
		}
		
		public override void _PhysicsProcess(double delta)
		{
			// NUCLEAR OPTION: Pure 2D movement, no gravity
			Vector2 currentVelocity = Vector2.Zero;
			
			// Get input direction
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			
			if (direction != Vector2.Zero)
			{
				_lastDirection = direction;
				currentVelocity = direction * Speed;
				
				// Flip sprite for left/right movement (only if sprite exists)
				if (_sprite != null && direction.X != 0)
				{
					_sprite.FlipH = direction.X < 0;
				}
			}
			
			// FORCE velocity to only be our movement (no gravity)
			Velocity = currentVelocity;
			
			// Move the character
			MoveAndSlide();
		}
		
		// Method to disable/enable movement (for menus, combat, etc.)
		public void SetInputEnabled(bool enabled)
		{
			SetPhysicsProcess(enabled);
		}
	}
}
