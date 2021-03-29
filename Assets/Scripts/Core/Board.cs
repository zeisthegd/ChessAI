using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;


    //Lưu trữ piece code của các piece trên bàn cờ
    //piece code:  loại | colorCode
    public int[] Squares;

    public bool WhiteToMove;
    public int ColorToMove;
    public int OpponentColor;
    public int ColorToMoveIndex;


    public uint currentGameState;

    public int plyCount;
    public int fiftyMoveCounter;

    public int[] KingSquare;//Vị trí của white&black kings

    public PieceList[] rooks;
    public PieceList[] bishops;
    public PieceList[] queens;
    public PieceList[] knights;
    public PieceList[] pawns;

    PieceList[] allPieceLists;


    PieceList GetPieceList(int pieceType, int colorIndex)
    {
        return allPieceLists[colorIndex * 8 + pieceType];
    }

    public void LoadStartPosition()
    {
        LoadPosition(FenUtility.startFen);
    }

    public void LoadPosition(string fen)
    {
        Initialized();
        var loadedPositionInfo = FenUtility.PositionFromFen(fen);

        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            int piece = loadedPositionInfo.squares[squareIndex];//code
            Squares[squareIndex] = piece;

            if (piece != Piece.None)
            {
                int pieceType = Piece.PieceType(piece);
                int pieceColorIndex = (Piece.IsColor(piece, Piece.White)) ? WhiteIndex : BlackIndex;
                if (Piece.IsSlidingPiece(piece))
                {
                    if (pieceType == Piece.Queen)
                    {
                        queens[pieceColorIndex].AddPieceAtSquare(squareIndex);
                    }
                    else if (pieceType == Piece.Rook)
                    {
                        rooks[pieceColorIndex].AddPieceAtSquare(squareIndex);
                    }
                    else if (pieceType == Piece.Bishop)
                    {
                        bishops[pieceColorIndex].AddPieceAtSquare(squareIndex);
                    }
                }
                else if (pieceType == Piece.Knight)
                {
                    knights[pieceColorIndex].AddPieceAtSquare(squareIndex);
                }
                else if (pieceType == Piece.Pawn)
                {
                    pawns[pieceColorIndex].AddPieceAtSquare(squareIndex);
                }
                else if (pieceType == Piece.King)
                {
                    KingSquare[pieceColorIndex] = squareIndex;
                }
            }
        }

        //Side to move
        WhiteToMove = loadedPositionInfo.whiteToMove;
        ColorToMove = (WhiteToMove) ? Piece.White : Piece.Black;
        OpponentColor = (WhiteToMove) ? Piece.Black : Piece.White;
        ColorToMoveIndex = (WhiteToMove) ? WhiteIndex : BlackIndex;

    }


    void Initialized()
    {
        Squares = new int[64];
        KingSquare = new int[2];

        knights = new PieceList[] { new PieceList(10), new PieceList(10) };//2+Promotion Possibility
        pawns = new PieceList[] { new PieceList(8), new PieceList(8) };
        rooks = new PieceList[] { new PieceList(10), new PieceList(10) };
        bishops = new PieceList[] { new PieceList(10), new PieceList(10) };
        queens = new PieceList[] { new PieceList(9), new PieceList(9) };
        PieceList emptyList = new PieceList(0);
        allPieceLists = new PieceList[]{
            emptyList,
            emptyList,
            pawns[WhiteIndex],
            knights[WhiteIndex],
            emptyList,
            bishops[WhiteIndex],
            rooks[WhiteIndex],
            queens[WhiteIndex],
            emptyList,
            emptyList,
            pawns[BlackIndex],
            knights[BlackIndex],
            emptyList,
            bishops[BlackIndex],
            rooks[BlackIndex],
            queens[BlackIndex]
        };
    }
}
