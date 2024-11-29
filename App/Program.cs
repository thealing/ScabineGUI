namespace Scabine.App;

using Scabine.Scenes;
using System;

public class Program
{
	[STAThread]
	private static void Main()
	{
		SceneManager.Run(new MainScene());
	}
}
