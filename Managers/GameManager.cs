using Godot;
using Appanet.Scripts.Models;

namespace Appanet.Scripts.Managers
{
	public partial class GameManager : Node
	{
		public static GameManager Instance { get; private set; }
		
		public Player Player { get; private set; }
		
		public override void _Ready()
		{
			Instance = this;
			InitializeNewGame();
		}
		
		public void InitializeNewGame()
		{
		Player = new Player("Investigator", 100, 10, 3);
	
		// Add and EQUIP starting weapon
		var baseballBat = Weapon.CreateBaseballBat();
		Player.Inventory.AddItem(baseballBat);
		Player.EquipWeapon(baseballBat);  // ← ADD THIS
	
		// Add consumables for testing
		Player.Inventory.AddItem(Consumable.CreateHealthPotion());
		Player.Inventory.AddItem(Consumable.CreateHealthPotion());
		Player.Inventory.AddItem(Consumable.CreateMegaPotion());
		Player.Inventory.AddItem(Consumable.CreateMasonJarMountainDew());
		Player.Inventory.AddItem(Consumable.CreatePepperoniRoll());
	
		GD.Print("Game initialized!");
		}
		
		// Testing helper
		public void DebugPrintPlayerStatus()
		{
			GD.Print($"\n=== {Player.Name} ===");
			GD.Print($"HP: {Player.Health}/{Player.MaxHealth}");
			GD.Print($"ATK: {Player.AttackPower}");
			GD.Print($"DEF: {Player.Defense}");
			GD.Print($"Money: ${Player.Money}");
			GD.Print($"Inventory: {Player.Inventory.CurrentCount}/{Player.Inventory.Capacity}");
		}
	}
}
