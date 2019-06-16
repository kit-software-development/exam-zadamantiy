using System;

namespace SeaBattle.Lib.Messaging
{
    [Serializable]
    public class MessageAuth
    {
        public string Login { get; }
        public string Password { get; }
        public bool IsRegister { get; }

        public MessageAuth(string login, string password, bool isRegister)
        {
            Login = login;
            Password = password;
            IsRegister = isRegister;
        }
    }

    [Serializable]
    public class AnswerAuth
    {
        public bool IsSucceed { get; }
        public string Token { get; }

        public AnswerAuth(bool isSucceed, string token)
        {
            IsSucceed = isSucceed;
            Token = token;
        }
    }
}