using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CosmosPeddler.Game;

public partial class TraitsNode : PanelContainer
{
	private IList<WaypointTrait> _traits = null!;

	public IList<WaypointTrait> Traits
	{
		get => _traits;
		set
		{
			_traits = value;
			UpdateTraits();
		}
	}

	private Node traitsList = null!;

	public override void _EnterTree()
	{
		traitsList = GetNode("%Traits");
	}

	private void UpdateTraits()
	{
		var trimmedTraits = _traits.Where(t => t.Symbol != WaypointTraitSymbol.MARKETPLACE && t.Symbol != WaypointTraitSymbol.SHIPYARD)
								   .ToList();

		if (trimmedTraits.Count == 0)
		{
			trimmedTraits.Add
			(
				new WaypointTrait()
				{
					Name = "None",
					Description = "This waypoint has no traits.",
				}
			);
		}

		for (int i = 0; i < Mathf.Min(traitsList.GetChildCount(), trimmedTraits.Count); i++)
		{
			var trait = traitsList.GetChild<Label>(i);
			trait.Text = $"• {trimmedTraits[i].Name}";
			trait.TooltipText = trimmedTraits[i].Description;
		}

		for (int i = Mathf.Min(traitsList.GetChildCount(), trimmedTraits.Count); i <  trimmedTraits.Count; i++)
		{
            var trait = new Label
            {
                Text = $"• {trimmedTraits[i].Name}",
				TooltipText = trimmedTraits[i].Description,
				MouseFilter = MouseFilterEnum.Pass
            };

            traitsList.AddChild(trait);
		}

		for (int i = trimmedTraits.Count; i < traitsList.GetChildCount(); i++)
		{
			traitsList.GetChild(i).QueueFree();
		}
	}
}
