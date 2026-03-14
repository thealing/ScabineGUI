namespace ChessBand.Application;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public class TextRenderCache
{
	public TextRenderCache(Font font, TextFormatFlags flags, Color textColor, Color backgroundColor)
	{
		_font = font;
		_flags = flags;
		_textColor = textColor;
		_backgroundColor = backgroundColor;
		_cache = new Dictionary<Tuple<string, Size>, Bitmap>();
		_temp = new HashSet<Tuple<string, Size>>();
	}

	public void Render(Graphics g, string text, Rectangle rectangle)
	{
		if (!g.ClipBounds.IntersectsWith(rectangle))
		{
			return;
		}
		var key = Tuple.Create(text, rectangle.Size);
		if (_cache.TryGetValue(key, out Bitmap? bitmap) == false || bitmap == null)
		{
			bitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.Transparent);
				TextRenderer.DrawText(graphics, text, _font, new Rectangle(Point.Empty, rectangle.Size), _textColor, _backgroundColor, _flags);
			}
			_cache[key] = bitmap;
		}
		_temp.Add(key);
		g.DrawImage(bitmap, rectangle.Location);
	}

	public void EndFrame()
	{
		foreach (Tuple<string, Size> key in _cache.Keys.ToArray())
		{
			if (!_temp.Contains(key))
			{
				_cache[key].Dispose();
				_cache.Remove(key);
			}
		}
		_temp.Clear();
	}

	private readonly Font _font;
	private readonly TextFormatFlags _flags;
	private readonly Color _textColor;
	private readonly Color _backgroundColor;
	private readonly Dictionary<Tuple<string, Size>, Bitmap> _cache;
	private readonly HashSet<Tuple<string, Size>> _temp;
}
