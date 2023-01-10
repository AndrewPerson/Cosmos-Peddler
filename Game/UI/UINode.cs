using System;
using System.Collections.Generic;
using Godot;

namespace CosmosPeddler.Game;

public partial class UINode : Control
{
	public static UINode Instance { get; private set; } = null!;

	public event Action? UIAppear;
	public event Action? UIDisappear;

	private Control hiddenUI = null!;

	private readonly Stack<Control> _stack = new();

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

		hiddenUI.RemoveChild(control);
		AddChild(control);
		_stack.Push(control);

		//Weird hack to get Godot to trigger appropriate mouse_entered and mouse_exited signals
		EmitSignal("mouse_exited");
		WarpMouse(GetViewport().GetMousePosition());

		if (_stack.Count == 1)
		{
			UIAppear?.Invoke();
		}
	}

	public void HidePrevUI()
	{
		if (_stack.Count > 0)
		{
			var prevUI = _stack.Pop();
			RemoveChild(prevUI);
			hiddenUI.AddChild(prevUI);

			//Weird hack to get Godot to trigger appropriate mouse_entered and mouse_exited signals
			EmitSignal("mouse_exited");
			WarpMouse(GetViewport().GetMousePosition());

			if (_stack.Count == 0)
			{
				Visible = false;
				UIDisappear?.Invoke();
			}
		}
	}

	public void HideAllUI()
	{
		while (_stack.Count > 0)
		{
			HidePrevUI();
		}
	}
}
