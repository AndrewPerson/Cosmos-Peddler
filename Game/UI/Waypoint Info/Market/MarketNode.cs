using Godot;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class MarketNode : ReactiveUI<Waypoint>
{
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

	public override void UpdateUI()
	{
		if (!Data.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.MARKETPLACE))
		{
			SetStatus("No market");

			return;
		}

		SetStatus("Loading...");

		Data.GetMarket().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				GD.PrintErr(t.Exception);
				SetStatus(t.Exception?.Message ?? "Unknown error");
				return;
			}

			var market = t.Result;

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

			RenderList(marketItems, marketItemScene, items.Select(item => (item, market)).ToArray());
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
