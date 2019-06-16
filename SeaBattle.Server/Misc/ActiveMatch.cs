using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaBattle.Lib.Gaming;

namespace SeaBattle.Server.Misc
{
    /// <summary>
    /// Class that represents info about active match
    /// </summary>
    public class ActiveMatch
    {
        /// <summary>
        /// Match id
        /// </summary>
        public int MatchId { get; set; }

        /// <summary>
        /// Field of the first player
        /// </summary>
        public Field Field1 { get; set; }

        /// <summary>
        /// Field of the second player
        /// </summary>
        public Field Field2 { get; set; }

        /// <summary>
        /// Token of the first player
        /// </summary>
        public string Token1 { get; set; }

        /// <summary>
        /// Token of the second player
        /// </summary>
        public string Token2 { get; set; }

        /// <summary>
        /// Determines whose turn now (true - first, false - second player)
        /// </summary>
        public bool Turn { get; set; }

        public ActiveMatch(int matchId, string token1, string token2, Field field1 = null, Field field2 = null, bool turn = false)
        {
            MatchId = matchId;
            Field1 = field1;
            Field2 = field2;
            Token1 = token1;
            Token2 = token2;
            Turn = turn;
        }
    }
}
