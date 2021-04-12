using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum PlayerType { Human, AI }
    public enum Result { Playing, WhiteIsMated, BlackIsMated, Stalemate, Repetition, FiftyMoveRule, InsufficientMaterial }

    public TMPro.TMP_Text resultUI;
    public event System.Action onPositionLoaded;
    public event System.Action<Move> onMoveMade;


    public string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";
    public bool loadCustomPosition;

    public PlayerType blackPlayerType;
    public PlayerType whitePlayerType;
    public AISettings aiSettings;

    Player whitePlayer;
    Player blackPlayer;
    Player playerToMove;
    public Color[] colors;

    //

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

        NewGame(whitePlayerType, blackPlayerType);
    }

    void Update()
    {
        if (gameResult == Result.Playing)
        {
            playerToMove.Update();
        }
    }

    public void NewGame(PlayerType whitePlayerType = PlayerType.Human, PlayerType blackPlayerType = PlayerType.Human)
    {
        if (loadCustomPosition)
        {
            board.LoadPosition(customPosition);
            searchBoard.LoadPosition(customPosition);
        }
        else
        {
            board.LoadStartPosition();
            searchBoard.LoadStartPosition();
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
        else
        {
            player = new AIPlayer(searchBoard, aiSettings);
        }

        player.onMoveChosen += OnMoveChosen;
    }


    void OnMoveChosen(Move move)
    {
        bool animatedMove = playerToMove is AIPlayer;
        board.MakeMove(move);
        searchBoard.MakeMove(move);

        gameMoves.Add(move);
        onMoveMade?.Invoke(move);
        boardUI.OnMoveMade(board, move, animatedMove);
        NotifyPlayerToMove();
    }

    void NotifyPlayerToMove()
    {
        gameResult = GetGameState();
        PrintGameResult(gameResult);
        if (gameResult == Result.Playing)
        {
            playerToMove = (board.WhiteToMove) ? whitePlayer : blackPlayer;
            playerToMove.NotifyTurnToMove();
        }
        else
        {
            Debug.Log("Game Over: " + gameResult);
        }
    }

    Result GetGameState()
    {
        MoveGenerator moveGen = new MoveGenerator();
        var moves = moveGen.GenerateMoves(board);

        //Mate/Stalemate
        if (moves.Count == 0)
        {
            if (moveGen.InCheck)
            {
                return (board.WhiteToMove) ? Result.WhiteIsMated : Result.BlackIsMated;
            }
            return Result.Stalemate;
        }

        //Fifty-move rule
        if (board.fiftyMoveCounter >= 50)
        {
            return Result.FiftyMoveRule;
        }

        //
        int numPawns = board.pawns[Board.WhiteIndex].Count + board.pawns[Board.BlackIndex].Count;
        int numRooks = board.rooks[Board.WhiteIndex].Count + board.rooks[Board.BlackIndex].Count;
        int numBishops = board.bishops[Board.WhiteIndex].Count + board.bishops[Board.BlackIndex].Count;
        int numKnights = board.knights[Board.WhiteIndex].Count + board.knights[Board.BlackIndex].Count;
        int numQueens = board.queens[Board.WhiteIndex].Count + board.queens[Board.BlackIndex].Count;

        if (numPawns + numRooks + numQueens == 0)
        {
            if (numKnights == 1 || numBishops == 1)
            {
                return Result.InsufficientMaterial;
            }
        }

        return Result.Playing;
    }

    void PrintGameResult(Result result)
    {
        float subtitleSize = resultUI.fontSize * 0.75f;
        string subtitleSettings = $"<color=#787878> <size={subtitleSize}>";

        if (result == Result.Playing)
        {
            resultUI.text = "Playing";
        }
        else if (result == Result.WhiteIsMated || result == Result.BlackIsMated)
        {
            resultUI.text = "Checkmate!";
        }
        else if (result == Result.FiftyMoveRule)
        {
            resultUI.text = "Draw";
            resultUI.text += subtitleSettings + "\n(50 move rule)";
        }
        else if (result == Result.Repetition)
        {
            resultUI.text = "Draw";
            resultUI.text += subtitleSettings + "\n(3-fold repetition)";
        }
        else if (result == Result.Stalemate)
        {
            resultUI.text = "Draw";
            resultUI.text += subtitleSettings + "\n(Stalemate)";
        }
        else if (result == Result.InsufficientMaterial)
        {
            resultUI.text = "Draw";
            resultUI.text += subtitleSettings + "\n(Insufficient material)";
        }
    }

    public void NewGame(bool humanPlaysWhite)
    {
        boardUI.SetPerspective(humanPlaysWhite);
        NewGame((humanPlaysWhite) ? PlayerType.Human : PlayerType.AI, (humanPlaysWhite) ? PlayerType.AI : PlayerType.Human);
    }public void NewHumanVersusHumanGame()
    {
        boardUI.SetPerspective(true);
        NewGame(PlayerType.Human, PlayerType.Human);
    }
    public void NewComputerVersusComputerGame()
    {
        boardUI.SetPerspective(true);
        NewGame(PlayerType.AI, PlayerType.AI);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
