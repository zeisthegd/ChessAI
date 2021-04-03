using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    public enum InputState
    {
        None,
        PieceSelected,
        DraggingPiece
    }

    InputState currentState;

    BoardUI boardUI;
    Camera camera;
    Coord selectedPieceSquare;
    Board board;

    public HumanPlayer(Board board)
    {
        boardUI = GameObject.FindObjectOfType<BoardUI>();
        camera = Camera.main;
        this.board = board;
    }

    public override void NotifyTurnToMove()
    {

    }

    public override void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        if (currentState == InputState.None)
        {
            HandlePieceSelection(mousePos);
        }
        else if (currentState == InputState.DraggingPiece)
        {
            HandleDragMovement(mousePos);

        }
        else if (currentState == InputState.PieceSelected)
        {
            HandlePointAndClickMovement(mousePos);

        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPieceSelection();
        }
    }



    private void HandleDragMovement(Vector2 mousePos)
    {
        boardUI.DragPiece(selectedPieceSquare, mousePos);
        if (Input.GetMouseButtonUp(0))
        {
            HandlePiecePlacement(mousePos);
        }
    }

    void HandlePointAndClickMovement(Vector2 mousePos)
    {
        if (Input.GetMouseButton(0))
        {
            HandlePiecePlacement(mousePos);
        }
    }

    void HandlePiecePlacement(Vector2 mousePos)
    {
        Coord targetSquare;
        if (boardUI.TryGetSquareUnderMouse(mousePos, out targetSquare))//Nếu ô được chọn là một ô trên bàn cờ
        {
            if (targetSquare.Equals(selectedPieceSquare))//Nếu như ô được chọn là ô của quân cờ đang được chọn
            {
                boardUI.ResetPiecePosition(selectedPieceSquare);
                if (currentState == InputState.DraggingPiece)
                    currentState = InputState.PieceSelected;
                else
                    currentState = InputState.None;
                boardUI.DeselectSquare(selectedPieceSquare);
            }
            else
            {
                int targetIndex = BoardRepresentation.IndexFromCoord(targetSquare.fileIndex, targetSquare.rankIndex);

                //Nếu như quân cờ bị target cùng màu với phe đang được đi
                if (Piece.IsColor(board.Squares[targetIndex], board.ColorToMove) && board.Squares[targetIndex] != 0)
                {
                    CancelPieceSelection();
                    HandlePieceSelection(mousePos);
                }
                else
                {
                    TryMakeMove(selectedPieceSquare, targetSquare);
                }
            }
        }
        else
        {
            CancelPieceSelection();
        }
    }

    private void HandlePieceSelection(Vector2 mousePos)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (boardUI.TryGetSquareUnderMouse(mousePos, out selectedPieceSquare))
            {
                int index = BoardRepresentation.IndexFromCoord(selectedPieceSquare);
                //Nếu ô này có một quân cờ, chọn quân cờ để kéo đi
                if (Piece.IsColor(board.Squares[index], board.ColorToMove))//Nếu quân cờ thuộc về phe đi lượt tiếp theo
                {
                    boardUI.HighlightLegalMoves(board, selectedPieceSquare);
                    boardUI.SelectSquare(selectedPieceSquare);
                    currentState = InputState.DraggingPiece;
                }
            }
        }
    }



    void TryMakeMove(Coord startSquare, Coord targetSquare)
    {

        int startIndex = BoardRepresentation.IndexFromCoord(startSquare);
        int targetIndex = BoardRepresentation.IndexFromCoord(targetSquare);
        //Debug.Log("Target: " + targetIndex);
        bool moveIsLegal = false;
        Move chosenMove = new Move();

        MoveGenerator moveGenerator = new MoveGenerator();
        bool wantsKnightPromotion = Input.GetKey(KeyCode.LeftAlt);

        var legalMoves = moveGenerator.GenerateMoves(board);

        for (int i = 0; i < legalMoves.Count; i++)
        {
            var legalMove = legalMoves[i];
            //Debug.Log("Checking " + legalMove.StartSquare + " | " + legalMove.TargetSquare);
            //Debug.Log("Index " + startIndex + " | " + targetIndex);
            if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex)
            {

                if (legalMove.IsPromotion)
                {
                    if (legalMove.MoveFlag == Move.Flag.PromoteToQueen && wantsKnightPromotion)
                    {
                        continue;
                    }
                    if (legalMove.MoveFlag != Move.Flag.PromoteToQueen && !wantsKnightPromotion)
                    {
                        continue;
                    }
                }

                moveIsLegal = true;
                chosenMove = legalMove;
                break;
            }

        }


        if (moveIsLegal)
        {
            ChoseMove(chosenMove);
            currentState = InputState.None;
        }
        else
            CancelPieceSelection();



    }

    private void CancelPieceSelection()
    {
        if (currentState != InputState.None)
        {
            currentState = InputState.None;
            boardUI.DeselectSquare(selectedPieceSquare);
            boardUI.ResetPiecePosition(selectedPieceSquare);
        }
    }
}