namespace ChessBand.Scenes;

using System;
using System.Drawing;
using System.Runtime.InteropServices;

public class GdiClipChanger : IDisposable
{
	public GdiClipChanger(Graphics graphics, Rectangle clip)
	{
		clip.Offset((int)graphics.Transform.OffsetX, (int)graphics.Transform.OffsetY);
		IntPtr hdc = graphics.GetHdc();
		_graphics = graphics;
		_newRegion = CreateRectRgn(clip.Left, clip.Top, clip.Right, clip.Bottom);
		_oldRegion = CreateRectRgn(0, 0, 0, 0);
		if (GetClipRgn(hdc, _oldRegion) != 1)
		{
			DeleteObject(_oldRegion);
			_oldRegion = IntPtr.Zero;
		}
		SelectClipRgn(hdc, _newRegion);
		_graphics.ReleaseHdc(hdc);
	}

	public void Dispose()
	{
		IntPtr hdc = _graphics.GetHdc();
		SelectClipRgn(hdc, _oldRegion);
		DeleteObject(_newRegion);
		DeleteObject(_oldRegion);
		_graphics.ReleaseHdc(hdc);
	}

	private readonly Graphics _graphics;
	private readonly IntPtr _oldRegion;
	private readonly IntPtr _newRegion;

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

	[DllImport("gdi32.dll")]
	private static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

	[DllImport("gdi32.dll")]
	private static extern int GetClipRgn(IntPtr hdc, IntPtr hrgn);

	[DllImport("gdi32.dll")]
	private static extern int DeleteObject(IntPtr hObject);
}
