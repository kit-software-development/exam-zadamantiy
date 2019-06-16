using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Lib.Messaging
{
    /// <summary>
    /// Class with list of used ports
    /// </summary>
    public abstract class Ports
    {
        public const int AuthPort = 7360;
        public const int AuthAnswerPort = 7361;

        public const int DuelPort = 7362;
        public const int DuelAnswerPort = 7363;

        public const int MatchPort = 7364;
        public const int MatchAnswerPort = 7365;
    }
}
