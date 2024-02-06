using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{
    // The Opponent class handles logic for guessing player ship locations and hunting them down when a ship is hit.
    internal class Opponent
    {
        private int _lastGuess;                 // last coordinant guessed
        private int _firstHit;                  // randomly guessed coordinant where a ship was first hit
        private int _gridWidth;                 // width of the play grid. used to find coordinants on the same row as another coordinant
        private int _attackDirection;           // direction and number of places in the _allCoordinants index to find an adjacent coordinant
        private int _playerShipsSunk;           // count of how many player ships have been unalived
        private bool _lastGuessHit;             // whether or not the last guess was a hit
        private bool _searchingForTarget;       // whether or not the Opponent is searching for a ship or is activly attacking one. used to determine which method to use to get next guess
        private bool _onTarget;                 // whetehr or not the Opponent should continue to guess coordinants in the same attack direction
        private int _dificultyLevel;            // determines which functions are used to gess where to attack when searching for a player ship
        private int _smallestShip;              // length of the smallest player shipt that has not yet been sunk
        private int _emptySpacesRequired;               // number of spaces adjacent to a given coordinant that need to be empty to be guessed in medium dificulty

        private List<int> _allCoordinants;          // list of all grid coordiant values
        private List<int> _gridCoordinants;
        private List<int> _availableCoordinants;    // list of grid coordinant values that have not been guessed 
        private List<int> _cardinalDirections;      // list of values 
        private List<int> _availableDirections;     // shallow copy of cardinal directions. values are removed as they are used
        private List<Ship> _playerShips;            // reference to the list of ships from the player Board

        private Random _rand;

        public Opponent(List<int> allCoordinants, int gridwidth, List<Ship> playerShips,int dificultyLevel)
        {
            this._allCoordinants = allCoordinants;
            this._availableCoordinants = new List<int>(allCoordinants);
            this._gridWidth = gridwidth;
            this._playerShips = playerShips;
            this._availableDirections = new List<int>();
            this._gridCoordinants = new List<int>();
            this._rand = new Random();
            this._dificultyLevel = dificultyLevel;
            TargetDestroyed(); // Starts out searching for a ship
            
            this._cardinalDirections = new List<int> {1, -1, _gridWidth, _gridWidth * -1};
            this._playerShipsSunk = CountShipsSunk();
            this._emptySpacesRequired = 4;
        }


        // Determines which method to use to get a guess for the computer turn.
        public int MakeGuess()
        {
            if (_searchingForTarget)
            {
                return _dificultyLevel switch
                {
                    1 => ShotInTheDark(),  // easy mode
                    2 => GridGuess(),  // medium
                    // 3 => ProbabilityGuess(),  // hard mode : Determines next coordinant based on probability matrix. May or may not happen based on if my brain cooperates.
                    _ => ShotInTheDark()
                };
            }
            return EducatedGuess();
        }


        // Easy Mode: this function picks a random available coordinant to attack.
        private int ShotInTheDark()
        {
            int guessIndex = _rand.Next(_availableCoordinants.Count - 1);
            int guess = _availableCoordinants[guessIndex];
            _availableCoordinants.RemoveAt(guessIndex);
            _lastGuess = guess;
            return guess;
        }
         

        // Medium difficulty: makes guesses from a grid of coordinants where the spacing is based on the shortest player ship that has not been sunk. Preferrs guesses with the neighbors that have not been guessed.
        private int GridGuess()
        {
            List<int> tempAvailableCoordinants = new List<int>(_gridCoordinants);
            while (true)
            {
                if (tempAvailableCoordinants.Count == 0)
                {
                    tempAvailableCoordinants = new List<int>(_gridCoordinants);
                    this._emptySpacesRequired--;
                }

                int guessIndex = _rand.Next(tempAvailableCoordinants.Count - 1);
                int guess = tempAvailableCoordinants[guessIndex];
                tempAvailableCoordinants.Remove(guess);

                if (ConfirmEmptyNeighbors(guess, _emptySpacesRequired))
                {
                    _availableCoordinants.Remove(guess);
                    _gridCoordinants.Remove(guess);
                    _lastGuess = guess;
                    return guess;
                }
            }
        }


        // This function is used to determine where to attack after a ship has been hit while searching but before a ship has been marked as sunk.
        private int EducatedGuess()
        {
            while (true)
            {   // Selecting the point of origin for attack based on if the last guess was a hit or not
                int initialCoordinant = (_lastGuessHit) ? _lastGuess : _firstHit;
                
                // If a new ship is found, or if the computer ran out of directions to attack in, get a new list of cardinal directions and try one.
                if (_attackDirection == 0)
                {
                    _availableDirections = new List<int>(_cardinalDirections);
                    //attackDirection = PickAttackDirection(firstHit);  
                    _attackDirection = PickAttackDirection();
                }


                // If the computer has gotten multiple hits in one direction and then scores a miss, turns around and attacks again from the first hit location
                if (!_lastGuessHit & _onTarget)
                {
                    _attackDirection = _attackDirection *= -1;
                    _availableDirections.Remove(_attackDirection);
                }
                
                // Default state. If we get a miss, pick a different direction to try.
                else if (!_lastGuessHit)
                {
                    //attackDirection = PickAttackDirection(firstHit);
                    _attackDirection = PickAttackDirection();
                }


                int nextCoordiantIndex = GetNextCoordinantIndex(initialCoordinant, _attackDirection);
                if (!ValidateLegalCoordinant(initialCoordinant, _attackDirection, nextCoordiantIndex))
                {
                    // treating the potenital guess as a miss in order to pick a new direction
                    _lastGuessHit = false; 
                    continue;
                }

                int guess = _allCoordinants[nextCoordiantIndex];
                _availableCoordinants.Remove(guess);
                _lastGuess = guess;
                return guess;
            }
        } 


        // Uses a given coordinant and a value for the attack direction to get the index of the next guess in AllCoordinants.
        private int GetNextCoordinantIndex(int seedCoordinant, int direction)
        {
            int seedCoordinantIndex = _allCoordinants.IndexOf(seedCoordinant);
            return seedCoordinantIndex + direction;
        }


        // Logic to run when the previous guess came back as a hit.
        public void ConfirmHit()
        {
            if (_searchingForTarget)
            {
                _searchingForTarget = false;
                _firstHit = _lastGuess;
            }

            CheckIfOnTarget();
            _lastGuessHit = true;
            CheckForSunkShip();
        }


        // Logic to run when the previous guess came back as a miss.
        public void HandleMiss()
        {
            _lastGuessHit = false;
        }


        // Clears the variables associated with attacking a ship after one has been located. Sets Opponent back to searching for a new ship.
        public void TargetDestroyed()
        {
            _lastGuessHit = false;
            _searchingForTarget = true;
            _attackDirection = 0;
            _firstHit = -1;
            _onTarget = false;

            if (_dificultyLevel == 2)
            {   // Gets a new list of grid coordiants to search for a potentially new smallest ship for medium difficulty.
                _gridCoordinants = GetSearchGrid();
            }
        }


        // Picks randomly from a list of cardinal directions for the next guess to go in.
        private int PickAttackDirection()
        //private int PickAttackDirection(int coordinant)

        {
            // If we somehow find our way here with a blank directions list, sets attackDirection to 0. This won't pass the ValidateLegalCoordinant function and we'll get a new set of directions at the top of the loop.
            if (_availableDirections.Count == 0) { return 0; } 

            int directionIndex = _rand.Next(_availableDirections.Count - 1);
            int direction = _availableDirections[directionIndex];
            _availableDirections.RemoveAt(directionIndex);
            
            return direction;
        }


        // Returns true if the potential guess coordinant is on the same row/collumn as the previous guess and hasn't been guessed yet.
        private bool ValidateLegalCoordinant(int lastCoordinant, int direction, int nextIndex)
        {
            // checking to see if the next index is outside of the AllCoordinants list
            if (nextIndex < 0) { return false; }
            if (nextIndex >= _allCoordinants.Count) { return false; }

            int nextCoordinant = _allCoordinants[nextIndex];

            // Checking to see if the next coordinant is in the same column if the attack direction is up or down
            if ((lastCoordinant >> 4 != nextCoordinant >> 4) & (Math.Abs(direction) == 1)) {  return false; }
            // returnig false if the next coordinant has already been guessed
            return _availableCoordinants.Contains(nextCoordinant);   
        }


        // Checks each player ship and counts how many have been sunk.
        private int CountShipsSunk()
        {
            int counter = 0;
            _playerShips.ForEach(ship =>
            {
                if(ship.IsShipSunk()) { counter++; }
            });
            return counter;
        }


        // Logic to check to see if the last hit is in the same row/column as the first hit on the target ship.
        private void CheckIfOnTarget()
        {
            if (_attackDirection == 0)
            {
                _onTarget = false;
                return; // can't be on target if we aren't attacking in a particular direction now, can we?
            }

            if (Math.Abs(_attackDirection) == 1)
            {
                _onTarget = (_lastGuess >> 4 == _firstHit >> 4); // checking if the last guess is on the same column as the first hit
                return;
            }

            int mask = 0b00001111;
            _onTarget = ((_lastGuess & mask) == (_firstHit & mask)); // checking if the last guess is on the same row as the first hit
        }


        // Called each time a hit is made to see if the ship sunk. 
        private void CheckForSunkShip()
        {
            int sunkShips = CountShipsSunk();
            if (_playerShipsSunk < sunkShips)
            {
                TargetDestroyed();
                _playerShipsSunk = sunkShips;
            }
        }


        // Finds the length of the smallest player ship not yet sunk.
        private void UpdateSmallestShipLength()
        {
            int smallestShip = _gridWidth;
            List<Ship> activeShips = _playerShips.Where(ship => ship.IsShipSunk() == false).ToList();
            activeShips.ForEach(ship =>
            {
                int length = ship.GetShipLength();
                smallestShip = (length < smallestShip) ? length : smallestShip;
            });
            this._smallestShip = smallestShip;
        }


        // Creates a grid-like list of coordiants to search for player ships based on the length of the smallest ship left alive.
        private List<int> GetSearchGrid()
        {
            UpdateSmallestShipLength();
            List<int> grid = new List<int>();
            int mask = 0b00001111;
            _allCoordinants.ForEach(coordinant =>
            {
                int left = (coordinant >> 4);
                int top = (coordinant & mask);
                int remainder = left % _smallestShip;

                if(top % _smallestShip == 0 & _availableCoordinants.Contains(coordinant + remainder)) { grid.Add(coordinant + remainder); }
            });
            return grid;
        }


        // returns true if the required number of neighboring places of a coordinant have not been guessed
        private bool ConfirmEmptyNeighbors(int coordinant, int requirement)
        {
            int coordinantIndex = _allCoordinants.IndexOf(coordinant);
            int emptyNeighbors = 0;

            foreach (var direction in _cardinalDirections)
            {
                int adjacentCoordinantIndex = coordinantIndex + direction;
                if (adjacentCoordinantIndex >= _allCoordinants.Count | adjacentCoordinantIndex < 0)
                {
                    requirement--;  // requirement is decremented to make sure corners/edges are weighed the same as inner coordinants.
                    continue;
                }
                if ((coordinant >> 4 != _allCoordinants[adjacentCoordinantIndex] >> 4) & (Math.Abs(direction) == 1))
                {
                    requirement--;
                    continue;
                }
                if (_availableCoordinants.Contains(_allCoordinants[adjacentCoordinantIndex])) { emptyNeighbors++; }
            }
            return (emptyNeighbors >= requirement) ? true : false;
        }
    }
}
