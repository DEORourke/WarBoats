using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{
    internal class Opponent
    {
        private int lastGuess;
        public int firstHit;
        private int gridWidth;
        private int attackDirection;
        private bool lastGuessHit;
        private bool searchingForTarget;
        private bool onTarget;
        List<int> AllCoordinants;
        List<int> AvailableCoordinants;
        List<Ship> PlayerShips;
        int PlayerShipsSunk;
        List<int> CardinalDirections;
        List<int> AvailableDirections;
        Random rand;

        public Opponent(List<int> AllCoordinants, int gridwidth, List<Ship> playerShips)
        {
            this.AllCoordinants = AllCoordinants;
            AvailableCoordinants = new List<int>(AllCoordinants);
            this.gridWidth = gridwidth;
            PlayerShips = playerShips;

            rand = new Random();
            TargetDestroyed(); // Starts out searching for a ship

            CardinalDirections = new List<int> {1, -1, gridWidth, gridWidth * -1};
            PlayerShipsSunk = CountShipsSunk();
        }

        public int MakeGuess()
        {
            if (searchingForTarget)
            {
                //swich case for other dificulty levels here
                return ShotInTheDark(); // easy mode
                // normal mode : search in a grid where the size is based on the smallest ship remaining
                // hard mode : Determines next coordinant based on probability matrix. May or may not happen based on if my brain cooperates.
            }

            return EducatedGuess();
        }

        private int ShotInTheDark()
        {
            int guessIndex = rand.Next(AvailableCoordinants.Count - 1);
            int guess = AvailableCoordinants[guessIndex];
            AvailableCoordinants.RemoveAt(guessIndex);
            lastGuess = guess;
            return guess;
        }
         
        private int EducatedGuess()
        {
            while (true)
            {
                int initialCoordinant = (lastGuessHit) ? lastGuess : firstHit;
                
                if (attackDirection == 0)
                {
                    AvailableDirections = new List<int>(CardinalDirections);
                    attackDirection = PickAttackDirection(firstHit);  // if attack directions somehow runs out of options and PickAttackDirection returns 0, logic will pick a new direction here and in the else if statement below. Must fix.

                }

                if (!lastGuessHit & onTarget)
                {
                    attackDirection = attackDirection *= -1;
                    AvailableDirections.Remove(attackDirection);
                }
                
                else if (!lastGuessHit)
                {
                    attackDirection = PickAttackDirection(firstHit);
                }

                int nextCoordiantIndex = GetNextCoordinantIndex(initialCoordinant, attackDirection);
                if (!ValidateLegalCoordinant(initialCoordinant, attackDirection, nextCoordiantIndex))
                {
                    lastGuessHit = false; //treating the potenital guess as a miss in order to pick a new direction
                    continue;
                    // do we need to do something else here to avoid getting stuck in a loop?
                }

                int guess = AllCoordinants[nextCoordiantIndex];
                AvailableCoordinants.Remove(guess);
                lastGuess = guess;
                return guess;
            }
        } 

        public int ControlledGuess(int coordinant)
        {
            AvailableCoordinants.Remove(coordinant);
            lastGuess = coordinant;
            return coordinant;
        }

        private int GetNextCoordinantIndex(int initialCoordinant, int direction)
        {
            int seedCoordinantIndex = AllCoordinants.IndexOf(initialCoordinant);
            return seedCoordinantIndex + direction;
        }

        public void ConfirmHit()
        {
            if (searchingForTarget)
            {
                searchingForTarget = false;
                firstHit = lastGuess;
            }

            CheckIfOnTarget();
            lastGuessHit = true;
            CheckForSunkShip();
        }

        public void HandleMiss()
        {
            lastGuessHit = false;
        }

        public void TargetDestroyed()
        {
            lastGuessHit = false;
            searchingForTarget = true;
            attackDirection = 0;
            firstHit = -1;
            onTarget = false;
        }

        private int PickAttackDirection(int coordinant)
        {
            // should this copy a new line instead? or would that lead to a wild loop
            // No, because the ValidateLegalCoordinant function will return false and we'll get a new list at the top of the loop.
            if (AvailableDirections.Count == 0) { return 0; } 

            int directionIndex = rand.Next(AvailableDirections.Count - 1);
            int direction = AvailableDirections[directionIndex];
            AvailableDirections.RemoveAt(directionIndex);
            
            return direction;
        }

        private bool ValidateLegalCoordinant(int lastCoordinant, int direction, int nextIndex)
        {
            // checking to see if the next index is outside of the AllCoordinants list
            if (nextIndex < 0) { return false; }
            if (nextIndex >= AllCoordinants.Count) { return false; }

            int nextCoordinant = AllCoordinants[nextIndex];

            // Checking to see if the next coordinant is in the same column if the attack direction is up or down
            if ((lastCoordinant >> 4 != nextCoordinant >> 4) & (Math.Abs(direction) == 1)) {  return false; }
            // returnig false if the next coordinant has already been guessed
            return AvailableCoordinants.Contains(nextCoordinant);   
        }

        private int CountShipsSunk()
        {
            int counter = 0;
            PlayerShips.ForEach(ship =>
            {
                if(ship.IsShipSunk()) { counter++; }
            });
            return counter;
        }

        private void CheckIfOnTarget()
        {
            if (attackDirection == 0)
            {
                onTarget = false;
                return; // can't be on target if we aren't attacking in a particular direction now, can we?
            }

            if (Math.Abs(attackDirection) == 1)
            {
                onTarget = (lastGuess >> 4 == firstHit >> 4); // checking if the last guess is on the same column as the first hit
                return;
            }

            int mask = 0b00001111;
            onTarget = ((lastGuess & mask) == (firstHit & mask)); // checking if the last guess is on the same row as the first hit
        }

        private void CheckForSunkShip()
        {
            int sunkShips = CountShipsSunk();
            if (PlayerShipsSunk < sunkShips)
            {
                TargetDestroyed();
                PlayerShipsSunk = sunkShips;
            }
        }
    }
}
