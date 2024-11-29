namespace Scabine.App.Dialogs;

using System.Drawing;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using System.Windows.Forms;
using Scabine.Scenes;

internal static class DialogHelper
{
	public static DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		SoundManager.StopAllSounds();
		return SceneManager.ShowMessageBox(text, caption, buttons, icon);
	}

	public static void ShowDialog(Form form)
	{
		SoundManager.StopAllSounds();
		form.ShowDialog();
	}

	public static void ShowCancellableDialog(Form form)
	{
		SoundManager.StopAllSounds();
		form.Show();
		form.Deactivate += (s, e) => form.Close();
		while (form.Visible)
		{
			if (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
			{
				TranslateMessage(ref msg);
				DispatchMessage(ref msg);
			}
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage(ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage(ref MSG lpMsg);

	private struct MSG
	{
		public IntPtr hwnd;
		public uint message;
		public IntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}

	private struct POINT
	{
		public int x;
		public int y;
	}
}
