namespace Scabine.App.Menus;

using Scabine.App.Dialogs;
using Scabine.Scenes;
using System.Windows.Forms;

internal static class EnginesMenu
{
	public static void Init(ToolStripMenuItem menu)
	{
		MenuCreator.AddSubMenuItem(menu, "Manage engines", null, ManageEngines);
		MenuCreator.AddSubMenuItem(menu, "Start engine", null, StartEngine);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Close all engines", null, StopEngines);
	}

	private static void ManageEngines()
	{
		EnginesDialog dialog = new EnginesDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void StartEngine()
	{
		StartEngineDialog dialog = new StartEngineDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void StopEngines()
	{
		EngineManager.StopAllEngines();
	}
}
