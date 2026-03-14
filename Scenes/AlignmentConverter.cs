namespace ChessBand.Scenes;

using System.Drawing;
using System.Windows.Forms;

public static class AlignmentConverter
{
	public static TextFormatFlags ConvertContentAlignmentToTextFormatFlags(ContentAlignment alignment)
	{
		TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
		switch (alignment)
		{
			case ContentAlignment.TopLeft:
				flags |= TextFormatFlags.Left | TextFormatFlags.Top;
				break;
			case ContentAlignment.TopCenter:
				flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.Top;
				break;
			case ContentAlignment.TopRight:
				flags |= TextFormatFlags.Right | TextFormatFlags.Top;
				break;
			case ContentAlignment.MiddleLeft:
				flags |= TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
				break;
			case ContentAlignment.MiddleCenter:
				flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
				break;
			case ContentAlignment.MiddleRight:
				flags |= TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
				break;
			case ContentAlignment.BottomLeft:
				flags |= TextFormatFlags.Left | TextFormatFlags.Bottom;
				break;
			case ContentAlignment.BottomCenter:
				flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
				break;
			case ContentAlignment.BottomRight:
				flags |= TextFormatFlags.Right | TextFormatFlags.Bottom;
				break;
		}
		return flags;
	}
}