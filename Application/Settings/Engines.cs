namespace ChessBand.Application.Settings;

using ChessBand.Engines;
using ChessBand.Scenes;

internal static class Engines
{
	public static int MaxAnalysisTime = 2000;
	public static bool PauseWhenInBackground = true;
	public static bool ResetBeforeEveryMove = false;

	static Engines()
	{
		SaveManager.Save += () => SaveManager.Sync(nameof(MaxAnalysisTime), ref MaxAnalysisTime);
		SaveManager.Save += () => SaveManager.Sync(nameof(PauseWhenInBackground), ref PauseWhenInBackground);
		SaveManager.Save += () => SaveManager.Sync(nameof(ResetBeforeEveryMove), ref ResetBeforeEveryMove);
		SaveManager.Save += () => SaveManager.Sync(nameof(ExternalEngine.AllowNonCompliantEngines), ref ExternalEngine.AllowNonCompliantEngines);
		SaveManager.Save += () => SaveManager.Sync(nameof(ExternalEngine.StartTimeout), ref ExternalEngine.StartTimeout);
		InvalidationManager.RegisterInvalidatingStaticField(typeof(Engines), nameof(MaxAnalysisTime));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(Engines), nameof(PauseWhenInBackground));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(Engines), nameof(ResetBeforeEveryMove));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(ExternalEngine), nameof(ExternalEngine.AllowNonCompliantEngines));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(ExternalEngine), nameof(ExternalEngine.StartTimeout));
	}
}
