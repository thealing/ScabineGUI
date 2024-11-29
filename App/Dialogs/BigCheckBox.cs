namespace Scabine.App.Dialogs;

using System;
using System.Drawing;
using System.Windows.Forms;

internal class BigCheckBox : CheckBox
{
	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor == Color.Transparent ? Color.White : BackColor);
		int side = Math.Min(Width, Height) * 7 / 8;
		Size size = new Size(side, side);
		ControlPaint.DrawCheckBox(e.Graphics, new Rectangle(Point.Empty + (Size - size) / 2, size), ButtonState.Flat | (Checked ? ButtonState.Checked : ButtonState.Normal));
	}
}
