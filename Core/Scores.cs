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

	public static int ToMateDistance(int score)
	{
		return MateScore * Math.Sign(score) - score;
	}

	public static int ToMateScore(int distance)
	{
		return Math.Sign(distance) * MateScore - distance;
	}

	public static double ToWinProbability(int score)
	{
		return Math.Clamp(Math.Atan(score * 0.007) * 0.32 + 0.5, 0, 1);
	}
}
