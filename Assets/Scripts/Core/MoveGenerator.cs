using System.Collections.Generic;
using static PrecomputedData;
using static BoardRepresentation;
using System;
using UnityEngine;

public class MoveGenerator
{
    public enum PromotionMode { All, QueenAndKnight, QueenOnly };

    public PromotionMode promotionToGenerate = PromotionMode.All;

    List<Move> moves;
    bool isWhiteToMove;
    int friendlyColor;
    int opponentColor;
    int friendlyKingSquare;
    int friendlyColorIndex;
    int opponentColorIndex;
    bool inCheck;
    bool inDoubleCheck;
    bool pinExistInPosition;
    ulong checkRayBitmask;
    ulong pinRayBitmask;
    ulong opponentKnightAttacks;
    ulong opponentAttackNoPawns;
    public ulong opponentAttackMap;
    public ulong opponentPawnAttackMap;
    ulong opponentSlidingAttackMap;

    bool genQuiets;
    Board board;

    public List<Move> GenerateMoves(Board board, bool includeQuietMoves = true)
    {
        this.board = board;
        genQuiets = includeQuietMoves;
        Init();

        CalculateAttackData();
        GenerateKingMoves();

        if (inDoubleCheck)
        {
            return moves;
        }

        GenerateSlidingPieceMoves();
        GenerateKnightMoves();
        GeneratePawnMoves();
        return moves;
    }

    void CalculateAttackData()
    {

    }

    public bool InCheck
    {
        get { return inCheck; }
    }

    void Init()
    {
        moves = new List<Move>(64);
        inCheck = false;
        inDoubleCheck = false;
        pinExistInPosition = false;
        checkRayBitmask = 0;
        pinRayBitmask = 0;

        isWhiteToMove = board.ColorToMove == Piece.White;
        friendlyColor = board.ColorToMove;
        opponentColor = board.OpponentColor;
        friendlyKingSquare = board.KingSquare[board.ColorToMoveIndex];
        friendlyColorIndex = (board.WhiteToMove) ? Board.WhiteIndex : Board.BlackIndex;
        opponentColorIndex = 1 - friendlyColorIndex;
    }

    void GenerateKingMoves()
    {

    }

    void GeneratePawnMoves()
    {
        PieceList myPawns = board.pawns[friendlyColorIndex];
        int pawnOffset = (friendlyColor == Piece.White) ? 8 : -8;
        int startRank = (board.WhiteToMove) ? 1 : 6;
        int finalRankBeforePromotion = (board.WhiteToMove) ? 6 : 1;

        int enPassantFile = ((int)(board.currentGameState >> 4) % 15) - 1;
        int enPassantSquare = -1;
        if (enPassantFile != -1)
        {
            enPassantSquare = 8 * ((board.WhiteToMove) ? 5 : 2) + enPassantFile;
        }

        for (int i = 0; i < myPawns.Count; i++)
        {
            int startSquare = myPawns[i];
            int rank = RankIndex(startSquare);
            bool oneStepPromotion = rank == finalRankBeforePromotion;

            if (genQuiets)
            {
                int squareOneStepForward = startSquare + pawnOffset;
                if (board.Squares[squareOneStepForward] == Piece.None)
                {
                    moves.Add(new Move(startSquare, squareOneStepForward));
                }
            }


        }
    }

    void GenerateKnightMoves()
    {

    }

    void GenerateSlidingPieceMoves()
    {

    }



}