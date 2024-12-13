namespace Scabine.App.Dialogs;

using Scabine.App.Prefs;
using Scabine.Engines;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class AnalyzisDialog : BaseDialog
{
	public IEngine? Engine { get; private set; }
	public int Depth { get; private set; }

	public AnalyzisDialog()
	{
		ClientSize = new Size(590, 280);
		Text = "Auto game analysis";
		Font = new Font("Segoe UI", 14);
		AddLabel(Controls, "Program", 20, 20, 110, 30);
		AddLabel(Controls, "Preset", 20, 80, 110, 30);
		AddLabel(Controls, "Depth", 20, 140, 110, 30);
		_programList = AddDropDownList(Controls, 270, 20, 280, 200);
		_programList.SelectedIndexChanged += (sender, e) => UpdatePresets();
		UpdateEngines();
		_presetList = AddDropDownList(Controls, 270, 80, 280, 200);
		SaveManager.Sync(this, nameof(_depth), ref _depth);
		AddNumber(Controls, true, 400, 140, 150, 200, _depth, 1, 99, (sender, e) =>
		{
			if (sender is NumericUpDown numericUpDown)
			{
				_depth = (int)numericUpDown.Value;
				SaveManager.Sync(this, nameof(_depth), ref _depth);
			}
		});
		Paint += (sender, e) =>
		{
			if (_programList.SelectedItem is string programName)
			{
				SaveManager.Sync(this, nameof(programName), programName);
			}
			if (_presetList.SelectedItem is string presetName)
			{
				SaveManager.Sync(this, nameof(presetName), presetName);
			}
			_programList.SelectedItem = SaveManager.GetValue<string>(this, nameof(programName));
			_presetList.SelectedItem = SaveManager.GetValue<string>(this, nameof(presetName));
		};
		Button startButton = AddButton(Controls, "Run", 210, 200, 150, 50, Start);
	}

	private void Start(object? sender, EventArgs e)
	{
		if (_programList.SelectedIndex == -1 || _presetList.SelectedIndex == -1)
		{
			DialogHelper.ShowMessageBox("No engine selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		EngineInfo engineInfo = _engines[_programList.SelectedIndex];
		if (EngineManager.StartEngine(engineInfo, (string)_presetList.SelectedItem, out IEngine? engine, true))
		{
			Engine = engine;
			Depth = _depth;
			Close();
		}
	}

	[MemberNotNull(nameof(_engines))]
	private void UpdateEngines()
	{
		_engines = EngineManager.GetInstalledEngines().ToArray();
		_programList.Items.Clear();
		_programList.Items.AddRange(_engines.Select(engine => engine.Name).ToArray());
	}

	private void UpdatePresets()
	{
		int index = _programList.SelectedIndex;
		_presetList.Items.Clear();
		if (index != -1)
		{
			_presetList.Items.AddRange(_engines[index].Presets.Keys.ToArray());
			_presetList.SelectedIndex = 0;
		}
	}

	static AnalyzisDialog()
	{
		_depth = 12;
	}

	private static int _depth;
	private readonly ComboBox _programList;
	private readonly ComboBox _presetList;
	private EngineInfo[] _engines;
}
