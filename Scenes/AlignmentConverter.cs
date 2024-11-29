namespace Scabine.Scenes;

using System.Drawing;

public static class AlignmentConverter
{
	public static StringFormat ConvertContentAlignmentToStringFormat(ContentAlignment alignment)
	{
		StringFormat format = new StringFormat();
		switch (alignment)
		{
			case ContentAlignment.TopLeft:
				format.Alignment = StringAlignment.Near;
				format.LineAlignment = StringAlignment.Near;
				break;
			case ContentAlignment.TopCenter:
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Near;
				break;
			case ContentAlignment.TopRight:
				format.Alignment = StringAlignment.Far;
				format.LineAlignment = StringAlignment.Near;
				break;
			case ContentAlignment.MiddleLeft:
				format.Alignment = StringAlignment.Near;
				format.LineAlignment = StringAlignment.Center;
				break;
			case ContentAlignment.MiddleCenter:
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;
				break;
			case ContentAlignment.MiddleRight:
				format.Alignment = StringAlignment.Far;
				format.LineAlignment = StringAlignment.Center;
				break;
			case ContentAlignment.BottomLeft:
				format.Alignment = StringAlignment.Near;
				format.LineAlignment = StringAlignment.Far;
				break;
			case ContentAlignment.BottomCenter:
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Far;
				break;
			case ContentAlignment.BottomRight:
				format.Alignment = StringAlignment.Far;
				format.LineAlignment = StringAlignment.Far;
				break;
		}
		format.Trimming = StringTrimming.None;
		format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
		return format;
	}
}