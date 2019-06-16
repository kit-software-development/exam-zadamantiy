using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Lib.Gaming
{
    /// <summary>
    /// Editor class for field
    /// </summary>
    public class Editor
    {
        /// <summary>
        /// Field 
        /// </summary>
        public Field Field { get; set; }

        /// <summary>
        /// Amount of left ships
        /// [0] - of size 1
        /// [1] - of size 2
        /// ...
        /// [3] - of size 4
        /// </summary>
        private readonly int[] _shipAmount;

        /// <summary>
        /// Returns amount of ships of size
        /// </summary>
        /// <param name="size">size</param>
        /// <returns>amount of such ships left</returns>
        public int GetShipsLeft(int size)
        {
            return _shipAmount[size - 1];
        }

        /// <summary>
        /// Checks if we can place ship on such position
        /// </summary>
        /// <param name="ship">ship</param>
        /// <returns>True if we can and false if not</returns>
        private bool CanPlaceShip(Ship ship)
        {
            if (_shipAmount[ship.Size - 1] <= 0) return false;

            Point l, r;
            ship.GetRectangle(out l, out r);
            if (r.X > 9 || r.Y > 9) return false;

            foreach (var anotherShip in Field.Ships)
            {
                if (ship.IsCollidingWith(anotherShip))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns ship on position (from field object)
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Ship object</returns>
        private Ship GetShipUnderCursor(int x, int y)
        {
            return Field.GetShipUnderCursor(x, y);
        }
        
        public Editor(byte[,] marks, List<Ship> ships, int ship1Amount, int ship2Amount, int ship3Amount, int ship4Amount)
        {
            Field = new Field(marks, ships);

            _shipAmount = new[]
            {
                ship1Amount,
                ship2Amount,
                ship3Amount,
                ship4Amount
            };
        }

        public Editor(Field field, int ship1Amount, int ship2Amount, int ship3Amount, int ship4Amount)
        {
            Field = field;
            _shipAmount = new[]
            {
                ship1Amount,
                ship2Amount,
                ship3Amount,
                ship4Amount
            };
        }

        /// <summary>
        /// Add ship on the field
        /// </summary>
        /// <param name="ship">Ship</param>
        private void AddShip(Ship ship)
        {
            Field.Ships.Add(ship);
            _shipAmount[ship.Size - 1]--;
        }

        /// <summary>
        /// Removing ship from the field
        /// </summary>
        /// <param name="ship">Ship</param>
        private void RemoveShip(Ship ship)
        {
            Field.Ships.Remove(ship);
            _shipAmount[ship.Size - 1]++;
        }

        /// <summary>
        /// Proceeds default action on coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="size">Ship size</param>
        /// <param name="vertical">Ship rotation</param>
        /// <returns>True if action is succeed, else returns false</returns>
        public bool ProceedAction(int x, int y, int size, bool vertical = false)
        {
            var ship = new Ship(x, y, vertical, size);
            if (CanPlaceShip(ship))
            {
                AddShip(ship);
                return true;
            }

            ship = new Ship(x, y, !vertical, size);
            if (CanPlaceShip(ship))
            {
                AddShip(ship);
                return true;
            }
            
            ship = GetShipUnderCursor(x, y);
            if (ship == null) return false;

            RemoveShip(ship);
            var tmpShip = new Ship(ship.PosX, ship.PosY, !ship.Vertical, ship.Size);
            if (CanPlaceShip(tmpShip))
            {
                AddShip(tmpShip);
                return true;
            }

            AddShip(ship);
            return false;
        }

        /// <summary>
        /// Removes ships at coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if action is succeed, else returns false</returns>
        public bool RemoveAt(int x, int y)
        {
            var ship = GetShipUnderCursor(x, y);
            if (ship == null) return false;
            RemoveShip(ship);
            return true;
        }
    }
}
