namespace ChessBand.Scenes;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChessBand.Application;

public class SceneToolStripRenderer : ToolStripProfessionalRenderer
{
	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		e.Graphics.FillRectangle(Brushes.White, e.AffectedBounds);
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		if (e.Item is ToolStripMenuItem menu && menu.DropDown.Visible)
		{
			base.OnRenderMenuItemBackground(e);
			return;
		}
		Rectangle rectangle = new Rectangle(Point.Empty, e.Item.Size);
		if (e.Item.Enabled)
		{
			if (e.Item.Pressed)
			{
				e.Graphics.FillRectangle(Brushes.DarkGray, rectangle);
				return;
			}
			if (e.Item.Selected)
			{
				e.Graphics.FillRectangle(Brushes.LightGray, rectangle);
				return;
			}
		}
		e.Graphics.FillRectangle(Brushes.White, rectangle);
	}

	protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
	{
		Rectangle rectangle = new Rectangle(Point.Empty, e.Item.Size);
		e.Graphics.FillRectangle(Brushes.White, rectangle);
	}

	protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
	{
		e.Graphics.FillRectangle(Brushes.White, e.AffectedBounds);
	}

	protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
	{
		if (e.Image == null)
		{
			return;
		}
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
		e.Graphics.DrawImage(e.Image, e.ImageRectangle);
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		Rectangle rectangle = e.TextRectangle;
		if ((e.TextFormat & TextFormatFlags.Right) != 0)
		{
			rectangle = e.Item.ContentRectangle;
			rectangle.Offset(-4, 0);
		}
		GraphicsHelper.DrawText(e.Graphics, e.Text, e.TextFont, rectangle, e.Item.Enabled ? Color.Black : Color.Gray, e.TextFormat);
	}
}
