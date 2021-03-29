using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : MonoBehaviour
{
    public BoardTheme boardTheme;
    public PieceTheme pieceTheme;
    public bool showLegalMoves;
    public bool whiteIsBottom;

    public MeshRenderer[,] squareRenderers;
    public SpriteRenderer[,] squarePieceRenderers;
    const float pieceDepth = -0.1f;

    void Awake()
    {
        CreateBoardUI();
    }

    private void CreateBoardUI()
    {
        Shader squareShader = Shader.Find("Unlit/Color");
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
                print(square.name);
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


    public void UpdatePosition(Board board)
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Coord coord = new Coord(file, rank);
                int piece = board.Squares[BoardRepresentation.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
                squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
                squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file,rank,pieceDepth);
            }
        }
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
