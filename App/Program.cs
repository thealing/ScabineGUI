namespace Scabine.App;

using Scabine.Scenes;
using System;
using System.Diagnostics;
using System.Windows.Forms;

public class Program
{
	[STAThread]
	private static void Main()
	{
		try
		{
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
