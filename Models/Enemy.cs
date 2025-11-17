using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public class Enemy : Character
	{
		public int ExperienceReward { get; private set; }
		public int MinMoneyDrop { get; private set; }
		public int MaxMoneyDrop { get; private set; }
		public EnemyType EnemyType { get; private set; }
		public DamageType PrimaryDamageType { get; private set; }
		private static Random _random = new Random();
		
		public Enemy(string name, int maxHealth, int attackPower, int defense, 
					 int experienceReward = 25, int minMoney = 5, int maxMoney = 15,
					 EnemyType enemyType = EnemyType.Normal,
					 DamageType damageType = DamageType.Physical)
			: base(name, maxHealth, attackPower, defense)
		{
			ExperienceReward = experienceReward;
			MinMoneyDrop = minMoney;
			MaxMoneyDrop = maxMoney;
			EnemyType = enemyType;
			PrimaryDamageType = damageType;
		}
		
		public override int Attack()
		{
			Console.WriteLine($"{Name} attacks!");
			return base.Attack();
		}
		
		public override int Attack(DamageType damageType)
		{
			Console.WriteLine($"{Name} attacks with {PrimaryDamageType} energy!");
			return base.Attack(PrimaryDamageType);
		}
		
		public int GetMoneyDrop()
		{
			return _random.Next(MinMoneyDrop, MaxMoneyDrop + 1);
		}
		
		protected override void OnDeath()
		{
			base.OnDeath();
			int moneyDropped = GetMoneyDrop();
			Console.WriteLine($"{Name} dropped {ExperienceReward} XP and ${moneyDropped}!");
		}
		
		public void Describe()
		{
			Console.WriteLine($"\n{Name} [{EnemyType}]");
			DisplayStats();
		}
		
		// ===== TIER 1: WEAK ENEMIES (Roadside Oddities) =====
		
		public static Enemy CreateBarnWirePossum()
		{
			return new Enemy(
				"Barn-Wire Possum", 
				20, 6, 1, 
				10, 2, 8, 
				EnemyType.Normal, 
				DamageType.Electric
			);
		}
		
		public static Enemy CreateBackroadsGremmlin()
		{
			return new Enemy(
				"Backroads Gremmlin", 
				30, 8, 2, 
				15, 5, 15, 
				EnemyType.Tech, 
				DamageType.Physical
			);
		}
		
		public static Enemy CreateSkeletonKeyer()
		{
			var skeleton = new Enemy(
				"Skeleton Keyer", 
				25, 7, 3, 
				12, 4, 12, 
				EnemyType.Spectral, 
				DamageType.Curse
			);
			skeleton.Resistances.SetResistance(DamageType.Physical, 0.3f); // 30% physical resistance
			return skeleton;
		}
		
		// ===== TIER 2: MEDIUM ENEMIES (Stranger Wilderness / Low-level Federal Oddities) =====
		
		public static Enemy CreateRidgeRunnerHowler()
		{
			var howler = new Enemy(
				"Ridge-Runner Howler", 
				40, 10, 3, 
				25, 10, 30, 
				EnemyType.Normal, 
				DamageType.Physical
			);
			howler.DodgeChance = 0.15f; // 15% dodge chance
			return howler;
		}
		
		public static Enemy CreateOffGridScavver()
		{
			return new Enemy(
				"Off-Grid Scavver", 
				45, 11, 5, 
				28, 20, 45, 
				EnemyType.Normal, 
				DamageType.Physical
			);
		}
		
		public static Enemy CreateBunkerBrute()
		{
			return new Enemy(
				"Bunker Brute (Class-B Security Contractor)", 
				50, 12, 4, 
				30, 15, 35, 
				EnemyType.Normal, 
				DamageType.Physical
			);
		}
		
		public static Enemy CreateBridgeTroll()
		{
			var troll = new Enemy(
				"Bridge-Troll of Exit 17", 
				70, 14, 6, 
				40, 25, 50, 
				EnemyType.Folklore, 
				DamageType.Physical
			);
			troll.Resistances.SetResistance(DamageType.Physical, 0.2f); // 20% physical resistance
			return troll;
		}
		
		// ===== TIER 3: HARD ENEMIES (Spectral Tech, Advanced Feds, Folklore Gone Wrong) =====
		
		public static Enemy CreateNightDialer()
		{
			var dialer = new Enemy(
				"The Night-Dialer", 
				80, 16, 8, 
				60, 40, 70, 
				EnemyType.Spectral, 
				DamageType.Curse
			);
			dialer.Resistances.SetResistance(DamageType.Physical, 0.25f); // 25% physical resistance
			dialer.Resistances.SetResistance(DamageType.Psychic, 0.5f);   // 50% psychic resistance
			return dialer;
		}
		
		public static Enemy CreateHornedServerman()
		{
			var serverman = new Enemy(
				"Horned Serverman", 
				100, 18, 7, 
				70, 50, 80, 
				EnemyType.Folklore, 
				DamageType.Physical
			);
			serverman.Resistances.SetResistance(DamageType.Curse, 0.3f); // 30% curse resistance
			return serverman;
		}
		
		public static Enemy CreateBlackBadgeEnforcer()
		{
			var enforcer = new Enemy(
				"Black-Badge Enforcer", 
				90, 17, 10, 
				65, 45, 75, 
				EnemyType.Normal, 
				DamageType.Curse
			);
			enforcer.Resistances.SetResistance(DamageType.Physical, 0.4f); // 40% physical resistance (ceramic armor)
			return enforcer;
		}
		
		// ===== TIER 4: BOSS ENEMIES (Cosmic Infrastructure, Appalachian Megafauna, Gov't Secrets) =====
		
		public static Enemy CreateThunderHollowWyrm()
		{
			var wyrm = new Enemy(
				"Thunder-Hollow Wyrm", 
				150, 25, 12, 
				100, 80, 150, 
				EnemyType.Folklore, 
				DamageType.Electric
			);
			wyrm.Resistances.SetResistance(DamageType.Physical, 0.3f);  // 30% physical resistance
			wyrm.Resistances.SetResistance(DamageType.Electric, 0.5f);  // 50% electric resistance
			return wyrm;
		}
		
		public static Enemy CreateArchivistOfEchoVault()
		{
			var archivist = new Enemy(
				"The Archivist of Echo Vault", 
				120, 22, 15, 
				120, 90, 160, 
				EnemyType.Spectral, 
				DamageType.Psychic
			);
			archivist.Resistances.SetResistance(DamageType.Physical, 0.5f); // 50% physical resistance
			archivist.Resistances.SetResistance(DamageType.Psychic, 0.6f);  // 60% psychic resistance
			archivist.Resistances.SetResistance(DamageType.Curse, 0.7f);    // 70% curse resistance
			return archivist;
		}
		
		public static Enemy CreateDirectorOfBeneathNet()
		{
			var director = new Enemy(
				"Director of the Beneath-Net", 
				200, 30, 14, 
				150, 100, 200, 
				EnemyType.Spectral, 
				DamageType.Curse
			);
			director.Resistances.SetResistance(DamageType.Physical, 0.2f);  // 20% physical resistance
			director.Resistances.SetResistance(DamageType.Spectral, 0.5f);  // 50% spectral resistance
			director.Resistances.SetResistance(DamageType.Curse, 0.8f);     // 80% curse resistance
			return director;
		}
	}
}
