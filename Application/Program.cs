namespace ChessPanel.Application;

using System;
using System.Diagnostics;
using System.Windows.Forms;
using ChessPanel.Scenes;

public class Program
{
	[STAThread]
	private static void Main()
	{
		try
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			SceneManager.Run(new MainScene());
		}
		catch (Exception exception)
		{
			if (Debugger.IsAttached)
			{
				throw;
			}
			else
			{
				MessageBox.Show(exception.ToString(), "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
