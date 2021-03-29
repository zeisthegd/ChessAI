using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum PlayerType { Human, AI }

    BoardUI boardUI;
    
    public event System.Action onPositionLoaded;
    public string customPosition;
    public bool loadCustomPosition;

    public Board board { get; private set; }
    Board searchBoard; // Duplicate version of board used for ai search

    void Start()
    {
        boardUI = FindObjectOfType<BoardUI>();
        board = new Board();

        NewGame();
    }

    void NewGame(PlayerType whitePlayerType = PlayerType.Human, PlayerType blackPlayerType = PlayerType.Human)
    {
        if(loadCustomPosition){
            board.LoadPosition(customPosition);
        }
        else{
            board.LoadStartPosition();
        }

        onPositionLoaded?.Invoke();
        boardUI.UpdatePosition(board);
        boardUI.ResetSquareColors();
    }
}
