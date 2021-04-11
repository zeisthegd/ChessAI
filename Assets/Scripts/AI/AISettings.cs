using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AISettings", menuName = "AI/AISettings", order = 0)]
public class AISettings : ScriptableObject {
    public event System.Action requestAbortSearch;

    public int depth;
    public bool useIterativeDeepening;

    public bool useFixedDepthSearch;
    public bool endlessSearchMode;


    public MoveGenerator.PromotionMode promotionsToSearch;
    public Search.SearchDiagnostics diagnostics;

    public void RequestAbortSearch()
    {
        requestAbortSearch?.Invoke();
    }

}

