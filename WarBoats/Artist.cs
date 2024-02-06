using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{
    // The Artist class handles moving the cursor position and drawing characters to the screen.
    internal class Artist
    {
        private int _gridWidth;             // width of the play grid
        private int _gridHeight;            // height of the play grid
        private int _topOffset;             // row number that the top of the player grid starts at
        private int _leftOffset;            // column number that the player grid starts at
        private int _enemyOffset;           // column number that the enemy grid starts at
        private int _mask = 0b00001111;     // mask to & a coordinant value with to get the 4 least significant bits
        private int _textBoxRow = 14;       // row number of the first message display line
        private int _textAreaWidth = 27;    // number of characters wide the message area is
        private int _centerGap;             // column number the center divider line is on

        private int _borderBuffer = 1;      // number of columns between the border and the index
        private int _centerBuffer = 2;      // number of columns between the player board and the divider line
        private int _indexRow = 2;          // row number of the column index

        private char[] _alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private char _waterChar = '\u2592';                 // ▒
        private char _hitChar = '\u2593';                   // ▓ 
        private char _boatChar = '\u2588';                  // █
        private char _missChar = '\u2573';                  // ╳

        private char _borderTLCorner = '\u2554';            // ╔
        private char _borderTRCorner = '\u2557';            // ╗
        private char _borderBLCorner = '\u255A';            // ╚
        private char _borderBRCorner = '\u255D';            // ╝
        private char _borderHorDouble = '\u2550';           // ═
        private char _borderVerDouble = '\u2551';           // ║
        private char _borderVerDoubleLSingle = '\u255F';    // ╟
        private char _borderVerDoubleRSingle = '\u2562';    // ╢
        private char _borderSingleTdn = '\u252C';           // ┬
        private char _borderSingleTup = '\u2534';           // ┴
        private char _borderHorSingle = '\u2500';           // ─
        private char _borderVerSingle = '\u2502';           // │

        private ConsoleColor _fgColor = Console.ForegroundColor;
        private ConsoleColor _bgColor = Console.BackgroundColor;

        private ConsoleColor _waterFgColor = ConsoleColor.Cyan;
        private ConsoleColor _waterBgColor = ConsoleColor.Blue;

        private ConsoleColor _missFgColor = ConsoleColor.Gray;
        private ConsoleColor _hitFgColor = ConsoleColor.DarkRed;


        public Artist(int gridWidth, int gridHeight, int topOffset, int leftOffset)
        {
            this._gridWidth = gridWidth;
            this._gridHeight = gridHeight;
            this._topOffset = topOffset;
            this._leftOffset = leftOffset;

            this._centerGap = _borderBuffer * 2 + 2;
            this._enemyOffset = gridWidth + 9;
        }


        // Prints a given character to the console a specified amount of times.
        public void Repeat(char cha, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Console.Write(cha);
            }
        }


        // Prints a given string to the console a specified amount of times.
        public void Repeat(string str, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                Console.Write(str);
            }
        }


        // Draws the ocean characters on the game boards.
        public void DrawOcean()
        {
            Console.ForegroundColor = _waterFgColor;
            Console.BackgroundColor = _waterBgColor;

            for (int i = 0; i < _gridHeight; i++)
            {
                Console.SetCursorPosition(_leftOffset, _topOffset + i);
                Repeat(_waterChar, _gridWidth);
                Console.CursorLeft = Console.CursorLeft + _centerGap + 1;
                Repeat(_waterChar, _gridWidth);
            }
            Console.ForegroundColor = _fgColor;
            Console.BackgroundColor = _bgColor;
        }


        // Method to call other methods to draw the top and left indicies to the game boards.
        public void DrawIndicies()
        {
            DrawTopIndex();
            DrawLeftIndex();
        }


        // Prints the column index above the game boards.
        private void DrawTopIndex()
        {

            Console.SetCursorPosition(_leftOffset, _topOffset - 1);
            for (int i = 0; i < _gridWidth; i++)
            {
                Console.Write(i);
            }
            Console.CursorLeft = Console.CursorLeft + _centerGap + 1;
            for (int i = 0; i < _gridWidth; i++)
            {
                Console.Write(i);
            }
        }


        // Prints the row index on the side of the game boards.
        private void DrawLeftIndex()
        {
            int playerIndexColumn = _leftOffset - 1;
            int enemyIndexColumn = playerIndexColumn + _gridWidth + _centerGap + 1;
            for (int i = 0; i < _gridHeight; i++)
            {
                int currentRow = _indexRow + 1 + i;
                Console.SetCursorPosition(playerIndexColumn, currentRow);
                Console.Write(_alphabet[i]);
                Console.SetCursorPosition(enemyIndexColumn, currentRow);
                Console.Write(_alphabet[i]);
            }
        }


        // Method to place the cursor at a given coordnant on a game board.
        public void SetCursorPosition(int coordinants)
        {
            int left = (coordinants >> 4) + _leftOffset;
            int top = (coordinants & _mask) + _topOffset;

            Console.SetCursorPosition(left, top);
        }


        // Draws ship characters to the game board given a list of coordinants.
        public void DrawBoats(List<int> coordenants)
        {
            for (int i = 0; i < coordenants.Count; i++)
            {
                this.SetCursorPosition(coordenants[i]);
                Console.Write(_boatChar);
            }
        }


        // Prints strings of text to the message display area.
        public void WritePrompt(string line1, string line2="")
        {
            Console.SetCursorPosition(_leftOffset - 1, _textBoxRow);
            Console.Write(line1);
            Console.SetCursorPosition(_leftOffset - 1, _textBoxRow + 1);
            Console.Write(line2);

        }


        // Prints two strings of space characters the width of the message display area to effectivly clear that part of the console.
        public void ClearPromptArea()
        {
            Console.SetCursorPosition(_leftOffset - 1, _textBoxRow);
            Repeat(' ', _textAreaWidth);
            Console.SetCursorPosition(_leftOffset - 1, _textBoxRow + 1);
            Repeat(' ', _textAreaWidth);
        }


        // Draws the boarder around the play area as well as the divider between the game boards and message display area.
        public void DrawBorder()
        {
            int rightLinePos = (_borderBuffer + 1 + _gridWidth + _centerBuffer + 1) * 2;
            int middleRowCount = _indexRow + _gridHeight;
            int textDeviderRow = middleRowCount + 1;

            // Top line of the screen
            Console.Write(_borderTLCorner);
            Repeat(_borderHorDouble, 9);
            Console.Write(" WAR BOATS ");
            Repeat(_borderHorDouble, 9);
            Console.Write(_borderTRCorner);

            for (int i = 1; i <= middleRowCount; i++) { DrawMiddleRow(i); }

            // Divider row between game boards and message display area
            Console.SetCursorPosition(0, textDeviderRow);
            Console.Write(_borderVerDoubleLSingle);
            Repeat(_borderHorSingle, rightLinePos - 1);
            Console.Write(_borderVerDoubleRSingle);

            for (int i = 0; i < 2; i++)
            {
                DrawMiddleRow(i + textDeviderRow + 1);
            }

            // Bottom row
            Console.SetCursorPosition(0, textDeviderRow + 3);
            Console.Write(_borderBLCorner);
            Repeat(_borderHorDouble, rightLinePos - 1);
            Console.Write(_borderBRCorner);

            void DrawMiddleRow(int row)
            {
                Console.SetCursorPosition(0, row);
                Console.Write(_borderVerDouble);
                Console.SetCursorPosition(rightLinePos, row);
                Console.Write(_borderVerDouble);
            }
        }


        // Draws the game board label line as well as the center dividing line.
        public void DrawGameDivider()
        {
            int centerLinePos = 1 + _borderBuffer + _gridWidth + _centerBuffer + 1;
            int middleRowCount = _indexRow + _gridHeight;
            int textDeviderRow = middleRowCount + 1;

            // Game board headers
            Console.SetCursorPosition(0, _indexRow - 1);
            Console.Write(_borderVerDoubleLSingle);
            Repeat(_borderHorSingle, 3);
            Console.Write(" PLAYER ");
            Repeat(_borderHorSingle, 3);
            Console.Write(_borderSingleTdn);
            Repeat(_borderHorSingle, 4);
            Console.Write(" ENEMY ");
            Repeat(_borderHorSingle, 3);
            Console.Write(_borderVerDoubleRSingle);

            for( int i = _indexRow; i <= _gridHeight + _indexRow; i++)
            {
                Console.SetCursorPosition(centerLinePos, i);
                Console.Write(_borderVerSingle);

            }

            Console.SetCursorPosition(centerLinePos, _textBoxRow - 1);
            Console.Write(_borderSingleTup);
        }


        // Draws the miss character at a given coordinant.
        public void DrawMiss(int coordinants)
        {
            SetCursorPosition(coordinants);

            Console.ForegroundColor = _missFgColor;
            Console.BackgroundColor = _waterBgColor;

            Console.Write(_missChar);

            Console.ForegroundColor = _fgColor;
            Console.BackgroundColor = _bgColor;
        }


        // Draws the hit character at a given coordinant.
        public void DrawHit(int coordinants)
        {
            SetCursorPosition(coordinants);

            Console.ForegroundColor = _hitFgColor;
            Console.BackgroundColor = _waterBgColor;

            Console.Write(_hitChar);

            Console.ForegroundColor = _fgColor;
            Console.BackgroundColor = _bgColor;
        }


        // Draws a cool war boat for the title screen.
        public void DrawSweetGraphics()
        {
            Console.SetCursorPosition(19, 4);   Console.Write(".");
            Console.SetCursorPosition(17, 5);   Console.Write(",'");
            Console.SetCursorPosition(14, 6);   Console.Write("__||_");
            Console.SetCursorPosition(14, 7);   Console.Write("| o |");
            Console.SetCursorPosition( 4, 8);   Console.Write("====||__---------_||===");
            Console.SetCursorPosition( 2, 9);   Console.Write("<------- o  o  o  o ------>");
            Console.SetCursorPosition( 3,10);   Console.Write("\\_______________________/\\");
        }
    }
}

    




