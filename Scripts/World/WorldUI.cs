using Godot;
using Appanet.Managers;

namespace Appanet.Scripts.World
{
	public partial class WorldUI : CanvasLayer
	{
		private Label _hpLabel;
		private Label _moneyLabel;
		private Button _menuButton;
		
		public override void _Ready()
		{
			_hpLabel = GetNode<Label>("Control/TopBar/HPLabel");
			_moneyLabel = GetNode<Label>("Control/TopBar/MoneyLabel");
			_menuButton = GetNode<Button>("Control/MenuButton");
			
			_menuButton.Pressed += OnMenuPressed;
			
			UpdateUI();
		}
		
		public override void _Process(double delta)
		{
			UpdateUI();
		}
		
		private void UpdateUI()
		{
			if (GameManager.Instance?.Player == null) return;
			
			var player = GameManager.Instance.Player;
			_hpLabel.Text = $"HP: {player.Health}/{player.MaxHealth}";
			_moneyLabel.Text = $"${player.Money}";
		}
		
		private void OnMenuPressed()
		{
			WorldManager.Instance?.OpenInventory();
		}
	}
}
