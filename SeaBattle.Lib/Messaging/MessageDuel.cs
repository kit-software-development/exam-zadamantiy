using System;

namespace SeaBattle.Lib.Messaging
{
    [Serializable]
    public class MessageDuel
    {
        public string Token { get; }
        public string Nickname { get; }
        public int Rating { get; }
        public int Type { get; }
        public bool IsSucceed { get; }

        public MessageDuel(string token, string nickname, int type = 1, bool isSucceed = false, int rating = -1)
        {
            Token = token;
            Nickname = nickname;
            IsSucceed = isSucceed;
            Type = type;
            Rating = rating;
        }
    }
}
