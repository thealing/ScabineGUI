namespace ChessBand.Application;

using System;
using System.Diagnostics;
using System.Windows.Forms;
using ChessBand.Scenes;

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
				MessageBox.Show(exception.ToString(), "Unhandled exception occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
