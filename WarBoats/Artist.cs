using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{

    internal class Artist
    {
        int Gridwidth;
        int Gridheight;
        int TopOffset;  // Row number that the top of the player grid starts at
        int LeftOffset; // column number that the player grid starts at
        int EnemyOffset; // column number that the enemy grid starts at
        int mask = 0b00001111;
        int TextBoxRow = 14;
        int TextAreaWidth = 27;
        int CenterGap;


        int BorderBuffer = 1;   // will be provided by the game laetr. Maybe.
        int CenterBuffer = 2;
        int indexRow = 2;


        char[] Alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M' };

        char WaterChar = '\u2592';          //  
        char BoatChar = '\u2588';           //  
        char BurnChar = '\u2593';           //  
        char MissChar = 'X';

        char BorderTLCorner = '\u2554';     //  
        char BorderTRCorner = '\u2557';     //  
        char BorderBLCorner = '\u255A';     //  
        char BorderBRCorner = '\u255D';     //  
        char BorderHorDouble = '\u2550';    //  
        char BorderVerDouble = '\u2551';    //  
        char BorderVdLS = '\u255F';         //  
        char BorderVdRS = '\u2562';         //  
        char BorderSingleTdn = '\u252C';    //  
        char BorderSingleTup = '\u2534';    //  
        char BorderHorSingle = '\u2500';    //  
        char BorderVerSingle = '\u2502';    //  




        ConsoleColor Fg = Console.ForegroundColor;
        ConsoleColor Bg = Console.BackgroundColor;

        ConsoleColor WaterFG = ConsoleColor.Cyan;
        ConsoleColor WaterBG = ConsoleColor.Blue;

        ConsoleColor MissFG = ConsoleColor.Gray;
        ConsoleColor BurnFG = ConsoleColor.DarkRed;


        public Artist(int Gridwidth, int Gridheight, int TopOffset, int LeftOffset)
        {
            this.Gridwidth = Gridwidth;
            this.Gridheight = Gridheight;
            this.TopOffset = TopOffset;
            this.LeftOffset = LeftOffset;

            this.CenterGap = BorderBuffer * 2 + 2;
            this.EnemyOffset = Gridwidth + 9;
        }


        public void Repeat(char cha, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Console.Write(cha);
            }
        }


        public void Repeat(string str, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                Console.Write(str);
            }
        }


        public void DrawOcean()
        {
            Console.ForegroundColor = WaterFG;
            Console.BackgroundColor = WaterBG;

            for (int i = 0; i < Gridheight; i++)
            {
                Console.SetCursorPosition(LeftOffset, TopOffset + i);
                Repeat(WaterChar, Gridwidth);
                Console.CursorLeft = Console.CursorLeft + CenterGap + 1;
                Repeat(WaterChar, Gridwidth);
            }
            Console.ForegroundColor = Fg;
            Console.BackgroundColor = Bg;
        }


        public void DrawIndicies()
        {
            DrawTopIndex();
            DrawLeftIndex();
        }


        private void DrawTopIndex()
        {

            Console.SetCursorPosition(LeftOffset, TopOffset - 1);
            for (int i = 0; i < Gridwidth; i++)
            {
                Console.Write(i);
            }
            Console.CursorLeft = Console.CursorLeft + CenterGap + 1;
            for (int i = 0; i < Gridwidth; i++)
            {
                Console.Write(i);
            }
        }


        private void DrawLeftIndex()
        {
            int PlayerIndexColumn = LeftOffset - 1;
            int EnemyIndexColumn = PlayerIndexColumn + Gridwidth + CenterGap + 1;
            for (int i = 0; i < Gridheight; i++)
            {
                int currentRow = indexRow + 1 + i;
                Console.SetCursorPosition(PlayerIndexColumn, currentRow);
                Console.Write(Alphabet[i]);
                Console.SetCursorPosition(EnemyIndexColumn, currentRow);
                Console.Write(Alphabet[i]);
            }

        }


        public void SetCursorPosition(int coordinants)
        {
            // int top = (coordinants >> 4) + LeftOffset;
            // int left = (coordinants & mask) + TopOffset;
            // switching these so so we can add to the left int and hit the enemy grid
            int left = (coordinants >> 4) + LeftOffset;
            int top = (coordinants & mask) + TopOffset;

            Console.SetCursorPosition(left, top);
        }


        public void DrawBoats(List<int> coordenants)
        {
            for (int i = 0; i < coordenants.Count; i++)
            {
                this.SetCursorPosition(coordenants[i]);
                Console.Write(BoatChar);
            }
        }


        public void WritePrompt(string prompt)
        {
            Console.SetCursorPosition(LeftOffset - 1, TextBoxRow);
            Console.Write(prompt);
        }


        public void WritePrompt(string line1, string line2)
        {
            Console.SetCursorPosition(LeftOffset - 1, TextBoxRow);
            Console.Write(line1);
            Console.SetCursorPosition(LeftOffset - 1, TextBoxRow + 1);
            Console.Write(line2);

        }


        public void ClearPromptArea()
        {
            Console.SetCursorPosition(LeftOffset - 1, TextBoxRow);
            Repeat(" ", TextAreaWidth);
            Console.SetCursorPosition(LeftOffset - 1, TextBoxRow + 1);
            Repeat(" ", TextAreaWidth);
        }


        public void DrawBorder()
        {

            int centerLinePos = 1 + BorderBuffer + Gridwidth + CenterBuffer + 1;
            int rightLinePos = (centerLinePos * 2);
            int textDeviderRow = Gridheight + 4;
            int bottomRow = textDeviderRow + 2;

            Console.SetCursorPosition(0, 0);

            Console.Write(BorderTLCorner);
            Repeat(BorderHorDouble, 9);
            Console.Write(" WAR BOATS ");
            Repeat(BorderHorDouble, 9);
            Console.Write(BorderTRCorner);

            Console.SetCursorPosition(0, 1);

            Console.Write(BorderVdLS);
            Repeat(BorderHorSingle, 3);
            Console.Write(" PLAYER ");
            Repeat(BorderHorSingle, 3);
            Console.Write(BorderSingleTdn);
            Repeat(BorderHorSingle, 4);
            Console.Write(" ENEMY ");
            Repeat(BorderHorSingle, 3);
            Console.Write(BorderVdRS);


            for (int i = 0; i <= Gridheight; i++)
            {
                DrawMiddleRow(i + 2, true);
            }

            Console.SetCursorPosition(0, Gridheight + 3);

            Console.Write(BorderVdLS);
            Repeat(BorderHorSingle, BorderBuffer + 1 + Gridwidth + CenterBuffer);
            Console.Write(BorderSingleTup);
            Repeat(BorderHorSingle, BorderBuffer + 1 + Gridwidth + CenterBuffer);
            Console.Write(BorderVdRS);

            for (int i = 0; i < 2; i++)
            {
                DrawMiddleRow(i + textDeviderRow, false);
            }

            Console.SetCursorPosition(0, bottomRow);

            Console.Write(BorderBLCorner);
            Repeat(BorderHorDouble, rightLinePos - 1);
            Console.Write(BorderBRCorner);


            void DrawMiddleRow(int row, bool CenterLine)
            {
                Console.SetCursorPosition(0, row);
                Console.Write(BorderVerDouble);
                if (CenterLine)
                {
                    Console.SetCursorPosition(centerLinePos, row);
                    Console.Write(BorderVerSingle);
                }
                Console.SetCursorPosition(rightLinePos, row);
                Console.Write(BorderVerDouble);
            }

        }


        public void NewDrawBorder()
        {
            int rightLinePos = (BorderBuffer + 1 + Gridwidth + CenterBuffer + 1) * 2;
            int middleRowCount = indexRow + Gridheight;
            int textDeviderRow = middleRowCount + 1;

            Console.Write(BorderTLCorner);
            Repeat(BorderHorDouble, 9);
            Console.Write(" WAR BOATS ");
            Repeat(BorderHorDouble, 9);
            Console.Write(BorderTRCorner);

            for (int i = 1; i <= middleRowCount; i++) { DrawMiddleRow(i); }

            Console.SetCursorPosition(0, textDeviderRow);
            Console.Write(BorderVdLS);
            Repeat(BorderHorSingle, rightLinePos - 1);
            Console.Write(BorderVdRS);

            for (int i = 0; i < 2; i++)
            {
                DrawMiddleRow(i + textDeviderRow + 1);
            }

            Console.SetCursorPosition(0, textDeviderRow + 3);
            Console.Write(BorderBLCorner);
            Repeat(BorderHorDouble, rightLinePos - 1);
            Console.Write(BorderBRCorner);

            void DrawMiddleRow(int row)
            {
                Console.SetCursorPosition(0, row);
                Console.Write(BorderVerDouble);
                Console.SetCursorPosition(rightLinePos, row);
                Console.Write(BorderVerDouble);
            }

        }


        public void DrawCenterLines()
        {
            int centerLinePos = 1 + BorderBuffer + Gridwidth + CenterBuffer + 1;
        }


        public void DrawMiss(int coordinants)
        {
            SetCursorPosition(coordinants);

            Console.ForegroundColor = MissFG;
            Console.BackgroundColor = WaterBG;

            Console.Write(MissChar);

            Console.ForegroundColor = Fg;
            Console.BackgroundColor = Bg;
        }


        public void DrawHit(int coordinants)
        {
            SetCursorPosition(coordinants);

            Console.ForegroundColor = BurnFG;
            Console.BackgroundColor = WaterBG;

            Console.Write(BurnChar);

            Console.ForegroundColor = Fg;
            Console.BackgroundColor = Bg;
        }


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

    




