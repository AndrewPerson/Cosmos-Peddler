using Godot;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class CrewNode : ReactiveUI<ShipCrew>
{
	private Label crewCount = null!;
	private Label maxCrew = null!;
	private Label requiredCrew = null!;
	private Label morale = null!;
	private Label rotation = null!;
	private Label wages = null!;

	public override void _EnterTree()
	{
		crewCount = GetNode<Label>("%Crew Count");
		maxCrew = GetNode<Label>("%Max Crew");
		requiredCrew = GetNode<Label>("%Required");
		morale = GetNode<Label>("%Morale");
		rotation = GetNode<Label>("%Rotation");
		wages = GetNode<Label>("%Wages");
	}

	public override void UpdateUI()
	{
		crewCount.Text = Data.Current.ToString();
		if (Data.Current < Data.Required) crewCount.LabelSettings.FontColor = Colors.Red;
		else crewCount.LabelSettings.FontColor = Colors.White;

		maxCrew.Text = Data.Capacity.ToString();

		requiredCrew.Text = Data.Required.ToString();
		if (Data.Current < Data.Required) requiredCrew.LabelSettings.FontColor = Colors.Red;
		else requiredCrew.LabelSettings.FontColor = Colors.White;

		morale.Text = $"{Data.Morale}%";
		rotation.Text = Data.Rotation.ToString();
		wages.Text = Data.Wages.ToString();
	}
}
