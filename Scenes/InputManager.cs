namespace Scabine.Scenes;

using System;
using System.Drawing;
using System.Windows.Forms;

public static class InputManager
{
	public static bool IsKeyDown(Keys key)
	{
		return _currentState.Keys[(int)key];
	}

	public static bool IsKeyPressed(Keys key)
	{
		return _currentState.Keys[(int)key] && !_previousState.Keys[(int)key];
	}

	public static bool IsKeyReleased(Keys key)
	{
		return !_currentState.Keys[(int)key] && _previousState.Keys[(int)key];
	}

	public static bool IsKeyRepeated(Keys key)
	{
		return _pressedKeys[(int)key];
	}

	public static bool IsLeftButtonDown()
	{
		return _currentState.LeftButton;
	}

	public static bool IsRightButtonDown()
	{
		return _currentState.RightButton;
	}

	public static bool IsLeftButtonPressed()
	{
		return _currentState.LeftButton && !_previousState.LeftButton;
	}

	public static bool IsLeftButtonReleased()
	{
		return !_currentState.LeftButton && _previousState.LeftButton;
	}

	public static bool IsRightButtonPressed()
	{
		return _currentState.RightButton && !_previousState.RightButton;
	}

	public static bool IsRightButtonReleased()
	{
		return !_currentState.RightButton && _previousState.RightButton;
	}

	public static Point GetMousePosition()
	{
		return new Point(_currentState.MouseX, _currentState.MouseY);
	}

	public static Size GetMouseDelta()
	{
		return new Size(_currentState.MouseX - _previousState.MouseX, _currentState.MouseY - _previousState.MouseY);
	}

	public static int GetMouseScroll()
	{
		return _currentState.MouseScroll - _previousState.MouseScroll;
	}

	internal static void OnKeyDown(KeyEventArgs e)
	{
		_currentState.Keys[(int)e.KeyCode] = true;
		_pressedKeys[(int)e.KeyCode] = true;
	}

	internal static void OnKeyUp(KeyEventArgs e)
	{
		_currentState.Keys[(int)e.KeyCode] = false;
	}

	internal static void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_currentState.LeftButton = true;
		}
		if (e.Button == MouseButtons.Right)
		{
			_currentState.RightButton = true;
		}
	}

	internal static void OnMouseUp(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_currentState.LeftButton = false;
		}
		if (e.Button == MouseButtons.Right)
		{
			_currentState.RightButton = false;
		}
	}

	internal static void OnMouseWheel(MouseEventArgs e)
	{
		_currentState.MouseScroll += e.Delta;
	}

	internal static void OnMouseMove(MouseEventArgs e)
	{
		_currentState.MouseX = e.X;
		_currentState.MouseY = e.Y;
	}

	internal static void OnMouseLeave(EventArgs e)
	{
		_currentState.MouseX = int.MinValue;
		_currentState.MouseY = int.MinValue;
	}

	internal static void OnDeactivate(EventArgs e)
	{
		Array.Fill(_currentState.Keys, false);
	}

	internal static void Commit()
	{
		Array.Copy(_currentState.Keys, _previousState.Keys, _previousState.Keys.Length);
		_previousState.LeftButton = _currentState.LeftButton;
		_previousState.RightButton = _currentState.RightButton;
		_previousState.MouseX = _currentState.MouseX;
		_previousState.MouseY = _currentState.MouseY;
		_previousState.MouseScroll = _currentState.MouseScroll;
		Array.Fill(_pressedKeys, false);
	}

	private static InputState _currentState = new InputState();
	private static InputState _previousState = new InputState();
	private static bool[] _pressedKeys = new bool[256];
}
