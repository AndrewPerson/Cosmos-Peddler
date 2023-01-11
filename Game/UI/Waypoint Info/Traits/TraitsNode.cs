using Godot;
using System.Collections.Generic;
using System.Linq;

namespace CosmosPeddler.Game;

public partial class TraitsNode : ReactiveUI<IList<WaypointTrait>>
{
	private Node traitsList = null!;

	public override void _EnterTree()
	{
		traitsList = GetNode("%Traits");
	}

	public override void UpdateUI()
	{
		var trimmedTraits = Data.Where(t =>
									t.Symbol != WaypointTraitSymbol.MARKETPLACE &&
									t.Symbol != WaypointTraitSymbol.SHIPYARD
								)
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

		RenderList(
			traitsList,
			trimmedTraits,
			(trait, label) =>
			{
				label.Text = $"• {trait.Name}";
				label.TooltipText = trait.Description;
			},
			() => new Label()
			{
				MouseFilter = MouseFilterEnum.Pass
			}
		);
	}
}
