using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AIPlayer : Player
{
    Search search;
    AISettings settings;
    bool moveFound;
    Move move;
    Board board;

    public AIPlayer(Board board, AISettings settings)
    {
        this.settings = settings;
        this.board = board;
        //settings.requestAbortSearch += TimeOutth
        search = new Search(board, settings);
        search.onSearchComplete += OnSearchComplete;
        search.searchDiagnostics = new Search.SearchDiagnostics();


    }

    public override void Update()
    {
        if(moveFound)
        {
            moveFound = false;
            ChoseMove(move);           
        }

        settings.diagnostics = search.searchDiagnostics;
    }

    public override void NotifyTurnToMove()
    {
        search.searchDiagnostics.isBook = false;
        moveFound = false;
        Move bookMove = Move.InvalidMove;
        StartSearch();
    }

    void StartSearch()
    {
        search.StartSearch();
        moveFound = true;
    }

    private void OnSearchComplete(Move move)
    {
        moveFound = true;
        this.move = move;
    }
}