namespace Scabine.Scenes;

using System;
using System.ComponentModel;
using System.Windows.Forms;

public sealed class SceneWindow : Form
{
	public SceneWindow()
	{
		SetStyle(ControlStyles.UserPaint, true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		Cursor = CursorManager.GetCursor();
		base.OnMouseMove(e);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		e.Cancel = true;
		SceneManager.Exit();
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}
}
