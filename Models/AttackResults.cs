namespace Appanet.Scripts.Models
{
	public class AttackResult
	{
		public int Damage { get; set; }
		public bool IsCritical { get; set; }
		public DamageType DamageType { get; set; }
		
		public AttackResult(int damage, bool isCritical, DamageType damageType)
		{
			Damage = damage;
			IsCritical = isCritical;
			DamageType = damageType;
		}
	}
	
	public class DamageResult
	{
		public int DamageTaken { get; set; }
		public bool WasDodged { get; set; }
		public int DamageBeforeDefense { get; set; }
		public int DefenseReduction { get; set; }
		
		public DamageResult(int damageTaken, bool wasDodged, int damageBeforeDefense, int defenseReduction)
		{
			DamageTaken = damageTaken;
			WasDodged = wasDodged;
			DamageBeforeDefense = damageBeforeDefense;
			DefenseReduction = defenseReduction;
		}
	}
}
