using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Net;
using SeaBattle.Lib.Gaming;
using SeaBattle.Lib.Messaging;
using SeaBattle.Lib.Networking;
using SeaBattle.Lib.Threading;
using SeaBattle.Server.Misc;

namespace SeaBattle.Server
{
    /// <summary>
    /// The main class of the server
    /// </summary>
    class Program
    {
        private static readonly Random Rnd = new Random();
        private static string _connectionString;

        /// <summary>
        /// The main function of the server
        /// </summary>
        /// <param name="args">--</param>
        static void Main(string[] args)
        {
            _connectionString = GetConnectionString();

            var listener = new UdpMessageListener<MessageAuth>(Ports.AuthPort);
            listener.IncomingMessage += OnMessageAuth;
            listener.Start();

            var duelListener = new UdpMessageListener<MessageDuel>(Ports.DuelPort);
            duelListener.IncomingMessage += OnDuelMessage;
            duelListener.Start();

            var matchInfoListener = new UdpMessageListener<MessageMatchInfo>(Ports.MatchPort);
            matchInfoListener.IncomingMessage += OnMatchMessage;
            matchInfoListener.Start();

            //Clear matches on restart
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var getTop10 = @"DELETE FROM [ActiveMatch]";
                using (var cmd = new SqlCommand(getTop10, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Server started\nType 'help' if you need :)");
            
            while (true)
            {
                var s = Console.ReadLine();
                switch (s)
                {
                    case "help":
                        Console.WriteLine("exit - exit from app");
                        Console.WriteLine("top - get top players");
                        break;
                    case "exit":
                        goto end;
                        break;
                    case "top":
                        var getTop10 = @"SELECT [Login], [Rating] FROM [Users] ORDER BY [RATING] DESC";
                        int i = 0;
                        using (var connection = new SqlConnection(_connectionString))
                        {
                            connection.Open();

                            using (var cmd = new SqlCommand(getTop10, connection))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read() && i < 10)
                                    {
                                        i++;
                                        string Login = reader.GetString(reader.GetOrdinal("Login"));
                                        short Rating = reader.GetInt16(reader.GetOrdinal("Rating")); 
                                        Console.WriteLine("{0}. {1} [{2}]", i, Login, Rating);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Команда не распознана!");
                        break;
                }
            }
            end: Console.WriteLine("Press <enter> to quit");
            Console.ReadKey();
        }

        /// <summary>
        /// Handles what will be going on after receiving match message
        /// </summary>
        /// <param name="sender">--</param>
        /// <param name="e">Info about match</param>
        private static void OnMatchMessage(object sender, IncomingMessageEventArgs<MessageMatchInfo> e)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                //Get match info
                var am = GetMatchByToken(e.Message.Token, connection);
                var isMyFirstToken = am.Token1 == e.Message.Token;
                string opponentToken;
                Field opponentField;

                if (isMyFirstToken)
                {
                    opponentField = am.Field2;
                    opponentToken = am.Token2;
                }
                else
                {
                    opponentField = am.Field1;
                    opponentToken = am.Token1;
                }

                //opponentIp
                string commandSelectText2 = @"SELECT [Ip] FROM [Users] WHERE Token = @Token";
                string opponentIp;
                using (var cmd = new SqlCommand(commandSelectText2, connection))
                {
                    cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                    cmd.Parameters["@Token"].Value = opponentToken;

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        if (!reader.HasRows)
                        {
                            return;
                        }
                        else
                        {
                            opponentIp = reader.GetString(reader.GetOrdinal("Ip"));
                        }
                    }
                }

                switch (e.Message.Type)
                {
                    case MessageMatchType.SendField:
                        e.Message.Field.Marks = new byte[10,10];
                        if (isMyFirstToken)
                        {
                            am.Field1 = e.Message.Field;
                        }
                        else
                        {
                            am.Field2 = e.Message.Field;
                        }

                        var tmpField = isMyFirstToken ? am.Field1 : am.Field2;

                        string commandText = @"UPDATE [ActiveMatch] SET [" + (isMyFirstToken ? "Field1" : "Field2") + "] = @Field WHERE [Token1] = @Token OR [Token2] = @Token";
                        
                        using (var command = new SqlCommand(commandText, connection))
                        {
                            command.Parameters.AddWithValue("@Field", SqlDbType.VarBinary);
                            command.Parameters["@Field"].Value = tmpField.Serialize();

                            command.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                            command.Parameters["@Token"].Value = e.Message.Token;

                            command.ExecuteNonQuery();
                        }
                        
                        if (am.Field1 == null || am.Field2 == null)
                        {
                            using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(e.Sender.Address, Ports.MatchAnswerPort))
                            {
                                var info = new MessageMatchInfo(e.Message.Token, null, null, MessageMatchType.OpponentStillThinking);
                                writer.Write(info);
                            }
                        }
                        else
                        {
                            //Generating first turn
                            bool first = Rnd.Next(2) < 1;
                            string setTurnQuery = @"UPDATE [ActiveMatch] SET [Turn] = @Turn WHERE [Token1] = @Token OR [Token2] = @Token";
                            
                            using (var command = new SqlCommand(setTurnQuery, connection))
                            {
                                command.Parameters.AddWithValue("@Turn", SqlDbType.VarBinary);
                                command.Parameters["@Turn"].Value = first;

                                command.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                                command.Parameters["@Token"].Value = e.Message.Token;

                                command.ExecuteNonQuery();
                            }

                            using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(e.Sender.Address, Ports.MatchAnswerPort))
                            {
                                var info = new MessageMatchInfo(null, opponentField, null, MessageMatchType.StartMatch, isMyFirstToken ? !first : first);
                                writer.Write(info);
                            }

                            using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(IPAddress.Parse(opponentIp), Ports.MatchAnswerPort))
                            {
                                var info = new MessageMatchInfo(null, e.Message.Field, null, MessageMatchType.StartMatch, isMyFirstToken ? first : !first);
                                writer.Write(info);
                            }
                        }
                        break;
                    case MessageMatchType.SendTurn:
                        if (isMyFirstToken)
                        {
                            if (!am.Turn)
                                return;
                        }
                        else
                        {
                            if (am.Turn)
                                return;
                        }

                        var ship = opponentField.GetShipUnderCursor(e.Message.Turn.X, e.Message.Turn.Y);
                        if (ship == null)
                        {
                            opponentField.Marks[e.Message.Turn.X, e.Message.Turn.Y] = 1;
                        }
                        else
                        {
                            opponentField.Marks[e.Message.Turn.X, e.Message.Turn.Y] = 2;
                            ship.Lives--;
                            if (ship.Lives == 0)
                            {
                                Point topLeft, botRight;
                                ship.GetFullRectangle(out topLeft, out botRight);

                                if (topLeft.X < 0) topLeft.X = 0;
                                if (topLeft.Y < 0) topLeft.Y = 0;

                                if (botRight.X > 9) botRight.X = 9;
                                if (botRight.Y > 9) botRight.Y = 9;
                                
                                for (int i = topLeft.X; i <= botRight.X; i++)
                                {
                                    for (int j = topLeft.Y; j <= botRight.Y; j++)
                                    {
                                        if (opponentField.Marks[i, j] == 0)
                                        {
                                            opponentField.Marks[i, j] = 1;
                                        }
                                    }
                                }
                            }

                            int amount = 0;
                            for (int i = 0; i < 10; i++)
                            for (int j = 0; j < 10; j++)
                            {
                                if (opponentField.Marks[i, j] == 2) amount++;
                            }

                            if (amount == 20)
                            {


                                using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(e.Sender.Address, Ports.MatchAnswerPort))
                                {
                                    var info = new MessageMatchInfo(null, new Field(opponentField.Marks, null), null, MessageMatchType.EndMatch, false, true);
                                    writer.Write(info);
                                }

                                using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(IPAddress.Parse(opponentIp), Ports.MatchAnswerPort))
                                {
                                    var info = new MessageMatchInfo(null, new Field(opponentField.Marks, null), null, MessageMatchType.EndMatch, false, false);
                                    writer.Write(info);
                                }

                                short mmrDiff = ChangeRating(e.Message.Token, opponentToken, true, connection);

                                WriteMatchToHistory(e.Message.Token, opponentToken, true, mmrDiff, connection);

                                DropMatch(am.MatchId, connection);
                                return;
                            }
                        }

                        string updateField = @"UPDATE [ActiveMatch] SET [" + (isMyFirstToken ? "Field2" : "Field1") + "] = @Field, [Turn] = @Turn WHERE [Token1] = @Token OR [Token2] = @Token";

                        using (var command = new SqlCommand(updateField, connection))
                        {
                            command.Parameters.AddWithValue("@Field", SqlDbType.VarBinary);
                            command.Parameters["@Field"].Value = opponentField.Serialize();

                            command.Parameters.AddWithValue("@Turn", SqlDbType.Bit);
                            command.Parameters["@Turn"].Value = isMyFirstToken ? ship != null : ship == null;

                            command.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                            command.Parameters["@Token"].Value = e.Message.Token;

                            command.ExecuteNonQuery();
                        }

                        using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(e.Sender.Address, Ports.MatchAnswerPort))
                        {
                            var info = new MessageMatchInfo(null, new Field(opponentField.Marks, null), null, MessageMatchType.SendTurn, ship != null, true);
                            writer.Write(info);
                        }

                        using (var writer = NetworkingFactory.UdpWriter<MessageMatchInfo>(IPAddress.Parse(opponentIp), Ports.MatchAnswerPort))
                        {
                            var info = new MessageMatchInfo(null, new Field(opponentField.Marks, null), null, MessageMatchType.SendTurn, ship == null, false);
                            writer.Write(info);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Handles what will be going on after receiving duel message
        /// </summary>
        /// <param name="sender">--</param>
        /// <param name="e">Duel message</param>
        private static void OnDuelMessage(object sender, IncomingMessageEventArgs<MessageDuel> e)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                if (e.Message.Type == 1)
                {
                    //Get info about user
                    string commandSelectText = @"SELECT [Rating], [Login] FROM [Users] WHERE [Token] = @Token";
                    string login;
                    short rating;
                    using (var cmd = new SqlCommand(cmdText: commandSelectText, connection: connection))
                    {
                        
                        cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                        cmd.Parameters["@Token"].Value = e.Message.Token;
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            if (!reader.HasRows)
                            {
                                using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(e.Sender.Address, Ports.DuelAnswerPort))
                                {
                                    var info = new MessageDuel(e.Message.Token, null, 4, false);
                                    writer.Write(info);
                                }
                                return;
                            }
                            else
                            {
                                login = reader.GetString(reader.GetOrdinal("Login"));
                                rating = reader.GetInt16(reader.GetOrdinal("Rating"));

                                if (login == null)
                                { 
                                    using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(e.Sender.Address, Ports.DuelAnswerPort))
                                    {
                                        var info = new MessageDuel(e.Message.Token, null, 2, false, rating);
                                        writer.Write(info);
                                    }
                                    return;
                                }
                            }
                        }
                    }

                    //Get info about opponent
                    string commandSelectText2 = @"SELECT [Token], [Ip] FROM [Users] WHERE Login = @Login";
                    using (var cmd = new SqlCommand(commandSelectText2, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", SqlDbType.NVarChar);
                        cmd.Parameters["@Login"].Value = e.Message.Nickname;

                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();

                            if (!reader.HasRows)
                            {
                                using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(e.Sender.Address, Ports.DuelAnswerPort))
                                {
                                    var info = new MessageDuel(e.Message.Token, null, 4, false);
                                    writer.Write(info);
                                }
                                return;
                            }
                            else
                            {
                                string opponentIp = reader.GetString(reader.GetOrdinal("Ip"));
                                string opponentToken = reader.GetString(reader.GetOrdinal("Token"));
                                reader.Close();

                                if (opponentToken == e.Message.Token)
                                {
                                    using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(e.Sender.Address, Ports.DuelAnswerPort))
                                    {
                                        var info = new MessageDuel(e.Message.Token, null, 4, false);
                                        writer.Write(info);
                                    }
                                    return;
                                }

                                NewMatch(e.Message.Token, opponentToken, connection);

                                using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(IPAddress.Parse(opponentIp), Ports.DuelAnswerPort))
                                {
                                    var info = new MessageDuel(opponentToken, login, 2, false, rating);
                                    writer.Write(info);
                                }

                                return;
                            }
                        }
                    }
                }
                else if (e.Message.Type == 3)
                {
                    //Get match info
                    string commandSelectText = @"SELECT [Id], [Token1], [Token2] FROM [ActiveMatch] WHERE [Token1] = @Token OR [Token2] = @Token";
                    int matchId;
                    string opponentToken;

                    using (var cmd = new SqlCommand(cmdText: commandSelectText, connection: connection))
                    {

                        cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                        cmd.Parameters["@Token"].Value = e.Message.Token;
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            if (!reader.HasRows)
                            {
                                return;
                            }
                            else
                            {
                                matchId = reader.GetInt32(reader.GetOrdinal("Id"));
                                string tmp = reader.GetString(reader.GetOrdinal("Token1"));
                                opponentToken = (tmp == e.Message.Token) ? reader.GetString(reader.GetOrdinal("Token2")) : tmp;
                            }
                        }
                    }

                    //Get opponent ip
                    string opponentIp;

                    string commandSelectText2 = @"SELECT [Ip] FROM [Users] WHERE Token = @Token";
                    using (var cmd = new SqlCommand(commandSelectText2, connection))
                    {
                        cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                        cmd.Parameters["@Token"].Value = opponentToken;

                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();

                            if (!reader.HasRows)
                            {
                                return;
                            }
                            else
                            {
                                opponentIp = reader.GetString(reader.GetOrdinal("Ip"));
                            }
                        }
                    }

                    if (e.Message.IsSucceed)
                    {
                        if (opponentToken != null && e.Message.Token != null)
                        {
                            using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(e.Sender.Address, Ports.DuelAnswerPort))
                            {
                                var info = new MessageDuel(e.Message.Token, null, 4, true);
                                writer.Write(info);
                            }

                            using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(IPAddress.Parse(opponentIp), Ports.DuelAnswerPort))
                            {
                                var info = new MessageDuel(e.Message.Token, null, 4, true);
                                writer.Write(info);
                            }

                            return;
                        }
                    }
                    else
                    {
                        DropMatch(matchId, connection);
                        
                        using (var writer = NetworkingFactory.UdpWriter<MessageDuel>(IPAddress.Parse(opponentIp), Ports.DuelAnswerPort))
                        {
                            var info = new MessageDuel(e.Message.Token, null, 4, false);
                            writer.Write(info);
                        }

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Handles what will be going on after receiving auth message
        /// </summary>
        /// <param name="sender">--</param>
        /// <param name="e">Auth message</param>
        private static void OnMessageAuth(object sender, IncomingMessageEventArgs<MessageAuth> e)
        {

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                string query = @"SELECT PWDCOMPARE(@Pass, [PassHash]) SamePass FROM [Users] WHERE [Login] = @Login";
                string token;
                bool samePass = false;
                bool flag = false;

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Pass", SqlDbType.NVarChar);
                    command.Parameters["@Pass"].Value = e.Message.Password;

                    command.Parameters.AddWithValue("@Login", SqlDbType.NVarChar);
                    command.Parameters["@Login"].Value = e.Message.Login;

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        if (!reader.HasRows)
                        {
                            if (e.Message.IsRegister)
                            {
                                flag = true;
                            }
                            else
                            {
                                using (var writer = NetworkingFactory.UdpWriter<AnswerAuth>(e.Sender.Address, Ports.AuthAnswerPort))
                                {
                                    var info = new AnswerAuth(false, null);
                                    writer.Write(info);
                                }
                                return;
                            }
                        }
                        else
                        {
                            samePass = reader.GetInt32(reader.GetOrdinal("SamePass")) == 1;
                        }
                    }
                }

                if (flag)
                {
                    token = StringGenerator.RandomString(30);
                    string queryAddUser = @"INSERT INTO [USERS] (Login, PassHash, Token, Rating, Ip) VALUES(@Login, PWDENCRYPT(@Pass), @Token, 1600, @Ip)";
                    var commandAddUser = new SqlCommand(queryAddUser, connection);

                    commandAddUser.Parameters.AddWithValue("@Login", SqlDbType.NVarChar);
                    commandAddUser.Parameters["@Login"].Value = e.Message.Login;

                    commandAddUser.Parameters.AddWithValue("@Pass", SqlDbType.NVarChar);
                    commandAddUser.Parameters["@Pass"].Value = e.Message.Password;

                    commandAddUser.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                    commandAddUser.Parameters["@Token"].Value = token;

                    commandAddUser.Parameters.AddWithValue("@Ip", SqlDbType.NVarChar);
                    commandAddUser.Parameters["@Ip"].Value = e.Sender.Address.ToString();

                    commandAddUser.ExecuteNonQuery();
                }
                else
                {
                    if (samePass)
                    { 
                        token = StringGenerator.RandomString(30);
                        query = @"UPDATE [USERS] SET [Token] = @Token, [Ip] = @Ip WHERE [Login] = @Login";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Login", SqlDbType.NVarChar);
                            command.Parameters["@Login"].Value = e.Message.Login;

                            command.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                            command.Parameters["@Token"].Value = token;

                            command.Parameters.AddWithValue("@Ip", SqlDbType.NVarChar);
                            command.Parameters["@Ip"].Value = e.Sender.Address.ToString();

                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var writer = NetworkingFactory.UdpWriter<AnswerAuth>(e.Sender.Address, Ports.AuthAnswerPort))
                        {
                            var info = new AnswerAuth(false, null);
                            writer.Write(info);
                        }
                        return;
                    }
                }

                using (var writer = NetworkingFactory.UdpWriter<AnswerAuth>(e.Sender.Address, Ports.AuthAnswerPort))
                {
                    var info = new AnswerAuth(true, token);
                    writer.Write(info);
                }
            }
        }

        /// <summary>
        /// Writes match result to history
        /// </summary>
        /// <param name="token1">Token of the first player</param>
        /// <param name="token2">Token of the second player</param>
        /// <param name="result">Match result: true if first won, false if second</param>
        /// <param name="ratingChange">Value of rating change</param>
        /// <param name="connection">Sql connection</param>
        private static void WriteMatchToHistory(string token1, string token2, bool result, short ratingChange, SqlConnection connection)
        {
            string login1 = GetLoginByToken(token1, connection);
            string login2 = GetLoginByToken(token2, connection);
            
            string commandText = @"INSERT INTO [Match] ([FirstPlayer], [SecondPlayer], [Result], [RatingChange]) VALUES(@Login1, @Login2, @Result, @RatingChange)";

            using (var command = new SqlCommand(commandText, connection))
            {
                command.Parameters.AddWithValue("@Login1", SqlDbType.NVarChar);
                command.Parameters["@Login1"].Value = login1;

                command.Parameters.AddWithValue("@Login2", SqlDbType.NVarChar);
                command.Parameters["@Login2"].Value = login2;

                command.Parameters.AddWithValue("@Result", SqlDbType.Bit);
                command.Parameters["@Result"].Value = result;

                command.Parameters.AddWithValue("@RatingChange", SqlDbType.SmallInt);
                command.Parameters["@RatingChange"].Value = ratingChange;

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Returns login by token
        /// </summary>
        /// <param name="token">Player's token</param>
        /// <param name="connection">Sql connection</param>
        /// <returns></returns>
        private static string GetLoginByToken(string token, SqlConnection connection)
        {
            string commandSelectText = @"SELECT [Login] FROM [Users] WHERE [Token] = @Token";
            string login = null;

            using (var cmd = new SqlCommand(cmdText: commandSelectText, connection: connection))
            {

                cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                cmd.Parameters["@Token"].Value = token;

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    if (!reader.HasRows)
                    {
                        return null;
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("Login")))
                    {
                        login = reader.GetString(reader.GetOrdinal("Login"));
                    }
                }
            }

            return login;
        }

        /// <summary>
        /// Returns player rating by his token
        /// </summary>
        /// <param name="token">Player's token</param>
        /// <param name="connection">Sql connection</param>
        /// <returns></returns>
        private static short GetRatingByToken(string token, SqlConnection connection)
        {
            string commandSelectText = @"SELECT [Rating] FROM [Users] WHERE [Token] = @Token";
            short rating = -1;

            using (var cmd = new SqlCommand(cmdText: commandSelectText, connection: connection))
            {

                cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                cmd.Parameters["@Token"].Value = token;

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    if (!reader.HasRows)
                    {
                        return rating;
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("Rating")))
                    {
                        rating = reader.GetInt16(reader.GetOrdinal("Rating"));
                    }
                }
            }

            return rating;
        }

        /// <summary>
        /// Recalculate and save new rating after match between two players
        /// </summary>
        /// <param name="token1">token of the first player</param>
        /// <param name="token2">token of the second player</param>
        /// <param name="firstWon">true if first player won, false if second</param>
        /// <param name="connection">Sql connection</param>
        /// <returns></returns>
        private static short ChangeRating(string token1, string token2, bool firstWon, SqlConnection connection)
        {
            if (!firstWon)
            {
                var tmp = token1;
                token1 = token2;
                token2 = tmp;
            }

            int rating1 = GetRatingByToken(token1, connection);
            int rating2 = GetRatingByToken(token2, connection);

            if (rating1 == -1 || rating2 == -1) return -1;

            string commandText = @"UPDATE [Users] SET [Rating] = @Rating WHERE [Token] = @Token";

            double e1 = 1.0 / (1 + Math.Pow(10, (double) (rating2 - rating1) / 400));
            double e2 = 1.0 / (1 + Math.Pow(10, (double) (rating1 - rating2) / 400));

            short ratingChange = (short) (50 * (1.0 - e1));
            short newRating1 = (short) (rating1 + ratingChange);
            short newRating2 = (short) (rating2 - ratingChange);

            using (var command = new SqlCommand(commandText, connection))
            {
                command.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                command.Parameters["@Token"].Value = token1;

                command.Parameters.AddWithValue("@Rating", SqlDbType.SmallInt);
                command.Parameters["@Rating"].Value = newRating1;
                
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand(commandText, connection))
            {
                command.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                command.Parameters["@Token"].Value = token2;

                command.Parameters.AddWithValue("@Rating", SqlDbType.SmallInt);
                command.Parameters["@Rating"].Value = newRating2;

                command.ExecuteNonQuery();
            }

            return ratingChange;
        }

        /// <summary>
        /// Returns info about active match by token of one player
        /// </summary>
        /// <param name="token">Token of one of the participating players</param>
        /// <param name="connection">Sql connection</param>
        /// <returns></returns>
        private static ActiveMatch GetMatchByToken(string token, SqlConnection connection)
        {
            string commandSelectText =
                            @"SELECT [Id], [Token1], [Token2], [Field1], [Field2], [Turn] FROM [ActiveMatch] WHERE [Token1] = @Token OR [Token2] = @Token";
            ActiveMatch am;

            using (var cmd = new SqlCommand(cmdText: commandSelectText, connection: connection))
            {
                cmd.Parameters.AddWithValue("@Token", SqlDbType.NVarChar);
                cmd.Parameters["@Token"].Value = token;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    if (!reader.HasRows)
                    {
                        //TODO:
                        return null;
                    }
                    else
                    {
                        am = new ActiveMatch(
                            reader.GetInt32(reader.GetOrdinal("Id")), 
                            reader.GetString(reader.GetOrdinal("Token1")),
                            reader.GetString(reader.GetOrdinal("Token2")));

                        //TODO:
                        //opponentToken = (token1 == e.Message.Token) ? token2 : token1;

                        int field1Pos = reader.GetOrdinal("Field1");
                        if (!reader.IsDBNull(field1Pos))
                        {
                            am.Field1 = MessageFactory.Deserialize<Field>((byte[])reader[field1Pos]);
                        }
                        
                        int field2Pos = reader.GetOrdinal("Field2");
                        if (!reader.IsDBNull(field2Pos))
                        {
                            am.Field2 = MessageFactory.Deserialize<Field>((byte[])reader[field2Pos]);
                        }

                        int turnPos = reader.GetOrdinal("Turn");
                        if (!reader.IsDBNull(turnPos))
                        {
                            am.Turn = reader.GetBoolean(reader.GetOrdinal("Turn"));
                        }
                    }
                }
            }

            return am;
        }

        /// <summary>
        /// Creates new match between two players
        /// </summary>
        /// <param name="token1">token of the first player</param>
        /// <param name="token2">token of the second player</param>
        /// <param name="connection">Sql connection</param>
        private static void NewMatch(string token1, string token2, SqlConnection connection)
        {
            string commandText = @"INSERT INTO [ActiveMatch] ([Token1], [Token2]) VALUES(@Token1, @Token2)";

            using (var command = new SqlCommand(commandText, connection))
            {
                command.Parameters.AddWithValue("@Token1", SqlDbType.NVarChar);
                command.Parameters["@Token1"].Value = token1;

                command.Parameters.AddWithValue("@Token2", SqlDbType.NVarChar);
                command.Parameters["@Token2"].Value = token2;

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes active match
        /// </summary>
        /// <param name="matchId">Match id</param>
        /// <param name="connection">Sql connection</param>
        private static void DropMatch(int matchId, SqlConnection connection)
        {
            string commandText = @"DELETE FROM [ActiveMatch] WHERE id = @id";

            using (var command = new SqlCommand(commandText, connection))
            {
                command.Parameters.AddWithValue("@id", SqlDbType.NVarChar);
                command.Parameters["@id"].Value = matchId;

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Return connection string
        /// </summary>
        /// <returns>Connection string</returns>
        private static string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                AttachDBFilename = Path.GetFullPath("ServerDB.mdf"),
                IntegratedSecurity = true
            };
            return builder.ConnectionString;
        }
    }
}
