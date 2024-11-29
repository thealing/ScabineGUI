namespace Scabine.Core;

using System;
using static Bitboards;
using static Squares;

public static class Bitmasks
{
	public static readonly ulong EdgeMask;
	public static readonly ulong CornerMask;

	public static readonly ulong[] KnightMoveMasks;
	public static readonly ulong[] BishopMoveMasks;
	public static readonly ulong[] RookMoveMasks;
	public static readonly ulong[] QueenMoveMasks;
	public static readonly ulong[] KingMoveMasks;

	static Bitmasks()
	{
		EdgeMask = GetRankMask(Rank1) | GetRankMask(Rank8) | GetFileMask(FileA) | GetFileMask(FileH);
		CornerMask = GetSquareMask(A1) | GetSquareMask(H1) | GetSquareMask(A8) | GetSquareMask(H8);
		KnightMoveMasks = new ulong[SquareCount];
		BishopMoveMasks = new ulong[SquareCount];
		RookMoveMasks = new ulong[SquareCount];
		QueenMoveMasks = new ulong[SquareCount];
		KingMoveMasks = new ulong[SquareCount];
		for (int sourceRank = 0; sourceRank < RankCount; sourceRank++)
		{
			for (int sourceFile = 0; sourceFile < FileCount; sourceFile++)
			{
				for (int targetRank = 0; targetRank < RankCount; targetRank++)
				{
					for (int targetFile = 0; targetFile < FileCount; targetFile++)
					{
						if (targetRank == sourceRank && targetFile == sourceFile)
						{
							continue;
						}
						int sourceSquare = MakeSquare(sourceRank, sourceFile);
						int targetSquare = MakeSquare(targetRank, targetFile);
						int rankDifference = Math.Abs(sourceRank - targetRank);
						int fileDifference = Math.Abs(sourceFile - targetFile);
						if (rankDifference == 1 && fileDifference == 2 || rankDifference == 2 && fileDifference == 1)
						{
							SetSquare(ref KnightMoveMasks[sourceSquare], targetSquare);
						}
						if (rankDifference == fileDifference)
						{
							SetSquare(ref BishopMoveMasks[sourceSquare], targetSquare);
						}
						if (rankDifference == 0 || fileDifference == 0)
						{
							SetSquare(ref RookMoveMasks[sourceSquare], targetSquare);
						}
						if (rankDifference == fileDifference || rankDifference == 0 || fileDifference == 0)
						{
							SetSquare(ref QueenMoveMasks[sourceSquare], targetSquare);
						}
						if (rankDifference <= 1 && fileDifference <= 1)
						{
							SetSquare(ref KingMoveMasks[sourceSquare], targetSquare);
						}
					}
				}
			}
		}
	}
}