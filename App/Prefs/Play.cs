namespace Scabine.App.Prefs;

using Scabine.Core;
using System;

internal static class Play
{
	public static bool AutoQueen = false;
	public static int AutoPlayInterval = 500;
	public static Tuple<string, string[]> MoveAnimation;
	public static Tuple<string, string[]> MoveMethod;

	public static bool CanMoveByClicking()
	{
		return MoveMethod.Item1.Contains("Click");
	}

	public static bool CanMoveByDragging()
	{
		return MoveMethod.Item1.Contains("Drag");
	}

	public static bool IsAnimationEnabled()
	{
		return GetAnimationIndex() != 0;
	}

	public static double GetAnimationDuration()
	{
		double[] animationDurations = new double[] { 0, 0.1, 0.2, 0.35, 0.6 };
		return animationDurations[GetAnimationIndex()];
	}

	private static int GetAnimationIndex()
	{
		int animationIndex = Array.IndexOf(MoveAnimation.Item2, MoveAnimation.Item1);
		return animationIndex == -1 ? 0 : animationIndex;
	}

	static Play()
	{
		MoveAnimation = Tuple.Create(_moveAnimations[1], _moveAnimations);
		MoveMethod = Tuple.Create(_moveMethods[0], _moveMethods);
		SaveManager.Save += () => SaveManager.Sync(nameof(AutoQueen), ref AutoQueen);
		SaveManager.Save += () => SaveManager.Sync(nameof(AutoPlayInterval), ref AutoPlayInterval);
		SaveManager.Save += () => SaveManager.Sync(nameof(MoveAnimation), ref MoveAnimation);
		SaveManager.Save += () => SaveManager.Sync(nameof(MoveMethod), ref MoveMethod);
	}

	private static readonly string[] _moveAnimations = new string[] { "None", "Very fast", "Fast", "Medium", "Slow" };
	private static readonly string[] _moveMethods = new string[] { "Click or Drag", "Click squares", "Drag pieces" };
}
