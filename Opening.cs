using Godot;
using System;

public partial class Opening : Control
{
	[Export] private Label quoteText;
	[Export] private Node2D fireflyContainer;
	[Export] private Control menuScreen;
	
	private PackedScene fireflyScene;
	private const int FireflyCount = 20;
	
	public override void _Ready()
	{
		// Load firefly scene
		fireflyScene = GD.Load<PackedScene>("res://Firefly.tscn");
		
		// Get node references (or use [Export] and assign in editor)
		quoteText = GetNode<Label>("QuoteText");
		fireflyContainer = GetNode<Node2D>("FireflyContainer");
		menuScreen = GetNode<Control>("MenuScreen");
		
		// Start with everything hidden
		quoteText.Modulate = new Color(1, 1, 1, 0);
		menuScreen.Modulate = new Color(1, 1, 1, 0);
		
		// Spawn fireflies
		SpawnFireflies();
		
		// Start the sequence
		ShowText();
	}
	
	private void SpawnFireflies()
	{
		var viewportSize = GetViewportRect().Size;
		
		for (int i = 0; i < FireflyCount; i++)
		{
			var firefly = fireflyScene.Instantiate<Sprite2D>();
			firefly.Position = new Vector2(
				GD.Randf() * viewportSize.X,
				GD.Randf() * viewportSize.Y
			);
			fireflyContainer.AddChild(firefly);
		}
	}
	
	private void ShowText()
	{
		// Fade in text
		var tween = CreateTween();
		tween.TweenProperty(quoteText, "modulate:a", 1.0, 2.0);
		tween.TweenInterval(3.0); // Hold the text
		tween.TweenProperty(quoteText, "modulate:a", 0.0, 1.5);
		tween.TweenCallback(Callable.From(TransitionToMenu));
	}
	
	private void TransitionToMenu()
	{
		// Fade out fireflies and fade in menu
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(fireflyContainer, "modulate:a", 0.0, 2.0);
		tween.TweenProperty(menuScreen, "modulate:a", 1.0, 2.0);
	}
}
