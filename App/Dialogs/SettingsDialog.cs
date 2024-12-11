namespace Scabine.App.Dialogs;

using Scabine.App.Prefs;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class SettingsDialog : BaseDialog
{
	private event EventHandler? Save;

	public SettingsDialog()
	{
		ClientSize = new Size(500, 480);
		Text = "Settings";
		Font = new Font("Segoe UI", 14);
		_tabControl = AddTabControl(Controls, 0, 0, ClientSize.Width + 2, ClientSize.Height - 80);
		AddButton(Controls, "OK", ClientSize.Width - 120, ClientSize.Height - 60, 100, 40, Ok);
		AddButton(Controls, "Cancel", 20, ClientSize.Height - 60, 100, 40, Cancel);
		TabPage page = AddTab(typeof(General));
		AddValue(page, nameof(General.Name));
		AddValue(page, nameof(General.PlaySounds));
		AddValue(page, nameof(General.ConfirmExit));
		//AddValue(page, nameof(General.AutoSaveInterval), "Auto-save interval (ms)");
		page = AddTab(typeof(Themes));
		AddValue(page, nameof(Themes.Pieces));
		AddValue(page, nameof(Themes.Board));
		page = AddTab(typeof(Play));
		AddValue(page, nameof(Play.AutoQueen));
		AddValue(page, nameof(Play.MoveMethod));
		AddValue(page, nameof(Play.MoveAnimation));
		AddValue(page, nameof(Play.AutoPlayInterval), "Auto-play interval (ms)");
		page = AddTab(typeof(Board));
		AddValue(page, nameof(Board.ShowCoordinates));
		AddValue(page, nameof(Board.ShowLegalMoves));
		AddValue(page, nameof(Board.HighlightSelection));
		AddValue(page, nameof(Board.HighlightMoves));
		AddValue(page, nameof(Board.HighlightCheck));
		page = AddTab(typeof(Engines));
		AddValue(page, nameof(Engines.ResetBeforeEveryMove));
		AddValue(page, nameof(Engines.PauseWhenInBackground));
		AddValue(page, nameof(Engines.MaxAnalysisTime), "Max analysis time (ms)");
		_tabControl.Selected += (sender, e) => _currentTab = _tabControl.SelectedTab.Text;
		foreach (TabPage tabPage in _tabControl.TabPages)
		{
			if (tabPage.Text == _currentTab)
			{
				_tabControl.SelectedTab = tabPage;
			}
		}
	}

	private void Ok(object? sender, EventArgs e)
	{
		Save?.Invoke(sender, e);
		Close();
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private TabPage AddTab(Type type)
	{
		RuntimeHelpers.RunClassConstructor(type.TypeHandle);
		TabPage page = AddTabPage(_tabControl.Controls, type.Name + " ", 0, 30, _tabControl.Width, _tabControl.Height);
		return page;
	}

	private void AddValue(TabPage page, string tag, string? label = null)
	{
		label ??= SplitWords(tag);
		object? value = SaveManager.GetValue(tag);
		int padding = 25;
		int height = Font.Height * 5 / 4;
		int labelWidth = TextRenderer.MeasureText(label, Font).Width;
		int width = Math.Min(page.Width / 2, page.Width - padding * 3 - labelWidth) - padding;
		int x = page.Width - padding - width;
		int y = page.Controls.Cast<Control>().Select(c => c.Bottom).DefaultIfEmpty(0).Max() + padding;
		AddLabel(page.Controls, label, padding, y, labelWidth, height);
		if (value is string stringValue)
		{
			TextBox textBox = AddTextBox(page.Controls, stringValue, x, y, width, height, null);
			Save += (sender, e) =>
			{
				if (textBox.Text.Any())
				{
					SaveManager.SetValue(tag, textBox.Text);
				}
			};
		}
		if (value is int intValue)
		{
			NumericUpDown number = AddNumber(page.Controls, false, x, y, width, height, intValue, 0, int.MaxValue, null);
			Save += (sender, e) =>
			{
				SaveManager.SetValue(tag, (int)number.Value);
			};
		}
		if (value is bool boolValue)
		{
			width = height * 8 / 7;
			CheckBox checkBox = AddBigCheckBox(page.Controls, "", page.Width - padding * 2 - width, y, width, height * 8 / 7, null);
			checkBox.Checked = boolValue;
			Save += (sender, e) =>
			{
				SaveManager.SetValue(tag, checkBox.Checked);
			};
		}
		if (value is Tuple<string, string[]> comboValue)
		{
			ComboBox comboBox = AddDropDownList(page.Controls, x, y, width, height);
			comboBox.Items.AddRange(comboValue.Item2);
			comboBox.SelectedItem = comboValue.Item1;
			Save += (sender, e) =>
			{
				if (comboBox.SelectedItem is string selectedOption)
				{
					SaveManager.SetValue(tag, Tuple.Create(selectedOption, comboValue.Item2));
				}
			};
		}
	}

	private string SplitWords(string input)
	{
		string result = Regex.Replace(input, "(?<!^)([A-Z])", " $1").ToLower();
		return char.ToUpper(result[0]) + result.Substring(1);
	}

	static SettingsDialog()
	{
		_currentTab = "";
		SaveManager.Save += () => SaveManager.Sync(nameof(_currentTab), ref _currentTab);
	}

	private static string _currentTab;
	private readonly TabControl _tabControl;
}
