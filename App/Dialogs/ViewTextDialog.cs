namespace Scabine.App.Dialogs;

using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using static Scabine.App.Dialogs.DialogCreator;

internal class ViewTextDialog : BaseDialog
{
	public ViewTextDialog(string title, string text)
	{
		ClientSize = new Size(600, 500);
		Text = title;
		Font = new Font("Segoe UI", 10);
		TextBox box = AddTextBox(Controls, text, 25, 25, 550, 400, null);
		box.Multiline = true;
		box.WordWrap = true;
		box.ScrollBars = ScrollBars.Vertical;
		AddButton(Controls, "Close", 495, ClientSize.Height - 55, 80, 30, (sender, e) => Close());
	}
}
