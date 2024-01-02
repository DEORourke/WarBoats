using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{
    internal class Board
    {
        private List<int> ShipCoordinates = new List<int>();
        private int HitsTaken;
        private List<int> GuessList = new List<int>();
        private List<Ship> ShipList = new List<Ship>();
        private List<int> AllCoordinants = new List<int>();
        private List<int> AvailableCoordinants = new List<int>();
        private int gridwidth;

        public Board(List<int> AllCoordinants, int gridwidth)
        {
            MakeShipList();
            this.AllCoordinants = AllCoordinants;
            AvailableCoordinants = new List<int>(AllCoordinants);
            this.gridwidth = gridwidth;
            this.HitsTaken = 0;
        }

        public void PopulateTestPlayerShips()
        {
            AddShipToList(4, 5, 10);
            AddShipToList(101, 4, 1);
            AddShipToList(23, 3, 10);
            AddShipToList(129, 3, 1);
            AddShipToList(150, 2, 1);
        }

        public void PlayerPopulateShipCoordinants(Game game)
        {
        // Maybe rework this process so the board doesn't talk to the artist class to keep thing seperate.
            foreach (Ship shp in ShipList)
            {
                bool shipAdded = false;
                while (!shipAdded)
                {
                    int nose = game.PromptForCoordinant("WHERE IS THE NOSE OF THE", $"{shp.name}({shp.length})? ");
                    string dir = game.PromptForDirection("WHICH DIRECTION IS THE", $"{shp.name}({shp.length}) FACING? ");
                    int directionNumber = GetDirectionNumber(dir);

                    shipAdded = AddShipToList(nose, shp.length, directionNumber);
                    if (shipAdded)
                    {
                        game.DoDrawPlayerBoat(this.ShipCoordinates);
                        continue;
                    }

                    game.DisplayText("INVALID PLACEMENT. SHIP", "OVERLAP OR OUTSIDE OF GRID");
                }
            }
        }


        public List<int> ComputerPopulateShipCoordinants()
        {
            Random rand = new Random();
            foreach (Ship shp in ShipList)
            {
                bool shipAdded = false;
                while (!shipAdded)
                {
                    List<int> thisShipAvailableCoords = new List<int>(AvailableCoordinants);
                    List<string> cardinalDirections = new List<string> { "N","S","E","W" };

                    int noseindex = rand.Next(0, thisShipAvailableCoords.Count - 1);
                    int nose = thisShipAvailableCoords[noseindex];
                    thisShipAvailableCoords.RemoveAt(noseindex);

                    while (cardinalDirections.Count > 0)    // Attempting to place ship using all four directions before picking a new coordinant
                    {
                        int randomDirectionIndex = rand.Next(0, cardinalDirections.Count - 1);
                        int directionNumber = GetDirectionNumber(cardinalDirections[randomDirectionIndex]);
                        cardinalDirections.RemoveAt(randomDirectionIndex);

                        shipAdded = AddShipToList(nose, shp.length, directionNumber);

                        if (shipAdded) { break; }
                    }
                }
            }

            return(ShipCoordinates);

        }
    

        private bool AddShipToList(int coordinant, int length, int dir)
        {
            List<int> tempList = new List<int>();

            if (ShipCoordinates.Contains(coordinant)){ return false; }
            tempList.Add(coordinant);

            for (int i = 1; i < length ; i++)
            {
                int lastCoord = tempList.Last();
                int lastIndex = AllCoordinants.IndexOf(lastCoord);
                int nextIndex = lastIndex + dir;
                if ((nextIndex < 0) || (nextIndex > AllCoordinants.Count -1)) { return false; } // checking if the ship does not fit on the grid vertically
                int nextCoord = AllCoordinants[nextIndex];
                if (ShipCoordinates.Contains(nextCoord)) { return false; }
                if ((lastCoord >> 4 != nextCoord >> 4) & (Math.Abs(dir) == 1)) { return false; } // checking if the ship does not fit on the grid horazontally
                tempList.Add(nextCoord);
            }

            foreach (var coord in tempList)
            {
                ShipCoordinates.Add(coord);
                AvailableCoordinants.Remove(coord);
            }

            return true;
        }


        private int GetDirectionNumber(string dir)
        {
            return dir switch
            {
                /*
                "N" or "U" => this.gridwidth,
                "S" or "D" => this.gridwidth * -1,
                "E" or "R" => -1,
                "W" or "L" => 1,
                _ => 0, 
                */

                "N" or "U" => 1,
                "S" or "D" => -1,
                "E" or "R" => this.gridwidth * -1,
                "W" or "L" => this.gridwidth,
                _ => 0,
            };
        }

        private void MakeShipList()
        {
            ShipList.Add(new Ship("CARRIER", 5));
            ShipList.Add(new Ship("BATTLESHIP", 4));
            ShipList.Add(new Ship("CRUISER", 3));
            ShipList.Add(new Ship("SUBMARINE", 3));
            ShipList.Add(new Ship("DESTROYER", 2));
        }

        public List<Ship> GetShipList()
        {
            return ShipList;
        }

        public bool CheckForHit(int coordinant)
        {
            GuessList.Add(coordinant);
            return ShipCoordinates.Contains(coordinant);
        }

        public Ship HandleHit(int coordinant)
        {
            this.HitsTaken++;
            int shipIndex = DetermineHitShipIndex(coordinant);
            Ship hitShip = ShipList[shipIndex];
            hitShip.HitShip();
            if (hitShip.IsShipSunk())
            { 
                return hitShip;   // returns a refference to the ship if sunk, otherwise returns null
            }
            return null;
        }

        private int DetermineHitShipIndex(int coordinant)
        {   
            // Because we always load the ships in the same order, we can use the index of the coordinant in the ShipCoordinants list to get the index of the right ship from the ShipList
            switch (ShipCoordinates.IndexOf(coordinant))
            {
                case <= 4:
                    return 0;
                case <= 8:
                    return 1;
                case <= 11:
                    return 2;
                case <= 14:
                    return 3;
                case <= 16:
                    return 4;
                default: return -1;
            }
        }

        public bool CheckAlreadyGuessed(int coordinant)
        {
            return GuessList.Contains(coordinant);
        }

        public int GetHitsRemaining()
        {
            return ShipCoordinates.Count;
        }

        public bool AllShipsSunk()
        {
            return ShipCoordinates.Count <= HitsTaken;
        }

        public List<int> GetShipCoordinates() { return ShipCoordinates; }
    }
}
