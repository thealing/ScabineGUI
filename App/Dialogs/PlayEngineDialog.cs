namespace Scabine.App.Dialogs;

using Scabine.App;
using Scabine.App.Prefs;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Scabine.Core.Pieces;
using static Scabine.App.Dialogs.DialogCreator;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

internal class PlayEngineDialog : BaseDialog
{
	public PlayEngineDialog()
	{
		ClientSize = new Size(580, 580);
		Text = "Play Against Engine";
		Font = new Font("Segoe UI", 14);
		_engineGroup = new EngineGroup("eg") { Text = "Engine", Location = new Point(20, 20) };
		Controls.Add(_engineGroup);
		GroupBox playerGroup = AddGroupBox(Controls, "You", 20, 240, 540, 250);
		AddLabel(playerGroup.Controls, "Base time", 30, 40, 150, 30);
		AddLabel(playerGroup.Controls, "Increment", 30, 90, 150, 30);
		AddLabel(playerGroup.Controls, "Side", 30, 190, 100, 30);
		AddLabel(playerGroup.Controls, "Minutes", 250, 40, 90, 30);
		AddLabel(playerGroup.Controls, "Seconds", 440, 40, 90, 30);
		AddLabel(playerGroup.Controls, "Seconds", 440, 90, 90, 30);
		_playerMinutesControl = AddNumber(playerGroup.Controls, true, 180, 40, 60, 30, _playerTime / 60, 0, 999, UpdatePlayer);
		_playerSecondsControl = AddNumber(playerGroup.Controls, true, 370, 40, 60, 30, _playerTime % 60, 0, 59, UpdatePlayer);
		_playerIncrementControl = AddNumber(playerGroup.Controls, true, 370, 90, 60, 30, _playerIncrement, 0, 999, UpdatePlayer);
		_unlimitedTimeCheckBox = AddCheckBox(playerGroup.Controls, "Unlimited time", 30, 140, 155, 30, UpdatePlayer);
		_unlimitedTimeCheckBox.CheckAlign = ContentAlignment.MiddleRight;
		_unlimitedTimeCheckBox.Checked = _playerUnlimited;
		_whiteSideButton = AddRadioButton(playerGroup.Controls, "White", 140, 190, 90, 40, UpdatePlayer, Appearance.Button);
		_blackSideButton = AddRadioButton(playerGroup.Controls, "Black", 250, 190, 90, 40, UpdatePlayer, Appearance.Button);
		_randomSideButton = AddRadioButton(playerGroup.Controls, "Random", 360, 190, 90, 40, UpdatePlayer, Appearance.Button);
		switch (_playerSide)
		{
			case 0:
				_whiteSideButton.Checked = true;
				break;
			case 1:
				_blackSideButton.Checked = true;
				break;
			case 2:
				_randomSideButton.Checked = true;
				break;
		}
		UpdatePlayer(null, EventArgs.Empty);
		AddButton(Controls, "Cancel", 20, 515, 150, 45, Cancel);
		AddButton(Controls, "Play", 410, 515, 150, 45, Play);
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void Play(object? sender, EventArgs e)
	{
		if (_engineGroup.SelectedEngine == null)
		{
			DialogHelper.ShowMessageBox("No engine selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		int side = _playerSide;
		if (side == 2)
		{
			side = _randomSide;
			if (Time.GetTime() % 0.001 < 0.00075)
			{
				_randomSide ^= 1;
			}
		}
		PlayerMatchDefinition matchDefinition = new PlayerMatchDefinition(_playerUnlimited, _playerTime, _playerIncrement, side, _engineGroup.SelectedEngine, _engineGroup.PresetName, _engineGroup.ThinkingLimit);
		if (!MatchManager.StartPlayerMatch(matchDefinition))
		{
			return;
		}
		Board.Flipped = side == Black;
		Close();
	}

	private void UpdatePlayer(object? sender, EventArgs e)
	{
		_playerTime = (int)_playerMinutesControl.Value * 60 + (int)_playerSecondsControl.Value;
		_playerIncrement = (int)_playerIncrementControl.Value;
		_playerUnlimited = _unlimitedTimeCheckBox.Checked;
		_playerMinutesControl.Enabled = !_playerUnlimited;
		_playerSecondsControl.Enabled = !_playerUnlimited;
		_playerIncrementControl.Enabled = !_playerUnlimited;
		if (_whiteSideButton.Checked)
		{
			_playerSide = 0;
		}
		if (_blackSideButton.Checked)
		{
			_playerSide = 1;
		}
		if (_randomSideButton.Checked)
		{
			_playerSide = 2;
		}
	}

	static PlayEngineDialog()
	{
		_playerTime = 180;
		_playerIncrement = 1;
		_playerUnlimited = false;
		_playerSide = 2;
		_randomSide = 0;
		SaveManager.Save += () => SaveManager.Sync(nameof(_playerTime), ref _playerTime);
		SaveManager.Save += () => SaveManager.Sync(nameof(_playerIncrement), ref _playerIncrement);
		SaveManager.Save += () => SaveManager.Sync(nameof(_playerUnlimited), ref _playerUnlimited);
		SaveManager.Save += () => SaveManager.Sync(nameof(_playerSide), ref _playerSide);
		SaveManager.Save += () => SaveManager.Sync(nameof(_randomSide), ref _randomSide);
	}

	private readonly EngineGroup _engineGroup;
	private readonly NumericUpDown _playerMinutesControl;
	private readonly NumericUpDown _playerSecondsControl;
	private readonly NumericUpDown _playerIncrementControl;
	private readonly CheckBox _unlimitedTimeCheckBox;
	private readonly RadioButton _whiteSideButton;
	private readonly RadioButton _blackSideButton;
	private readonly RadioButton _randomSideButton;
	private static int _playerTime;
	private static int _playerIncrement;
	private static bool _playerUnlimited;
	private static int _playerSide;
	private static int _randomSide;
}
