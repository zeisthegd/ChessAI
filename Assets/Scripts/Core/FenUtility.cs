using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FenUtility : MonoBehaviour
{
    static Dictionary<char,int> pieceTypeFromSymbol = new Dictionary<char, int>(){
        ['k'] = Piece.King, ['p'] = Piece.Pawn, ['n'] = Piece.Knight,  
        ['b'] = Piece.Bishop, ['r'] = Piece.Rook, ['q'] = Piece.Queen
    };

    public const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


    //
    public static LoadedPositionInfo PositionFromFen(string fen)
    {
        LoadedPositionInfo loadedPositionInfo = new LoadedPositionInfo();
        string[] sections = fen.Split(' ');

        int file = 0;
        int rank = 7;

        //Pieces index
        foreach(char symbol in sections[0])
        {
            if(symbol == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if(char.IsDigit(symbol)){
                    file += (int)char.GetNumericValue(symbol);
                }
                else{
                    int pieceColor = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    loadedPositionInfo.squares[rank * 8 + file] = pieceType | pieceColor;
                    file++;
                }
            }
        }

        //Who to move
        loadedPositionInfo.whiteToMove = (sections[1] == "w");


        //Castling rights
        string castlingRights = (sections.Length > 2) ? sections[2] : "KQkq";
        loadedPositionInfo.whiteCastleKingSide = castlingRights.Contains("K");
        loadedPositionInfo.whiteCastleQueenSide = castlingRights.Contains("Q");
        loadedPositionInfo.blackCastleKingSide = castlingRights.Contains("k");
        loadedPositionInfo.blackCastleQueenSide = castlingRights.Contains("q");

        //En passant target square
        if(sections.Length > 3)
        {
            string enPassantFileName  = sections[3][0].ToString();
            if(BoardRepresentation.fileNames.Contains(enPassantFileName))
                loadedPositionInfo.epFile = BoardRepresentation.fileNames.IndexOf(enPassantFileName) + 1;
        }

        //Half-move clock (half - full)
        if(sections.Length > 4)
        {
            int.TryParse(sections[4], out loadedPositionInfo.plyCount);
        }
        return loadedPositionInfo;

    }


    public class LoadedPositionInfo
    {
        public int[] squares;
        public bool whiteCastleKingSide;
        public bool whiteCastleQueenSide;
        public bool blackCastleKingSide;
        public bool blackCastleQueenSide;
        public int epFile;//en Passant
        public bool whiteToMove;
        public int plyCount;
        public LoadedPositionInfo()
        {
            squares = new int[64];
        }
    }
}
