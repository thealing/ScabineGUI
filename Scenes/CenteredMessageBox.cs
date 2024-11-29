namespace Scabine.Scenes;

using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System;

internal static class CenteredMessageBox
{
	public static DialogResult Show(IWin32Window owner, string text)
	{
		_owner = owner;
		Initialize();
		return MessageBox.Show(owner, text);
	}

	public static DialogResult Show(IWin32Window owner, string text, string caption)
	{
		_owner = owner;
		Initialize();
		return MessageBox.Show(owner, text, caption);
	}

	public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
	{
		_owner = owner;
		Initialize();
		return MessageBox.Show(owner, text, caption, buttons);
	}

	public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		_owner = owner;
		Initialize();
		return MessageBox.Show(owner, text, caption, buttons, icon);
	}

	public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
	{
		_owner = owner;
		Initialize();
		return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
	}

	public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
	{
		_owner = owner;
		Initialize();
		return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
	}

	private static void Initialize()
	{
		_hook = SetWindowsHookEx(WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero, GetCurrentThreadId());
	}

	private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
	{
		CWPRETSTRUCT? msg = (CWPRETSTRUCT?)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
		if (nCode < 0 || _owner == null || msg == null)
		{
			return CallNextHookEx(_hook, nCode, wParam, lParam);
		}
		IntPtr hook = _hook;
		if (msg.Value.message == (int)CbtHookAction.HCBT_ACTIVATE)
		{
			try
			{
				CenterWindow(_owner.Handle, msg.Value.hwnd);
			}
			finally
			{
				UnhookWindowsHookEx(_hook);
				_hook = IntPtr.Zero;
			}
		}
		return CallNextHookEx(hook, nCode, wParam, lParam);
	}

	private static void CenterWindow(IntPtr parent, IntPtr child)
	{
		GetWindowRect(parent, out RECT parentRect);
		GetWindowRect(child, out RECT childRect);
		Rectangle parentRectangle = parentRect.ToRectangle();
		Rectangle childRectangle = childRect.ToRectangle();
		int newX = parentRectangle.Left + (parentRectangle.Width - childRectangle.Width) / 2;
		int newY = parentRectangle.Top + (parentRectangle.Height - childRectangle.Height) / 2;
		MoveWindow(child, newX, newY, childRectangle.Width, childRectangle.Height, true);
	}

	static CenteredMessageBox()
	{
		_hookProc = new HookProc(MessageBoxHookProc);
		_hook = IntPtr.Zero;
	}

	private static IWin32Window? _owner;
	private static HookProc _hookProc;
	private static IntPtr _hook;

	private const int WH_CALLWNDPROCRET = 12;

	private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

	private delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIDEvent, uint dwTime);

	private enum CbtHookAction : int
	{
		HCBT_MOVESIZE = 0,
		HCBT_MINMAX = 1,
		HCBT_QS = 2,
		HCBT_CREATEWND = 3,
		HCBT_DESTROYWND = 4,
		HCBT_ACTIVATE = 5,
		HCBT_CLICKSKIPPED = 6,
		HCBT_KEYSKIPPED = 7,
		HCBT_SYSCOMMAND = 8,
		HCBT_SETFOCUS = 9
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct CWPRETSTRUCT
	{
		public IntPtr lResult;
		public IntPtr lParam;
		public IntPtr wParam;
		public uint message;
		public IntPtr hwnd;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;

		public Rectangle ToRectangle()
		{
			return new Rectangle(left, top, right - left, bottom - top);
		}
	}

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, uint threadId);

	[DllImport("user32.dll")]
	private static extern int UnhookWindowsHookEx(IntPtr idHook);

	[DllImport("user32.dll")]
	private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);
}
