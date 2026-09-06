namespace ChessPanel.Application.Settings;

using ChessPanel.Scenes;

internal static class General
{
	public static string Name = "Player";
	public static bool PlaySounds = true;
	public static bool ConfirmExit = true;
	public static int AutoSaveInterval = 1000;
	public static bool EnableIdleMode = true;
	public static bool EnableClipping = true;

	static General()
	{
		SaveManager.Save += () => SaveManager.Sync(nameof(Name), ref Name);
		SaveManager.Save += () => SaveManager.Sync(nameof(PlaySounds), ref PlaySounds);
		SaveManager.Save += () => SaveManager.Sync(nameof(ConfirmExit), ref ConfirmExit);
		SaveManager.Save += () => SaveManager.Sync(nameof(AutoSaveInterval), ref AutoSaveInterval);
		SaveManager.Save += () => SaveManager.Sync(nameof(EnableIdleMode), ref EnableIdleMode);
		SaveManager.Save += () => SaveManager.Sync(nameof(EnableClipping), ref EnableClipping);
		InvalidationManager.RegisterInvalidatingStaticField(typeof(General), nameof(Name));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(General), nameof(PlaySounds));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(General), nameof(ConfirmExit));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(General), nameof(AutoSaveInterval));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(General), nameof(EnableIdleMode));
		InvalidationManager.RegisterInvalidatingStaticField(typeof(General), nameof(EnableClipping));
	}
}
