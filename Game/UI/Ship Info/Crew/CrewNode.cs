using Godot;
using System;

namespace CosmosPeddler.Game;

public partial class CrewNode : PanelContainer
{
	private ShipCrew _crew = null!;

	public ShipCrew Crew
	{
		get => _crew;
		set
		{
			_crew = value;
			UpdateCrewInfo();
		}
	}

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

	private void UpdateCrewInfo()
	{
		crewCount.Text = _crew.Current.ToString();
		if (_crew.Current < _crew.Required) crewCount.LabelSettings.FontColor = Colors.Red;
		else crewCount.LabelSettings.FontColor = Colors.White;

		maxCrew.Text = _crew.Capacity.ToString();

		requiredCrew.Text = _crew.Required.ToString();
		if (_crew.Current < _crew.Required) requiredCrew.LabelSettings.FontColor = Colors.Red;
		else requiredCrew.LabelSettings.FontColor = Colors.White;

		morale.Text = $"{_crew.Morale}%";
		rotation.Text = _crew.Rotation.ToString();
		wages.Text = _crew.Wages.ToString();
	}
}
