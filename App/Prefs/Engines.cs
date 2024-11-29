namespace Scabine.App.Prefs;

internal static class Engines
{
	public static int MaxAnalysisTime = 1000;
	public static bool PauseWhenInBackground = true;
	public static bool ResetBeforeEveryMove = false;

	static Engines()
	{
		SaveManager.Save += () => SaveManager.Sync(nameof(MaxAnalysisTime), ref MaxAnalysisTime);
		SaveManager.Save += () => SaveManager.Sync(nameof(PauseWhenInBackground), ref PauseWhenInBackground);
		SaveManager.Save += () => SaveManager.Sync(nameof(ResetBeforeEveryMove), ref ResetBeforeEveryMove);
	}
}
