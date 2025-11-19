using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public class StatusEffectInstance
	{
		public StatusEffect Effect { get; private set; }
		public int Duration { get; private set; }  // Turns remaining
		public int Potency { get; private set; }   // Effect strength (e.g., poison damage per turn)
		
		public StatusEffectInstance(StatusEffect effect, int duration, int potency = 0)
		{
			Effect = effect;
			Duration = duration;
			Potency = potency;
		}
		
		public void DecrementDuration()
		{
			Duration--;
		}
		
		public bool IsExpired => Duration <= 0;
		
		public string GetDescription()
		{
			return Effect switch
			{
				StatusEffect.Poisoned => $"Poisoned ({Potency} damage/turn, {Duration} turns left)",
				StatusEffect.Stunned => $"Stunned ({Duration} turns left)",
				StatusEffect.Confused => $"Confused ({Duration} turns left)",
				StatusEffect.Cursed => $"Cursed (-{Potency}% stats, {Duration} turns left)",
				StatusEffect.Possessed => $"Possessed ({Duration} turns left)",
				StatusEffect.Blessed => $"Blessed (+{Potency}% stats, {Duration} turns left)",
				StatusEffect.Defending => $"Defending (+50% defense, {Duration} turns left)",
				StatusEffect.DefenseBoost => $"Defense Boost (+{Potency}% defense, {Duration} turns left)", 
				_ => "None"
			};
		}
	}
}
