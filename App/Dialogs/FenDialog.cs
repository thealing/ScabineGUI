namespace Scabine.App.Dialogs;

using System;
using System.Drawing;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class FenDialog : BaseDialog
{
	public FenDialog()
	{
		ClientSize = new Size(600, 135);
		Text = "Edit FEN";
		Font = new Font("Segoe UI", 11);
		string fen = GameManager.GetGame().GetFen();
		_fenEdit = AddTextBox(Controls, fen, 25, 25, 550, 30, null);
		AddButton(Controls, "Cancel", 395, 80, 80, 30, Cancel);
		AddButton(Controls, "Done", 495, 80, 80, 30, Done);
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void Done(object? sender, EventArgs e)
	{
		PgnManager.NewGame(_fenEdit.Text);
		Close();
	}

	private readonly TextBox _fenEdit;
}
