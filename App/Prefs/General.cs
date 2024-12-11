namespace Scabine.App.Prefs;

using System;

internal static class General
{
	public static string Name = "Player";
	public static bool PlaySounds = true;
	public static bool ConfirmExit = true;
	public static int AutoSaveInterval = 1000;

	static General()
	{
		SaveManager.Save += () => SaveManager.Sync(nameof(Name), ref Name);
		SaveManager.Save += () => SaveManager.Sync(nameof(PlaySounds), ref PlaySounds);
		SaveManager.Save += () => SaveManager.Sync(nameof(ConfirmExit), ref ConfirmExit);
		SaveManager.Save += () => SaveManager.Sync(nameof(AutoSaveInterval), ref AutoSaveInterval);
	}
}
