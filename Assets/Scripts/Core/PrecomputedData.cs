using System.Collections.Generic;
using static System.Math;

public static class PrecomputedData
{
    //N,S,W,E,NW,SE,NE,SW
    public static readonly int[] directionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };

    public static readonly int[][] numSquaresToEdge;

    public static readonly byte[][] knightMoves;
    //Ex: kingMoves[0][1] == 1: king ở á có thể đi tới a2
    public static readonly byte[][] kingMoves;

    public static readonly byte[][] pawnAttackDirections =
    {
        new byte[] {4, 6},
        new byte[] {7, 5}
    };

    public static readonly int[][] pawnAttacksWhite;//[squareIndex][bitBoard]
    public static readonly int[][] pawnAttacksBlack;//Same
    public static readonly int[] directionLookUp;//Same
    public static readonly ulong[] kingAttackBitBoards;//Ex: kingAttackBitBoards[i] = [64bit]. Những ô mà king tại ô i đang tấn công
    public static readonly ulong[] knightAttackBitBoards;//Ex: knightAttackBitBoards[i] = [64bit]. Những ô mà knight tại ô i đang tấn công

    //Ex: pawnAttackBitBoards[i][Board.ColorIndex] = [64bit]. Những ô mà pawn thộc ColorIndex tại ô i đang tấn công
    public static readonly ulong[][] pawnAttackBitBoards;

    public static readonly ulong[] rookMoves;
    public static readonly ulong[] bishopMoves;
    public static readonly ulong[] queenMoves;

    //Manhattan distance: số ô để một rook cần đi để đến ô b từ ô a.
    public static int[,] orthogonalDistance;

    //Chebyshev distance: số ô để một king cần đi để đến ô b từ ô a.
    public static int[,] kingDistance;
    //Sô ô để king đi từ vị trí hiện tại đến một trong các ô {d4, d5, e4, e5}
    public static int[] centerManhattanDistance;

    public static int NumRookMovesToReachSquare(int startSquare, int targetSquare)
    {
        return orthogonalDistance[startSquare, targetSquare];
    }

    public static int NumKingMovesToReachSquare(int startSquare, int targetSquare)
    {
        return kingDistance[startSquare, targetSquare];
    }

    static PrecomputedData()
    {
        pawnAttacksWhite = new int[64][];
        pawnAttacksBlack = new int[64][];
        numSquaresToEdge = new int[64][];

        knightMoves = new byte[64][];
        kingMoves = new byte[64][];

        rookMoves = new ulong[64];
        bishopMoves = new ulong[64];
        queenMoves = new ulong[64];

        //
        int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };
        knightAttackBitBoards = new ulong[64];
        kingAttackBitBoards = new ulong[64];
        pawnAttackBitBoards = new ulong[64][];

        //Lặp qua từng ô trên bàn cờ
        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            int rank = squareIndex / 8;
            int file = squareIndex - rank * 8;

            int north = 7 - rank;
            int south = rank;
            int west = file;
            int east = 7 - file;

            numSquaresToEdge[squareIndex] = new int[8];
            numSquaresToEdge[squareIndex][0] = north;
            numSquaresToEdge[squareIndex][1] = south;
            numSquaresToEdge[squareIndex][2] = west;
            numSquaresToEdge[squareIndex][3] = east;
            numSquaresToEdge[squareIndex][4] = System.Math.Min(north, west);
            numSquaresToEdge[squareIndex][5] = System.Math.Min(south, east);
            numSquaresToEdge[squareIndex][6] = System.Math.Min(north, east);
            numSquaresToEdge[squareIndex][7] = System.Math.Min(south, west);

            //Tìm tất cả những ô mà knight có thể đi tới từ ô này.
            var legalKnightJumps = new List<byte>();
            ulong knightBitBoard = 0;
            foreach (int knightJumpDelta in allKnightJumps)
            {
                //Những ô knight có thể nhảy tới
                int knightJumpToSquare = squareIndex + knightJumpDelta;
                //Nếu ô này nằm trong bàn cờ
                if (knightJumpToSquare >= 0 && knightJumpToSquare < 64)
                {
                    int knightJumpToSquareRank = knightJumpToSquare / 8;
                    int knightJumpToSquareFile = knightJumpToSquare - knightJumpToSquareRank * 8;


                    int maxCoordMoveDst = System.Math.Max(System.Math.Abs(rank - knightJumpToSquareRank), System.Math.Abs(file - knightJumpToSquareFile));
                    if (maxCoordMoveDst == 2)
                    {
                        legalKnightJumps.Add((byte)knightJumpToSquare);
                        knightBitBoard |= 1ul << knightJumpToSquare;
                    }
                }
            }
            knightMoves[squareIndex] = legalKnightJumps.ToArray();
            knightAttackBitBoards[squareIndex] = knightBitBoard;

            //Tìm tất cả những ô mà king có thể đi tới từ ô này.
            var legalKingMoves = new List<byte>();
            foreach (int kingMoveDelta in directionOffsets)
            {
                int kingMoveToSquare = squareIndex + kingMoveDelta;
                if (kingMoveToSquare >= 0 && kingMoveToSquare < 64)
                {
                    int kingMoveToSquareRank = kingMoveToSquare / 8;
                    int kingMoveToSquareFile = kingMoveToSquare - kingMoveToSquareRank * 8;

                    int maxCoordMoveDst = System.Math.Max(System.Math.Abs(rank - kingMoveToSquareRank), System.Math.Abs(file - kingMoveToSquareFile));
                    if (maxCoordMoveDst == 1)
                    {
                        legalKnightJumps.Add((byte)kingMoveToSquare);
                        kingAttackBitBoards[squareIndex] |= 1ul << kingMoveToSquare;
                    }
                }
            }
            kingMoves[squareIndex] = legalKingMoves.ToArray();

            //Tìm những nước đi hợp lệ cho pawn ở ô này.
            List<int> pawnCapturesWhite = new List<int>();
            List<int> pawnCapturesBlack = new List<int>();
            pawnAttackBitBoards[squareIndex] = new ulong[2];
            if (file > 0)
            {
                if (rank < 7)
                {
                    pawnCapturesWhite.Add(squareIndex + 7);
                    pawnAttackBitBoards[squareIndex][Board.WhiteIndex] |= 1ul << (squareIndex + 7);
                }
                if (rank > 0)
                {
                    pawnCapturesWhite.Add(squareIndex - 9);
                    pawnAttackBitBoards[squareIndex][Board.BlackIndex] |= 1ul << (squareIndex - 9);
                }
            }
            if (file < 7)
            {
                if (rank < 7)
                {
                    pawnCapturesWhite.Add(squareIndex + 7);
                    pawnAttackBitBoards[squareIndex][Board.WhiteIndex] |= 1ul << (squareIndex + 9);
                }
                if (rank > 0)
                {
                    pawnCapturesWhite.Add(squareIndex - 7);
                    pawnAttackBitBoards[squareIndex][Board.BlackIndex] |= 1ul << (squareIndex - 7);
                }
            }
            pawnAttacksWhite[squareIndex] = pawnCapturesWhite.ToArray();
            pawnAttacksBlack[squareIndex] = pawnCapturesBlack.ToArray();

            //Rook
            for (int directionIndex = 0; directionIndex < 4; directionIndex++)
            {
                int currentDirOffset = directionOffsets[directionIndex];
                for (int n = 0; n < numSquaresToEdge[squareIndex][directionIndex]; n++)
                {
                    int targetSquare = squareIndex + currentDirOffset * (n + 1);
                    rookMoves[squareIndex] |= 1ul << targetSquare;
                }
            }

            for (int directionIndex = 4; directionIndex < 8; directionIndex++)
            {
                int currentDirOffset = directionOffsets[directionIndex];
                for (int n = 0; n < numSquaresToEdge[squareIndex][directionIndex]; n++)
                {
                    int targetSquare = squareIndex + currentDirOffset * (n + 1);
                    bishopMoves[squareIndex] |= 1ul << targetSquare;
                }
            }

            queenMoves[squareIndex] = rookMoves[squareIndex] | bishopMoves[squareIndex];

            directionLookUp = new int[127];
            for (int i = 0; i < 127; i++)
            {
                int offset = i - 63;
                int absOffset = System.Math.Abs(offset);
                int absDir = 1;
                if (absOffset % 9 == 0)/////////////////Chưa hiểu
                {
                    absDir = 9;
                }
                else if (absOffset % 8 == 0)
                {
                    absDir = 8;
                }
                else if (absOffset % 7 == 0)
                {
                    absDir = 7;
                }

                directionLookUp[i] = absDir * System.Math.Sign(offset);
            }

            orthogonalDistance = new int[64, 64];
            kingDistance = new int[64, 64];
            centerManhattanDistance = new int[64];
            for (int squareA = 0; squareA < 64; squareA++)
            {
                Coord coordA = BoardRepresentation.CoordFromIndex(squareA);
                int fileDstFromCenter = Max(3 - coordA.fileIndex, coordA.fileIndex - 4);
                int rankDstFromCenter = Max(3 - coordA.rankIndex, coordA.rankIndex - 4);
                centerManhattanDistance[squareA] = fileDstFromCenter + rankDstFromCenter;

                for (int squareB = 0; squareA < 64; squareA++)
                {
                    Coord coordB = BoardRepresentation.CoordFromIndex(squareB);
                    int fileDstFromAToB = Abs(coordA.fileIndex - coordB.fileIndex);
                    int rankDstFromAToB = Abs(coordA.rankIndex - coordB.rankIndex);
                    orthogonalDistance[squareA, squareB] = fileDstFromAToB + rankDstFromAToB;
                    kingDistance[squareA, squareA] = Max(fileDstFromAToB, rankDstFromAToB);
                }
            }

        }
    }

}