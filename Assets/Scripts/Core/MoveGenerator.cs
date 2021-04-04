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
    int friendlyKingSquare;//ô hiện tại mà vua đồng minh đang chiếm
    int friendlyColorIndex;
    int opponentColorIndex;
    bool inCheck;
    bool inDoubleCheck;
    bool pinExistInPosition;
    ulong checkRayBitmask;
    ulong pinRayBitmask;
    ulong opponentKnightAttacks;//Các ô mà knight của đối thủ có thể tấn công
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

        GenerateSlidingMoves();
        GenerateKnightMoves();
        GeneratePawnMoves();
        return moves;
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
        for (int i = 0; i < kingMoves[friendlyKingSquare].Length; i++)
        {
            int targetSquare = kingMoves[friendlyKingSquare][i];
            int pieceOnTargetSquare = board.Squares[targetSquare];
            //Skip những ô friendly
            if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
            {
                continue;
            }

            bool isCapture = Piece.IsColor(pieceOnTargetSquare, opponentColor);
            if (!isCapture)
            {
                if (!genQuiets || SquareIsInCheckRay(targetSquare))
                {
                    continue;
                }
            }

            if (!SquareIsAttacked(targetSquare))
            {

                moves.Add(new Move(friendlyKingSquare, targetSquare));

                //Castling
                if (!inCheck && !isCapture)
                {
                    //Castle kingside
                    if ((targetSquare == f1 || targetSquare == f8) && HasKingSideCastleRight)
                    {
                        int castleKingSideSquare = targetSquare + 1;
                        if (board.Squares[castleKingSideSquare] == Piece.None)
                        {
                            if (!SquareIsAttacked(castleKingSideSquare))
                            {
                                moves.Add(new Move(friendlyKingSquare, castleKingSideSquare, Move.Flag.Castling));
                            }
                        }
                    }

                    else if ((targetSquare == d1 || targetSquare == d8) && HasQueenSideCastleRight)
                    {
                        int castleQueenSideSquare = targetSquare - 1;
                        if (board.Squares[castleQueenSideSquare] == Piece.None)
                        {
                            if (!SquareIsAttacked(castleQueenSideSquare))
                            {
                                moves.Add(new Move(friendlyKingSquare, castleQueenSideSquare, Move.Flag.Castling));
                            }
                        }
                    }
                }
            }
        }
    }

    void GeneratePawnMoves()
    {
        PieceList myPawns = board.pawns[friendlyColorIndex];
        //Offset để tính nước đi tới 1 ô của pawn
        int pawnOffset = (friendlyColor == Piece.White) ? 8 : -8;
        //Rank bắt đầu của 1 trong 2 quân trắng/đen
        int startRank = (board.WhiteToMove) ? 1 : 6;
        //Rank trước rank promotion
        int finalRankBeforePromotion = (board.WhiteToMove) ? 6 : 1;

        //File của ô có thể bị en passant
        int enPassantFile = ((int)(board.currentGameState >> 4) & 15) - 1;
        int enPassantSquare = -1;
        //Debug.Log("EP File: "+enPassantFile);
        if (enPassantFile != -1)
        {
            //Index của pawn sau khi đi nước en passant
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
                //Nếu ô phía trước không có quân nào
                if (board.Squares[squareOneStepForward] == Piece.None)
                {
                    //Nếu pawn không bị pin hoặc đang di chuyển theo huong
                    if (!IsPinned(startSquare) || IsMovingAlongRay(pawnOffset, startSquare, friendlyKingSquare))
                    {
                        if (!inCheck || SquareIsInCheckRay(squareOneStepForward))
                        {
                            if (oneStepPromotion)//Nếu là nước promotion
                            {
                                MakePromotionMoves(startRank, squareOneStepForward);
                            }
                            else
                            {
                                moves.Add(new Move(startSquare, squareOneStepForward));
                            }
                        }

                        //Rank hiện tại là rank bắt đầu nên có thể tiến tới 2 ô nếu không bị ngăn cản
                        if (rank == startRank)
                        {
                            int squareTwoForward = squareOneStepForward + pawnOffset;
                            if (board.Squares[squareTwoForward] == Piece.None)
                            {
                                if (!inCheck || SquareIsInCheckRay(squareTwoForward))
                                {
                                    moves.Add(new Move(startSquare, squareTwoForward, Move.Flag.PawnTwoForward));
                                }
                            }
                        }
                    }
                }
            }

            //Captures
            for (int j = 0; j < 2; j++)
            {
                //Nếu hướng chéo của pawn là ô thuộc bàn cờ
                if (numSquaresToEdge[startSquare][pawnAttackDirections[friendlyColorIndex][j]] > 0)
                {

                    int pawnCaptureDir = directionOffsets[pawnAttackDirections[friendlyColorIndex][j]];
                    int targetSquare = startSquare + pawnCaptureDir;
                    int targetPiece = board.Squares[targetSquare];

                    if (IsPinned(startSquare) && !IsMovingAlongRay(pawnCaptureDir, friendlyKingSquare, startSquare))
                    {
                        continue;
                    }

                    //Regular capture
                    if (Piece.IsColor(targetPiece, opponentColor))
                    {
                        if (inCheck && !SquareIsInCheckRay(targetSquare))
                        {
                            continue;
                        }
                        if (oneStepPromotion)
                        {
                            MakePromotionMoves(startSquare, targetSquare);

                        }
                        else
                        {

                            moves.Add(new Move(startSquare, targetSquare));
                        }
                    }

                    //Debug.Log("En passant: " + enPassantSquare + "Target: " + targetSquare);
                    //En passant capture
                    if (targetSquare == enPassantSquare)
                    {

                        int epCapturedPawnSquare = targetSquare + ((board.WhiteToMove) ? -8 : 8);
                        if (!InCheckAfterPassant(startSquare, targetSquare, epCapturedPawnSquare))
                        {
                            moves.Add(new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture));
                        }
                    }

                }
            }


        }
    }

    void GenerateKnightMoves()
    {
        //Danh sách các ô mà knight của đồng minh đang chiếm giữ
        PieceList myKnights = board.knights[friendlyColorIndex];
        //Lặp danh sách
        for (int i = 0; i < myKnights.Count; i++)
        {
            int startSquare = myKnights[i];
            //Nếu knight ở ô này đang bị pin thì nó không được di chuyển
            if (IsPinned(startSquare))
                continue;
            //Xét lần lượt các ô mà 1 knight có thể tấn công.
            for (int knightMoveIndex = 0; knightMoveIndex < knightMoves[startSquare].Length; knightMoveIndex++)
            {
                int targetSquare = knightMoves[startSquare][knightMoveIndex];
                int targetSquarePiece = board.Squares[targetSquare];
                bool isCapture = Piece.IsColor(targetSquarePiece, opponentColor);
                if (genQuiets || isCapture)
                {
                    if (Piece.IsColor(targetSquarePiece, friendlyColor) || (inCheck && !SquareIsInCheckRay(targetSquare)))
                    {
                        continue;
                    }
                    moves.Add(new Move(startSquare, targetSquare));
                }
            }
        }
    }

    void GenerateSlidingMoves()
    {
        PieceList rookMoves = board.rooks[friendlyColorIndex];
        for (int i = 0; i < rookMoves.Count; i++)
        {
            GenerateSlidingPieceMoves(rookMoves[i], 0 ,4);
        }
        PieceList bishopMoves = board.bishops[friendlyColorIndex];
        for (int i = 0; i < bishopMoves.Count; i++)
        {
            GenerateSlidingPieceMoves(bishopMoves[i], 4 ,8);
        }
        PieceList queenMoves = board.queens[friendlyColorIndex];
        for (int i = 0; i < queenMoves.Count; i++)
        {
            GenerateSlidingPieceMoves(queenMoves[i], 0 ,8);
        }

    }
    void GenerateSlidingPieceMoves(int startSquare, int startDirIndex, int endDirIndex)
    {
        bool isPinned = IsPinned(startSquare);

        //Nếu quân này đang bị pinned và king đang bị check thì quân này không thể di chuyển
        if (inCheck && isPinned)
        {
            return;
        }

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            int currentDirOffset = directionOffsets[directionIndex];

            if (isPinned && !IsMovingAlongRay(currentDirOffset, friendlyKingSquare, startSquare))
            {
                continue;
            }

            for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + currentDirOffset * (n + 1);
                int targetSquarePiece = board.Squares[targetSquare];

                //Nếu bị chặn bởi quân đồng minh
                if (Piece.IsColor(targetSquarePiece, friendlyColor))
                {
                    break;
                }
                bool isCapture = targetSquarePiece != Piece.None;

                bool movePreventsCheck = SquareIsInCheckRay(targetSquare);
                if (movePreventsCheck || !inCheck)
                {
                    if (genQuiets || isCapture)
                    {
                        moves.Add(new Move(startSquare, targetSquare));
                    }
                }

                //Nếu square target có quân nào đó thì không thể đi tiếp
                //Hoặc nếu nước đi này ngăn chặn một nước check thì những nước sau nó không thể chặn check
                if (isCapture || movePreventsCheck)
                    break;

            }

        }


    }

    void CalculateAttackData()
    {
        GenerateSlidingAttackMap();


        //Kiểm tra king có đang bị check/pin bởi sliding pieces của đổi thủ không.
        int startDirIndex = 0;
        int endDirIndex = 8;


        //Xác định những hướng sẽ kiểm tra
        if (board.queens[opponentColorIndex].Count == 0)
        {
            startDirIndex = (board.rooks[opponentColorIndex].Count > 0) ? 0 : 4;
            endDirIndex = (board.bishops[opponentColorIndex].Count > 0) ? 8 : 4;
        }

        for (int dir = startDirIndex; dir < endDirIndex; dir++)
        {
            bool isDiagonal = dir > 3;

            int n = numSquaresToEdge[friendlyKingSquare][dir];
            int directionOffset = directionOffsets[dir];
            bool isFriendlyPieceAlongRay = false;
            ulong rayMask = 0;

            for (int i = 0; i < n; i++)
            {
                int squareIndex = friendlyKingSquare + directionOffset * (i + 1);
                rayMask |= 1ul << squareIndex;
                int piece = board.Squares[squareIndex];

                //Nếu có quân cờ ở hướng này
                if (piece != Piece.None)
                {
                    //Nếu là quân đồng mình thì có thể nó đang bị pin
                    if (Piece.IsColor(piece, friendlyColor))
                    {
                        if (!isFriendlyPieceAlongRay)
                        {
                            isFriendlyPieceAlongRay = true;
                        }
                        else
                        {
                            //Nếu tìm thấy quân đồng minh thứ 2, 
                            //có nghĩa là hướng này không có quân địch nào đang pin king
                            break;
                        }

                    }
                    else//Nếu là quân địch
                    {
                        int pieceType = Piece.PieceType(piece);
                        if (isDiagonal && Piece.IsBishopOrQueen(piece) || !isDiagonal && Piece.IsRookOrQueen(piece))
                        {
                            //Nếu có 1 quân đồng minh đang chặn quân địch
                            //Thì chắc chắn king đang bị pin
                            if (isFriendlyPieceAlongRay)
                            {
                                pinExistInPosition = true;
                                pinRayBitmask |= rayMask;
                            }
                            else//Nếu không có quân đồng minh thì đây là check
                            {
                                checkRayBitmask |= rayMask;
                                inDoubleCheck = inCheck;//Nếu đã đang bị check bởi quân khác thì đây là double check
                                inCheck = true;
                            }
                            break;
                        }
                        else
                        {
                            //Quân cờ của địch không thể di chuyển hướng này(không phải sliding piece), thì ko thể pin/check
                            break;
                        }
                    }
                }
            }

            //Nếu king đang bị double check thì không cần search nữa, vì chỉ có king là có thể di chuyển lúc này
            if (inDoubleCheck)
                break;
        }

        //Knight attacks
        PieceList opponentKnights = board.knights[opponentColorIndex];
        opponentKnightAttacks = 0;
        bool isKnightCheck = false;

        for (int knightIndex = 0; knightIndex < opponentKnights.Count; knightIndex++)
        {
            int startSquare = opponentKnights[knightIndex];
            opponentKnightAttacks |= knightAttackBitBoards[startSquare];

            if (!isKnightCheck && BitBoardUtility.ContainsSquare(opponentKnightAttacks, friendlyKingSquare))
            {
                isKnightCheck = true;
                inDoubleCheck = inCheck;
                inCheck = true;
                checkRayBitmask |= 1ul << startSquare;
            }
        }

        //Pawn attacks
        PieceList opponentPawns = board.pawns[opponentColorIndex];
        opponentPawnAttackMap = 0;
        bool isPawnCheck = false;
        for (int pawnIndex = 0; pawnIndex < opponentPawns.Count; pawnIndex++)
        {
            int pawnSquare = opponentPawns[pawnIndex];
            ulong pawnAttacks = pawnAttackBitBoards[pawnSquare][opponentColorIndex];
            opponentPawnAttackMap |= pawnAttacks;

            if (!isPawnCheck && BitBoardUtility.ContainsSquare(pawnAttacks, friendlyKingSquare))
            {
                isPawnCheck = true;
                inDoubleCheck = inCheck;
                inCheck = true;
                checkRayBitmask |= 1ul << pawnSquare;
            }
        }

        int enemyKingSquare = board.KingSquare[opponentColorIndex];
        opponentAttackNoPawns = opponentSlidingAttackMap | opponentKnightAttacks | kingAttackBitBoards[enemyKingSquare];
        opponentAttackMap = opponentPawnAttackMap | opponentAttackNoPawns;

    }

    void GenerateSlidingAttackMap()
    {
        //Tính toán các ô có thể bị tấn công bởi các quân sliding của đối phương
        opponentSlidingAttackMap = 0;

        PieceList enemyRooks = board.rooks[opponentColorIndex];
        for (int i = 0; i < enemyRooks.Count; i++)
        {
            UpdateSlidingAttackPiece(enemyRooks[i], 0, 4);
        }

        PieceList enemyQueens = board.queens[opponentColorIndex];
        for (int i = 0; i < enemyQueens.Count; i++)
        {
            UpdateSlidingAttackPiece(enemyQueens[i], 0, 8);
        }

        PieceList enemyBishop = board.bishops[opponentColorIndex];
        for (int i = 0; i < enemyBishop.Count; i++)
        {
            UpdateSlidingAttackPiece(enemyBishop[i], 4, 8);
        }

    }



    void UpdateSlidingAttackPiece(int startSquare, int startDirIndex, int endDirIndex)
    {
        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            int currentDirOffset = directionOffsets[directionIndex];
            for (int n = 0; n < numSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + currentDirOffset * (n + 1);
                int targetSquarePiece = board.Squares[targetSquare];
                opponentSlidingAttackMap |= 1ul << targetSquare;
                if (targetSquare != friendlyKingSquare)
                {
                    if (targetSquare != Piece.None)
                    {
                        break;
                    }
                }
            }
        }
    }

    bool SquareIsAttacked(int square)
    {
        return BitBoardUtility.ContainsSquare(opponentAttackMap, square);
    }

    void MakePromotionMoves(int fromSquare, int toSquare)
    {
        moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToQueen));
        if (promotionToGenerate == PromotionMode.All)
        {
            moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToBishop));
            moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToRook));
            moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToKnight));
        }
        else
        {
            moves.Add(new Move(fromSquare, toSquare, Move.Flag.PromoteToKnight));
        }
    }

    bool IsMovingAlongRay(int rayDir, int startSquare, int targetSquare)
    {
        int moveDir = directionLookUp[targetSquare - startSquare + 63];
        return (rayDir == moveDir || -rayDir == moveDir);
    }
    bool IsPinned(int square)
    {
        return pinExistInPosition && ((pinRayBitmask >> square) & 1) != 0;
    }

    bool SquareIsInCheckRay(int square)
    {
        return inCheck && ((checkRayBitmask >> square) & 1) != 0;
    }

    bool HasKingSideCastleRight
    {
        get
        {
            int mask = (board.WhiteToMove) ? 1 : 4;
            return (board.currentGameState & mask) != 0;
        }
    }

    bool HasQueenSideCastleRight
    {
        get
        {
            int mask = (board.WhiteToMove) ? 2 : 6;
            return (board.currentGameState & mask) != 0;
        }
    }

    bool InCheckAfterPassant(int startSquare, int targetSquare, int epCapturedPawnSquare)
    {
        board.Squares[targetSquare] = board.Squares[startSquare];
        board.Squares[startSquare] = Piece.None;
        board.Squares[epCapturedPawnSquare] = Piece.None;

        bool inCheckAfterEpCap = false;
        if (SquareAttackAfterEPCapture(epCapturedPawnSquare, startSquare))
        {
            inCheckAfterEpCap = true;
        }

        board.Squares[targetSquare] = Piece.None;
        board.Squares[startSquare] = Piece.Pawn | friendlyColor;
        board.Squares[epCapturedPawnSquare] = Piece.Pawn | opponentColor;
        return inCheckAfterEpCap;
    }

    bool SquareAttackAfterEPCapture(int epCaptureSquare, int capturingStartSquare)
    {
        if (BitBoardUtility.ContainsSquare(opponentAttackNoPawns, friendlyKingSquare))
        {
            return true;
        }

        //Chạy ngang theo hướng của ô en passant đê tìm xem có ô nào có thể tấn công king hay không
        int dirIndex = (epCaptureSquare < friendlyKingSquare) ? 2 : 3;//W or E
        for (int i = 0; i < numSquaresToEdge[friendlyKingSquare][dirIndex]; i++)
        {
            int squareIndex = friendlyKingSquare + directionOffsets[dirIndex] * (i + 1);
            int piece = board.Squares[squareIndex];
            if (piece != Piece.None)//Nếu có 1 quân cờ ở hướng này
            {
                if (Piece.IsColor(piece, friendlyColor))//Nếu là quân đồng minh
                {
                    break;
                }
                else
                {
                    if (Piece.IsRookOrQueen(piece))
                    {//Nếu là rook or queen
                        return true;
                    }
                    else
                    {//Nếu không phải thì king không thể bị check nếu quân pawn hiện tại đi en passant
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < 2; i++)
        {

            if (numSquaresToEdge[friendlyKingSquare][pawnAttackDirections[friendlyColorIndex][i]] > 0)
            {
                int piece = board.Squares[friendlyKingSquare + directionOffsets[pawnAttackDirections[friendlyColorIndex][i]]];
                if (piece == (Piece.Pawn | opponentColor))
                {
                    return true;
                }
            }
        }

        return false;


    }










}