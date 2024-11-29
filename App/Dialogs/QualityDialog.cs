namespace Scabine.App.Dialogs;

using Scabine.App.Prefs;
using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class QualityDialog : BaseDialog
{
	public bool Success => _success;
	public long Quality => _trackBar.Value;

	public QualityDialog()
	{
		ClientSize = new Size(340, 120);
		Text = "Image Quality";
		Font = new Font("Segoe UI", 11);
		_label = AddLabel(Controls, $"Quality: {_lastQuality}", 20, 70, 140, 30);
		_trackBar = AddTrackBar(Controls, 20, 20, 300, 30, Change);
		_trackBar.Minimum = 0;
		_trackBar.Maximum = 100;
		_trackBar.TickFrequency = 10;
		_trackBar.Value = _lastQuality;
		AddButton(Controls, "Ok", 240, 70, 80, 30, Ok);
		_success = false;
	}

	private void Ok(object? sender, EventArgs e)
	{
		_success = true;
		_lastQuality = _trackBar.Value;
		Close();
	}

	private void Change(object? sender, EventArgs e)
	{
		_label.Text = $"Quality: {_trackBar.Value}";
	}

	private readonly TrackBar _trackBar;
	private readonly Label _label;
	private bool _success;

	static QualityDialog()
	{
		_lastQuality = 90;
		SaveManager.Save += () => SaveManager.Sync(nameof(_lastQuality), ref _lastQuality);
	}

	private static int _lastQuality;
}
