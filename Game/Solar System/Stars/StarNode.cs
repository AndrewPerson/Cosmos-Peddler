using Godot;
using System;
using System.Collections.Generic;

namespace CosmosPeddler.Game.SolarSystem.Stars;

public partial class StarNode : Node3D
{
	private static Dictionary<string, PackedScene>? starTypes = null;

	[Signal]
	public delegate void MouseEnterEventHandler();

	[Signal]
	public delegate void MouseExitEventHandler();

	public SystemType systemType;

	[Export]
	public StringName[] starTypeNames = Array.Empty<StringName>();

	[Export]
	public PackedScene[] starTypeScenes = Array.Empty<PackedScene>();

	private CollisionShape3D bounds = null!;

	public override void _EnterTree()
	{
		bounds = GetNode<CollisionShape3D>("%Bounds");
	}

	public override void _Ready()
	{
		if (starTypes == null)
		{
			starTypes = new Dictionary<string, PackedScene>();

			lock (starTypes)
			{
				for (int i = 0; i < starTypeNames.Length; i++)
				{
					starTypes.Add(starTypeNames[i], starTypeScenes[i]);
				}
			}
		}

		if (starTypes.TryGetValue(systemType.ToString(), out var scene))
		{
			var starInstance = scene.Instantiate();
			AddChild(starInstance);

			((BoxShape3D)bounds.Shape).Size = ((IDimensionedObject)starInstance).Dimensions / Scale;
		}
		else
		{
			var starInstance = starTypes["UNKNOWN"].Instantiate();
			AddChild(starInstance);

			((BoxShape3D)bounds.Shape).Size = ((IDimensionedObject)starInstance).Dimensions / Scale;
		}
	}

	private void _MouseEnter()
	{
		EmitSignal(SignalName.MouseEnter);
	}

	private void _MouseExit()
	{
		EmitSignal(SignalName.MouseExit);
	}
}
