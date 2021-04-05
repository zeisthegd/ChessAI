using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AISettings", menuName = "AI/AISettings", order = 0)]
public class AISettings : ScriptableObject {
    public event System.Action requestAbortSearch;

    public int depth;
    public bool useIterativeDeepening;
    public bool useTranspositionTable;

    public bool useThreading;
    public bool useFixedDepthSearch;
    public int searchTimeMilis = 1000;
    public bool endlessSearchMode;
    public bool clearTTEachMove;

    public bool useBook;
    //public TextAs
    public int maxBookPly = 10;

    public MoveGenerator.PromotionMode promotionsToSearch;
    public Search.SearchDiagnostics diagnostics;

    public void RequestAbortSearch()
    {
        requestAbortSearch?.Invoke();
    }

}

