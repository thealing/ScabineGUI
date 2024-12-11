namespace Scabine.App;

using Scabine.App.Prefs;
using Scabine.Core;
using Scabine.Scenes;

internal static class AnimationManager
{
	public static double AnimationDuration { get; private set; }
	public static double AnimationStartTime { get; private set; }
	public static Move? AnimatedMove { get; private set; }
	public static AnimationDirection AnimationDirection { get; private set; }

	public static void AnimateMove(Move? move, AnimationDirection direction)
	{
		AnimationDuration = Play.GetAnimationDuration();
		AnimationStartTime = Time.GetTime();
		AnimatedMove = move;
		AnimationDirection = direction;
	}

	public static bool IsAnimating()
	{
		return Play.IsAnimationEnabled() && Time.GetTime() < AnimationStartTime + AnimationDuration;
	}

	public static double GetAnimationProgress()
	{
		double progress = (Time.GetTime() - AnimationStartTime) / AnimationDuration;
		return EaseInOutQuad(progress);
	}

	private static double EaseInOutQuad(double x)
	{
		return x < 0.5 ? 2 * x * x : -1 + (4 - 2 * x) * x;
	}
}
