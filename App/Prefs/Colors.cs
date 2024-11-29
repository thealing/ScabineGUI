namespace Scabine.App.Prefs;

using System.Drawing;

internal static class Colors
{
	public static Color SelectedSquare;
	public static Color HighlightedSquare;
	public static Color LegalSquare;
	public static Color AttackedSquare;

	static Colors()
	{
		SelectedSquare = Color.FromArgb(150, 20, 150, 20);
		HighlightedSquare = Color.FromArgb(150, 200, 250, 50);
		LegalSquare = Color.FromArgb(100, 100, 250, 50);
		AttackedSquare = Color.FromArgb(100, 250, 10, 10);
	}
}
