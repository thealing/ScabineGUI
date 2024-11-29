namespace Scabine.App.Dialogs;

using System;
using System.Drawing;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class EditTextDialog : BaseDialog
{
	public string Result => _box.Text;

	public EditTextDialog(string title, string name, Action<EditTextDialog>? done)
	{
		ClientSize = new Size(500, 135);
		Text = title;
		Font = new Font("Segoe UI", 13);
		_box = AddTextBox(Controls, name, 25, 25, 450, 30, null);
		_box.KeyDown += (sender, e) =>
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
			if (e.KeyCode == Keys.Enter)
			{
				done?.Invoke(this);
			}
		};
		AddButton(Controls, "Done", 400, 80, 75, 30, (sender, e) => done?.Invoke(this));
	}

	private readonly TextBox _box;
}
