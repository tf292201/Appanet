using System;
using Appanet.Scripts.Models.Characters;      // ← ADD: For Character
using Appanet.Scripts.Models.Combat;          // ← ADD: For Team enum


public class CombatParticipant
{
	public Character Character { get; private set; }
	public Team Team { get; private set; }
	public bool IsAI { get; private set; }
	public int TurnOrder { get; set; }
	public bool HasActedThisTurn { get; set; }
	
	// For allies
	public string AllyID { get; private set; }
	
	// For tracking enemy drops ← ADD THESE
	public int XPDropped { get; set; }
	public int MoneyDropped { get; set; }
	
	public bool IsAlive => Character.IsAlive;
	
	public CombatParticipant(Character character, Team team, bool isAI = false, string allyID = null)
	{
		Character = character;
		Team = team;
		IsAI = isAI;
		AllyID = allyID;
		HasActedThisTurn = false;
		TurnOrder = 0;
		XPDropped = 0;      // ← ADD THESE
		MoneyDropped = 0;   // ← ADD THESE
	}
	
	public string GetDisplayName()
	{
		if (AllyID != null)
		{
			return $"{Character.Name} (Ally)";
		}
		return Character.Name;
	}
}
