namespace Scabine.App.Menus;

using Scabine.App.Dialogs;
using Scabine.Scenes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

internal static class GameMenu
{
	public static void Init(ToolStripMenuItem menu)
	{
		MenuCreator.AddSubMenuItem(menu, "Play against engine", null, PlayVsEngine);
		MenuCreator.AddSubMenuItem(menu, "Start engine match", null, EngineVsEngine);
		MenuCreator.AddSubMenuSeparator(menu);
		_cancelItem = MenuCreator.AddSubMenuItem(menu, "Cancel game", null, CancelGame);
		_pauseItem = MenuCreator.AddSubMenuItem(menu, "Pause game", null, TogglePausedState);
		_restartItem = MenuCreator.AddSubMenuItem(menu, "Restart game", null, RestartGame, Keys.Control | Keys.A);
		_rematchItem = MenuCreator.AddSubMenuItem(menu, "Start rematch", null, StartRematch, Keys.Control | Keys.R);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Edit game data", null, EditGameData);
		MenuCreator.AddSubMenuItem(menu, "Edit game PGN", null, EditGamePgn);
		MenuCreator.AddSubMenuItem(menu, "Copy game PGN", null, CopyGamePgn);
		MenuCreator.AddSubMenuSeparator(menu);
		MenuCreator.AddSubMenuItem(menu, "Copy all moves", null, CopyAllMoves);
		MenuCreator.AddSubMenuItem(menu, "Copy main line", null, CopyMainLine);
		MenuCreator.AddSubMenuItem(menu, "Copy current line", null, CopyCurrentLine);
	}

	public static void Update()
	{
		UpdateGameState();
		UpdatePausedState();
	}

	public static void UpdateGameState()
	{
		bool playing = MatchManager.IsPlaying();
		_cancelItem.Enabled = playing;
		_pauseItem.Enabled = playing;
		bool hasMatch = MatchManager.HasMatch();
		_restartItem.Enabled = hasMatch;
		_rematchItem.Enabled = hasMatch;
	}

	public static void UpdatePausedState()
	{
		_pauseItem.Text = MatchManager.IsPaused() ? "Resume game" : "Pause game";
	}

	private static void PlayVsEngine()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		PlayEngineDialog dialog = new PlayEngineDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void EngineVsEngine()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		EngineMatchDialog dialog = new EngineMatchDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void CancelGame()
	{
		MatchManager.ResetMatch();
	}

	private static void TogglePausedState()
	{
		MatchManager.SetPaused(!MatchManager.IsPaused());
	}

	private static void RestartGame()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		MatchManager.RestartMatch();
	}

	private static void StartRematch()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		MatchManager.StartRematch();
	}

	private static void EditGameData()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		PgnHeaderDialog dialog = new PgnHeaderDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void EditGamePgn()
	{
		if (!PgnManager.SaveBackup())
		{
			return;
		}
		PgnDialog dialog = new PgnDialog();
		DialogHelper.ShowDialog(dialog);
	}

	private static void CopyGamePgn()
	{
		Clipboard.SetText(PgnManager.GetPgn());
	}

	private static void CopyAllMoves()
	{
		StringBuilder stringBuilder = new StringBuilder();
		PgnManager.FormatMoves(stringBuilder, false);
		Clipboard.SetText(stringBuilder.ToString());
	}

	private static void CopyMainLine()
	{
		StringBuilder stringBuilder = new StringBuilder();
		PgnManager.FormatLine(stringBuilder, GameManager.GetGame().GetLastNode(), false);
		Clipboard.SetText(stringBuilder.ToString());
	}

	private static void CopyCurrentLine()
	{
		StringBuilder stringBuilder = new StringBuilder();
		PgnManager.FormatLine(stringBuilder, GameManager.GetGame().GetCurrentNode(), false);
		Clipboard.SetText(stringBuilder.ToString());
	}

	private static ToolStripMenuItem _cancelItem = new ToolStripMenuItem();
	private static ToolStripMenuItem _pauseItem = new ToolStripMenuItem();
	private static ToolStripMenuItem _restartItem = new ToolStripMenuItem();
	private static ToolStripMenuItem _rematchItem = new ToolStripMenuItem();
}
