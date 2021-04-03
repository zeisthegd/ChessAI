using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Back-end của bàn cờ.
public class Board
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


    // Bits 0-3 store white and black kingside/queenside castling legality
	// Bits 4-7 store file of ep square (starting at 1, so 0 = no ep square)
	// Bits 8-13 captured piece
	// Bits 14-... fifty mover counter
    public uint currentGameState;

    public int plyCount;//Tổng ply của ván cờ
    public int fiftyMoveCounter;//Tổng ply kể từ lần cuối cùng 1 quân pawn di chuyển hay bị captured

    public int[] KingSquare;//Vị trí của white&black kings

    public PieceList[] rooks;
    public PieceList[] bishops;
    public PieceList[] queens;
    public PieceList[] knights;
    public PieceList[] pawns;

    PieceList[] allPieceLists;

    const uint whiteCastleKingSideMask = 0b1111111111111110;
    const uint whiteCastleQueenSideMask = 0b1111111111111101;
    const uint blackCastleKingSideMask = 0b1111111111111011;
    const uint blackCastleQueenSideMask = 0b1111111111110111;

    const uint whiteCastleMask = whiteCastleKingSideMask & whiteCastleQueenSideMask;
    const uint blackCastleMask = blackCastleKingSideMask & blackCastleQueenSideMask;


    PieceList GetPieceList(int pieceType, int colorIndex)
    {
        return allPieceLists[colorIndex * 8 + pieceType];
    }

    //Load thế cờ mặc định
    public void LoadStartPosition()
    {
        LoadPosition(FenUtility.startFen);
    }

    //Load vị trí của thế cờ từ 1 đoạn FEN
    public void LoadPosition(string fen)
    {
        Initialized();
        //Load position info từ FEN
        var loadedPositionInfo = FenUtility.PositionFromFen(fen);

        //Lặp qua các ô trên bàn cờ
        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            int piece = loadedPositionInfo.squares[squareIndex];//Code của một quân cờ
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


    //Thực hiện nước đi và update bàn cờ
    public void MakeMove(Move move, bool inSearch = false)
    {
        uint oldEnPassantFile = (currentGameState >> 4) & 15;
        uint originalCastleState = currentGameState & 15;
        uint newCastleState = originalCastleState;
        currentGameState = 0;

        int opponentColorIndex = 1 - ColorToMoveIndex;
        int moveFrom = move.StartSquare;
        int moveTo = move.TargetSquare;


        int capturedPieceType = Piece.PieceType(Squares[moveTo]);
        int movePiece = Squares[moveFrom];
        int movePieceType = Piece.PieceType(movePiece);

        int moveFlag = move.MoveFlag;
        bool isPromotion = move.IsInvalid;
        bool isEnpassant = moveFlag == Move.Flag.EnPassantCapture;


        //handle captures
        currentGameState |= (ushort)(capturedPieceType << 8);
        if (capturedPieceType != 0 && !isEnpassant)
        {
            GetPieceList(capturedPieceType, opponentColorIndex).RemovePieceAtSquare(moveTo);
        }

        //Di chuyển piece trong piece list
        if (movePieceType == Piece.King)
        {
            KingSquare[ColorToMoveIndex] = moveTo;
            newCastleState &= (WhiteToMove) ? whiteCastleMask : blackCastleMask;
        }
        else
        {
            GetPieceList(movePieceType, ColorToMoveIndex).MovePiece(moveFrom, moveTo);
        }

        int pieceOnTargetSquare = movePiece;

        if (isPromotion)
        {
            int promotionType = 0;
            switch (moveFlag)
            {
                case Move.Flag.PromoteToQueen:
                    promotionType = Piece.Queen;
                    queens[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
                case Move.Flag.PromoteToRook:
                    promotionType = Piece.Rook;
                    rooks[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
                case Move.Flag.PromoteToBishop:
                    promotionType = Piece.Bishop;
                    bishops[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
                case Move.Flag.PromoteToKnight:
                    promotionType = Piece.Knight;
                    knights[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
            }
            pieceOnTargetSquare = promotionType | ColorToMove;
            pawns[ColorToMoveIndex].RemovePieceAtSquare(moveTo);
        }
        else
        {
            //Handle other special moves: en passant, castling
            switch (moveFlag)
            {
                case Move.Flag.EnPassantCapture:
                    int epPawnSquare = moveTo + ((ColorToMove == Piece.White) ? -8 : 8);//Ô "phía sau" quân pawn đã đi nước en passant
                    currentGameState |= (ushort)(Squares[epPawnSquare] << 8);
                    Squares[epPawnSquare] = 0;
                    pawns[ColorToMoveIndex].RemovePieceAtSquare(epPawnSquare);

                    break;
                case Move.Flag.Castling:
                    bool kingSide = moveTo == BoardRepresentation.g1 || moveTo == BoardRepresentation.g8;
                    int castlingRookFromIndex = (kingSide) ? moveTo + 1: moveTo - 2;
                    int castlingRookToIndex = (kingSide) ? moveTo - 1 : moveTo + 1;

                    Squares[castlingRookFromIndex] = Piece.None;
                    Squares[castlingRookToIndex] = Piece.Rook | ColorToMove;

                    rooks[ColorToMoveIndex].MovePiece(castlingRookFromIndex,castlingRookToIndex);
                    break;
            }
        }


        Squares[moveTo] = pieceOnTargetSquare;
        Squares[moveFrom] = 0;

        if(moveFlag == Move.Flag.PawnTwoForward)
        {
            int file = BoardRepresentation.FileIndex(moveFrom) + 1;
            currentGameState |= (ushort) (file << 4);
        }

        //Xét quyền castle của 2 bên
        if(originalCastleState != 0)
        {
            if(moveTo == BoardRepresentation.h1 || moveFrom == BoardRepresentation.h1)
            {
                newCastleState &= whiteCastleKingSideMask;
            }
            else if(moveTo == BoardRepresentation.a1 || moveFrom == BoardRepresentation.a1)
            {
                newCastleState &= whiteCastleQueenSideMask;
            }
            if(moveTo == BoardRepresentation.h8 || moveFrom == BoardRepresentation.h8)
            {
                newCastleState &= blackCastleKingSideMask;
            }
            else if(moveTo == BoardRepresentation.a8 || moveFrom == BoardRepresentation.a8)
            {
                newCastleState &= blackCastleQueenSideMask;
            }
        }

        currentGameState |= newCastleState;
        currentGameState |= (uint)fiftyMoveCounter << 14;
        

        //Change side to move
        WhiteToMove = !WhiteToMove;
        ColorToMove = (WhiteToMove) ? Piece.White : Piece.Black;
        OpponentColor = (WhiteToMove) ? Piece.Black : Piece.White;
        ColorToMoveIndex = 1 - ColorToMoveIndex;
        plyCount++;
        fiftyMoveCounter++;
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
