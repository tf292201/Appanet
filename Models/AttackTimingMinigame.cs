using Godot;
using System;

namespace Appanet.Scripts.Combat
{
	public partial class AttackTimingMinigame : Control
	{
		[Signal]
		public delegate void TimingCompleteEventHandler(float multiplier);
		
		// UI references
		private ColorRect _indicator;
		private ColorRect _perfectZone;
		private ColorRect _barFill;
		
		// Movement variables
		private float _indicatorPosition = 0f;
		private float _speed = 300f; // pixels per second
		private bool _isMoving = true;
		private float _barWidth = 700f;
		
		// Timing zones
		private float _perfectZoneStart;
		private float _perfectZoneEnd;
		
		public override void _Ready()
		{
			// Get node references
			_indicator = GetNode<ColorRect>("TimingBar/Indicator");
			_perfectZone = GetNode<ColorRect>("TimingBar/PerfectZone");
			_barFill = GetNode<ColorRect>("TimingBar/BarFill");
			
			// Calculate perfect zone bounds
			_perfectZoneStart = _perfectZone.Position.X;
			_perfectZoneEnd = _perfectZoneStart + _perfectZone.Size.X;
			
			// Start indicator at left edge
			_indicatorPosition = 0f;
			_indicator.Position = new Vector2(_indicatorPosition, _indicator.Position.Y);
		}
		
		public override void _Process(double delta)
		{
			if (!_isMoving) return;
			
			// Move indicator
			_indicatorPosition += _speed * (float)delta;
			
			// Bounce at edges
			if (_indicatorPosition >= _barWidth)
			{
				_indicatorPosition = _barWidth;
				_speed *= -1; // Reverse direction
			}
			else if (_indicatorPosition <= 0)
			{
				_indicatorPosition = 0;
				_speed *= -1; // Reverse direction
			}
			
			// Update visual position
			_indicator.Position = new Vector2(_indicatorPosition, _indicator.Position.Y);
		}
		
		public override void _Input(InputEvent @event)
		{
			// Listen for space bar or mouse click
			if (_isMoving && (@event.IsActionPressed("ui_accept") || @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed))
			{
				StopAndCalculate();
			}
		}
		
		private void StopAndCalculate()
		{
			_isMoving = false;
			
			float multiplier = CalculateMultiplier(_indicatorPosition);
			
			// Visual feedback (flash the indicator)
			if (multiplier >= 2.0f)
			{
				_indicator.Color = new Color(1, 0.84f, 0); // Gold for critical
			}
			else if (multiplier >= 1.5f)
			{
				_indicator.Color = new Color(0, 1, 0); // Green for good
			}
			else if (multiplier >= 1.0f)
			{
				_indicator.Color = new Color(1, 1, 0); // Yellow for normal
			}
			else
			{
				_indicator.Color = new Color(1, 0, 0); // Red for weak
			}
			
			// Wait a moment for visual feedback, then emit signal
			GetTree().CreateTimer(0.3).Timeout += () =>
			{
				EmitSignal(SignalName.TimingComplete, multiplier);
				QueueFree();
			};
		}
		
		private float CalculateMultiplier(float position)
		{
			// Calculate distance from perfect zone center
			float perfectCenter = _perfectZoneStart + (_perfectZone.Size.X / 2);
			float distanceFromPerfect = Mathf.Abs(position - perfectCenter);
			
			// Perfect hit (in the yellow zone)
			if (position >= _perfectZoneStart && position <= _perfectZoneEnd)
			{
				return 2.0f; // CRITICAL HIT!
			}
			// Good hit (close to perfect zone)
			else if (distanceFromPerfect <= 80f)
			{
				return 1.5f; // Good hit
			}
			// Normal hit (medium distance)
			else if (distanceFromPerfect <= 150f)
			{
				return 1.0f; // Normal damage
			}
			// Bad hit (far from perfect)
			else if (distanceFromPerfect <= 250f)
			{
				return 0.75f; // Reduced damage
			}
			// Terrible hit (very far)
			else
			{
				return 0.5f; // Weak hit
			}
		}
	}
}
