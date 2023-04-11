using Godot;

namespace CosmosPeddler.UI;

[Tool]
public partial class ForceRatioNode : AspectRatioContainer
{
	public override void _Process(double delta)
	{
        if (StretchMode == StretchModeEnum.WidthControlsHeight) CustomMinimumSize = new Vector2(0, Size.X / Ratio);
        else if (StretchMode == StretchModeEnum.HeightControlsWidth) CustomMinimumSize = new Vector2(Size.Y * Ratio, 0);
	}
}
