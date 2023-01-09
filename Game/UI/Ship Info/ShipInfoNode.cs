using Godot;
using System;

namespace CosmosPeddler.Game;

public partial class ShipInfoNode : PopupUI<Ship>
{
	private Label name = null!;
	private CargoNode cargo = null!;
	private CrewNode crew = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		cargo = GetNode<CargoNode>("%Cargo");
		crew = GetNode<CrewNode>("%Crew");
	}

	public override void UpdateUI()
	{
		if (Data.Symbol == Data.Registration.Name) name.Text = $"{Data.Symbol} - {Data.Registration.Role}";
		else name.Text = $"{Data.Symbol} ({Data.Registration.Name}) - {Data.Registration.Role}";

		cargo.Data = (Data.Nav, Data.Cargo);
		crew.Data = Data.Crew;
		// engine.Engine = _ship.Engine;
		// engine.Fuel = _ship.Fuel;
		// frame.Frame = _ship.Frame;
		// modules.Modules = _ship.Modules;
		// modules.MaxModules = _ship.Frame.ModuleSlots;
		// mounts.Mounts = _ship.Mounts;
		// mounts.MaxMounts = _ship.Frame.MountingPoints;
		// reactor.Reactor = _ship.Reactor;
	}
}
