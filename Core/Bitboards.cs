namespace Scabine.Core;

using System.Numerics;

public static class Bitboards
{
	public static ulong GetRankMask(int rank)
	{
		return 0x00000000000000FFUL << rank * 8;
	}

	public static ulong GetFileMask(int file)
	{
		return 0x0101010101010101UL << file;
	}

	public static ulong GetSquareMask(int square)
	{
		return 1UL << square;
	}

	public static bool TestSquare(ulong bitboard, int square)
	{
		return (bitboard & GetSquareMask(square)) != 0;
	}

	public static void SetSquare(ref ulong bitboard, int square)
	{
		bitboard |= GetSquareMask(square);
	}

	public static void ClearSquare(ref ulong bitboard, int square)
	{
		bitboard &= ~GetSquareMask(square);
	}

	public static int PopSquare(ref ulong bitboard)
	{
		int square = GetSquare(bitboard);
		ClearSquare(ref bitboard, square);
		return square;
	}

	public static int GetSquare(ulong bitboard)
	{
		return BitOperations.TrailingZeroCount(bitboard);
	}

	public static int PopCount(ulong bitboard)
	{
		return BitOperations.PopCount(bitboard);
	}
}