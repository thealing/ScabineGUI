namespace Scabine.App.Dialogs;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;
using Scabine.App.Prefs;

internal class EngineGroup : GroupBox
{
	public EngineInfo? SelectedEngine => _selectedEngine;
	public string PresetName => (string)_presetList.SelectedItem;
	public ThinkingLimit ThinkingLimit => _thinkingLimit;

	public EngineGroup(string tag)
	{
		Size = new Size(540, 200);
		AddLabel(Controls, "Program", 30, 40, 150, 40);
		AddLabel(Controls, "Preset", 30, 90, 150, 40);
		AddLabel(Controls, "Thinking limit", 30, 140, 200, 40);
		_thinkingLimit = new ThinkingLimit();
		_selectedEngineHash = "";
		_engineHashToPreset = new Dictionary<string, int>();
		_programList = AddDropDownList(Controls, 250, 40, 270, 40);
		_presetList = AddDropDownList(Controls, 250, 90, 270, 40);
		List<EngineInfo> engines = EngineManager.GetInstalledEngines().ToList();
		_programList.Items.AddRange(engines.Select(engine => engine.Name).ToArray());
		_programList.SelectedIndexChanged += (sender, e) =>
		{
			_selectedEngine = engines[_programList.SelectedIndex];
			_selectedEngineHash = _selectedEngine.Hash;
			_presetList.Items.Clear();
			_presetList.Items.AddRange(_selectedEngine.Presets.Keys.ToArray());
			_presetList.SelectedIndexChanged += (sender, e) =>
			{
				_engineHashToPreset[_selectedEngineHash] = _presetList.SelectedIndex;
			};
			_presetList.SelectedIndex = _engineHashToPreset.GetValueOrDefault(_selectedEngineHash);
		};
		Paint += (sender, e) =>
		{
			SaveManager.Sync(tag + nameof(_programList), _programList.SelectedIndex);
			SaveManager.Sync(tag + nameof(_presetList), _presetList.SelectedIndex);
			SaveManager.Sync(tag + nameof(_thinkingLimit), _thinkingLimit);
			SaveManager.Sync(tag + nameof(_engineHashToPreset), _engineHashToPreset);
		};
		if (SaveManager.GetValue<int>(tag + nameof(_programList)) is int programIndex)
		{
			if (programIndex >= 0 && programIndex < _programList.Items.Count)
			{
				_programList.SelectedIndex = programIndex;
			}
		}
		if (SaveManager.GetValue<int>(tag + nameof(_presetList)) is int presetIndex)
		{
			if (presetIndex >= 0 && presetIndex < _presetList.Items.Count)
			{
				_presetList.SelectedIndex = presetIndex;
			}
		}
		if (SaveManager.GetValue<ThinkingLimit>(tag + nameof(_thinkingLimit)) is ThinkingLimit thinkingLimit)
		{
			_thinkingLimit = thinkingLimit;
		}
		if (SaveManager.GetValue<Dictionary<string, int>>(tag + nameof(_engineHashToPreset)) is Dictionary<string, int> engineHashToPreset)
		{
			_engineHashToPreset = engineHashToPreset;
		}
		_limitButton = AddButton(Controls, _thinkingLimit.ToString(), 300, 140, 220, 40, SetEngineLimit);
	}

	private void SetEngineLimit(object? sender, EventArgs e)
	{
		ThinkingLimitDialog dialog = new ThinkingLimitDialog(_thinkingLimit);
		DialogHelper.ShowDialog(dialog);
		_limitButton.Text = _thinkingLimit.ToString();
	}

	private ComboBox _programList;
	private ComboBox _presetList;
	private Dictionary<string, int> _engineHashToPreset;
	private ThinkingLimit _thinkingLimit;
	private Button _limitButton;
	private EngineInfo? _selectedEngine;
	private string _selectedEngineHash;
}
