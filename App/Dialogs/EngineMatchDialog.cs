namespace Scabine.App.Dialogs;

using Scabine.App;
using Scabine.App.Prefs;
using Scabine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Channels;
using System.Windows.Forms;
using static Scabine.Core.Pieces;
using static Scabine.App.Dialogs.DialogCreator;

internal class EngineMatchDialog : BaseDialog
{
	public EngineMatchDialog()
	{
		ClientSize = new Size(580, 585);
		Text = "Engine Match";
		Font = new Font("Segoe UI", 14);
		_engineGroups = new EngineGroup[2];
		_engineGroups[Black] = new EngineGroup("te") { Text = "Black Engine", Location = new Point(20, 20) };
		_engineGroups[White] = new EngineGroup("be") { Text = "White Engine", Location = new Point(20, 240) };
		if (Board.Flipped)
		{
			SwapEngines();
		}
		foreach (EngineGroup group in _engineGroups)
		{
			Controls.Add(group);
		}
		AddButton(Controls, "Switch Sides", 20, 460, 180, 40, SwitchSides);
		AddButton(Controls, "Cancel", 20, 520, 150, 45, Cancel);
		AddButton(Controls, "Play", 410, 520, 150, 45, Play);
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void Play(object? sender, EventArgs e)
	{
		EngineInfo? whiteEngine = _engineGroups[White].SelectedEngine;
		EngineInfo? blackEngine = _engineGroups[Black].SelectedEngine;
		if (whiteEngine == null || blackEngine == null)
		{
			DialogHelper.ShowMessageBox("No engine selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		EngineMatchDefinition matchDefinition = new EngineMatchDefinition(whiteEngine, _engineGroups[White].PresetName, _engineGroups[White].ThinkingLimit, blackEngine, _engineGroups[Black].PresetName, _engineGroups[Black].ThinkingLimit);
		if (!MatchManager.StartEngineMatch(matchDefinition))
		{ 
			return; 
		}
		Board.Flipped = _engineGroups[White].Location.Y < _engineGroups[Black].Location.Y;
		Close();
	}

	private void SwitchSides(object? sender, EventArgs e)
	{
		SwapEngines();
	}

	private void SwapEngines()
	{
		Array.Reverse(_engineGroups);
		_engineGroups[White].Text = "White Engine";
		_engineGroups[Black].Text = "Black Engine";
	}

	private EngineGroup[] _engineGroups;
}
