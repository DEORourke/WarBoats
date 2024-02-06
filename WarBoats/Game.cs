using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WarBoats
{
    // The Game object handles game setup and main gameplay loop. It also tells the Artist what to draw and where as well as validating player input.
    internal class Game
    {
        private int _gridWidth = 10;            // width of each game board
        private int _gridHeight = 10;           // height of each game board. I think at one point I was going to make this adjustable, but battleship has always been 10x10. Might go back and do that if I feel spicy later.
        private int _enemyGridOffset = 240;     // adding this number to a coordinant value causes the Artist to draw a hit/miss in the enemy grid. (grid width + 5 << 4)
        private int _defaultMessageTime = 1500; // number of ms to wait by default when displaying a message

        private List<int> _allCoordinants;      // list of all grid coordiant values
        private char[] _alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private Artist artist;
        private Board playerBoard;
        private Board computerBoard;
        private Opponent opponent;

        public Game(Artist artist, int gridWidth, int gridHeight, int difficulty)
        {
            this._allCoordinants = new List<int>();
            this.BuildAllCoordinantsList();
            this.artist = artist;
            this._gridWidth = gridWidth;
            this._gridHeight = gridHeight;
            this.playerBoard = new Board(
                  this._allCoordinants
                , this._gridWidth
            );
            this.computerBoard = new Board(
                  this._allCoordinants
                , this._gridWidth
            );
            this.opponent = new Opponent(
                  this._allCoordinants
                , this._gridWidth
                , this.playerBoard.GetShipList()
                , difficulty
            );
        }


        // Method called by the program to start a new game. Prompts to set up ships, then goes into the main game loop.
        public void StartNewGame()
        {
            this.DoArtistSetup();

            // Next two line are to speed up debugging (PlayerPopulateShipCoordinants will need to be commented out). Places player ships in a preset location and draws them to the board.
            //this.playerBoard.PopulateTestPlayerShips();
            //artist.DrawBoats(playerBoard.GetShipCoordinates());

            this.playerBoard.PlayerPopulateShipCoordinants(this);  // A reference to the Game is needed for the Board to be able to prompt for ship coordinants.
            this.computerBoard.ComputerPopulateShipCoordinants();
 
            bool playerWins = this.GameLoop();
            string winnerName = playerWins ? "PLAYER" : "COMPUTER";

            if (!playerWins) { ShowEnemyShips(); }

            DisplayText($"GAME OVER. {winnerName} WINS!");
            Console.ReadKey(true);
        }


        // Function for debugging. Prints a grid of all coordinant values, their index, and Row/Column notation to the console
        public void ListGridCoordinants()
        {
            int lIndex = 0;
            for (int i = 0; i < _allCoordinants.Count - 1; i++)
            {
                Console.SetCursorPosition(lIndex, i % _gridHeight);
                int coord = _allCoordinants[i];
                string RCnotation = ConvertBack(coord);
                Console.WriteLine(RCnotation + " - " + coord);
                if (i % _gridHeight == _gridHeight - 1) { lIndex += 10; }
            }
        }


        // Function for debugging. Prints coordinants for each enemy ship to the console below the game board.
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
        

        // Takes a coordinants integer value and returns its Row/Column notation
        private string ConvertBack(int coord)
        {
            int left = (coord >> 4);
            int top = (coord & 0b00001111);
            char alpha = _alphabet[top];
            return $"{alpha}{left}";
        }


        // Creates coordinant values column by column and populates the all coordinants list.
        // Row number is stored in the bottom 4 bits of the intiger (4 bits were chosen to leave room for possible grid expansion). Column number is stored in the top 4 bits.
        public void BuildAllCoordinantsList()
        {
            for (int i = 0; i < _gridWidth; i++)
            {
                for (int j = 0; j < _gridHeight; j++)
                {
                    int val = (i << 4) + j;
                    _allCoordinants.Add(val);
                }
            }
        }


        // Group of methods to call for the Artist class to draw a new game board.
        private void DoArtistSetup()
        {
            Console.Clear();
            this.artist.DrawBorder();
            this.artist.DrawGameDivider();
            this.artist.DrawIndicies();
            this.artist.DrawOcean();
        }


        // Main game loop. Returns true if the player wins and false on a loss.
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


        // Logic for handling player turns. 
        private void PlayerTurn()
        {
            int guess = GetPlayerGuess();

            if (computerBoard.CheckForHit(guess))
            {
                HandlePlayerHit(guess);
            }
            else
            {
                HandleMiss("PLAYER ", guess + _enemyGridOffset);
            }
        }


        // Loops until the player provides a coordinant that hasn't already been guessed.
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


        // Run when a player scores a hit. First calls to the computer Board to track the hit then to the Artist to show the good news.
        private void HandlePlayerHit(int coordinant)
        {
            string secondLine = "";
            Ship sunkShip = computerBoard.HandleHit(coordinant); //Handle Hit method returns a reference to the ship if it is sunk or null otherwise.
            artist.DrawHit(coordinant + _enemyGridOffset);
            if (sunkShip != null) 
            {
                secondLine = $"ENEMY {sunkShip.GetShipName()} SUNK!";
            }
            DisplayText("PLAYER HIT!", secondLine);
        }


        // Logic for requesting and handling computer guesses.
        private void ComputerTurn()
        {
            int guess = opponent.MakeGuess();
            string rcNotation = ConvertBack(guess);

            string msg = $"COMPUTER GUESS {rcNotation}... ";
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


        //  Run when the computer gets a hit. First calls to the player Board to track the hit then to the Artist to show the bad news.
        private void HandleComputerHit(string message, int coordinant)
        {
            // Very similar to the HandlePlayerHit method. I considered combining them, but that would add extra complexity. I ended up siding with simplicity
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


        // Calls to the Artist to draw the miss to the appropriate board and display the miss message
        private void HandleMiss(string message, int coordinant)
        {
            artist.DrawMiss(coordinant);
            DisplayText(message + "MISS");
        }


        // Prompts player to enter a coordinant. Loops until a valid input is received.
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


        // Tells the Artist to clear the message area and write two lines of text then sleeps for the defaul wait time.
        public void DisplayText(string line1, string line2 = "")
        {
            artist.ClearPromptArea();
            artist.WritePrompt(line1, line2);
            System.Threading.Thread.Sleep(_defaultMessageTime);
        }


        // Overload that allows sleep for a specific time.
        public void DisplayText(int sleepTime, string line1, string line2="")
        {
            artist.ClearPromptArea();
            artist.WritePrompt(line1, line2);
            System.Threading.Thread.Sleep(sleepTime);
        }


        // Prompts player to enter a direction for a ship heading. Loops until a valid input is received.
        public string PromptForDirection(string line1, string line2)
        {
            while (true)
            {
                DisplayText(line1, line2);
                string dir = Console.ReadKey().KeyChar.ToString().ToUpper();

                if (Regex.Match(dir, "[N,S,E,W,U,D,L,R]").Success)
                {
                    return dir;
                }

                DisplayText("INVALID DIRECTION.", "USE  U,D,L,R  OR  N,S,E,W");
            }
        }


        // Logic for confirming valid coordinant input from player. Allows for Row/Column or Column/Row input.
        private int ConfirmCoordinant(char y, char x)
        {
            if (Regex.Match($"{y}{x}", "([a-z,A-Z][0-9]|[0-9][a-z,A-Z])").Success) // Matching for one letter and one number
            {
                if (Regex.Match($"{y}", "[a-z,A-Z]").Success) // Runs this block if the letter comes first
                {
                    int alphaIndex = GetAlphaIndex(y);
                    bool alphaInRange = ValidateYCoordinant(alphaIndex);

                    int intIndex = int.Parse(x.ToString());
                    bool intInRange = ValadateXcoordinant(intIndex);

                    if (alphaInRange && intInRange)
                    {
                        return (intIndex << 4) + alphaIndex;
                    }
                }
                else
                {
                    int alphaIndex = GetAlphaIndex(x);
                    bool alphaInRange = ValidateYCoordinant(alphaIndex);

                    int intIndex = int.Parse(y.ToString());
                    bool intInRange = ValadateXcoordinant(intIndex);

                    if (alphaInRange && intInRange)
                    {
                        return (intIndex << 4) + alphaIndex;
                    }
                }
            }
            return -1;
        }


        // Gets the index of a letter in the alphabet. 
        private int GetAlphaIndex(char a)
        {
            char character = a.ToString().ToUpper().ToCharArray()[0];
            return Array.FindIndex(_alphabet, (char item) => { return character == item; });
        }


        // Returns true if the input for the column number is inside the width of the game grid.
        private bool ValadateXcoordinant(int x)
        {
            return ((x >= 0) && (x <= _gridWidth)) ? true : false;
        }


        // Returns true if the input for the row letter is inside the height of the game grid.
        private bool ValidateYCoordinant(int alphaIndex)
        {
            return ((alphaIndex >= 0) && (alphaIndex <= _gridHeight)) ? true : false;
        }


        // Pass-through so the Board can send the Artist a list of coordinants for it to draw ships on.
        public void DrawPlayerShip(List<int> coordinants)
        {
            this.artist.DrawBoats(coordinants);
        }


        // Gets a list of enemy ship coordinants that were not guessed, adds the enemy grid offset to each, then 
        private void ShowEnemyShips()
        {
            List<int> unmodifiedCoordinants = computerBoard.GetUnhitCoordinants();
            List<int> modifiedCoordiants = new List<int>();
            unmodifiedCoordinants.ForEach(coordinant => modifiedCoordiants.Add(coordinant + _enemyGridOffset));
            artist.DrawBoats(modifiedCoordiants);
        }
    }
}
