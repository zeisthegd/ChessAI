using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Piece Theme", menuName = "Theme/Piece")]
public class PieceTheme : ScriptableObject
{
    public PieceSprites whitePieces;
    public PieceSprites blackPieces;

    //Xét màu của piece và lấy về một Sprite tùy theo loại piece.
    public Sprite GetPieceSprite(int piece)
    {
        PieceSprites pieceSprites = Piece.IsColor(piece, Piece.White) ? whitePieces : blackPieces;
        switch (Piece.PieceType(piece))
        {
            case Piece.Pawn:
                return pieceSprites.pawn;
            case Piece.Rook:
                return pieceSprites.rook;
            case Piece.Knight:
                return pieceSprites.knight;
            case Piece.Bishop:
                return pieceSprites.bishop;
            case Piece.Queen:
                return pieceSprites.queen;
            case Piece.King:
                return pieceSprites.king;
            default:
                if(piece != 0){
                    Debug.Log(piece);
                }
                return null;           
        }       
    }

    [System.Serializable]
    public class PieceSprites
    {
        public Sprite pawn, knight, rook, bishop, queen, king;

        public Sprite this[int i]
        {
            get
            {
                return new Sprite[] { pawn, knight, rook, bishop, queen, king }[i];
            }
        }
    }
}
