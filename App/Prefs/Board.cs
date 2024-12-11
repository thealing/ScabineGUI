namespace Scabine.App.Prefs;

internal static class Board
{
	public static bool Flipped = false;
	public static bool ShowCoordinates = true;
	public static bool ShowLegalMoves = true;
	public static bool HighlightSelection = true;
	public static bool HighlightMoves = true;
	public static bool HighlightCheck = true;

	static Board()
	{
		SaveManager.Save += () => SaveManager.Sync(nameof(Flipped), ref Flipped);
		SaveManager.Save += () => SaveManager.Sync(nameof(ShowCoordinates), ref ShowCoordinates);
		SaveManager.Save += () => SaveManager.Sync(nameof(ShowLegalMoves), ref ShowLegalMoves);
		SaveManager.Save += () => SaveManager.Sync(nameof(HighlightSelection), ref HighlightSelection);
		SaveManager.Save += () => SaveManager.Sync(nameof(HighlightMoves), ref HighlightMoves);
		SaveManager.Save += () => SaveManager.Sync(nameof(HighlightCheck), ref HighlightCheck);
	}
}
