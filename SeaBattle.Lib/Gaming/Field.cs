using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeaBattle.Lib.Gaming
{
    /// <summary>
    /// Field class
    /// </summary>
    [Serializable]
    public class Field
    {
        /// <summary>
        /// Array of marks
        /// 0 - no hit
        /// 1 - hit w/o ship
        /// 2 - hit on ship
        /// </summary>
        public byte[,] Marks { get; set; }

        /// <summary>
        /// List of ships
        /// </summary>
        public List<Ship> Ships { get; set; }
        
        public Field(byte[,] marks, List<Ship> ships)
        {
            Marks = marks;
            Ships = ships;
        }

        /// <summary>
        /// Returns ship on coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Ship object or null</returns>
        public Ship GetShipUnderCursor(int x, int y)
        {
            var pt = new Point(x, y);
            foreach (var ship in Ships)
            {
                if (ship.IsCollidingWith(pt))
                {
                    return ship;
                }
            }

            return null;
        }
        
        protected Field()
        {
        }
    }
}

