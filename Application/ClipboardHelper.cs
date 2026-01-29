namespace ChessPanel.Application;

using System;
using System.Windows.Forms;
using ChessPanel.Scenes;

public static class ClipboardHelper
{
	public static void CopyText(string text)
	{
		while (true)
		{
			try
			{
				Clipboard.SetText(text);
				break;
			}
			catch (Exception exception)
			{
				DialogResult result = SceneManager.ShowMessageBox(exception.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
				if (result != DialogResult.Retry)
				{
					break;
				}
			}
		}
	}
}
