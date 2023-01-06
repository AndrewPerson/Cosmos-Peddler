using Godot;
using System;

namespace CosmosPeddler.Game;

public partial class ShipInfoNode : Control
{
	private static ShipInfoNode Instance { get; set; } = null!;

	private Ship _ship = null!;

	public Ship Ship
	{
		get => _ship;
		set
		{
			_ship = value;
			UpdateShipInfo();
		}
	}

	private Label name = null!;
	private CargoNode cargo = null!;
	private CrewNode crew = null!;

	public static void Show(Ship ship)
	{
		Instance.Ship = ship;
		UINode.Instance.Show(Instance);
	}

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		cargo = GetNode<CargoNode>("%Cargo");
		crew = GetNode<CrewNode>("%Crew");
	}

	public override void _Ready()
	{
		Instance = this;
	}

	private void UpdateShipInfo()
	{
		if (_ship.Symbol == _ship.Registration.Name) name.Text = $"{_ship.Symbol} - {_ship.Registration.Role}";
		else name.Text = $"{_ship.Symbol} ({_ship.Registration.Name}) - {_ship.Registration.Role}";

		cargo.CargoInfo = (_ship.Nav, _ship.Cargo);
		crew.Crew = _ship.Crew;
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
