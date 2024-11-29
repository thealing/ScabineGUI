namespace Scabine.Core;

using static Squares;
using static Bitboards;
using static Bitmasks;

public static class Magics
{
	public static ulong GetBishopMoveMask(int square, ulong occupiedMask)
	{
		return _bishopTables[square][GetBishopHash(square, occupiedMask & _bishopMasks[square])];
	}

	public static ulong GetRookMoveMask(int square, ulong occupiedMask)
	{
		return _rookTables[square][GetRookHash(square, occupiedMask & _rookMasks[square])];
	}

	public static ulong GetQueenMoveMask(int square, ulong occupiedMask)
	{
		return GetBishopMoveMask(square, occupiedMask) | GetRookMoveMask(square, occupiedMask);
	}

	private static ulong GetBishopHash(int square, ulong value)
	{
		return value * _bishopMultipliers[square] >> _bishopShifts[square];
	}

	private static ulong GetRookHash(int square, ulong value)
	{
		return value * _rookMultipliers[square] >> _rookShifts[square];
	}

	static Magics()
	{
		_bishopMasks = new ulong[SquareCount];
		_rookMasks = new ulong[SquareCount];
		for (int square = 0; square < SquareCount; square++)
		{
			_bishopMasks[square] = BishopMoveMasks[square] & ~EdgeMask;
			if (TestSquare(EdgeMask, square))
			{
				_rookMasks[square] = RookMoveMasks[square] & ~CornerMask;
				if (GetSquareRank(square) == Rank1 || GetSquareRank(square) == Rank8)
				{
					_rookMasks[square] &= ~GetSquareMask(MirrorRank(square));
				}
				if (GetSquareFile(square) == FileA || GetSquareFile(square) == FileH)
				{
					_rookMasks[square] &= ~GetSquareMask(MirrorFile(square));
				}
			}
			else
			{
				_rookMasks[square] = RookMoveMasks[square] & ~EdgeMask;
			}
		}
		_bishopMultipliers = new ulong[SquareCount]
		{
			0x208A200214110024, 0x0008826802002000, 0x00080E1C00202584, 0x0102208203680004, 0x00220A1020449802, 0x0004884440101100, 0x00110090100A4250, 0x0043004202A04800,
			0x400028900C082042, 0x4082082801040020, 0x1062221204460880, 0x0441082042409000, 0x20802E0211800000, 0x0000221202208000, 0x3902140202026000, 0x0010004118011001,
			0x0140208450040108, 0x0050100212021402, 0x0848001000204113, 0x0805800802004104, 0x0414005E020A0408, 0x2201000201038A00, 0x0900400401441020, 0x0401040344088440,
			0x0020140130108200, 0x0110480C04018400, 0x0080680030004243, 0x0000802108020060, 0x0880840010802000, 0x2001020007024520, 0x0201004011041000, 0x0008420309010100,
			0x0C04044020200220, 0x0012014401200840, 0x000C241000010100, 0x0008200801110050, 0x000801004084004E, 0x0409046101520100, 0x0502080060090400, 0x000088B200008201,
			0x0808085411041C21, 0x00212C1004000200, 0x0102001048040410, 0x0000402011010800, 0x2022A83101400400, 0x0005101001401080, 0x0102040400912400, 0x1004008401400104,
			0x4641081206200810, 0x3101008630060008, 0x24000D1041101820, 0x4400100442020402, 0x10A028C0082A0A02, 0x0208400208224060, 0x2229200404104000, 0x31A014040160C002,
			0x40A2042504100400, 0x0044250048020840, 0x050004014200B000, 0x40400400088C0C00, 0x4430800108210900, 0x1001024008210704, 0x0204A08204280881, 0x0004018202040700,
		};
		_rookMultipliers = new ulong[SquareCount]
		{
			0x4080008010204000, 0x0040014010002000, 0x0480100008802002, 0x0480080030000581, 0x0100028C10080100, 0x0100080A04000500, 0x0080060010800100, 0x4200010204412084,
			0x0104800020804002, 0x000A00220882C900, 0x0001801001802000, 0x02A0801000080182, 0x000A800800800400, 0x000C800200808400, 0x0001006402008100, 0x09F0800040800100,
			0x4008808000204000, 0x1010004001200040, 0x2091010020001148, 0x0800210010030008, 0x0488004004004200, 0x000180800A000400, 0x2000040010010822, 0x0000020010C08401,
			0x1280104440062000, 0x00A5008100214000, 0x0250002020080400, 0x0610890100201000, 0x0409003100060800, 0x0802005A00100804, 0x0422521400500128, 0x001011A200014304,
			0x0B10604000800480, 0x0010006000400940, 0x2010802000801000, 0x0100821800801000, 0x1810050801001100, 0x0401800400802200, 0x0800020104001008, 0x0403000043000092,
			0x0380204008848000, 0x4010002000C84003, 0x002100A000410014, 0x20212A0010420020, 0x0000110008010024, 0x0204440002008080, 0x0822000C08060083, 0x1012042240820001,
			0x0250210142018A00, 0x5200401002200240, 0x0810008020001080, 0x0042281001002100, 0x00208048004C0080, 0x0420060004008080, 0x4030020805100400, 0x1004210044008200,
			0x0309001048208001, 0x1800308440010065, 0x0008200040110319, 0x50110015100008A1, 0x080A00102008040A, 0x044E000450080102, 0x040200C210080104, 0x10480C0D02408222,
		};
		_bishopShifts = new int[SquareCount]
		{
			58, 59, 59, 59, 59, 59, 59, 58,
			59, 59, 59, 59, 59, 59, 59, 59,
			59, 59, 57, 57, 57, 57, 59, 59,
			59, 59, 57, 55, 55, 57, 59, 59,
			59, 59, 57, 55, 55, 57, 59, 59,
			59, 59, 57, 57, 57, 57, 59, 59,
			59, 59, 59, 59, 59, 59, 59, 59,
			58, 59, 59, 59, 59, 59, 59, 58,
		};
		_rookShifts = new int[SquareCount]
		{
			52, 53, 53, 53, 53, 53, 53, 52,
			53, 54, 54, 54, 54, 54, 54, 53,
			53, 54, 54, 54, 54, 54, 54, 53,
			53, 54, 54, 54, 54, 54, 54, 53,
			53, 54, 54, 54, 54, 54, 54, 53,
			53, 54, 54, 54, 54, 54, 54, 53,
			53, 54, 54, 54, 54, 54, 54, 53,
			52, 53, 53, 53, 53, 53, 53, 52,
		};
		_bishopTables = new ulong[SquareCount][];
		for (int sourceRank = 0; sourceRank < RankCount; sourceRank++)
		{
			for (int sourceFile = 0; sourceFile < FileCount; sourceFile++)
			{
				int sourceSquare = MakeSquare(sourceRank, sourceFile);
				_bishopTables[sourceSquare] = new ulong[1 << 64 - _bishopShifts[sourceSquare]];
				ulong maskMask = _bishopMasks[sourceSquare];
				ulong subMask = maskMask + 1;
				do
				{
					subMask = (subMask - 1) & maskMask;
					ulong hash = GetBishopHash(sourceSquare, subMask);
					bool AddSquare(int rank, int file)
					{
						int targetSquare = MakeSquare(rank, file);
						SetSquare(ref _bishopTables[sourceSquare][hash], targetSquare);
						return TestSquare(subMask, targetSquare);
					}
					for (int targetRank = sourceRank + 1, targetFile = sourceFile + 1; targetRank < 8 && targetFile < 8; targetRank++, targetFile++)
					{
						if (AddSquare(targetRank, targetFile))
						{
							break;
						}
					}
					for (int targetRank = sourceRank + 1, targetFile = sourceFile - 1; targetRank < 8 && targetFile >= 0; targetRank++, targetFile--)
					{
						if (AddSquare(targetRank, targetFile))
						{
							break;
						}
					}
					for (int targetRank = sourceRank - 1, targetFile = sourceFile - 1; targetRank >= 0 && targetFile >= 0; targetRank--, targetFile--)
					{
						if (AddSquare(targetRank, targetFile))
						{
							break;
						}
					}
					for (int targetRank = sourceRank - 1, targetFile = sourceFile + 1; targetRank >= 0 && targetFile < 8; targetRank--, targetFile++)
					{
						if (AddSquare(targetRank, targetFile))
						{
							break;
						}
					}
				}
				while (subMask != 0);
			}
		}
		_rookTables = new ulong[SquareCount][];
		for (int sourceRank = 0; sourceRank < RankCount; sourceRank++)
		{
			for (int sourceFile = 0; sourceFile < FileCount; sourceFile++)
			{
				int sourceSquare = MakeSquare(sourceRank, sourceFile);
				_rookTables[sourceSquare] = new ulong[1 << 64 - _rookShifts[sourceSquare]];
				ulong maskMask = _rookMasks[sourceSquare];
				ulong subMask = maskMask + 1;
				do
				{
					subMask = (subMask - 1) & maskMask;
					ulong hash = GetRookHash(sourceSquare, subMask);
					bool AddSquare(int rank, int file)
					{
						int targetSquare = MakeSquare(rank, file);
						SetSquare(ref _rookTables[sourceSquare][hash], targetSquare);
						return TestSquare(subMask, targetSquare);
					}
					for (int targetFile = sourceFile + 1; targetFile < 8; targetFile++)
					{
						if (AddSquare(sourceRank, targetFile))
						{
							break;
						}
					}
					for (int targetFile = sourceFile - 1; targetFile >= 0; targetFile--)
					{
						if (AddSquare(sourceRank, targetFile))
						{
							break;
						}
					}
					for (int targetRank = sourceRank + 1; targetRank < 8; targetRank++)
					{
						if (AddSquare(targetRank, sourceFile))
						{
							break;
						}
					}
					for (int targetRank = sourceRank - 1; targetRank >= 0; targetRank--)
					{
						if (AddSquare(targetRank, sourceFile))
						{
							break;
						}
					}
				}
				while (subMask != 0);
			}
		}
	}

	private static readonly ulong[] _bishopMasks;
	private static readonly ulong[] _rookMasks;
	private static readonly ulong[] _bishopMultipliers;
	private static readonly ulong[] _rookMultipliers;
	private static readonly int[] _bishopShifts;
	private static readonly int[] _rookShifts;
	private static readonly ulong[][] _bishopTables;
	private static readonly ulong[][] _rookTables;
}
