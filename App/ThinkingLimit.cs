namespace Scabine.App;

using System;

internal class ThinkingLimit
{
	public ThinkingMode Mode;
	public decimal BaseTime;
	public decimal Increment;
	public decimal MoveTime;
	public int Depth;
	public int Nodes;

	public ThinkingLimit()
	{
		Mode = ThinkingMode.GameTime;
		BaseTime = 20m;
		Increment = 1m;
		MoveTime = 0.5m;
		Depth = 6;
		Nodes = 10000;
	}

	public void Copy(ThinkingLimit other)
	{
		Mode = other.Mode;
		BaseTime = other.BaseTime;
		Increment = other.Increment;
		MoveTime = other.MoveTime;
		Depth = other.Depth;
		Nodes = other.Nodes;
	}

	public override string ToString()
	{
		switch (Mode)
		{
			case ThinkingMode.GameTime:
				if (BaseTime % 60 == 0)
				{
					return $"{BaseTime / 60}+{Increment}";
				}
				else
				{
					return $"{Math.Truncate(BaseTime / 60)}:{BaseTime % 60:00}+{Increment}";
				}
			case ThinkingMode.MoveTime:
				return $"{MoveTime}s per move";
			case ThinkingMode.FixedDepth:
				return $"Depth {Depth}";
			case ThinkingMode.FixedNodes:
				return $"{Nodes} nodes";
			default:
				return "";
		}
	}
}
