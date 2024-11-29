namespace Scabine.App.Menus;

using Scabine.App.Dialogs;
using Scabine.App.Prefs;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

internal static class FileMenu
{
	public static void Init(ToolStripMenuItem menu)
	{
		MenuCreator.AddSubMenuItem(menu, "New", null, New, Keys.Control | Keys.N);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Open", null, Open, Keys.Control | Keys.O);
		_recentMenu = MenuCreator.AddSubMenuItem(menu, "Open Recent", null, null);
		menu.DropDownOpening += (sender, e) => _recentMenu.Enabled = _recentMenu.HasDropDownItems;
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Save", null, Save, Keys.Control | Keys.S);
		MenuCreator.AddSubMenuItem(menu, "Save As", null, SaveAs, Keys.Control | Keys.Shift | Keys.S);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Settings", null, Settings);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Exit", null, FileExit, Keys.Alt | Keys.F4);
		UpdateRecentPaths();
	}

	private static void Settings()
	{
		SettingsDialog dialog = new SettingsDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void New()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		PgnManager.NewGame();
	}

	private static void Open()
	{
		string? path = FileChooser.ChooseOpenFile("Open Game", "Pgn", "pgn");
		if (path != null)
		{
			_path = path;
			string content = File.ReadAllText(_path);
			PgnManager.SetPgn(content);
		}
		UpdateRecentPaths();
	}

	private static void Save()
	{
		if (_path != null)
		{
			string content = PgnManager.GetPgn();
			File.WriteAllText(_path, content);
		}
		else
		{
			SaveAs();
		}
	}

	private static void SaveAs()
	{
		string? path = FileChooser.ChooseSaveFile("Save Game", "Pgn", "pgn", "Game");
		if (path != null)
		{
			_path = path;
			string content = PgnManager.GetPgn();
			File.WriteAllText(_path, content);
		}
		UpdateRecentPaths();
	}

	private static void FileExit()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		SceneManager.Exit();
	}

	private static void UpdateRecentPaths()
	{
		if (_path != null)
		{
			_recentPaths.Remove(_path);
			_recentPaths.Insert(0, _path);
		}
		if (_recentPaths.Count > 15)
		{
			_recentPaths.RemoveAt(_recentPaths.Count - 1);
		}
		SaveManager.Sync(nameof(_recentPaths), ref _recentPaths);
		_recentMenu.DropDownItems.Clear();
		foreach (string path in _recentPaths)
		{
			ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileName(path));
			item.TextAlign = ContentAlignment.MiddleLeft;
			item.Click += (senter, e) => OpenFile(path);
			_recentMenu.DropDownItems.Add(item);
		}
	}

	private static void OpenFile(string path)
	{
		_path = path;
		string content = File.ReadAllText(path);
		PgnManager.SetPgn(content);
		UpdateRecentPaths();
	}

	private static string? _path = null;
	private static List<string> _recentPaths = new List<string>();
	private static ToolStripMenuItem _recentMenu = new ToolStripMenuItem();
}
