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
            beta = Math.Min(beta, immediateMateScore * plyFromRoot);
            if (alpha >= beta)
            {
                return alpha;
            }
        }


        if (depth == 0)
        {
            int evaluation = QuiescenceSearch(alpha, beta);
        }

        return 1;
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
            board.MakeMove(moves[i], false);
            eval = -QuiescenceSearch(alpha, beta);
            board.UnmakeMove(moves[i], false);
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
