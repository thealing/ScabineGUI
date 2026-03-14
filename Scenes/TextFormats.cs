namespace ChessBand.Scenes;

using System.Windows.Forms;

public static class TextFormats
{
	public static readonly TextFormatFlags LeftAligned;
	public static readonly TextFormatFlags RightAligned;
	public static readonly TextFormatFlags Centered;
	public static readonly TextFormatFlags LeftWrapped;
	public static readonly TextFormatFlags LeftClipped;
	public static readonly TextFormatFlags CenteredClipped;

	static TextFormats()
	{
		LeftAligned = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping;
		RightAligned = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping;
		Centered = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping;
		LeftWrapped = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoClipping;
		LeftClipped = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
		CenteredClipped = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
	}
}
