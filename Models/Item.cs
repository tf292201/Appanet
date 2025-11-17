using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Appanet.Scripts.Models
{
	public abstract class Item
	{
		public string Name { get; protected set; }
		public string Description { get; protected set; }
		public int Value { get; protected set; }
		
		protected Item(string name, string description, int value = 0)
		{
			Name = name;
			Description = description;
			Value = value;
		}
		
		// Changed from Player to Character to avoid circular dependency
		public abstract void Use(Character character);
		
		public virtual void Examine()
		{
			Console.WriteLine($"\n{Name}");
			Console.WriteLine($"{Description}");
			Console.WriteLine($"Value: {Value} gold");
		}
	}
}
