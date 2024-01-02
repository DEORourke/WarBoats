using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WarBoats
{
    internal class Game
    {
        int Gridwidth = 10;
        int Gridheight = 10;
        int TopOffset = 3;  // two header rows and one index row
        int LeftOffset = 3; // Border column, buffer, and index column
        List<int> AllCoordinants = new List<int>();
        int EnemyGridOffset = 240;
        int defaultMessageTime = 1500; // initial value 1500

        char[] Alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M' };

        Artist artist;
        Board playerBoard;
        Board computerBoard;
        Opponent opponent;

        public Game()
        {
            this.BuildAllCoordinantsList();
            this.artist = new Artist(
                  this.Gridwidth
                , this.Gridheight
                , this.TopOffset
                , this.LeftOffset
            );
            this.playerBoard = new Board(
                  this.AllCoordinants   // Are we making a real copy of this list, or passing in a reference? Make experament to Create board then remove itmes from the list in Game
                , this.Gridwidth
            );
            this.computerBoard = new Board(
                  this.AllCoordinants
                , this.Gridwidth
            );
            this.opponent = new Opponent(
                  this.AllCoordinants
                , this.Gridwidth
                , this.playerBoard.GetShipList()
            );
        }

        public void TitleScreen()
        {
            Console.Clear();
            this.artist.NewDrawBorder();
            this.artist.DrawSweetGraphics();
            this.artist.WritePrompt("     PRESS THE ANY KEY", "       FOR NEW GAME");
            Console.ReadKey(true);
        }

        public void RunGame()
        {
            Console.Clear();
            this.DoArtistSetup();
            
            // for speeding up testing
         //   this.playerBoard.PopulateTestPlayerShips();
         //   artist.DrawBoats(playerBoard.GetShipCoordinates());
                        
            this.playerBoard.PlayerPopulateShipCoordinants(this);
            this.computerBoard.ComputerPopulateShipCoordinants();
            // this.Cheat();
            bool playerWins = this.GameLoop();
            string winnerName = playerWins ? "PLAYER" : "COMPUTER";

            DisplayText($"GAME OVER. {winnerName} WINS!");
            Console.SetCursorPosition(0, 20);
        }


        public void TestFun()
        { 

        }

        public void ListGridCoordinants()
        {
            int lIndex = 0;
            for (int i = 0; i < AllCoordinants.Count - 1; i++)
            {
                Console.SetCursorPosition(lIndex, i % Gridheight);
                int coord = AllCoordinants[i];
                string RCnotation = ConvertBack(coord);
                Console.WriteLine(RCnotation + " - " + coord);
                if (i % Gridheight == Gridheight - 1) { lIndex += 10; }
            }
        }

        public void TestFun2()
        {
            this.DoArtistSetup();
            this.playerBoard.PopulateTestPlayerShips();
            artist.DrawBoats(playerBoard.GetShipCoordinates());
            int guessCounter = 0;
            int guess;
            while (true)
            {
                /*
                if (guessCounter == 0) { guess = opponent.ControlledGuess(65); }
                else if (guessCounter == 1) { guess = opponent.ControlledGuess(20); }
                else { guess = opponent.MakeGuess(); }
                */

                guess = opponent.MakeGuess();
                string RCnotation = ConvertBack(guess);
                
                string msg = $"COMPUTER GUESS {RCnotation}... ";
                DisplayText(100, msg); // initial value 500

                if (playerBoard.CheckForHit(guess))
                {   
                    HandleComputerHit(msg, guess);
                    opponent.ConfirmHit();
                } 
                else
                {
                    HandleMiss(msg, guess);
                    opponent.HandleMiss();
                }
                guessCounter++;
                artist.ClearPromptArea();
                System.Threading.Thread.Sleep(100); // initial value 500
            }

        }
        
        public void Cheat()
        {
            List<int> eShips = computerBoard.GetShipCoordinates();
            Console.SetCursorPosition(0, 20);

            int i = 0;
            foreach (int spot in eShips)
            {
                string coord = ConvertBack(spot); 
                Console.Write(coord + " ");
                    if ( i==4 || i==8 || i==11 || i==14 )
                    {
                        Console.WriteLine(" ");
                    }
                i++;
            }
        }
        
        private string ConvertBack(int coord)
        {
            int left = (coord >> 4);
            int top = (coord & 0b00001111);
            char alpha = Alphabet[top];
            return $"{alpha}{left}";
        }

        public List<int> GetAllCoordinantsList() { return this.AllCoordinants; }
        public char[] GetAlphabet() { return this.Alphabet; }

        public void BuildAllCoordinantsList()
        {
            for (int i = 0; i < Gridwidth; i++)
            {
                for (int j = 0; j < Gridheight; j++)
                {
                    int val = (i << 4) + j;
                    AllCoordinants.Add(val);
                }
            }
        }

        private void DoArtistSetup()
        {
            // Shit for the artist class to do here
            Console.Clear();
            this.artist.DrawBorder();
            this.artist.DrawIndicies();
            this.artist.DrawOcean();
        }

        private bool GameLoop()
        {
            while (true)
            {
                PlayerTurn();
                if (computerBoard.AllShipsSunk()) { return true; }
                
                ComputerTurn();
                if (playerBoard.AllShipsSunk()) { return false; }
            }
        }

        private void PlayerTurn()
        {
            int guess = GetPlayerGuess();

            if (computerBoard.CheckForHit(guess))
            {
                HandlePlayerHit(guess);
            }
            else
            {
                HandleMiss("PLAYER ", guess + EnemyGridOffset);
            }
        }

        private int GetPlayerGuess()
        {
            int guess;
            while (true)
            {
                guess = PromptForCoordinant("ATTACK WHICH COORDIANT?");
                if (!computerBoard.CheckAlreadyGuessed(guess))
                { 
                    return guess; 
                }
                DisplayText("COORDIANT ALREDY GUESSED.", "TRY AGAIN");
            }
        }

        private void HandlePlayerHit(int coordinant)
        {
            string secondLine = "";
            Ship sunkShip = computerBoard.HandleHit(coordinant);
            artist.DrawHit(coordinant + EnemyGridOffset);
            if (sunkShip != null) 
            {
                secondLine = $"ENEMY {sunkShip.GetShipName()} SUNK!";
            }
            DisplayText("PLAYER HIT!", secondLine);
        }

        private void ComputerTurn()
        {
            int guess = opponent.MakeGuess();
            string RCnotation = ConvertBack(guess);

            string msg = $"COMPUTER GUESS {RCnotation}... ";
            DisplayText(500, msg); // initial value 500

            if (playerBoard.CheckForHit(guess))
            {
                HandleComputerHit(msg, guess);
                opponent.ConfirmHit();
            }
            else
            {
                HandleMiss(msg, guess);
                opponent.HandleMiss();
            }

            System.Threading.Thread.Sleep(500); // initial value 500
        }

        private void HandleComputerHit(string message, int coordinant)
        {
            string secondLine = "";
            Ship sunkShip = playerBoard.HandleHit(coordinant);
            artist.DrawHit(coordinant);
            if (sunkShip != null)
            {
                secondLine = $"YOUR {sunkShip.GetShipName()} HAS SUNK";
                opponent.TargetDestroyed();
            }
            DisplayText(message + "HIT", secondLine);
        }


        private void HandleMiss(string message, int coordinant)
        {
            artist.DrawMiss(coordinant);
            DisplayText(message + "MISS");
        }

        public void ListAllCoordinants()
        {
            for (int i = 0; i < Gridheight * Gridwidth; i++)
            {
                //if (i % 10 == 0) { Console.Write("\n"); }
                Console.Write(AllCoordinants[i] + ", ");
            }
        }


        public int PromptForCoordinant(string line1, string line2 = "")
        {
            while (true)
            {
                this.DisplayText(line1, line2);

                char yCoordinant = Console.ReadKey().KeyChar;
                char xCoordinant = Console.ReadKey().KeyChar;

                int coordinant = ConfirmCoordinant(yCoordinant, xCoordinant);
                if (coordinant >= 0) { return coordinant; }

                this.DisplayText("", "INVALID COORDINANTS");
            }
        }

        public void DisplayText(int sleepTime, string line1, string line2="")
        {
            artist.ClearPromptArea();
            artist.WritePrompt(line1, line2);
            System.Threading.Thread.Sleep(sleepTime);
        }

        public void DisplayText(string line1, string line2="") 
        {
            artist.ClearPromptArea();
            artist.WritePrompt(line1, line2);
            System.Threading.Thread.Sleep(defaultMessageTime);
        }

        public string PromptForDirection(string line1, string line2)
        {
            while (true)
            {
                artist.WritePrompt(line1, line2);
                string dir = Console.ReadKey().KeyChar.ToString().ToUpper();

                if (Regex.Match(dir, "[N,S,E,W,U,D,L,R]").Success)
                {
                    return dir;
                }

                artist.ClearPromptArea();
                artist.WritePrompt("INVALID DIRECTION.", "USE  U,D,L,R  OR  N,S,E,W");
                System.Threading.Thread.Sleep(1500);
                artist.ClearPromptArea();
            }
        }

        private int ConfirmCoordinant(char y, char x)
        {
            if (Regex.Match($"{y}{x}", "([a-z,A-Z][0-9]|[0-9][a-z,A-Z])").Success)
            {
                if (Regex.Match($"{y}", "[a-z,A-Z]").Success)
                {
                    int alphaIndex = GetAlphaRange(y);
                    bool alphaInRange = ValidateYCoordinant(alphaIndex);

                    int intIndex = int.Parse(x.ToString());
                    bool intInRange = ValadateXcoordinant(intIndex);

                    if (alphaInRange && intInRange)
                    {
                        // return (alphaIndex << 4) + intIndex;
                        return (intIndex << 4) + alphaIndex;

                    }
                }
                else
                {
                    int alphaIndex = GetAlphaRange(x);
                    bool alphaInRange = ValidateYCoordinant(alphaIndex);

                    int intIndex = int.Parse(y.ToString());
                    bool intInRange = ValadateXcoordinant(intIndex);

                    if (alphaInRange && intInRange)
                    {
                        // return (alphaIndex << 4) + intIndex;
                        return (intIndex << 4) + alphaIndex;
                    }
                }
            }
            return -1;
        }

        private int GetAlphaRange(char a)
        {
            char character = a.ToString().ToUpper().ToCharArray()[0];
            int index = Array.FindIndex(Alphabet, (char item) => { return character == item; });
            return index;
        }

        private bool ValadateXcoordinant(int x)
        {
            if ((x >= 0) && (x <= Gridwidth)) { return true; }
            return false;
        }

        private bool ValidateYCoordinant(int alphaIndex)
        {
            if ((alphaIndex >= 0) && (alphaIndex <= Gridheight)) { return true; }
            return false;
        }

        public void DoDrawPlayerBoat(List<int> coordinants)
        {
            this.artist.DrawBoats(coordinants);
        }


        private bool WinConditionMet(int remainingLocations)
        {
            if (remainingLocations > 0) { return false; }
            return true;
        }
    }
}
