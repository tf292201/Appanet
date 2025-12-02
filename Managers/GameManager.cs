using Godot;
using Appanet.Scripts.Models.Characters;  
using Appanet.Scripts.Models.Items;       
using System.Linq; 
using System.Collections.Generic;

namespace Appanet.Managers
{
	public partial class GameManager : Node
	{
		public static GameManager Instance { get; private set; }
		
		public Player Player { get; private set; }
		public List<Ally> PartyMembers { get; private set; }
		
		// NEW - Track defeated enemies persistently
		public HashSet<string> DefeatedEnemies { get; private set; }
		
		public override void _Ready()
		{
			Instance = this;
			PartyMembers = new List<Ally>();
			DefeatedEnemies = new HashSet<string>();  // NEW
			InitializeNewGame();
		}
		
		public void InitializeNewGame()
		{
			Player = new Player("Investigator", 100, 10, 3, "res://Assets/Icons/party/Investigator.png");

			// Add starting weapons
			var baseballBat = Weapon.CreateBaseballBat();
			Player.Inventory.AddItem(baseballBat);
			Player.EquipWeapon(baseballBat);
			
			Player.Inventory.AddItem(Weapon.CreateTireIron());
			Player.Inventory.AddItem(Weapon.CreateMagliteFlashlight());
			Player.Inventory.AddItem(Weapon.CreatePocketKnife());
			Player.Inventory.AddItem(Weapon.CreateCodebookHaintbreaker());
			Player.Inventory.AddItem(Weapon.CreateHamRadioTuningRod());
			Player.Inventory.AddItem(Weapon.CreateFloppySigils());
			Player.Inventory.AddItem(Weapon.CreateAcousticCouplerGrenade());
			Player.Inventory.AddItem(Weapon.CreateCRTBlaster());
			
			// Add starting armor
			var leatherJacket = Armor.CreateLeatherJacket();
			Player.Inventory.AddItem(leatherJacket);
			Player.EquipArmor(leatherJacket);
			
			Player.Inventory.AddItem(Armor.CreateFlannel());
			Player.Inventory.AddItem(Armor.CreateConstructionVest());
			Player.Inventory.AddItem(Armor.CreateMountainModemMail());
			Player.Inventory.AddItem(Armor.CreateFloppyPlateJerkin());
			Player.Inventory.AddItem(Armor.CreateCRTCathodeCarapace());
			Player.Inventory.AddItem(Armor.CreateVHSPhantomPoncho());
			Player.Inventory.AddItem(Armor.CreateAppalachianSysOpDuster());
			Player.Inventory.AddItem(Armor.CreateCarhartJacket());

			// Add consumables for testing
			Player.Inventory.AddItem(Consumable.CreateMasonJarMountainDew());
			Player.Inventory.AddItem(Consumable.CreatePepperoniRoll());
			Player.Inventory.AddItem(Consumable.CreateGasolineCoffee());
			Player.Inventory.AddItem(Consumable.CreateMoonPie());
			Player.Inventory.AddItem(Consumable.CreateDialUpHealingTonic());
			Player.Inventory.AddItem(Consumable.CreateCoalDustCandiedPecans());
			Player.Inventory.AddItem(Consumable.CreateVHSComfortBlanket());
			Player.Inventory.AddItem(Consumable.CreateTerminalCuredJerky());
			Player.Inventory.AddItem(Consumable.CreateTerminalCuredJerky());
			
			// ADD ALLIES HERE ONCE
			var michael = Ally.CreateMichaelWebb();
			var casey = Ally.CreateCase();
			AddAllyToParty(michael);
			AddAllyToParty(casey);

			GD.Print("Game initialized with starting equipment!");
		}
		
		public void AddAllyToParty(Ally ally)
		{
			if (PartyMembers.Any(a => a.AllyID == ally.AllyID))
			{
				GD.Print($"{ally.Name} is already in the party!");
				return;
			}
			
			PartyMembers.Add(ally);
			GD.Print($"{ally.Name} joined the party!");
		}
		
		public void RemoveAllyFromParty(Ally ally)
		{
			if (PartyMembers.Contains(ally))
			{
				PartyMembers.Remove(ally);
				GD.Print($"{ally.Name} left the party.");
			}
		}
		
		public List<Character> GetActiveParty()
		{
			var party = new List<Character> { Player };
			party.AddRange(PartyMembers);
			return party;
		}
		
		// NEW - Enemy defeat tracking
		public void MarkEnemyDefeated(string enemyID)
		{
			DefeatedEnemies.Add(enemyID);
			GD.Print($"✅ Enemy marked as defeated: {enemyID} (Total: {DefeatedEnemies.Count})");
		}
		
		public bool IsEnemyDefeated(string enemyID)
		{
			return DefeatedEnemies.Contains(enemyID);
		}
		
		public void DebugPrintPlayerStatus()
		{
			GD.Print($"\n=== {Player.Name} ===");
			GD.Print($"HP: {Player.Health}/{Player.MaxHealth}");
			GD.Print($"Money: ${Player.Money}");
			GD.Print($"Inventory: {Player.Inventory.CurrentCount} items");
		}
	}
}
