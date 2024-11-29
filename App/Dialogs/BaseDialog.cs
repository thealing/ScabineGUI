namespace Scabine.App.Dialogs;

using System.Windows.Forms;

internal class BaseDialog : Form
{
	public BaseDialog()
	{
		StartPosition = FormStartPosition.CenterParent;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
	}
}
