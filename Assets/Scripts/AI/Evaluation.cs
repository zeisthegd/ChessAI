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

        whiteEval += whiteMaterial;
        blackEval += blackMaterial;

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

}
