namespace Scabine.App;

using Scabine.Scenes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

internal class GraphicsHelper
{
	public static void DrawLine(Graphics g, Pen pen, Point pt1, Point pt2)
	{
		Rectangle lineBounds = new Rectangle(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Abs(pt1.X - pt2.X), Math.Abs(pt1.Y - pt2.Y));
		if (g.ClipBounds.IntersectsWith(lineBounds))
		{
			g.DrawLine(pen, pt1, pt2);
		}
	}

	public static void DrawString(Graphics g, string text, Font font, Brush brush, Rectangle layoutRectangle, StringFormat format)
	{
		if (g.ClipBounds.IntersectsWith(layoutRectangle))
		{
			g.DrawString(text, font, brush, layoutRectangle, format);
		}
	}

	public static void FillRectangle(Graphics g, Brush brush, Rectangle rect)
	{
		if (g.ClipBounds.IntersectsWith(rect))
		{
			g.FillRectangle(brush, rect);
		}
	}

	public static void DrawRectangle(Graphics g, Pen pen, Rectangle rect)
	{
		if (g.ClipBounds.IntersectsWith(rect))
		{
			g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
		}
	}

	public static Image ResizeImage(Image image, int width, int height)
	{
		Image resizedImage = new Bitmap(width, height);
		using (Graphics graphics = Graphics.FromImage(resizedImage))
		using (new CompositingQualityChanger(graphics, CompositingQuality.HighQuality))
		{
			graphics.DrawImage(image, 0, 0, width, height);
		}
		return resizedImage;
	}

	public static Color MixColors(double ratio, Color color, Color otherColor)
	{
		int a = (int)(color.A * ratio + otherColor.A * (1 - ratio));
		int r = (int)(color.R * ratio + otherColor.R * (1 - ratio));
		int g = (int)(color.G * ratio + otherColor.G * (1 - ratio));
		int b = (int)(color.B * ratio + otherColor.B * (1 - ratio));
		return Color.FromArgb(a, r, g, b);
	}
}
