namespace Scabine.Scenes;

using System.Drawing.Drawing2D;
using System.Drawing;
using System;

public sealed class CompositingQualityChanger : IDisposable
{
	public CompositingQualityChanger(Graphics graphics, CompositingQuality quality)
	{
		_graphics = graphics;
		_oldQuality = _graphics.CompositingQuality;
		_graphics.CompositingQuality = quality;
	}

	public void Dispose()
	{
		_graphics.CompositingQuality = _oldQuality;
	}

	private readonly Graphics _graphics;
	private readonly CompositingQuality _oldQuality;
}
