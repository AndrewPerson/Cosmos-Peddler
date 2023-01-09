using Godot;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class ShipsNode : AsyncReactiveUI<Waypoint>
{
	[Export]
	public PackedScene shipScene = null!;

	private Label status = null!;
	private Control shipsList = null!;

	public override void _EnterTree()
	{
		status = GetNode<Label>("%Status");
		shipsList = GetNode<Control>("%Ships List");
	}

	private void SetStatus(string text)
	{
		status.Text = text;
		status.Visible = true;
		shipsList.Visible = false;
	}

	private void ClearStatus()
	{
		status.Visible = false;
		shipsList.Visible = true;
	}

	public override async Task UpdateUIAsync()
	{
		try
		{
			SetStatus("Loading...");

			for (int i = 0; i < shipsList.GetChildCount(); i++)
			{
				shipsList.GetChild(i).QueueFree();
			}

			bool noShips = true;
			await foreach (var ship in Data.GetShips())
			{
				noShips = false;
				ClearStatus();

				var shipNode = shipScene.Instantiate<ShipNode>();
				shipNode.Ready += () => shipNode.Data = ship;
				shipsList.AddChild(shipNode);
			}

			if (noShips)
			{
				SetStatus("No ships");
			}
		}
		catch (System.Exception e)
		{
			GD.PrintErr(e);
			SetStatus(e.Message);
		}
	}
}
