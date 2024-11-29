namespace Scabine.Core;

using static Pieces;

public static class Squares
{
	public const int Rank8 = 0;
	public const int Rank7 = 1;
	public const int Rank6 = 2;
	public const int Rank5 = 3;
	public const int Rank4 = 4;
	public const int Rank3 = 5;
	public const int Rank2 = 6;
	public const int Rank1 = 7;

	public const int FileA = 0;
	public const int FileB = 1;
	public const int FileC = 2;
	public const int FileD = 3;
	public const int FileE = 4;
	public const int FileF = 5;
	public const int FileG = 6;
	public const int FileH = 7;

	public const int A8 = 0, B8 = 1, C8 = 2, D8 = 3, E8 = 4, F8 = 5, G8 = 6, H8 = 7;
	public const int A7 = 8, B7 = 9, C7 = 10, D7 = 11, E7 = 12, F7 = 13, G7 = 14, H7 = 15;
	public const int A6 = 16, B6 = 17, C6 = 18, D6 = 19, E6 = 20, F6 = 21, G6 = 22, H6 = 23;
	public const int A5 = 24, B5 = 25, C5 = 26, D5 = 27, E5 = 28, F5 = 29, G5 = 30, H5 = 31;
	public const int A4 = 32, B4 = 33, C4 = 34, D4 = 35, E4 = 36, F4 = 37, G4 = 38, H4 = 39;
	public const int A3 = 40, B3 = 41, C3 = 42, D3 = 43, E3 = 44, F3 = 45, G3 = 46, H3 = 47;
	public const int A2 = 48, B2 = 49, C2 = 50, D2 = 51, E2 = 52, F2 = 53, G2 = 54, H2 = 55;
	public const int A1 = 56, B1 = 57, C1 = 58, D1 = 59, E1 = 60, F1 = 61, G1 = 62, H1 = 63;

	public const int NoSquare = -1;

	public const int RankCount = 8;
	public const int FileCount = 8;
	public const int SquareCount = 64;

	public const int Up = -8;
	public const int Down = 8;
	public const int Left = -1;
	public const int Right = 1;

	public static int MakeSquare(int rank, int file)
	{
		return rank * 8 + file;
	}

	public static int GetSquareRank(int square)
	{
		return square / 8;
	}

	public static int GetSquareFile(int square)
	{
		return square % 8;
	}

	public static int MirrorSquare(int square)
	{
		return square ^ 63;
	}

	public static int MirrorRank(int square)
	{
		return square ^ 56;
	}

	public static int MirrorFile(int square)
	{
		return square ^ 7;
	}

	public static int GetMoveDirection(int color)
	{
		return color == White ? Up : Down;
	}

	public static int GetPromotionRank(int color)
	{
		return color == White ? Rank8 : Rank1;
	}

	public static char GetRankChar(int rank)
	{
		return (char)('8' - rank);
	}

	public static char GetFileChar(int file)
	{
		return (char)('a' + file);
	}

	public static char GetSquareRankChar(int square)
	{
		return GetRankChar(GetSquareRank(square));
	}

	public static char GetSquareFileChar(int square)
	{
		return GetFileChar(GetSquareFile(square));
	}
}
