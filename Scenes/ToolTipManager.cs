namespace Scabine.Scenes;

public static class ToolTipManager
{
	public static void SetToolTip(string text)
	{
		_active = true;
		_changed = _toolTipText != text;
		_toolTipText = text;
	}

	internal static string? GetToolTipText()
	{
		return _toolTipText;
	}

	internal static bool IsDirty()
	{
		return _changed;
	}

	internal static void Commit()
	{
		if (!_active)
		{
			_toolTipText = null;
		}
		_active = false;
		_changed = false;
	}

	private static string? _toolTipText;
	private static bool _active;
	private static bool _changed;
}
