namespace Scabine.App;

using System;
using System.Drawing;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AnalyzisResult
{
	public double Accuracy;
	public short Best;
	public short Great;
	public short Good;
	public short Inaccuracy;
	public short Mistake;
	public short Blunder;

	public static string EncodeToBase64(AnalyzisResult result)
	{
		int size = Marshal.SizeOf(result);
		byte[] bytes = new byte[size];
		IntPtr ptr = Marshal.AllocHGlobal(size);
		try
		{
			Marshal.StructureToPtr(result, ptr, true);
			Marshal.Copy(ptr, bytes, 0, size);
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}
		return Convert.ToBase64String(bytes);
	}

	public static AnalyzisResult DecodeFromBase64(string base64)
	{
		int size = Marshal.SizeOf(typeof(AnalyzisResult));
		IntPtr ptr = Marshal.AllocHGlobal(size);
		try
		{
			byte[] bytes = Convert.FromBase64String(base64);
			Marshal.Copy(bytes, 0, ptr, size);
			return (AnalyzisResult?)Marshal.PtrToStructure(ptr, typeof(AnalyzisResult)) ?? default;
		}
		catch
		{
			return default;
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}
	}
}
