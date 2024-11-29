namespace Scabine.App.Dialogs;

using Scabine.App;
using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class EnginesDialog : BaseDialog
{
	public EnginesDialog()
	{
		ClientSize = new Size(770, 490);
		Text = "Manage Engines";
		Font = new Font("Segoe UI", 12);
		_engineList = AddListBox(Controls, 20, 20, 260, 340, EngineChanged);
		_engineList.Items.AddRange(EngineManager.GetInstalledEngines().Select(engine => engine.Name).ToArray());
		_tabControl = AddTabControl(Controls, 300, 20, 450, 390);
		_generalPage = AddTabPage(_tabControl.Controls, "General", 0, 30, 450, 360);
		_startupPage = AddTabPage(_tabControl.Controls, "Startup", 0, 30, 450, 360);
		_presetsPage = AddTabPage(_tabControl.Controls, "Presets", 0, 30, 450, 360);
		AddButton(Controls, "Install", 40, 380, 100, 40, Install);
		AddButton(Controls, "Uninstall", 160, 380, 100, 40, Uninstall);
		AddButton(Controls, "Done", 630, 430, 100, 40, Done);
		EngineChanged(null, EventArgs.Empty);
	}

	private void EngineChanged(object? sender, EventArgs e)
	{
		_tabControl.Visible = false;
		if (_engineList.SelectedIndex == -1)
		{
			return;
		}
		UpdateGeneralPage(_generalPage);
		UpdateStartupPage(_startupPage);
		UpdatePresetsPage(_presetsPage);
		_tabControl.Visible = true;
	}

	private void Install(object? sender, EventArgs e)
	{
		string? path = FileChooser.ChooseOpenFile("Install Engine", "Program", "exe");
		if (path != null)
		{
			if (EngineManager.InstallEngine(path))
			{
				DialogHelper.ShowMessageBox("Engine installed successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		_engineList.Items.Clear();
		_engineList.Items.AddRange(EngineManager.GetInstalledEngines().Select(engine => engine.Name).ToArray());
	}

	private void Uninstall(object? sender, EventArgs e)
	{
		if (_engineList.SelectedIndex == -1)
		{
			return;
		}
		DialogResult result = DialogHelper.ShowMessageBox("Are you sure?", "Uninstall Engine", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
		if (result == DialogResult.Yes)
		{
			EngineInfo engine = GetCurrentEngine();
			EngineManager.UninstallEngine(engine);
			_engineList.Items.RemoveAt(_engineList.SelectedIndex);
			EngineChanged(null, EventArgs.Empty);
		}
	}

	private void Done(object? sender, EventArgs e)
	{
		Close();
	}

	private void UpdateGeneralPage(TabPage page)
	{
		EngineInfo engine = GetCurrentEngine();
		page.Controls.Clear();
		AddLabel(page.Controls, "Path", 20, 30, 100, 30, ContentAlignment.TopLeft);
		TextBox pathEdit = AddTextBox(page.Controls, engine.Path, 120, 30, 240, 30, (sender, e) =>
		{
			if (sender is TextBox textBox && File.Exists(textBox.Text))
			{
				engine.Path = textBox.Text;
			}
		});
		pathEdit.SelectionStart = engine.Path.Length;
		AddButton(page.Controls, "...", 380, 30, 30, 30, (sender, e) =>
		{
			string? path = FileChooser.ChooseOpenFile("Select Engine Path", "Program Files", "exe");
			if (path != null)
			{
				engine.Path = path;
				pathEdit.Text = path;
				pathEdit.SelectionStart = path.Length;
			}
		});
		AddLabel(page.Controls, "Name", 20, 90, 100, 30, ContentAlignment.TopLeft);
		AddTextBox(page.Controls, engine.Name, 120, 90, 290, 30, (sender, e) =>
		{
			if (sender is TextBox textBox && textBox.Text != "")
			{
				engine.Name = textBox.Text;
				_engineList.Items[_engineList.SelectedIndex] = textBox.Text;
			}
		});
		AddLabel(page.Controls, "Author", 20, 150, 100, 30, ContentAlignment.TopLeft);
		AddTextBox(page.Controls, engine.Author, 120, 150, 290, 30, (sender, e) =>
		{
			if (sender is TextBox textBox && textBox.Text != "")
			{
				engine.Author = textBox.Text;
			}
		});
	}

	private void UpdateStartupPage(TabPage page)
	{
		EngineInfo engine = GetCurrentEngine();
		page.Controls.Clear();
		AddLabel(page.Controls, "Command Line Arguments", 20, 10, 390, 40, ContentAlignment.BottomCenter);
		AddTextBox(page.Controls, engine.Arguments, 20, 60, 390, 30, (sender, e) =>
		{
			if (sender is TextBox textBox)
			{
				engine.Arguments = textBox.Text;
			}
		});
		AddLabel(page.Controls, "Input Commands", 20, 90, 390, 40, ContentAlignment.BottomCenter);
		TextBox textBox = AddTextBox(page.Controls, "", 20, 140, 390, 200, (sender, e) =>
		{
			if (sender is TextBox textBox)
			{
				engine.Commands = textBox.Lines;
			}
		});
		textBox.Lines = engine.Commands;
		textBox.Multiline = true;
		textBox.WordWrap = false;
		textBox.ScrollBars = ScrollBars.Both;
	}

	private void UpdatePresetsPage(TabPage page)
	{
		EngineInfo engine = GetCurrentEngine();
		page.Controls.Clear();
		ListBox presetList = AddListBox(page.Controls, 30, 30, 240, 300, null);
		void UpdatePresetList(int index)
		{
			presetList.Items.Clear();
			presetList.Items.AddRange(engine.Presets.Keys.ToArray());
			presetList.SelectedIndex = index;
		}
		UpdatePresetList(0);
		AddButton(page.Controls, "New", 300, 30, 100, 40, (sender, e) =>
		{
			EditTextDialog dialog = new EditTextDialog("New Preset", "", (dialog) =>
			{
				string newName = dialog.Result.Trim();
				if (newName.Length == 0)
				{
					DialogHelper.ShowMessageBox("Name cannot be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				if (engine.Presets.Keys.Contains(newName))
				{
					DialogHelper.ShowMessageBox("Name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				dialog.Close();
				engine.Presets.Add(newName, new EngineOptions(engine.Presets.First().Value.Options.Keys));
				UpdatePresetList(engine.Presets.Count - 1);
			});
			DialogHelper.ShowDialog(dialog);
		});
		AddButton(page.Controls, "Duplicate", 300, 90, 100, 40, (sender, e) =>
		{
			string name = (string)presetList.SelectedItem;
			var match = Regex.Match(name, @"\((\d+)\)$");
			string newName;
			int number;
			int length;
			if (match.Success)
			{
				newName = name;
				number = int.Parse(match.Groups[1].Value);
				length = match.Groups[0].Value.Length;
			}
			else
			{
				newName = name + " (2)";
				number = 2;
				length = 3;
			}
			while (engine.Presets.ContainsKey(newName))
			{
				number++;
				string numberString = $"({number})";
				newName = newName[..^length] + numberString;
				length = numberString.Length;
			}
			engine.Presets.Add(newName, new EngineOptions(engine.Presets[name]));
			UpdatePresetList(engine.Presets.Count - 1);
		});
		AddButton(page.Controls, "Rename", 300, 150, 100, 40, (sender, e) =>
		{
			string name = (string)presetList.SelectedItem;
			EditTextDialog dialog = new EditTextDialog("Rename Preset", name, (dialog) =>
			{
				string newName = dialog.Result.Trim();
				if (newName == name)
				{
					dialog.Close();
					return;
				}
				if (newName.Length == 0)
				{
					DialogHelper.ShowMessageBox("Name cannot be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				if (engine.Presets.Keys.Contains(newName))
				{
					DialogHelper.ShowMessageBox("Name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				dialog.Close();
				List<KeyValuePair<string, EngineOptions>> list = engine.Presets.ToList();
				engine.Presets.Clear();
				foreach (var (key, value) in list)
				{
					engine.Presets[key == name ? newName : key] = value;
				}
				UpdatePresetList(list.FindIndex(pair => pair.Key == name));
			});
			DialogHelper.ShowDialog(dialog);
		});
		AddButton(page.Controls, "Edit", 300, 210, 100, 40, (sender, e) =>
		{
			PresetDialog dialog = new PresetDialog(engine, (string)presetList.SelectedItem);
			DialogHelper.ShowDialog(dialog);
		});
		AddButton(page.Controls, "Delete", 300, 270, 100, 40, (sender, e) =>
		{
			if (engine.Presets.Count == 1)
			{
				DialogHelper.ShowMessageBox("Cannot delete last preset!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			engine.Presets.Remove((string)presetList.SelectedItem);
			UpdatePresetList(Math.Min(presetList.SelectedIndex, engine.Presets.Count - 1));
		});
	}

	private EngineInfo GetCurrentEngine()
	{
		EngineInfo[] engines = EngineManager.GetInstalledEngines().ToArray();
		return engines[_engineList.SelectedIndex];
	}

	private readonly ListBox _engineList;
	private readonly TabControl _tabControl;
	private readonly TabPage _generalPage;
	private readonly TabPage _startupPage;
	private readonly TabPage _presetsPage;
}
