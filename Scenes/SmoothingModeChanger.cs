namespace Scabine.Scenes;

using System.Drawing.Drawing2D;
using System.Drawing;
using System;

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
