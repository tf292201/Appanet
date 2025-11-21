using Godot;
using Appanet.Scripts.Models;
using System.Linq; 
using System.Collections.Generic;

namespace Appanet.Scripts.Managers
{
	public partial class GameManager : Node
	{
		public static GameManager Instance { get; private set; }
		
		public Player Player { get; private set; }
		public List<Ally> PartyMembers { get; private set; }  // NEW - Track party
		
		public override void _Ready()
		{
			Instance = this;
			PartyMembers = new List<Ally>();  // NEW
			InitializeNewGame();
		}
		
		public void InitializeNewGame()
		{
			Player = new Player("Investigator", 100, 10, 3);
	
			// Add and EQUIP starting weapon
			var baseballBat = Weapon.CreateBaseballBat();
			Player.Inventory.AddItem(baseballBat);
			Player.EquipWeapon(baseballBat);
	
			// Add consumables for testing
			Player.Inventory.AddItem(Consumable.CreateHealthPotion());
			Player.Inventory.AddItem(Consumable.CreateHealthPotion());
			Player.Inventory.AddItem(Consumable.CreateMegaPotion());
			Player.Inventory.AddItem(Consumable.CreateMasonJarMountainDew());
			Player.Inventory.AddItem(Consumable.CreatePepperoniRoll());
	
			GD.Print("Game initialized!");
		}
		
		// NEW - Add ally to party
		public void AddAllyToParty(Ally ally)
{
	// Check if this ally is already in the party by AllyID
	if (PartyMembers.Any(a => a.AllyID == ally.AllyID))
	{
		GD.Print($"{ally.Name} is already in the party!");
		return;
	}
	
	PartyMembers.Add(ally);
	GD.Print($"{ally.Name} joined the party!");
}
		
		// NEW - Remove ally from party
		public void RemoveAllyFromParty(Ally ally)
		{
			if (PartyMembers.Contains(ally))
			{
				PartyMembers.Remove(ally);
				GD.Print($"{ally.Name} left the party.");
			}
		}
		
		// NEW - Get all active party members (player + allies)
		public List<Character> GetActiveParty()
		{
			var party = new List<Character> { Player };
			party.AddRange(PartyMembers);
			return party;
		}
		
		// Testing helper
		public void DebugPrintPlayerStatus()
		{
			GD.Print($"\n=== {Player.Name} ===");
			GD.Print($"HP: {Player.Health}/{Player.MaxHealth}");
			GD.Print($"Money: ${Player.Money}");
			GD.Print($"Inventory: {Player.Inventory.CurrentCount} items");
		}
	}
}
