using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    const int transpositionTableSize = 64000;
    const int immediateMateScore = 100000;
    const int positiveInfinity = 9999999;
    const int negativeInfinity = -positiveInfinity;

    public event System.Action<Move> onSearchComplete;

    MoveGenerator moveGenerator;

    Move bestMoveThisIteration;
    int bestEvalThisIteration;
    Move bestMove;
    int bestEval;
    int currentIterativeSearchDepth;
    bool abortSearch;

    Move invalidMove;
    //moveordering
    AISettings settings;
    Board board;
    Evaluation evaluation;


    public SearchDiagnostics searchDiagnostics;
    int numNodes;
    int numQNodes;
    int numCutoffs;
    int numTransposition;
    System.Diagnostics.Stopwatch searchStopWatch;

    public Search(Board board, AISettings settings)
    {
        this.settings = settings;
        this.board = board;
        evaluation = new Evaluation();
        moveGenerator = new MoveGenerator();
        invalidMove = Move.InvalidMove;

    }

    public void StartSearch()
    {
        InitDebugInfo();

        //Khởi tạo search
        bestEvalThisIteration = bestEval = 0;
        bestMoveThisIteration = bestMove = Move.InvalidMove;

        moveGenerator.promotionToGenerate = settings.promotionsToSearch;
        currentIterativeSearchDepth = 0;
        abortSearch = false;
        searchDiagnostics = new SearchDiagnostics();

        SearchMoves(settings.depth, 0, negativeInfinity, positiveInfinity);
        bestMove = bestMoveThisIteration;
        bestEval = bestEvalThisIteration;

        onSearchComplete?.Invoke(bestMove);

        //LogDebugInfo();

    }


    public (Move move, int eval) GetSearchValue()
    {
        return (bestMove, bestEval);
    }
    void EndSearch()
    {
        abortSearch = true;
    }
    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta)
    {
        if (abortSearch)
            return 0;

        if (plyFromRoot > 0)
        {
            alpha = Math.Max(alpha, -immediateMateScore + plyFromRoot);
            beta = Math.Min(beta, immediateMateScore - plyFromRoot);
            if (alpha >= beta)
            {
                return alpha;
            }
        }


        if (depth == 0)
        {
            int evaluation = QuiescenceSearch(alpha, beta);
            return evaluation;
        }

        List<Move> moves = moveGenerator.GenerateMoves(board);

        if (moves.Count == 0)
        {
            if (moveGenerator.InCheck)
            {
                int mateScore = immediateMateScore - plyFromRoot;
                return -mateScore;
            }
            else
            {
                return 0;
            }
        }

        Move bestMoveInThisPosition = invalidMove;

        for (int i = 0; i < moves.Count; i++)
        {
            //Debug.Log($"Searching move: {moves[i].StartSquare}/{moves[i].TargetSquare}");     
            board.MakeMove(moves[i], inSearch: true);
            int eval = -SearchMoves(depth - 1, plyFromRoot + 1, -beta, -alpha);
            board.UnmakeMove(moves[i], inSearch: true);
            numNodes++;

            //Nước đi tìm được quá "tốt" nên đối phương sẽ không để nó xảy ra
            //Vì vậy sẽ chọn một nước đi đã tìm được trước đó. Bỏ qua những nước đi còn lại
            if (eval >= beta)
            {
                numCutoffs++;
                return beta;
            }

            //Tìm thấy nước đi tốt hơn ở thế cờ này
            if (eval > alpha)
            {
                bestMoveInThisPosition = moves[i];

                alpha = eval;
                if (plyFromRoot == 0)
                {
                    bestMoveThisIteration = moves[i];
                    bestEvalThisIteration = eval;
                }
            }
        }

        return alpha;
    }


    //Tìm kiếm một quiet move position trong số những moves
    int QuiescenceSearch(int alpha, int beta)
    {
        //Người chơi hoặc AI không nhất thiết phải đi các nước capture
        //Vì vậy dừng tìm kiếm khi tìm xong các nước capture khá nguy hiểm
        //Quiescence Search giúp người chơi tìm ra các nước đi non-capture khác 
        //tốt hơn những nước đi capture bất lợi
        int eval = evaluation.Evalutate(board);
        searchDiagnostics.numPositionsEvaluated++;
        if (eval >= beta)
            return beta;
        if (eval > alpha)
            alpha = eval;

        List<Move> moves = moveGenerator.GenerateMoves(board, false);

        for (int i = 0; i < moves.Count; i++)
        {
            //Debug.Log($"QuiescenceSearching move: {moves[i].StartSquare}/{moves[i].TargetSquare}");
            board.MakeMove(moves[i], true);
            eval = -QuiescenceSearch(-alpha, -beta);
            board.UnmakeMove(moves[i], true);
            numQNodes++;

            if (eval >= beta)
            {
                numCutoffs++;
                return beta;
            }

            if (eval > alpha)
            {
                alpha = eval;
            }
        }
        return alpha;
    }






    public static bool IsMateScore(int score)
    {
        const int maxMateDepth = 1000;
        return System.Math.Abs(score) > immediateMateScore - maxMateDepth;
    }

    public static int NumPlyToMateFromScore(int score)
    {
        return immediateMateScore - System.Math.Abs(score);
    }
    void LogDebugInfo()
    {
        AnnounceMate();
        Debug.Log($"Best Move: {bestMoveThisIteration.Name} Eval: {bestEvalThisIteration} Search Time: {searchStopWatch.ElapsedMilliseconds} ms.");
        Debug.Log($"Num nodes: {numNodes} num QNodes: {numQNodes} num cutoffs: {numCutoffs} numTTHits {numTransposition}");
    }

    void AnnounceMate()
    {
        if (IsMateScore(bestEvalThisIteration))
        {
            int numPlyToMateFromScore = NumPlyToMateFromScore(bestEvalThisIteration);

            int numMovesToMate = (int)System.Math.Ceiling(numPlyToMateFromScore / 2f);
            string sideWithMate = (bestEvalThisIteration * ((board.WhiteToMove) ? 1 : -1) < 0) ? "Black" : "White";

            Debug.Log($"{sideWithMate} can mate in {numMovesToMate} move{((numMovesToMate > 1) ? "s" : " ")}");
        }
    }

    void InitDebugInfo()
    {
        searchStopWatch = System.Diagnostics.Stopwatch.StartNew();
        numNodes = 0;
        numQNodes = 0;
        numCutoffs = 0;
        numTransposition = 0;
    }




    [System.Serializable]
    public class SearchDiagnostics
    {
        public int lastCompleteDepth;
        public string moveVal;
        public string move;
        public int eval;
        public bool isBook;
        public int numPositionsEvaluated;
    }
}
