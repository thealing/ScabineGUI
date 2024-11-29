namespace Scabine.App.Dialogs;

using System;
using System.Drawing;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class PgnDialog : BaseDialog
{
	public PgnDialog()
	{
		ClientSize = new Size(600, 500);
		Text = "Edit PGN";
		Font = new Font("Segoe UI", 11);
		string fen = PgnManager.GetPgn();
		_pgnEdit = AddTextBox(Controls, fen, 25, 25, 550, 400, null);
		_pgnEdit.Multiline = true;
		_pgnEdit.WordWrap = true;
		AddButton(Controls, "Cancel", 395, ClientSize.Height - 55, 80, 30, Cancel);
		AddButton(Controls, "Done", 495, ClientSize.Height - 55, 80, 30, Done);
	}

	private void Cancel(object? sender, EventArgs e)
	{
		Close();
	}

	private void Done(object? sender, EventArgs e)
	{
		PgnManager.SetPgn(_pgnEdit.Text);
		Close();
	}

	private readonly TextBox _pgnEdit;
}
