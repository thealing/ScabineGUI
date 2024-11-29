namespace Scabine.Core;

using System.Text;
using static Pieces;
using static Squares;

public struct Move
{
	public const int NormalMove = 0;
	public const int DoubleMove = 1;
	public const int EnPassantMove = 2;
	public const int CastlingMove = 3;
	public const int PromotionMove = 4;

	public int SourceSquare;
	public int TargetSquare;
	public int SourcePiece;
	public int TargetPiece;
	public int Type;

	public readonly bool IsPromotion => Type >= PromotionMove;
	public readonly int PromotionPiece => IsPromotion ? Type - PromotionMove : None;

	public Move(int sourceSquare, int targetSquare, int sourcePiece, int targetPiece, int type)
	{
		SourceSquare = sourceSquare;
		TargetSquare = targetSquare;
		SourcePiece = sourcePiece;
		TargetPiece = targetPiece;
		Type = type;
	}

	public override readonly string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(5);
		stringBuilder.Append(GetSquareFileChar(SourceSquare));
		stringBuilder.Append(GetSquareRankChar(SourceSquare));
		stringBuilder.Append(GetSquareFileChar(TargetSquare));
		stringBuilder.Append(GetSquareRankChar(TargetSquare));
		if (IsPromotion)
		{
			stringBuilder.Append(GetLowerTypeChar(PromotionPiece));
		}
		return stringBuilder.ToString();
	}

	public override readonly bool Equals(object? obj)
	{
		return base.Equals(obj);
	}

	public override readonly int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(Move moveA, Move moveB)
	{
		return moveA.SourceSquare == moveB.SourceSquare && moveA.TargetSquare == moveB.TargetSquare && moveA.Type == moveB.Type;
	}

	public static bool operator !=(Move moveA, Move moveB)
	{
		return !(moveA == moveB);
	}

	public static bool operator !(Move move)
	{
		return move.SourceSquare == move.TargetSquare;
	}
}
