using Godot;
using System;
using System.Collections.Generic;

namespace CosmosPeddler.Game.SolarSystem.Stars;

public partial class StarNode : Area3D
{
	private static Dictionary<string, PackedScene>? starTypes = null;

	public SystemType SystemType { get; set; }

	[Export]
	public StringName[] StarTypeNames { get; set; } = Array.Empty<StringName>();

	[Export]
	public PackedScene[] StarTypeScenes { get; set; } = Array.Empty<PackedScene>();

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
				for (int i = 0; i < StarTypeNames.Length; i++)
				{
					starTypes.Add(StarTypeNames[i], StarTypeScenes[i]);
				}
			}
		}

		if (starTypes.TryGetValue(SystemType.ToString(), out var scene))
		{
			var starInstance = scene.Instantiate();
			AddChild(starInstance);

            bounds.Shape = new BoxShape3D()
            {
                Size = ((IDimensionedObject)starInstance).Dimensions
            };
		}
		else
		{
			var starInstance = starTypes["UNKNOWN"].Instantiate();
			AddChild(starInstance);

            bounds.Shape = new BoxShape3D()
            {
                Size = ((IDimensionedObject)starInstance).Dimensions
            };
		}
	}
}
