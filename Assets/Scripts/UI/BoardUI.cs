using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Front-end của bàn cờ.
public class BoardUI : MonoBehaviour
{
    public BoardTheme boardTheme;
    public PieceTheme pieceTheme;
    public bool showLegalMoves;
    public bool whiteIsBottom;

    [SerializeField]
    Shader squareShader;
    Move lastMadeMove;
    MoveGenerator moveGenerator;

    public MeshRenderer[,] squareRenderers;
    public SpriteRenderer[,] squarePieceRenderers;
    const float pieceDragDepth = -0.2f;
    const float pieceDepth = -0.1f;

    void Awake()
    {
        moveGenerator = new MoveGenerator();
        CreateBoardUI();
    }

    private void CreateBoardUI()
    {
        squareRenderers = new MeshRenderer[8, 8];
        squarePieceRenderers = new SpriteRenderer[8, 8];

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                square.parent = transform;
                square.name = BoardRepresentation.SquareNameFromCoordinate(file, rank);
                square.position = PositionFromCoord(file, rank, 0);
                Material squareMaterial = new Material(squareShader);

                squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer>();
                squareRenderers[file, rank].material = squareMaterial;

                SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
                pieceRenderer.transform.parent = square;
                pieceRenderer.transform.position = PositionFromCoord(file, rank, pieceDepth);
                pieceRenderer.transform.localScale = Vector3.one * 100 / (2000 / 6F);
                squarePieceRenderers[file, rank] = pieceRenderer;

            }
        }
        ResetSquareColors();
    }


    public bool TryGetSquareUnderMouse(Vector2 mouseWorldPos, out Coord selectedCoord)
    {
        int file = (int)(mouseWorldPos.x + 4);
        int rank = (int)(mouseWorldPos.y + 4);
        if (!whiteIsBottom)
        {
            file = 7 - file;
            rank = 7 - rank;
        }
        selectedCoord = new Coord(file, rank);
        return file >= 0 && file < 8 && rank >= 0 && rank < 8;

    }

    public void SetPerspective(bool whitePOV)
    {
        whiteIsBottom = whitePOV;
        ResetSquarePositions();
    }

    public void HighlightLegalMoves(Board board, Coord fromSquare)
    {
        if (showLegalMoves)
        {
            var moves = moveGenerator.GenerateMoves(board);
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                if (move.StartSquare == BoardRepresentation.IndexFromCoord(fromSquare))
                {
                    Coord coord = BoardRepresentation.CoordFromIndex(move.TargetSquare);
                    SetSquareColor(coord, boardTheme.lightSquares.legal, boardTheme.darkSquares.legal);
                }
            }
        }
    }

    public void SelectSquare(Coord coord)
    {
        SetSquareColor(coord, boardTheme.lightSquares.selected, boardTheme.darkSquares.selected);
    }

    public void DragPiece(Coord pieceCoord, Vector2 mousePos)
    {
        //Set vị trí của quân cờ thành vị trí của mouse
        squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = new Vector3(mousePos.x, mousePos.y, pieceDragDepth);
    }

    void HighlightMove(Move move)
    {
        SetSquareColor(BoardRepresentation.CoordFromIndex(move.StartSquare),
                boardTheme.lightSquares.moveFromHighlight,
                    boardTheme.darkSquares.moveFromHighlight);

        SetSquareColor(BoardRepresentation.CoordFromIndex(move.StartSquare),
                boardTheme.lightSquares.moveFromHighlight,
                    boardTheme.darkSquares.moveFromHighlight);
    }

    public void DeselectSquare(Coord coord)
    {
        ResetSquareColors();
    }

    public void UpdatePosition(Board board)
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Coord coord = new Coord(file, rank);
                int piece = board.Squares[BoardRepresentation.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
                squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
                squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
            }
        }

    }

    public void OnMoveMade(Board board, Move move, bool animated = false)
    {
        lastMadeMove = move;
        if (animated)
        {
            StartCoroutine(AnimateMove(move, board));
        }
        else
        {
            UpdatePosition(board);
            ResetSquareColors();
        }
    }

    IEnumerator AnimateMove(Move move, Board board)
    {
        float t = 0;
        const float moveAnimDuration = 0.15f;
        Coord startCoord = BoardRepresentation.CoordFromIndex(move.StartSquare);
        Coord targetCoord = BoardRepresentation.CoordFromIndex(move.TargetSquare);
        Transform pieceT = squarePieceRenderers[startCoord.fileIndex, startCoord.rankIndex].transform;
        Vector3 startPos = PositionFromChord(startCoord);
        Vector3 targetPos = PositionFromChord(targetCoord);
        SetSquareColor(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.lightSquares.moveFromHighlight, boardTheme.darkSquares.moveFromHighlight);

        while (t <= 1)
        {
            yield return null;
            t += Time.deltaTime * 1 / moveAnimDuration;
            pieceT.position = Vector3.Lerp(startPos, targetPos, t);
        }

        UpdatePosition(board);
        ResetSquareColors();
        pieceT.position = startPos;

    }

    public void ResetPiecePosition(Coord pieceCoord)
    {
        Vector3 pos = PositionFromChord(pieceCoord, pieceDepth);
        squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = pos;
    }

    public void ResetSquareColors()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                SetSquareColor(new Coord(file, rank), boardTheme.lightSquares.normal, boardTheme.darkSquares.normal);
            }
        }
    }

    void ResetSquarePositions()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                squareRenderers[file, rank].transform.position = PositionFromCoord(file, rank, 0);
                squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
            }
        }
    }

    private void SetSquareColor(Coord square, Color lightColor, Color darkColor)
    {
        squareRenderers[square.fileIndex, square.rankIndex].material.color = (square.IsLightSquare()) ? lightColor : darkColor;
    }
    private Vector3 PositionFromCoord(int file, int rank, float depth = 0)
    {
        float offset = -3.5f;
        if (whiteIsBottom)
            return new Vector3(offset + file, offset + rank, depth);
        else
            return new Vector3(7 - file + offset, 7 - rank + offset, depth);
    }

    private Vector3 PositionFromChord(Coord coord, float depth = 0)
    {
        return PositionFromCoord(coord.fileIndex, coord.rankIndex, depth);
    }
}
