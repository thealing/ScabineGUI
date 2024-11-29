namespace Scabine.Scenes;

using System.Windows.Forms;

public static class CursorManager
{
	public static void SetCursor(Cursor cursor)
	{
		_cursor = cursor;
		_changed = true;
	}

	public static void SetFocus(object focus)
	{
		_focus = focus;
	}

	public static object? GetFocus()
	{
		return _focus;
	}

	internal static Cursor GetCursor()
	{
		return _cursor;
	}

	internal static void Commit()
	{
		if (_changed)
		{
			_cursor = Cursors.Default;
			_changed = false;
		}
		_focus = null;
	}

	private static Cursor _cursor = Cursors.Default;
	private static bool _changed = false;
	private static object? _focus = null;
}
