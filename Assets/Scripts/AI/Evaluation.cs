using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evaluation
{
    public const int pawnValue = 100;
    public const int knightValue = 300;
    public const int bishopValue = 320;
    public const int rookValue = 500;
    public const int queenValue = 900;

    const float endGameMaterialStart = rookValue * 2 + bishopValue + knightValue;
    Board board;

    public int Evalutate(Board board)
    {
        this.board = board;
        int whiteEval = 0;
        int blackEval = 0;

        int whiteMaterial = CountMaterial(Board.WhiteIndex);
        int blackMaterial = CountMaterial(Board.BlackIndex);

        int whiteMaterialWithoutPawns = whiteEval - board.pawns[Board.WhiteIndex].Count * pawnValue;
        int blackMaterialWithoutPawns = blackEval - board.pawns[Board.BlackIndex].Count * pawnValue;
        float whiteEndgamePhaseWeight = EndGamePhaseWeight(whiteMaterialWithoutPawns);
        float blackEndgamePhaseWeight = EndGamePhaseWeight(blackMaterialWithoutPawns);


        whiteEval += whiteMaterial;
        blackEval += blackMaterial;
        whiteEval += MopUpEval(Board.WhiteIndex,Board.BlackIndex,whiteMaterial,blackMaterial,blackEndgamePhaseWeight);
        blackEval += MopUpEval(Board.BlackIndex,Board.WhiteIndex,blackMaterial,whiteMaterial,whiteEndgamePhaseWeight);

        whiteEval += EvaluatePieceSquareTables(Board.WhiteIndex, blackEndgamePhaseWeight);
        blackEval += EvaluatePieceSquareTables(Board.BlackIndex, whiteEndgamePhaseWeight);


        int eval = whiteEval - blackEval;
        int perspective = (board.WhiteToMove) ? 1 : -1;
        return eval * perspective;
    }


    int CountMaterial(int colorIndex)
    {
        int material = 0;
        material += board.pawns[colorIndex].Count * pawnValue;
        material += board.bishops[colorIndex].Count * bishopValue;
        material += board.rooks[colorIndex].Count * rookValue;
        material += board.knights[colorIndex].Count * knightValue;
        material += board.queens[colorIndex].Count * queenValue;

        return material;
    }

    float EndGamePhaseWeight(int materialCountWithoutPawns)
    {
        const float multiplier = 1 / endGameMaterialStart;
        return 1 - System.Math.Min(1, materialCountWithoutPawns * multiplier);
    }

    int MopUpEval(int friendlyIndex, int opponentIndex, int myMaterial,int opponentMaterial, float endGameWeight)
    {
        int mopUpScore = 0;
        if(myMaterial > opponentMaterial + pawnValue * 2 && endGameWeight > 0)
        {
            int friendlyKingSquare = board.KingSquare[friendlyIndex];
            int oppoenntKingSquare = board.KingSquare[opponentIndex];
            mopUpScore += PrecomputedData.centerManhattanDistance[oppoenntKingSquare] * 10;
            mopUpScore += (14 - PrecomputedData.NumRookMovesToReachSquare(friendlyKingSquare,oppoenntKingSquare)) * 4;
            
            return (int)(mopUpScore * endGameWeight);
        }
        return 0;
    }

    int EvaluatePieceSquareTables(int colorIndex, float endGamePhaseWeight)
    {
        int value = 0;
        bool isWhite = colorIndex == Board.WhiteIndex;
        value += EvalutatePieceSquareTable(PieceSquareTable.pawns,board.pawns[colorIndex],isWhite);
        value += EvalutatePieceSquareTable(PieceSquareTable.knights,board.knights[colorIndex],isWhite);
        value += EvalutatePieceSquareTable(PieceSquareTable.bishops,board.bishops[colorIndex],isWhite);
        value += EvalutatePieceSquareTable(PieceSquareTable.rooks,board.rooks[colorIndex],isWhite);
        value += EvalutatePieceSquareTable(PieceSquareTable.queens,board.queens[colorIndex],isWhite);
        int kingEarlyPhase = PieceSquareTable.Read(PieceSquareTable.kingMiddle,board.KingSquare[colorIndex],isWhite);
        value += (int)(kingEarlyPhase * (1 - endGamePhaseWeight));
        return value;
    }

    static int EvalutatePieceSquareTable(int[] table, PieceList pieceList, bool isWhite)
    {
        int value = 0;
        for (int i = 0; i < pieceList.Count; i++)
        {
            value += PieceSquareTable.Read(table, pieceList[i], isWhite);
        }
        return value;
    }


}
