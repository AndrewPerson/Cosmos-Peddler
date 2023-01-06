using Godot;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class MarketNode : PanelContainer
{
	private Waypoint _waypoint = null!;

	public Waypoint Waypoint
	{
		get => _waypoint;
		set
		{
			_waypoint = value;
			UpdateMarketInfo();
		}
	}

	[Export]
	public PackedScene marketItemScene = null!;

	private Label status = null!;
	private Control marketItems = null!;

	public override void _EnterTree()
	{
		status = GetNode<Label>("%Status");
		marketItems = GetNode<Control>("%Items List");
	}

	private void SetStatus(string text)
	{
		status.Text = text;
		status.Visible = true;
		marketItems.Visible = false;
	}

	private void ClearStatus()
	{
		status.Visible = false;
		marketItems.Visible = true;
	}

	private void UpdateMarketInfo()
	{
		if (!Waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.MARKETPLACE))
		{
			SetStatus("No market");

			return;
		}

		SetStatus("Loading...");

		Waypoint.GetMarket().ContinueWith(task =>
		{
			if (task.IsFaulted)
			{
				GD.PrintErr(task.Exception);
				SetStatus(task.Exception?.Message ?? "Unknown error");
				return;
			}

			var market = task.Result;

			if (market == null)
			{
				SetStatus("No market");
				return;
			}

			var items = market.GetMarketItems();

			if (items == null)
			{
				SetStatus("No ship present");
				return;
			}

			if (items.Length == 0)
			{
				SetStatus("No items for sale");
				return;
			}

			ClearStatus();

			for (int i = 0; i < Mathf.Min(marketItems.GetChildCount(), items.Length); i++)
			{
				var marketItem = marketItems.GetChild<MarketItemNode>(i);
				marketItem.Item = items[i];
			}

			for (int i = Mathf.Min(marketItems.GetChildCount(), items.Length); i < items.Length; i++)
			{
				var marketItem = marketItemScene.Instantiate<MarketItemNode>();
				marketItem.Ready += () => marketItem.Item = items[i];

				marketItems.AddChild(marketItem);
			}

			for (int i = items.Length; i < marketItems.GetChildCount(); i++)
			{
				marketItems.GetChild(i).QueueFree();
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
