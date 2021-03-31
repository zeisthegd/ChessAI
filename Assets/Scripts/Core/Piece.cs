public static class Piece
{
    public const int None = 0; //0b000
    public const int King = 1; //0b001
    public const int Pawn = 2; //0b010
    public const int Knight = 3; // 0b011
    public const int Bishop = 5; // 0b101
    public const int Rook = 6; //0b110
    public const int Queen = 7; //0b111

    public const int White = 8; //0b01000
    public const int Black = 16; //0b10000

    const int typeMask = 0b00111;
    const int blackMask = 0b10000;
    const int whiteMask = 0b01000;
    const int colorMask = whiteMask | blackMask;

    //Cộng giá trị của piece và colorMask để kiểm tra piece có cùng màu với tham số không
    public static bool IsColor(int piece, int color)
    {
        return (piece & colorMask) == color;
    }

    //Cộng giá trị của piece và colorMask bitwise để tìm được màu của piece
    public static int Color(int piece)
    {
        return piece & colorMask;
    }

    //Cộng piece với typeMask để tìm ra loại của piece
    public static int PieceType(int piece)
    {
        return piece & typeMask;
    }

    //Rook == 0b110
    public static bool IsRookOrQueen(int piece)
    {
        return (piece & 0b110) == 0b110;
    }

    //Bishop == 0b101
    public static bool IsBishopOrQueen(int piece)
    {
        return (piece & 0b101) == 0b101;
    }

    //4 == 0b100
    public static bool IsSlidingPiece(int piece)
    {
        //Sliding pieces: rook, bishop, queen
        return (piece & 0b100) != 0;
    }
}
