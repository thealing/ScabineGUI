namespace Scabine.Core;

using System;
using static Game;

public static class Scores
{
	public const int UnknownScore = -30000;
	public const int MinScore = -20000;
	public const int MaxScore = 20000;
	public const int MateScore = 19000;
	public const int DrawScore = 0;

	public static bool IsMateScore(int score)
	{
		return Math.Abs(score) > MateScore - MaxDepth;
	}
}
