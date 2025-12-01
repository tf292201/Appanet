using Godot;
using System;

public partial class Firefly : Sprite2D
{
	private Vector2 velocity;
	private float pulseTime = 0f;
	private float pulseSpeed;
	private const float BaseBrightness = 0.6f;
	
	// Add these for choppy movement
	private float moveTimer = 0f;
	private const float MoveInterval = 0.15f; // Update position every 0.15 seconds
	private Vector2 targetPosition;
	
	// Add for choppy pulse
	private int currentPulseFrame = 0;
	private float pulseTimer = 0f;
	private const float PulseFrameTime = 0.2f; // Change brightness every 0.2 seconds
	
	public override void _Ready()
	{
		// Create pixelated glow effect
		Texture = CreateFireflyTexture();
		
		// Force pixel-perfect rendering
		TextureFilter = TextureFilterEnum.Nearest;
		
		// Random movement - smaller, choppier values
		velocity = new Vector2(
			(float)GD.RandRange(-20, 20),
			(float)GD.RandRange(-20, 20)
		);
		
		targetPosition = Position;
		pulseSpeed = (float)GD.RandRange(2.0, 4.0);
		pulseTime = (float)GD.RandRange(0, Mathf.Tau);
	}
	
	private ImageTexture CreateFireflyTexture()
	{
		// Make it bigger and blockier - 16x16 instead of 8x8
		var img = Image.Create(16, 16, false, Image.Format.Rgba8);
		var yellow = new Color(1.0f, 0.9f, 0.3f, 1.0f);
		var darkYellow = new Color(0.8f, 0.7f, 0.2f, 1.0f);
		
		// Create a blocky cross pattern
		img.Fill(new Color(0, 0, 0, 0));
		
		// Center bright core (4x4)
		for (int x = 6; x <= 9; x++)
		{
			for (int y = 6; y <= 9; y++)
			{
				img.SetPixel(x, y, yellow);
			}
		}
		
		// Middle ring
		for (int x = 5; x <= 10; x++)
		{
			img.SetPixel(x, 5, darkYellow);
			img.SetPixel(x, 10, darkYellow);
		}
		for (int y = 6; y <= 9; y++)
		{
			img.SetPixel(5, y, darkYellow);
			img.SetPixel(10, y, darkYellow);
		}
		
		// Outer pixels for extra blockiness
		img.SetPixel(7, 4, darkYellow * 0.5f);
		img.SetPixel(8, 4, darkYellow * 0.5f);
		img.SetPixel(7, 11, darkYellow * 0.5f);
		img.SetPixel(8, 11, darkYellow * 0.5f);
		img.SetPixel(4, 7, darkYellow * 0.5f);
		img.SetPixel(4, 8, darkYellow * 0.5f);
		img.SetPixel(11, 7, darkYellow * 0.5f);
		img.SetPixel(11, 8, darkYellow * 0.5f);
		
		return ImageTexture.CreateFromImage(img);
	}
	
	public override void _Process(double delta)
	{
		// Choppy movement update
		moveTimer += (float)delta;
		if (moveTimer >= MoveInterval)
		{
			moveTimer = 0f;
			
			// Move in discrete steps
			targetPosition += velocity * MoveInterval;
			
			// Snap to pixel grid for extra crunchiness
			Position = new Vector2(
				Mathf.Round(targetPosition.X),
				Mathf.Round(targetPosition.Y)
			);
			
			// Wrap around screen
			var screenSize = GetViewportRect().Size;
			if (Position.X < -10)
			{
				Position = new Vector2(screenSize.X + 10, Position.Y);
				targetPosition = Position;
			}
			if (Position.X > screenSize.X + 10)
			{
				Position = new Vector2(-10, Position.Y);
				targetPosition = Position;
			}
			if (Position.Y < -10)
			{
				Position = new Vector2(Position.X, screenSize.Y + 10);
				targetPosition = Position;
			}
			if (Position.Y > screenSize.Y + 10)
			{
				Position = new Vector2(Position.X, -10);
				targetPosition = Position;
			}
		}
		
		// Choppy pulsing effect (frame-based instead of smooth)
		pulseTimer += (float)delta;
		if (pulseTimer >= PulseFrameTime)
		{
			pulseTimer = 0f;
			currentPulseFrame = (currentPulseFrame + 1) % 4; // 4 brightness frames
		}
		
		// Set brightness based on current frame (no smooth transition)
		float[] brightnessLevels = { 0.3f, 0.6f, 1.0f, 0.6f };
		float brightness = brightnessLevels[currentPulseFrame];
		Modulate = new Color(1, 1, 1, brightness);
		
		QueueRedraw();
	}
	
	public override void _Draw()
	{
		// Blockier glow effect - larger, fewer circles
		DrawCircle(Vector2.Zero, 16, new Color(1.0f, 0.9f, 0.3f, 0.08f));
		DrawCircle(Vector2.Zero, 10, new Color(1.0f, 0.9f, 0.3f, 0.15f));
	}
}
