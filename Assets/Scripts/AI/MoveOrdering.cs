using System.Collections;
using System.Collections.Generic;

public class MoveOrdering
{
    int[] movesScore;
    const int maxMoveCount = 218;

    const int squareControlledByOpponentPawnPenalty = 350;
    const int capturedPieceValueMultiplier = 10;

    MoveGenerator moveGenerator;
    Move invalidMove;

    public MoveOrdering(MoveGenerator moveGenerator)
    {
        movesScore = new int[maxMoveCount];
        this.moveGenerator = moveGenerator;
        invalidMove = Move.InvalidMove;
    }

    public void OrderMoves(Board board, List<Move> moves)
    {
        for(int i = 0;i<moves.Count;i++)
        {
            int score = 0;
            int movePieceType = Piece.PieceType(board.Squares[moves[i].StartSquare]);
            int capturedPieceType = Piece.PieceType(board.Squares[moves[i].TargetSquare]);
            int flag = moves[i].MoveFlag;

            if(capturedPieceType != Piece.None)
            {
                //Xắp sếp các nước đi để thử tìm ra các nước có thể ăn được quân cờ có giá trị cao của đổi phương
                //Bằng quân cờ có giá trị thấp hơn của quân ta
                //Trường capturedPieceValueMultiplier dùng để sắp xếp để các nước đi không tốt như QxP
                //Xếp trước so với các nước đi non-capture
                score = capturedPieceValueMultiplier * GetPieceValue(capturedPieceType) - GetPieceValue(movePieceType);
            }

            if(movePieceType == Piece.Pawn)
            {
                if(flag == Move.Flag.PromoteToQueen)
                {
                    score += Evaluation.queenValue;
                }
                if(flag == Move.Flag.PromoteToKnight)
                {
                    score += Evaluation.knightValue;
                }
                if(flag == Move.Flag.PromoteToRook)
                {
                    score += Evaluation.rookValue;
                }
                if(flag == Move.Flag.PromoteToBishop)
                {
                    score += Evaluation.bishopValue;
                }

            }
            else
            {
                //Trừ điểm những nước đi vào ô mà quân pawn của đối phương đang chiếm
                if(BitBoardUtility.ContainsSquare(moveGenerator.opponentPawnAttackMap,moves[i].TargetSquare))
                {
                    score-=squareControlledByOpponentPawnPenalty;
                }
            }
            movesScore[i] = score;
        }
    
    }

    static int GetPieceValue(int pieceType)
    {
        switch (pieceType)
        {
            case Piece.Queen:
                return Evaluation.queenValue;
            case Piece.Rook:
                return Evaluation.rookValue;
            case Piece.Bishop:
                return Evaluation.bishopValue;
            case Piece.Knight:
                return Evaluation.knightValue;
            case Piece.Pawn:
                return Evaluation.pawnValue;
            default:
                return 0;
        }
    }

    void Sort(List<Move> moves)
    {
        for(int i = 0; i < moves.Count;i++)
        {
            for(int j = i + 1;j > 0;j--)
            {
                int swapIndex = j - 1;
                if(movesScore[swapIndex] <movesScore[j])
                {
                    (moves[j],moves[swapIndex]) = (moves[swapIndex],moves[j]);
                    (movesScore[j],movesScore[swapIndex]) = (movesScore[swapIndex],movesScore[j]);
                }
            }
        }
    }

}