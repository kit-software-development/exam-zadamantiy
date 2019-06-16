using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeaBattle.Client.Misc;
using SeaBattle.Lib.Gaming;
using SeaBattle.Lib.Messaging;
using SeaBattle.Lib.Networking;
using SeaBattle.Lib.Threading;

namespace SeaBattle.Client
{
    /// <summary>
    /// Game form
    /// </summary>
    public partial class Game : Form
    {
        public PreGame ParentForm;

        private readonly SeaBattleDrawerClass _drawerClass;
        private Dictionary<string, Font> Fonts;

        public string Token;
        public string ServerIp;

        private bool MyTurn;
        private bool EndGame;

        private readonly INetworkWriter<MessageMatchInfo> _server;
        private readonly UdpMessageListener<MessageMatchInfo> _listener;

        private readonly Rectangle _fieldController;

        private Field _opponentField;
        private Field _myField;

        public Game(Dictionary<string, Font> fonts, string token, string serverIp, Field myField, bool myTurn)
        {
            EndGame = false;

            //SERVER
            Token = token;
            ServerIp = serverIp;
            _server = NetworkingFactory.UdpWriter<MessageMatchInfo>(IPAddress.Parse(ServerIp), Ports.MatchPort);

            _listener = new UdpMessageListener<MessageMatchInfo>(Ports.MatchAnswerPort);
            _listener.IncomingMessage += OnIncomingMatchAnswer;
            _listener.Start();

            this.Fonts = fonts;
            MyTurn = myTurn;

            InitializeComponent();

            //add all controls to list
            var graphics = FieldPanel.CreateGraphics();
            _drawerClass = new SeaBattleDrawerClass(FieldPanel, graphics);
            
            _fieldController = new Rectangle(-3 + 79 + 520, 187, 400, 400);

            var tmp = new List<ImageObj>
            {
            };
            
            _opponentField = new Field(new byte[10,10], new List<Ship>());
            _myField = myField;

            _drawerClass.DrawItems(tmp);

            UpdateField();
        }
        
        /// <summary>
        /// Updates field
        /// </summary>
        private void UpdateField()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    UpdateField();
                }));
            }
            else
            {
                _drawerClass.ChangeTexts(GetRefreshedTexts());
                _drawerClass.DrawFieldOnGraphics(_myField, -3, 147);
                _drawerClass.DrawFieldOnGraphics(_opponentField, -3 + 400 + 120, 147, 2);
                _drawerClass.RedrawAll();
            }
        }

        /// <summary>
        /// Closes form (Thread safe)
        /// </summary>
        private void CloseMatch()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    CloseMatch();
                }));
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// Defines behavior on match answer
        /// </summary>
        /// <param name="sender">--</param>
        /// <param name="e">Match info message</param>
        private void OnIncomingMatchAnswer(object sender, IncomingMessageEventArgs<MessageMatchInfo> e)
        {
            if (e.Message.Type == MessageMatchType.SendTurn)
            {
                if (e.Message.IsMyShot)
                {
                    _opponentField.Marks = e.Message.Field.Marks;
                }
                else
                {
                    _myField.Marks = e.Message.Field.Marks;
                }

                MyTurn = e.Message.IsMyTurn;

                UpdateField();
            }
            else if (e.Message.Type == MessageMatchType.EndMatch)
            {
                if (e.Message.IsMyShot)
                {
                    _opponentField.Marks = e.Message.Field.Marks;
                }
                else
                {
                    _myField.Marks = e.Message.Field.Marks;
                }

                UpdateField();

                MessageBox.Show(e.Message.IsMyShot ? "Вы победили!" : "Вы проиграли.",
                    @"Конец матча",
                    MessageBoxButtons.OK);

                EndGame = true;
                CloseMatch();
            }
        }
        
        /// <summary>
        /// Defines behavior after click on the panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldPanel_Click(object sender, EventArgs e)
        {
            var relativePoint = PointToClient(Cursor.Position);

            var t = e.GetType();
            if (t == typeof(MouseEventArgs))
            {
                var mouse = (MouseEventArgs)e;

                switch (mouse.Button)
                {
                    case MouseButtons.Left:
                        if (_fieldController.Contains(relativePoint))
                        {
                            var insidePoint = new Point(relativePoint.X - _fieldController.X, relativePoint.Y - _fieldController.Y);
                            var i = insidePoint.X / 40;
                            var j = insidePoint.Y / 40;

                            if (MyTurn && _opponentField.Marks[i, j] == 0)
                            {
                                MyTurn = false;

                                var sendTurn = new MessageMatchInfo(Token, null, new Turn(i, j), MessageMatchType.SendTurn);
                                _server.Write(sendTurn);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Returns updated texts for this form
        /// </summary>
        /// <returns>List of text objects</returns>
        List<TextObj> GetRefreshedTexts()
        {
            return new List<TextObj>
            {
                new TextObj(MyTurn ? "Ваш ход" : "Ходит противник", Fonts["Anime Font 30"], new SolidBrush(Color.FromArgb(62, 51, 98)), 539, 80, new StringFormat{Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center})
            };
        }
        
        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (EndGame)
            {
                _listener.Dispose();
                ParentForm.EndGame = EndGame;
                ParentForm.Close();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void Game_Activated(object sender, EventArgs e)
        {
            UpdateField();
        }

        private void DebugRefresh_Click(object sender, EventArgs e)
        {
            UpdateField();
        }
    }
}
