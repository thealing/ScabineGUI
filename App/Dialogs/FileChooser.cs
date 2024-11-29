namespace Scabine.App.Dialogs;

using Scabine.App.Prefs;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

internal static class FileChooser
{
	public static string? ChooseOpenFile(string title, string fileType, string fileExtension)
	{
		OpenFileDialog dialog = new OpenFileDialog
		{
			Title = title,
			Filter = $"{fileType} Files (*.{fileExtension})|*.{fileExtension}|All Files (*.*)|*.*",
			CheckFileExists = true,
			CheckPathExists = true,
			RestoreDirectory = true
		};
		LoadLastDirectory(title, dialog);
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			SaveLastDirectory(title, dialog);
			return dialog.FileName;
		}
		return null;
	}

	public static string? ChooseSaveFile(string title, string fileType, string fileExtension, string initialName = "")
	{
		SaveFileDialog dialog = new SaveFileDialog
		{
			Title = title,
			FileName = initialName,
			Filter = $"{fileType} Files (*.{fileExtension})|*.{fileExtension}|All Files (*.*)|*.*",
			OverwritePrompt = true,
			CheckPathExists = true
		};
		LoadLastDirectory(title, dialog);
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			SaveLastDirectory(title, dialog);
			return dialog.FileName;
		}
		return null;
	}

	private static void SaveLastDirectory(string tag, FileDialog dialog)
	{
		_lastPaths[tag] = Path.GetDirectoryName(dialog.FileName);
	}

	private static void LoadLastDirectory(string tag, FileDialog dialog)
	{
		if (_lastPaths.TryGetValue(tag, out string? directory) && directory != null)
		{
			dialog.InitialDirectory = directory;
		}
	}

	static FileChooser()
	{
		_lastPaths = new Dictionary<string, string?>();
		SaveManager.Save += () => SaveManager.Sync(nameof(_lastPaths), ref _lastPaths);
	}

	private static Dictionary<string, string?> _lastPaths;
}
