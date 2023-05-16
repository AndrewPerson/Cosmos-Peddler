using Godot;

namespace CosmosPeddler.UI;

public partial class SubViewportMouseFixNode : SubViewportContainer
{
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouse)
        {
            var mouseEvent = (InputEventMouse)@event.Duplicate();
            mouseEvent.Position = GetGlobalTransform().BasisXformInv(mouseEvent.GlobalPosition);
            GetChild<Viewport>(0)._UnhandledInput(mouseEvent);
        }
        else
        {
		    GetChild<Viewport>(0)._UnhandledInput(@event);
        }
    }
}
