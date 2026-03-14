namespace ChessBand.Application.Menus;

using System.Windows.Forms;
using ChessBand.Application.Dialogs;
using ChessBand.Scenes;

internal static class EnginesMenu
{
	public static void Init(ToolStripMenuItem menu)
	{
		MenuCreator.AddSubMenuItem(menu, "Manage engines", null, ManageEngines);
		MenuCreator.AddSubMenuItem(menu, "Start engine", null, StartEngine);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Close all engines", null, StopEngines);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Run game analysis", null, RunGameAnalysis);
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

	private static void RunGameAnalysis()
	{
		AnalyzisDialog dialog = new AnalyzisDialog();
		DialogHelper.ShowDialog(dialog);
		if (dialog.Engine != null)
		{
			AnalyzingDialog analyzingDialog = new AnalyzingDialog(dialog.Engine, dialog.Depth);
			DialogHelper.ShowDialog(analyzingDialog);
			dialog.Engine.Dispose();
		}
	}

	private static void StopEngines()
	{
		EngineManager.StopAllEngines();
	}
}
