using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{
    // The Ship class holds the name of each type of ship on a Board as well as how long it is and how many hits it has taken.
    // Actual coordinants of the Ship are stored in the Board class.
    internal class Ship
    {
        private string _name;   // the name of the ship
        private int _length;    // how many spaces the ship takes up on the board
        private int _hitsTaken; // number of hits the ship has taken

        public Ship(string name, int length)
        {
            this._name = name;
            this._length = length;
            this._hitsTaken = 0;
        }


        // Returns the name and length of ths ship in the format used by the Board class when prmpting palyer to place ships.
        public string GetShipListing()
        {
            return $"{this._name}({this._length})";
        }


        // Returns the name of the ship.
        public string GetShipName()
        {
            return _name;
        }


        // returns how many spaces the ship takes up.
        public int GetShipLength()
        {
            return _length;
        }


        // Incriments the hits taken counter.
        public void HitShip()
        {
            _hitsTaken++;
        }

        
        // returns true if the number of hits take is greater or equal to the length of the ship, but you already knew that because you have eyes.
        public bool IsShipSunk()
        {
            return (_hitsTaken >= _length);
        }
    }
}
