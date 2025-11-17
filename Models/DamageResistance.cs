using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public class DamageResistance
	{
		private Dictionary<DamageType, float> _resistances;
		
		public DamageResistance()
		{
			_resistances = new Dictionary<DamageType, float>
			{
				{ DamageType.Physical, 0f },
				{ DamageType.Electric, 0f },
				{ DamageType.Psychic, 0f },
				{ DamageType.Spectral, 0f },
				{ DamageType.Curse, 0f }
			};
		}
		
		// Set resistance for a damage type (0.0 = 0%, 0.5 = 50%, 1.0 = 100% immune)
		public void SetResistance(DamageType type, float percentage)
		{
			_resistances[type] = Math.Clamp(percentage, -1f, 1f); // -100% to +100%
		}
		
		// Add to existing resistance
		public void AddResistance(DamageType type, float percentage)
		{
			_resistances[type] = Math.Clamp(_resistances[type] + percentage, -1f, 1f);
		}
		
		// Get resistance percentage
		public float GetResistance(DamageType type)
		{
			return _resistances[type];
		}
		
		// Calculate damage after resistance
		public int ApplyResistance(DamageType type, int baseDamage)
		{
			float resistance = GetResistance(type);
			float damageMultiplier = 1f - resistance;
			int finalDamage = (int)(baseDamage * damageMultiplier);
			
			return Math.Max(1, finalDamage); // Always at least 1 damage
		}
		
		public void DisplayResistances()
		{
			Console.WriteLine("\n--- Resistances ---");
			foreach (var kvp in _resistances)
			{
				if (kvp.Value != 0)
				{
					int percentage = (int)(kvp.Value * 100);
					string sign = percentage > 0 ? "+" : "";
					Console.WriteLine($"{kvp.Key}: {sign}{percentage}%");
				}
			}
		}
	}
}
