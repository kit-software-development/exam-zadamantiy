using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SeaBattle.Lib.Gaming
{
    /// <summary>
    /// CLass that represents ship
    /// </summary>
    [Serializable]
    public class Ship
    {
        /// <summary>
        /// Rotation of the ship
        /// </summary>
        public bool Vertical;

        /// <summary>
        /// Size of the ship
        /// </summary>
        public int Size;

        /// <summary>
        /// X position on the field
        /// </summary>
        public int PosX;

        /// <summary>
        /// Y position on the field
        /// </summary>
        public int PosY;

        /// <summary>
        /// Lives amount
        /// </summary>
        public int Lives { get; set; }

        public Ship(int posX, int posY, bool vertical, int size)
        {
            this.PosX = posX;
            this.PosY = posY;
            this.Vertical = vertical;
            this.Size = size;
            this.Lives = size;
        }

        /// <summary>
        /// Gets full collision rectangle (with sea cells)
        /// </summary>
        /// <param name="leftTop"></param>
        /// <param name="rightBot"></param>
        public void GetFullRectangle(out Point leftTop, out Point rightBot)
        {
            leftTop = new Point(PosX - 1, PosY - 1);
            rightBot = Vertical ? new Point(PosX + Size, PosY + 1) : new Point(PosX + 1, PosY + Size);
        }

        /// <summary>
        /// Gets ship rectangle
        /// </summary>
        /// <param name="leftTop"></param>
        /// <param name="rightBot"></param>
        public void GetRectangle(out Point leftTop, out Point rightBot)
        {
            leftTop = new Point(PosX, PosY);
            rightBot = Vertical ? new Point(PosX + Size - 1, PosY) : new Point(PosX, PosY + Size - 1);
        }

        /// <summary>
        /// Checks for colliding with another ship
        /// </summary>
        /// <param name="s">Another ship</param>
        /// <returns>Are ships colliding</returns>
        public bool IsCollidingWith(Ship s)
        {
            Point l1, l2, r1, r2;
            GetFullRectangle(out l1, out r1);
            s.GetRectangle(out l2, out r2);

            if (l1.X > r2.X || l2.X > r1.X) return false;
            if (l1.Y > r2.Y || l2.Y > r1.Y) return false;

            return true;
        }

        /// <summary>
        /// Checks for colliding with point
        /// </summary>
        /// <param name="pt">Point</param>
        /// <returns>Is ship colliding with point</returns>
        public bool IsCollidingWith(Point pt)
        {
            Point l1, r1;
            GetRectangle(out l1, out r1);

            if (l1.X > pt.X || pt.X > r1.X) return false;
            if (l1.Y > pt.Y || pt.Y > r1.Y) return false;

            return true;
        }
    }
}
