namespace Scabine.Scenes;

using System.Drawing;

public static class StringFormats
{
	public static readonly StringFormat LeftAligned;
	public static readonly StringFormat RightAligned;
	public static readonly StringFormat Centered;
	public static readonly StringFormat LeftWrapped;
	public static readonly StringFormat LeftClipped;

	static StringFormats()
	{
		LeftAligned = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.None,
			FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip,
		};
		RightAligned = new StringFormat()
		{
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.None,
			FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip,
		};
		Centered = new StringFormat()
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.None,
			FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip,
		};
		LeftWrapped = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.None,
			FormatFlags = StringFormatFlags.NoClip,
		};
		LeftClipped = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.EllipsisWord,
			FormatFlags = StringFormatFlags.NoWrap,
		};
	}
}
