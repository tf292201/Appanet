using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Appanet.Scripts.Models.Items;

namespace Appanet.Scripts.Models
{
	public class Inventory
	{
		private List<Item> _items;
		private int _maxCapacity;
		
		public int Capacity => _maxCapacity;
		public int CurrentCount => _items.Count;
		public bool IsFull => CurrentCount >= _maxCapacity;
		
		public Inventory(int maxCapacity = 20)
		{
			_maxCapacity = maxCapacity;
			_items = new List<Item>();
		}
		
		public bool AddItem(Item item)
		{
			if (IsFull)
			{
				Console.WriteLine("Inventory is full!");
				return false;
			}
			
			_items.Add(item);
			Console.WriteLine($"Added {item.Name} to inventory.");
			return true;
		}
		
		public bool RemoveItem(Item item)
		{
			if (_items.Remove(item))
			{
				Console.WriteLine($"Removed {item.Name} from inventory.");
				return true;
			}
			
			Console.WriteLine($"{item.Name} not found in inventory.");
			return false;
		}
		
		public Item? GetItem(string itemName)
		{
			return _items.FirstOrDefault(i => 
				i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
		}
		
		public List<Item> GetAllItems()
		{
			return new List<Item>(_items); // Return copy to protect internal list
		}
		
		public void DisplayInventory()
		{
			Console.WriteLine($"\n=== Inventory ({CurrentCount}/{Capacity}) ===");
			
			if (_items.Count == 0)
			{
				Console.WriteLine("Empty");
				return;
			}
			
			for (int i = 0; i < _items.Count; i++)
			{
				Console.WriteLine($"{i + 1}. {_items[i].Name} - {_items[i].Description}");
			}
		}
		
		public bool HasItem(string itemName)
		{
			return _items.Any(i => 
				i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
		}
	}
}
