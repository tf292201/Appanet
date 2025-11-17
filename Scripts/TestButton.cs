using Godot;
using Appanet.Scripts.Models;
using Appanet.Scripts.Managers;

public partial class TestButton : Button
{
	public override void _Ready()
	{
		Text = "Test GameManager";
		Pressed += OnPressed;
	}
	
	private void OnPressed()
	{
		// Access the global GameManager
		var player = GameManager.Instance.Player;
		
		GD.Print("=== PLAYER INFO ===");
		GD.Print($"Name: {player.Name}");
		GD.Print($"HP: {player.Health}/{player.MaxHealth}");
		GD.Print($"Money: ${player.Money}");
		GD.Print($"Inventory: {player.Inventory.CurrentCount} items");
	}
}
