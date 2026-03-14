namespace ChessBand.Scenes;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public sealed class SmoothingModeChanger : IDisposable
{
	public SmoothingModeChanger(Graphics graphics, SmoothingMode mode)
	{
		_graphics = graphics;
		_oldMode = _graphics.SmoothingMode;
		_graphics.SmoothingMode = mode;
	}

	public void Dispose()
	{
		_graphics.SmoothingMode = _oldMode;
	}

	private readonly Graphics _graphics;
	private readonly SmoothingMode _oldMode;
}
