namespace Scabine.App.Dialogs;

using Scabine.App;
using System;
using System.Drawing;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class ThinkingLimitDialog : BaseDialog
{
	public ThinkingLimitDialog(ThinkingLimit limit)
	{
		ClientSize = new Size(510, 310);
		Text = "Thinking Limit";
		Font = new Font("Segoe UI", 14);
		_limit = limit;
		_newLimit = new ThinkingLimit();
		_newLimit.Copy(limit);
		AddButton(Controls, "OK", 400, 250, 90, 40, Ok);
		AddButton(Controls, "Cancel", 20, 250, 90, 40, Cancel);
		GroupBox groupBox = AddGroupBox(Controls, "Mode", 30, 20, 180, 210);
		_gameTimeButton = AddRadioButton(groupBox.Controls, "Game time", 25, 40, 140, 30, UpdateMode);
		_moveTimeButton = AddRadioButton(groupBox.Controls, "Move time", 25, 80, 140, 30, UpdateMode);
		_fixedDepthButton = AddRadioButton(groupBox.Controls, "Fixed depth", 25, 120, 140, 30, UpdateMode);
		_fixedNodesButton = AddRadioButton(groupBox.Controls, "Fixed nodes", 25, 160, 140, 30, UpdateMode);
		_valuesPanel = AddPanel(Controls, 230, 30, 250, 200);
		_valuesPanel.BackColor = Color.White;
		_baseMinutesControl = AddNumber(_valuesPanel.Controls, false, 150, 20, 70, 30, Math.Truncate(_limit.BaseTime / 60), 0, 99999, UpdateValues);
		_baseSecondsControl = AddNumber(_valuesPanel.Controls, false, 150, 80, 70, 30, _limit.BaseTime % 60, 0, 59, UpdateValues);
		_incSecondsControl = AddNumber(_valuesPanel.Controls, false, 150, 140, 70, 30, _limit.Increment, 0, 99999, UpdateValues);
		_moveSecondsControl = AddNumber(_valuesPanel.Controls, false, 150, 20, 70, 30, Math.Floor(_limit.MoveTime), 0, 99999, UpdateValues);
		_moveMillisecondsControl = AddNumber(_valuesPanel.Controls, false, 150, 80, 70, 30, _limit.MoveTime * 1000 % 1000, 0, 999, UpdateValues);
		_fixedDepthControl = AddNumber(_valuesPanel.Controls, false, 150, 20, 70, 30, _limit.Depth, 1, 99, UpdateValues);
		_fixedNodesControl = AddNumber(_valuesPanel.Controls, false, 100, 20, 120, 30, _limit.Nodes, 1, 999999999, UpdateValues);
		switch (_newLimit.Mode)
		{
			case ThinkingMode.GameTime:
				UpdateMode(_gameTimeButton, EventArgs.Empty);
				break;
			case ThinkingMode.MoveTime:
				UpdateMode(_moveTimeButton, EventArgs.Empty);
				break;
			case ThinkingMode.FixedDepth:
				UpdateMode(_fixedDepthButton, EventArgs.Empty);
				break;
			case ThinkingMode.FixedNodes:
				UpdateMode(_fixedNodesButton, EventArgs.Empty);
				break;

		}
	}

	private void Ok(object? sender, EventArgs e)
	{
		_limit.Copy(_newLimit);
		Close();
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void UpdateMode(object? sender, EventArgs e)
	{
		if (sender is RadioButton button)
		{
			button.Checked = true;
		}
		_valuesPanel.Controls.Clear();
		if (sender == _gameTimeButton)
		{
			_newLimit.Mode = ThinkingMode.GameTime;
			AddLabel(_valuesPanel.Controls, "Minutes", 20, 20, 130, 30);
			AddLabel(_valuesPanel.Controls, "Seconds", 20, 80, 130, 30);
			AddLabel(_valuesPanel.Controls, "Increment", 20, 140, 130, 30);
			_valuesPanel.Controls.Add(_baseMinutesControl);
			_valuesPanel.Controls.Add(_baseSecondsControl);
			_valuesPanel.Controls.Add(_incSecondsControl);
		}
		if (sender == _moveTimeButton)
		{
			_newLimit.Mode = ThinkingMode.MoveTime;
			AddLabel(_valuesPanel.Controls, "Seconds", 20, 20, 130, 30);
			AddLabel(_valuesPanel.Controls, "Milliseconds", 20, 80, 130, 30);
			_valuesPanel.Controls.Add(_moveSecondsControl);
			_valuesPanel.Controls.Add(_moveMillisecondsControl);
		}
		if (sender == _fixedDepthButton)
		{
			_newLimit.Mode = ThinkingMode.FixedDepth;
			AddLabel(_valuesPanel.Controls, "Depth", 20, 20, 130, 30);
			_valuesPanel.Controls.Add(_fixedDepthControl);
		}
		if (sender == _fixedNodesButton)
		{
			_newLimit.Mode = ThinkingMode.FixedNodes;
			AddLabel(_valuesPanel.Controls, "Nodes", 20, 20, 80, 30);
			_valuesPanel.Controls.Add(_fixedNodesControl);
		}
	}

	private void UpdateValues(object? sender, EventArgs e)
	{
		_newLimit.BaseTime = _baseMinutesControl.Value * 60 + _baseSecondsControl.Value;
		_newLimit.Increment = _incSecondsControl.Value;
		_newLimit.MoveTime = _moveSecondsControl.Value + _moveMillisecondsControl.Value / 1000;
		_newLimit.Depth = (int)_fixedDepthControl.Value;
		_newLimit.Nodes = (int)_fixedNodesControl.Value;
	}

	private readonly ThinkingLimit _limit;
	private readonly ThinkingLimit _newLimit;
	private readonly RadioButton _gameTimeButton;
	private readonly RadioButton _moveTimeButton;
	private readonly RadioButton _fixedDepthButton;
	private readonly RadioButton _fixedNodesButton;
	private readonly Panel _valuesPanel;
	private readonly NumericUpDown _baseMinutesControl;
	private readonly NumericUpDown _baseSecondsControl;
	private readonly NumericUpDown _incSecondsControl;
	private readonly NumericUpDown _moveSecondsControl;
	private readonly NumericUpDown _moveMillisecondsControl;
	private readonly NumericUpDown _fixedDepthControl;
	private readonly NumericUpDown _fixedNodesControl;
}
