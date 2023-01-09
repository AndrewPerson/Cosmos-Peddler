using System;
using System.Collections.Generic;
using Godot;

namespace CosmosPeddler.Game;

public partial class UINode : Control
{
	public static UINode Instance { get; private set; } = null!;

	public event Action? UIAppear;
	public event Action? UIDisappear;

	private readonly Stack<Control> _stack = new();

	private bool hovering = false;

	public override void _Ready()
	{
		Instance = this;
	}

	public void Show(Control control)
	{
		Visible = true;
		control.Visible = true;
		_stack.Push(control);

		if (_stack.Count == 1)
		{
			UIAppear?.Invoke();
		}
	}

	public void HidePrevUI()
	{
		if (_stack.Count > 0)
		{
			_stack.Pop().Visible = false;

			if (_stack.Count == 0)
			{
				Visible = false;
				UIDisappear?.Invoke();
			}
		}
	}

	public void HideAllUI()
	{
		if (_stack.Count > 0)
		{
			while (_stack.Count > 0)
			{
				_stack.Pop().Visible = false;
			}

			Visible = false;
			UIDisappear?.Invoke();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			HidePrevUI();
		}
		else if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				if (hovering)
				{
					HidePrevUI();
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
}
