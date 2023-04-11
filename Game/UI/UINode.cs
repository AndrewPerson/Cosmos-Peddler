using System;
using System.Collections.Generic;
using Godot;

namespace CosmosPeddler.Game.UI;

public partial class UINode : Control
{
	public static UINode Instance { get; private set; } = null!;

	public event Action? UIAppear;
	public event Action? UIDisappear;

	private Control hiddenUI = null!;

	private readonly Stack<Control> stack = new();
    private readonly Dictionary<Control, Stack<int>> occurrenceIndices = new();

	private bool hovering = false;

	public override void _EnterTree()
	{
		hiddenUI = GetNode<Control>("%Hidden UI");
	}

	public override void _Ready()
	{
		Instance = this;
	}

	private bool mouseDown = false;

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			HidePrevUI();
		}
		else if (@event is InputEventMouseButton mouseButton)
		{
			if (hovering && mouseButton.ButtonIndex == MouseButton.Left)
			{
				if (mouseButton.Pressed)
				{
					mouseDown = true;
				}
				else
				{
					if (mouseDown)
					{
						HidePrevUI();
					}

					mouseDown = false;
				}
			}
		}
	}

	public void MouseEnter()
	{
		hovering = true;
	}

	public void MouseExit()
	{
		hovering = false;
	}

	public void Show(Control control)
	{
		Visible = true;

        if (control.GetParent() == hiddenUI)
        {
            hiddenUI.RemoveChild(control);
            AddChild(control);
        }
        else
        {
            var indices = occurrenceIndices.GetValueOrDefault(control, new Stack<int>());
            indices.Push(control.GetIndex());

            occurrenceIndices[control] = indices;

            MoveChild(control, -1);
        }

        stack.Push(control);

		//Weird hack to get Godot to trigger appropriate mouse_entered and mouse_exited signals
		EmitSignal("mouse_exited");
		WarpMouse(GetViewport().GetMousePosition());

		if (stack.Count == 1)
		{
			UIAppear?.Invoke();
		}
	}

	public void HidePrevUI()
	{
		if (stack.Count > 0)
		{
			var prevUI = stack.Pop();

            var indices = occurrenceIndices.GetValueOrDefault(prevUI, new Stack<int>());

            if (indices.Count < 1)
            {
                RemoveChild(prevUI);
			    hiddenUI.AddChild(prevUI);
            }
            else
            {
                var prevIndex = indices.Pop();
                MoveChild(prevUI, prevIndex);
            }

            occurrenceIndices[prevUI] = indices;

			//Weird hack to get Godot to trigger appropriate mouse_entered and mouse_exited signals
			EmitSignal("mouse_exited");
			WarpMouse(GetViewport().GetMousePosition());

			if (stack.Count == 0)
			{
				Visible = false;
				UIDisappear?.Invoke();
			}
		}
	}

	public void HideAllUI()
	{
		while (stack.Count > 0)
		{
			HidePrevUI();
		}
	}
}
