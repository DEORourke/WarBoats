using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WarBoats
{
    // The Board class takes care of setting up and keeping track of where ships are on the grid. Also checks whether guesses are hits or misses and keeps track of hits scored.
    internal class Board
    {
        private int _gridWidth;                     // width of the play grid. Used when placing ships to determine if all spaces are contiguous
        private int _hitsTaken;                     // number of hits scored against ships on the board
        private List<int> _guessList;               // list of grid coordinant values that have been guessed. Both hit and miss
        private List<int> _shipCoordinates;         // list of grid coordinants where shpis pieces have been placed
        private List<int> _allCoordinants;          // list of all grid coordiant values 
        private List<int> _availableCoordinants;    // list of grid coordinant values where a ship piece may be placed
        private List<Ship> _shipList;               // list of ships on the board

        public Board(List<int> allCoordinants, int gridWidth)
        {
            this._shipList = new List<Ship>();
            MakeShipList();
            this._allCoordinants = new List<int>(allCoordinants);
            this._availableCoordinants = new List<int>(allCoordinants);
            this._gridWidth = gridWidth;
            this._hitsTaken = 0;
            this._shipCoordinates = new List<int>();
            this._guessList = new List<int>();
        }


        // Function for debugging. Places player ships at pre-specified locations.
        public void PopulateTestPlayerShips()
        {
            AddShipToList(4, 5, 10);
            AddShipToList(101, 4, 1);
            AddShipToList(23, 3, 10);
            AddShipToList(129, 3, 1);
            AddShipToList(150, 2, 1);
        }


        // Loops through each player ship and prompts for a coorinant and direction and draws the ship to the game board when a valid coordinant is received.
        public void PlayerPopulateShipCoordinants(Game game)
        {
            foreach (Ship shp in _shipList)
            {
                bool shipAdded = false;
                while (!shipAdded)
                {
                    string listing = shp.GetShipListing();

                    int nose = game.PromptForCoordinant("WHERE IS THE NOSE OF THE", $"{listing}? ");
                    string dir = game.PromptForDirection("WHICH DIRECTION IS THE", $"{listing} FACING? ");

                    int directionNumber = GetDirectionNumber(dir);

                    shipAdded = AddShipToList(nose, shp.GetShipLength(), directionNumber);
                    if (shipAdded)
                    {
                        game.DrawPlayerShip(this._shipCoordinates);
                        continue;
                    }

                    game.DisplayText("INVALID PLACEMENT. SHIP", "OVERLAP OR OUTSIDE OF GRID");
                }
            }
        }


        // Logic to randomly place all computer opponent ships.
        public List<int> ComputerPopulateShipCoordinants()
        {
            Random rand = new Random();
            foreach (Ship shp in _shipList)
            {
                bool shipAdded = false;
                while (!shipAdded)
                {
                    List<int> thisShipAvailableCoords = new List<int>(_availableCoordinants);
                    List<string> cardinalDirections = new List<string> { "N","S","E","W" };

                    int noseindex = rand.Next(0, thisShipAvailableCoords.Count - 1);
                    int nose = thisShipAvailableCoords[noseindex];
                    thisShipAvailableCoords.RemoveAt(noseindex);

                    while (cardinalDirections.Count > 0)    // Attempts to place ship using all four directions before picking a new coordinant
                    {
                        int randomDirectionIndex = rand.Next(0, cardinalDirections.Count - 1);
                        int directionNumber = GetDirectionNumber(cardinalDirections[randomDirectionIndex]);
                        cardinalDirections.RemoveAt(randomDirectionIndex);

                        shipAdded = AddShipToList(nose, shp.GetShipLength(), directionNumber);

                        if (shipAdded) { break; }
                    }
                }
            }

            return(_shipCoordinates);

        }


        // Validates that each part of a ship would be in a legal coordiannt before adding them to the _shipCoordinates list. Returns false if the ship is not able to be placed.
        private bool AddShipToList(int coordinant, int length, int dir)
        {
            List<int> tempList = new List<int>();

            if (_shipCoordinates.Contains(coordinant)){ return false; }
            tempList.Add(coordinant);

            for (int i = 1; i < length ; i++)
            {
                int lastCoord = tempList.Last();
                int lastIndex = _allCoordinants.IndexOf(lastCoord);
                int nextIndex = lastIndex + dir;
                if ((nextIndex < 0) || (nextIndex > _allCoordinants.Count -1)) { return false; }  // Checking if the ship does not fit on the grid vertically
                int nextCoord = _allCoordinants[nextIndex];
                if (_shipCoordinates.Contains(nextCoord)) { return false; }
                if ((lastCoord >> 4 != nextCoord >> 4) & (Math.Abs(dir) == 1)) { return false; }  // Checking if the ship does not fit on the grid horazontally
                tempList.Add(nextCoord);
            }

            foreach (var coord in tempList)
            {
                _shipCoordinates.Add(coord);
                _availableCoordinants.Remove(coord);
            }
            return true;
        }


        // Takes a direction and returns the number of places in the _allCoordinants index to move for that direction.
        private int GetDirectionNumber(string dir)
        {
            return dir switch
            {
                "N" or "U" => 1,
                "S" or "D" => -1,
                "E" or "R" => this._gridWidth * -1,
                "W" or "L" => this._gridWidth,
                _ => 0,
            };
        }


        // Adds the 5 different types of ships to the _shipList list.
        private void MakeShipList()
        {
            _shipList.Add(new Ship("CARRIER", 5));
            _shipList.Add(new Ship("BATTLESHIP", 4));
            _shipList.Add(new Ship("CRUISER", 3));
            _shipList.Add(new Ship("SUBMARINE", 3));
            _shipList.Add(new Ship("DESTROYER", 2));
        }


        // Returns a reference to the _shipList for the Opponent.
        public List<Ship> GetShipList()
        {
            return _shipList;
        }


        // Adds a given coordinant to the _guesList then returns true if the coorinant is contained in the _shipCoordinants list.
        public bool CheckForHit(int coordinant)
        {
            _guessList.Add(coordinant);
            return _shipCoordinates.Contains(coordinant);
        }


        // Increments the hit counter, checks to see which ship was hit, then returns a Ship if one was sunk.
        public Ship HandleHit(int coordinant)
        {
            this._hitsTaken++;
            int shipIndex = DetermineHitShipIndex(coordinant);
            Ship hitShip = _shipList[shipIndex];
            hitShip.HitShip();
            if (hitShip.IsShipSunk())
            { 
                return hitShip;   // returns a refference to the ship if sunk, otherwise returns null
            }
            return null;
        }


        // Takes a given coordinant value and returns the index of a ship in the _shipList based on where the coodinant is in the _shipCoordinants list.
        private int DetermineHitShipIndex(int coordinant)
        {
            // Because we always load the ships in the same order, we can use the index of the coordinant in the ShipCoordinants list to get the index of the right ship from the ShipList
            /* switch (_shipCoordinates.IndexOf(coordinant))
               {
                    case <= 4: return 0;
                    case <= 8: return 1;
                    case <= 11: return 2;
                    case <= 14: return 3;
                    case <= 16: return 4;
                    default: return -1;
                }
            */
            return _shipCoordinates.IndexOf(coordinant) switch
            {
                <=  4 => 0,
                <=  8 => 1,
                <= 11 => 2,
                <= 14 => 3,
                <= 16 => 4,
                _ => -1
            };

        }


        // Checks to see if a coordinant has already been guessed and returns true or false.
        public bool CheckAlreadyGuessed(int coordinant)
        {
            return _guessList.Contains(coordinant);
        }


        // Checks to see if the _hitsTaken counter is equal to the the length of the _shipCoordinants list to signify the end of the game.
        public bool AllShipsSunk()
        {
            return _shipCoordinates.Count <= _hitsTaken;
        }


        // Returns a reference to the _shipCoordiants list.
        public List<int> GetShipCoordinates() { return _shipCoordinates; }
    }
}
