using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum PlayerType { Human, AI }
    public enum Result { Playing, WhiteIsMated, BlackIsMated, Stalemate, Repetition, FiftyRuleMove, InsufficientMaterial }

    public event System.Action onPositionLoaded;
    public event System.Action<Move> onMoveMade;


    public string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";
    public bool loadCustomPosition;

    public PlayerType blackPlayerType;
    public PlayerType whitePlayerType;

    Player whitePlayer;
    Player blackPlayer;
    Player playerToMove;
    public Color[] colors;

    //Clock


    Result gameResult;

    BoardUI boardUI;
    List<Move> gameMoves;




    public Board board { get; private set; }
    Board searchBoard; // Duplicate version of board used for ai search

    void Start()
    {
        gameMoves = new List<Move>();
        boardUI = FindObjectOfType<BoardUI>();
        board = new Board();
        searchBoard = new Board();

        NewGame(whitePlayerType,blackPlayerType);
    }

    void Update()
    {
        if (gameResult == Result.Playing)
        {
            playerToMove.Update();
        }
    }

    void NewGame(PlayerType whitePlayerType = PlayerType.Human, PlayerType blackPlayerType = PlayerType.Human)
    {
        if (loadCustomPosition)
        {
            board.LoadPosition(customPosition);
        }
        else
        {
            board.LoadStartPosition();
        }

        onPositionLoaded?.Invoke();
        boardUI.UpdatePosition(board);
        boardUI.ResetSquareColors();

        CreatePlayer(ref whitePlayer, whitePlayerType);
        CreatePlayer(ref blackPlayer, blackPlayerType);

        NotifyPlayerToMove();
    }

    void CreatePlayer(ref Player player, PlayerType playerType)
    {
        if (player != null)
            player.onMoveChosen -= OnMoveChosen;

        if (playerType == PlayerType.Human)
            player = new HumanPlayer(board);

        player.onMoveChosen += OnMoveChosen;
    }


    void OnMoveChosen(Move move)
    {
        bool animatedMove = playerToMove is AIPlayer;
        board.MakeMove(move);
        //searchBoard.MakeMove(move);

        gameMoves.Add(move);
        onMoveMade?.Invoke(move);
        boardUI.OnMoveMade(board,move,animatedMove);
        NotifyPlayerToMove();
    }

    void NotifyPlayerToMove()
    {
        if (gameResult == Result.Playing)
        {
            playerToMove = (board.WhiteToMove) ? whitePlayer : blackPlayer;
            playerToMove.NotifyTurnToMove();
        }
        else
        {
            Debug.Log("Game Over");
        }
    }


}
