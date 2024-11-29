namespace Scabine.Scenes;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class Time
{
	public static double GetTime()
	{
		return (double)_stopwatch.ElapsedTicks / Stopwatch.Frequency;
	}

	public static void Sleep(double duration)
	{
		if (duration <= 0.0)
		{
			return;
		}
		long dueTime = -(long)(10000000 * duration);
		SetWaitableTimer(_timer, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false);
		WaitForSingleObject(_timer, 0xFFFFFFFF);
	}

	static Time()
	{
		_stopwatch = new Stopwatch();
		_stopwatch.Start();
		_timer = CreateWaitableTimer(IntPtr.Zero, true, "");
	}

	private static readonly Stopwatch _stopwatch;
	private static readonly IntPtr _timer;

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
}
