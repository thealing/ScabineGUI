namespace Scabine.Scenes;

using System;
using System.Drawing;

public class ClipChanger : IDisposable
{
	public ClipChanger(Graphics graphics, Rectangle clip)
	{
		_graphics = graphics;
		_oldClip = _graphics.Clip;
		_graphics.SetClip(clip);
	}

	public void Dispose()
	{
		_graphics.Clip = _oldClip;
	}

	private readonly Graphics _graphics;
	private readonly Region _oldClip;
}
