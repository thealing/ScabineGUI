namespace ChessBand.Application;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class TextMeasureCache
{
	public TextMeasureCache(Font font, TextFormatFlags flags)
	{
		_font = font;
		_flags = flags;
		_cache = new Dictionary<string, Size>();
		_temp = new HashSet<string>();
	}

	public Size Measure(string text)
	{
		_temp.Add(text);
		if (_cache.TryGetValue(text, out Size value) == false)
		{
			value = Size.Ceiling(TextRenderer.MeasureText(text, _font, Size.Empty, _flags));
			_cache[text] = value;
		}
		return value;
	}

	public void EndFrame()
	{
		foreach (string key in _cache.Keys.ToArray())
		{
			if (!_temp.Contains(key))
			{
				_cache.Remove(key);
			}
		}
		_temp.Clear();
	}

	private readonly Font _font;
	private readonly TextFormatFlags _flags;
	private readonly Dictionary<string, Size> _cache;
	private readonly HashSet<string> _temp;
}
