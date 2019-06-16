using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using SeaBattle.Lib.Gaming;

namespace SeaBattle.Lib.Messaging
{
    /// <summary>
    /// Class with the turn info
    /// </summary>
    [Serializable]
    public class Turn
    {
        public int X { get; }
        public int Y { get; }

        public Turn(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    
    /// <summary>
    /// Match message types enum
    /// </summary>
    public enum MessageMatchType
    {
        SendField,
        OpponentStillThinking,
        StartMatch,
        SendTurn,
        EndMatch
    }

    /// <summary>
    /// Class with the info about current match action
    /// </summary>
    [Serializable]
    public class MessageMatchInfo
    {
        /// <summary>
        /// Player's token depending on message type
        /// </summary>
        public string Token;

        /// <summary>
        /// Player's field depending on message type
        /// </summary>
        public Field Field;


        public Turn Turn;
        public MessageMatchType Type;
        public bool IsMyTurn;
        public bool IsMyShot;

        public MessageMatchInfo(string token, Field field, Turn turn, MessageMatchType msgType, bool isMyTurn = false, bool isMyShot = true)
        {
            Token = token;
            Field = field;
            Turn = turn;
            Type = msgType;
            IsMyTurn = isMyTurn;
            IsMyShot = isMyShot;
        }
    }
}
