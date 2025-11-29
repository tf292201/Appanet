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
		public string IconPath { get; private set; }
		private static Random _random = new Random();
		
		public Enemy(string name, int maxHealth, int attackPower, int defense, 
					 int experienceReward = 25, int minMoney = 5, int maxMoney = 15,
					 EnemyType enemyType = EnemyType.Normal,
					 DamageType damageType = DamageType.Physical,
					 string iconPath = "")
			: base(name, maxHealth, attackPower, defense)
		{
			ExperienceReward = experienceReward;
			MinMoneyDrop = minMoney;
			MaxMoneyDrop = maxMoney;
			EnemyType = enemyType;
			PrimaryDamageType = damageType;
			IconPath = iconPath;
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
				DamageType.Electric,
				"res://icons/enemies/Barnwire_Possum.png"
			);
		}
		
		public static Enemy CreateBackroadsGremmlin()
		{
			return new Enemy(
				"Backroads Gremmlin", 
				30, 8, 2, 
				15, 5, 15, 
				EnemyType.Tech, 
				DamageType.Physical,
				"res://icons/enemies/Backroads_Gremlin.png"
			);
		}
		
		public static Enemy CreateSkeletonKeyer()
		{
			var skeleton = new Enemy(
				"Skeleton Keyer", 
				25, 7, 3, 
				12, 4, 12, 
				EnemyType.Spectral, 
				DamageType.Curse,
				"res://icons/enemies/Skeleton_Keyer.png"
			);
			skeleton.Resistances.SetResistance(DamageType.Physical, 0.3f);
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
				DamageType.Physical,
				"res://icons/enemies/ridgerunner-howler.png"
			);
			howler.DodgeChance = 0.15f;
			return howler;
		}
		
		public static Enemy CreateOffGridScavver()
		{
			return new Enemy(
				"Off-Grid Scavver", 
				45, 11, 5, 
				28, 20, 45, 
				EnemyType.Normal, 
				DamageType.Physical,
				"res://icons/enemies/Off-grid_Scavver.png"
			);
		}
		
		public static Enemy CreateBunkerBrute()
		{
			return new Enemy(
				"Bunker Brute (Class-B Security Contractor)", 
				50, 12, 4, 
				30, 15, 35, 
				EnemyType.Normal, 
				DamageType.Physical,
				"res://icons/enemies/bunkerbrute.png"
			);
		}
		
		public static Enemy CreateBridgeTroll()
		{
			var troll = new Enemy(
				"Bridge-Troll of Exit 17", 
				70, 14, 6, 
				40, 25, 50, 
				EnemyType.Folklore, 
				DamageType.Physical,
				"res://icons/enemies/Bridge_Troll.png"
			);
			troll.Resistances.SetResistance(DamageType.Physical, 0.2f);
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
				DamageType.Curse,
				"res://icons/enemies/nightdialer.png"
			);
			dialer.Resistances.SetResistance(DamageType.Physical, 0.25f);
			dialer.Resistances.SetResistance(DamageType.Psychic, 0.5f);
			return dialer;
		}
		
		public static Enemy CreateHornedServerman()
		{
			var serverman = new Enemy(
				"Horned Serverman", 
				100, 18, 7, 
				70, 50, 80, 
				EnemyType.Folklore, 
				DamageType.Physical,
				"res://icons/enemies/Horned_Serverman.png"
			);
			serverman.Resistances.SetResistance(DamageType.Curse, 0.3f);
			return serverman;
		}
		
		public static Enemy CreateBlackBadgeEnforcer()
		{
			var enforcer = new Enemy(
				"Black-Badge Enforcer", 
				90, 17, 10, 
				65, 45, 75, 
				EnemyType.Normal, 
				DamageType.Curse,
				"res://icons/enemies/Blackbadge_Enforcer.png"
			);
			enforcer.Resistances.SetResistance(DamageType.Physical, 0.4f);
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
				DamageType.Electric,
				"res://icons/enemies/Thunder_Hollow_Wyrn.png"
			);
			wyrm.Resistances.SetResistance(DamageType.Physical, 0.3f);
			wyrm.Resistances.SetResistance(DamageType.Electric, 0.5f);
			return wyrm;
		}
		
		public static Enemy CreateArchivistOfEchoVault()
		{
			var archivist = new Enemy(
				"The Archivist of Echo Vault", 
				120, 22, 15, 
				120, 90, 160, 
				EnemyType.Spectral, 
				DamageType.Psychic,
				"res://icons/enemies/Archivist_EchoVault.png"
			);
			archivist.Resistances.SetResistance(DamageType.Physical, 0.5f);
			archivist.Resistances.SetResistance(DamageType.Psychic, 0.6f);
			archivist.Resistances.SetResistance(DamageType.Curse, 0.7f);
			return archivist;
		}
		
		public static Enemy CreateDirectorOfBeneathNet()
		{
			var director = new Enemy(
				"Director of the Beneath-Net", 
				200, 30, 14, 
				150, 100, 200, 
				EnemyType.Spectral, 
				DamageType.Curse,
				"res://icons/enemies/Director_BeneathNet.png"
			);
			director.Resistances.SetResistance(DamageType.Physical, 0.2f);
			director.Resistances.SetResistance(DamageType.Spectral, 0.5f);
			director.Resistances.SetResistance(DamageType.Curse, 0.8f);
			return director;
		}
	}
}
