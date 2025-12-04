using Godot;
using Appanet.Managers;

public partial class Opening : Control
{
	[Export] private Label quoteText;
	[Export] private Node2D fireflyContainer;
	[Export] private Control menuScreen;
	[Export] private Button startButton;
	[Export] private Button loadButton;
	
	private PackedScene fireflyScene;
	private const int FireflyCount = 20;
	
	public override void _Ready()
	{
		// Load firefly scene
		fireflyScene = GD.Load<PackedScene>("res://Scenes/Entities/Firefly.tscn");
		
		// Get node references
		quoteText = GetNode<Label>("QuoteText");
		fireflyContainer = GetNode<Node2D>("FireflyContainer");
		menuScreen = GetNode<Control>("MenuScreen");
		startButton = GetNode<Button>("MenuScreen/MenuButtons/StartButton");
		loadButton = GetNode<Button>("MenuScreen/MenuButtons/LoadButton");
		
		// Connect buttons
		startButton.Pressed += OnNewGamePressed;
		loadButton.Pressed += OnLoadGamePressed;
		
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
		var tween = CreateTween();
		tween.TweenProperty(quoteText, "modulate:a", 1.0, 2.0);
		tween.TweenInterval(3.0);
		tween.TweenProperty(quoteText, "modulate:a", 0.0, 1.5);
		tween.TweenCallback(Callable.From(TransitionToMenu));
	}
	
	private void TransitionToMenu()
	{
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(fireflyContainer, "modulate:a", 0.0, 2.0);
		tween.TweenProperty(menuScreen, "modulate:a", 1.0, 2.0);
	}
	
	private void OnNewGamePressed()
{
	GD.Print("🎮 Starting New Game...");
	// Go to character creation instead of directly to world
	GetTree().ChangeSceneToFile("res://Scenes/UI/CharacterCreation.tscn");
}
	
	private void OnLoadGamePressed()
	{
		GD.Print("💾 Load Game - Not implemented yet!");
		// TODO: Add save/load system later
	}
}
