using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarBoats
{
    internal class Ship
    {
        public string name;
        public int length;
        private int hitsTaken;

        public Ship(string name, int length)
        {
            this.name = name;
            this.length = length;
            this.hitsTaken = 0;
        }

        public string GetShipListing()
        {
            return $"{this.name}({this.length})";
        }

        public string GetShipName()
        {
            return name;
        }

        public void HitShip()
        {
            hitsTaken++;
        }

        public bool IsShipSunk()
        {
            return (hitsTaken >= length);
        }
    }
}
