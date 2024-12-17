namespace Scabine.App;

internal static class MoveClassifications
{
	public const int Best = 1;
	public const int Great = 2;
	public const int Good = 3;
	public const int Inaccuracy = 4;
	public const int Mistake = 5;
	public const int Blunder = 6;
	public const int MoveClassCount = 7;

	public static int GetClassForWinPercentageLoss(double loss)
	{
		if (loss >= 0.25)
		{
			return Blunder;
		}
		else if (loss >= 0.15)
		{
			return Mistake;
		}
		else if (loss >= 0.08)
		{
			return Inaccuracy;
		}
		else if (loss >= 0.02)
		{
			return Good;
		}
		else
		{
			return Great;
		}
	}
}
