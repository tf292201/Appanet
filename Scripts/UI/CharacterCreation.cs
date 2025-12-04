using Godot;
using Appanet.Managers;

public partial class CharacterCreation : Control
{
	[Export] private Node2D fireflyContainer;
	[Export] private LineEdit nameInput;
	[Export] private Button confirmButton;
	[Export] private Label promptLabel;
	[Export] private Label subtleHintLabel;
	
	private PackedScene fireflyScene;
	private Timer spawnTimer;
	
	// Center point for firefly emission
	private Vector2 centerPoint;
	private int currentFireflyCount = 0;
	
	// Pulsing effect
	private float pulseTimer = 0f;
	private const float PulseInterval = 3.0f;
	
	// Phase system for alternating normal/glitch
	private enum Phase
	{
		Normal,
		Transition,
		Glitching
	}
	
	private Phase currentPhase = Phase.Normal;
	private float phaseTimer = 0f;
	private const float NormalPhaseDuration = 8.0f;
	private const float TransitionDuration = 1.5f;
	private const float GlitchPhaseDuration = 4.0f;
	
	// Phase-specific settings
	private float CurrentSpawnRate => currentPhase switch
	{
		Phase.Normal => 0.25f,      // Slow, peaceful (4 per second)
		Phase.Transition => 0.10f,  // Starting to speed up
		Phase.Glitching => 0.02f,   // FAST chaos (50 per second!)
		_ => 0.25f
	};
	
	private int CurrentMaxFireflies => currentPhase switch
	{
		Phase.Normal => 40,         // Fewer fireflies
		Phase.Transition => 80,     // Medium
		Phase.Glitching => 180,     // TONS
		_ => 40
	};
	
	public override void _Ready()
	{
		fireflyScene = GD.Load<PackedScene>("res://Scenes/Entities/Firefly.tscn");
		
		fireflyContainer = GetNode<Node2D>("FireflyContainer");
		nameInput = GetNode<LineEdit>("CenterPanel/VBox/NameInput");
		confirmButton = GetNode<Button>("CenterPanel/VBox/ConfirmButton");
		promptLabel = GetNode<Label>("CenterPanel/VBox/PromptLabel");
		subtleHintLabel = GetNode<Label>("SubtleHint");
		
		confirmButton.Pressed += OnConfirmPressed;
		
		var viewportSize = GetViewportRect().Size;
		centerPoint = viewportSize / 2;
		
		// Setup dynamic spawn timer
		spawnTimer = new Timer();
		spawnTimer.WaitTime = CurrentSpawnRate;
		spawnTimer.Timeout += SpawnFirefly;
		spawnTimer.Autostart = true;
		AddChild(spawnTimer);
		
		nameInput.GrabFocus();
		
		subtleHintLabel.Text = "The signal grows stronger at night...";
		subtleHintLabel.Modulate = new Color(1, 1, 1, 0);
		var tween = CreateTween();
		tween.TweenInterval(2.0);
		tween.TweenProperty(subtleHintLabel, "modulate:a", 0.4f, 3.0);
		subtleHintLabel.AddThemeFontSizeOverride("font_size", 24);
	}
	
	private void SpawnFirefly()
	{
		if (currentFireflyCount >= CurrentMaxFireflies)
		{
			CleanupDistantFireflies();
			return;
		}
		
		var firefly = fireflyScene.Instantiate<Node2D>();
		
		// Spawn offset varies by phase
		float offsetRange = currentPhase switch
		{
			Phase.Normal => 15f,        // Wider spawn area (more natural)
			Phase.Transition => 10f,    // Getting tighter
			Phase.Glitching => 5f,      // Very tight (concentrated signal)
			_ => 15f
		};
		
		Vector2 offset = new Vector2(
			GD.Randf() * offsetRange * 2 - offsetRange,
			GD.Randf() * offsetRange * 2 - offsetRange
		);
		firefly.Position = centerPoint + offset;
		
		float angle = GD.Randf() * Mathf.Tau;
		Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		
		// Speed depends on phase
		float speed = GetSpeedForPhase();
		
		firefly.SetMeta("drift_direction", direction);
		firefly.SetMeta("drift_speed", speed);
		firefly.SetMeta("birth_time", Time.GetTicksMsec());
		firefly.SetMeta("base_speed", speed); // Store original speed
		
		// Color based on phase
		int colorMode = DetermineColorMode();
		firefly.SetMeta("color_mode", colorMode);
		
		// Fade in (more common in normal phase)
		float fadeChance = currentPhase == Phase.Normal ? 0.6f : 0.3f;
		if (GD.Randf() < fadeChance)
		{
			firefly.Modulate = new Color(1, 1, 1, 0.2f);
			var fadeTween = CreateTween();
			fadeTween.TweenProperty(firefly, "modulate:a", 1.0f, 0.8f);
		}
		
		fireflyContainer.AddChild(firefly);
		currentFireflyCount++;
	}
	
	private float GetSpeedForPhase()
	{
		if (currentPhase == Phase.Normal)
		{
			// Slow, natural drift
			return GD.Randf() * 20 + 15; // 15-35 pixels/sec (SLOW)
		}
		else if (currentPhase == Phase.Transition)
		{
			// Mix of slow and medium
			if (GD.Randf() < 0.5f)
				return GD.Randf() * 30 + 20; // 20-50 slow
			else
				return GD.Randf() * 60 + 50; // 50-110 medium
		}
		else // Glitching
		{
			// Wide variation - some slow, most FAST
			float roll = GD.Randf();
			if (roll < 0.2f)
				return GD.Randf() * 30 + 10;  // 10-40 slow
			else if (roll < 0.5f)
				return GD.Randf() * 80 + 50;  // 50-130 medium
			else
				return GD.Randf() * 200 + 150; // 150-350 SUPER FAST!
		}
	}
	
	private int DetermineColorMode()
	{
		if (currentPhase == Phase.Normal)
		{
			// 98% normal yellow, 2% subtle green hint
			return GD.Randf() < 0.98f ? 0 : 2;
		}
		else if (currentPhase == Phase.Transition)
		{
			// Gradually more corruption
			float roll = GD.Randf();
			if (roll < 0.6f)
				return 0; // Normal
			else if (roll < 0.8f)
				return 2; // Green
			else
				return 1; // Red
		}
		else // Glitching
		{
			// CHAOS!
			float roll = GD.Randf();
			if (roll < 0.35f)
				return 1; // Red
			else if (roll < 0.70f)
				return 2; // Green
			else if (roll < 0.92f)
				return 3; // White
			else
				return 0; // Rare normal (unsettling)
		}
	}
	
	public override void _Process(double delta)
	{
		// Update phase timer
		phaseTimer += (float)delta;
		UpdatePhase();
		
		// Pulse effect timer (only during glitch phase)
		if (currentPhase == Phase.Glitching)
		{
			pulseTimer += (float)delta;
			if (pulseTimer >= PulseInterval)
			{
				pulseTimer = 0f;
				TriggerPulse();
			}
		}
		
		// Move fireflies
		foreach (Node child in fireflyContainer.GetChildren())
		{
			if (child is Node2D firefly)
			{
				Vector2 direction = (Vector2)firefly.GetMeta("drift_direction");
				float baseSpeed = (float)firefly.GetMeta("base_speed");
				
				// Acceleration only during glitch phase
				float speed = baseSpeed;
				if (currentPhase == Phase.Glitching)
				{
					ulong birthTime = (ulong)firefly.GetMeta("birth_time");
					float age = (Time.GetTicksMsec() - birthTime) / 1000.0f;
					speed = baseSpeed * (1.0f + age * 0.3f); // Accelerate in glitch mode
				}
				
				firefly.Position += direction * speed * (float)delta;
				
				// Jitter only for glitchy fireflies in glitch phase
				if (currentPhase == Phase.Glitching && firefly.HasMeta("color_mode"))
				{
					int colorMode = (int)firefly.GetMeta("color_mode");
					if (colorMode != 0) // Not normal color
					{
						if (GD.Randf() < 0.08f)
						{
							firefly.Position += new Vector2(
								GD.Randf() * 25 - 12.5f,
								GD.Randf() * 25 - 12.5f
							);
						}
					}
				}
			}
		}
	}
	
	private void UpdatePhase()
	{
		float duration = currentPhase switch
		{
			Phase.Normal => NormalPhaseDuration,
			Phase.Transition => TransitionDuration,
			Phase.Glitching => GlitchPhaseDuration,
			_ => NormalPhaseDuration
		};
		
		if (phaseTimer >= duration)
		{
			phaseTimer = 0f;
			
			currentPhase = currentPhase switch
			{
				Phase.Normal => Phase.Transition,
				Phase.Transition => Phase.Glitching,
				Phase.Glitching => Phase.Normal,
				_ => Phase.Normal
			};
			
			GD.Print($"📡 Phase changed to: {currentPhase}");
			
			// Update spawn rate dynamically
			spawnTimer.WaitTime = CurrentSpawnRate;
			
			// Update hint text
			if (currentPhase == Phase.Glitching)
			{
				var customFont = GD.Load<FontFile>("res://Assets/Fonts/RETROTECH.ttf");
			subtleHintLabel.AddThemeFontOverride("font", customFont);
			subtleHintLabel.AddThemeFontSizeOverride("font_size", 28);
				
				
				subtleHintLabel.Text = ":::SIGNAL CORRUPTION DETECTED:::";
				var tween = CreateTween();
				tween.TweenProperty(subtleHintLabel, "modulate", new Color(1, 0.2f, 0.2f, 0.8f), 0.3);
				subtleHintLabel.AddThemeFontSizeOverride("font_size", 24);
			}
			else if (currentPhase == Phase.Normal)
			{
				subtleHintLabel.Text = "The signal grows stronger at night...";
				var tween = CreateTween();
				tween.TweenProperty(subtleHintLabel, "modulate", new Color(1, 1, 1, 0.4f), 2.0);
			}
			else // Transition
			{
				subtleHintLabel.Text = "Something's changing...";
			}
		}
	}
	
	private void TriggerPulse()
	{
		GD.Print("💥 GLITCH PULSE");
		
		foreach (Node child in fireflyContainer.GetChildren())
		{
			if (child is Node2D firefly)
			{
				Color currentColor = firefly.Modulate;
				
				var tween = CreateTween();
				tween.TweenProperty(firefly, "modulate", new Color(1.5f, 1.5f, 1.5f, 1.0f), 0.08);
				tween.TweenProperty(firefly, "modulate", currentColor, 0.2);
				
				// Big speed boost
				if (GD.Randf() < 0.5f)
				{
					float baseSpeed = (float)firefly.GetMeta("base_speed");
					firefly.SetMeta("base_speed", baseSpeed * 3.0f);
					
					var speedTween = CreateTween();
					speedTween.TweenInterval(0.4);
					speedTween.TweenCallback(Callable.From(() => {
						if (IsInstanceValid(firefly))
						{
							firefly.SetMeta("base_speed", baseSpeed);
						}
					}));
				}
			}
		}
		
		// Spawn burst
		for (int i = 0; i < 20; i++)
		{
			SpawnFirefly();
		}
	}
	
	private void CleanupDistantFireflies()
	{
		var viewportSize = GetViewportRect().Size;
		float maxDistance = viewportSize.Length() * 1.5f;
		
		foreach (Node child in fireflyContainer.GetChildren())
		{
			if (child is Node2D firefly)
			{
				float distance = firefly.Position.DistanceTo(centerPoint);
				if (distance > maxDistance)
				{
					firefly.QueueFree();
					currentFireflyCount--;
				}
			}
		}
	}
	
	private void OnConfirmPressed()
	{
		string characterName = nameInput.Text.Trim();
		
		if (string.IsNullOrEmpty(characterName))
		{
			promptLabel.Text = "Please enter a name...";
			promptLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			return;
		}
		
		GD.Print($"Character named: {characterName}");
		GameManager.Instance.InitializeNewGame(characterName);
		GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
	}
}
