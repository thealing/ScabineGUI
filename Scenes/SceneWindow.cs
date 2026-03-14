namespace ChessBand.Scenes;
using System.Windows.Forms;

public sealed class SceneWindow : Form
{
	public SceneWindow()
	{
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(Location));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(Size));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(ClientSize));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(BackColor));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(ForeColor));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(Font));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(Text));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(BackgroundImage));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(WindowState));
		InvalidationManager.RegisterInvalidatingProperty(this, nameof(Opacity));
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
