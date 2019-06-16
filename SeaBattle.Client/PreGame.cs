using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SeaBattle.Client.Misc;
using SeaBattle.Client.Properties;
using SeaBattle.Lib.Gaming;
using SeaBattle.Lib.Messaging;
using SeaBattle.Lib.Networking;
using SeaBattle.Lib.Threading;

namespace SeaBattle.Client
{
    /// <summary>
    /// Form with editor
    /// </summary>
    public sealed partial class PreGame : Form
    {
        public LobbyForm ParentForm;
        public bool EndGame;

        private readonly SeaBattleDrawerClass _drawerClass;
        private int _selected = -1;

        private static Editor Editor;

        public string Token;
        public string ServerIp;
        private readonly INetworkWriter<MessageMatchInfo> _server;
        private readonly UdpMessageListener<MessageMatchInfo> _listener;

        //Controls
        private readonly ImageObj _ship1;
        private readonly ImageObj _ship2;
        private readonly ImageObj _ship3;
        private readonly ImageObj _ship4;
        private readonly ImageObj _buttonNext;
        private readonly Rectangle _fieldController;

        private bool _disableEdit = false;

        public Dictionary<string, Font> Fonts;

        private Game _game;

        public PreGame(Dictionary<string, Font> fonts, string token, string serverIp)
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

            InitializeComponent();
            //lock
            MaximumSize = Size;
            MinimumSize = Size;

            //add all controls to list
            var graphics = FieldPanel.CreateGraphics();
            _drawerClass = new SeaBattleDrawerClass(FieldPanel, graphics);

            Editor = new Editor(new Field(new byte[10, 10], new List<Ship>()), 4, 3, 2, 1);
            _drawerClass.DrawFieldOnGraphics( Editor.Field, -3 + 120, 147);

            _ship1 = new ImageObj(Resources.Ship_1, 670, 218, Resources.Ship_1.Width, Resources.Ship_1.Height);
            _ship2 = new ImageObj(Resources.Ship_2, 670, 298, Resources.Ship_2.Width, Resources.Ship_2.Height);
            _ship3 = new ImageObj(Resources.Ship_3, 670, 378, Resources.Ship_3.Width, Resources.Ship_3.Height);
            _ship4 = new ImageObj(Resources.Ship_4, 670, 458, Resources.Ship_4.Width, Resources.Ship_4.Height);
            _buttonNext = new ImageObj(Resources.NextButton, 890, 608, Resources.NextButton.Width, Resources.NextButton.Height);
            _fieldController = new Rectangle(199, 187, 400, 400);

            var tmp = new List<ImageObj>
            {
                _ship1,
                _ship2,
                _ship3,
                _ship4,
                _buttonNext
            };

            _drawerClass.DrawItems(tmp);

            UpdateField();
        }

        /// <summary>
        /// Defines bahaviour on match answer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIncomingMatchAnswer(object sender, IncomingMessageEventArgs<MessageMatchInfo> e)
        {
            if (e.Message.Type == MessageMatchType.StartMatch)
            {
                GoToGame(e.Message.Token, e.Message.IsMyTurn);
            }
            else if (e.Message.Type == MessageMatchType.OpponentStillThinking)
            {
                MessageBox.Show(@"Соперник ещё не готов, подождите...",
                    @"Note",
                    MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Switches form to game
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isMyTurn"></param>
        private void GoToGame(string token, bool isMyTurn)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    GoToGame(token, isMyTurn);
                }));
            }
            else
            {
                _listener.Dispose();
                Hide();
                //TODO:
                _game = new Game(Fonts, Token, ServerIp, Editor.Field, !isMyTurn);
                _game.ParentForm = this;
                _game.Show();
            }
        }

        /// <summary>
        /// Defines behavior after clicking on Field Panel
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
                        if (_ship1.Contains(relativePoint))
                        {
                            SetSelection(_ship1, 1);
                        }
                        else if (_ship2.Contains(relativePoint))
                        {
                            SetSelection(_ship2, 2);
                        }
                        else if (_ship3.Contains(relativePoint))
                        {
                            SetSelection(_ship3, 3);
                        }
                        else if (_ship4.Contains(relativePoint))
                        {
                            SetSelection(_ship4, 4);
                        }
                        else if (_buttonNext.Contains(relativePoint))
                        {
                            if (Editor.GetShipsLeft(1) + Editor.GetShipsLeft(2) + Editor.GetShipsLeft(3) +
                                Editor.GetShipsLeft(4) == 0)
                            {
                                var sendField = new MessageMatchInfo(Token, Editor.Field, null, MessageMatchType.SendField);
                                _server.Write(sendField);
                                _disableEdit = true;
                                return;
                            }
                        }
                        else if (!_disableEdit && _fieldController.Contains(relativePoint))
                        {
                            var insidePoint = new Point(relativePoint.X - _fieldController.X, relativePoint.Y - _fieldController.Y);
                            var i = insidePoint.X / 40;
                            var j = insidePoint.Y / 40;
                            if (_selected != -1)
                            {
                                if (Editor.ProceedAction(i, j, _selected))
                                    UpdateField();
                            }
                        }
                        break;
                    case MouseButtons.Right:
                        if (_fieldController.Contains(relativePoint))
                        {
                            var insidePoint = new Point(relativePoint.X - _fieldController.X, relativePoint.Y - _fieldController.Y);
                            var i = insidePoint.X / 40;
                            var j = insidePoint.Y / 40;
                            if (Editor.RemoveAt(i, j))
                                UpdateField();
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// Manually refresh graphics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugRefreshButton_Click(object sender, EventArgs e)
        {
            UpdateField();
        }

        /// <summary>
        /// Refresh graphics
        /// </summary>
        private void UpdateField()
        {
            _drawerClass.ChangeTexts(GetRefreshedCountTexts());
            _drawerClass.DrawFieldOnGraphics(Editor.Field, -3 + 120, 147);
            _drawerClass.RedrawAll();
        }

        /// <summary>
        /// Return list of text objects from game form
        /// </summary>
        /// <returns>List of text objects</returns>
        List<TextObj> GetRefreshedCountTexts()
        {
            return new List<TextObj>
            {
                new TextObj("X" + Editor.GetShipsLeft(1), Fonts["Anime Font 30"], new SolidBrush(Color.FromArgb(62, 51, 98)), 847, 232),
                new TextObj("X" + Editor.GetShipsLeft(2), Fonts["Anime Font 30"], new SolidBrush(Color.FromArgb(62, 51, 98)), 847, 232 + 80),
                new TextObj("X" + Editor.GetShipsLeft(3), Fonts["Anime Font 30"], new SolidBrush(Color.FromArgb(62, 51, 98)), 847, 232 + 160),
                new TextObj("X" + Editor.GetShipsLeft(4), Fonts["Anime Font 30"], new SolidBrush(Color.FromArgb(62, 51, 98)), 847, 232 + 240),
            };
        }

        /// <summary>
        /// Sets selection on ith ship
        /// </summary>
        /// <param name="pb">position</param>
        /// <param name="i">ship number</param>
        private void SetSelection(ImageObj pb, int i)
        {
            SetSelection(pb.Rectangle, i);
        }

        /// <summary>
        /// Sets selection on ith ship
        /// </summary>
        /// <param name="pb">position</param>
        /// <param name="i">ship number</param>
        private void SetSelection(Rectangle pb, int i)
        {
            if (i <= 0 || i > 4) return;

            _selected = i;
            _drawerClass.DrawSelection(pb.Left, pb.Top, pb.Width, pb.Height);
        }

        /// <summary>
        /// Defines behavior on window normalize
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) //WM_SYSCOMMAND
            {
                //If Windows normalized (from Winuser.h)
                if (m.WParam == new IntPtr(0xF120))
                {
                    _drawerClass.RedrawAll();
                }
            }
            base.WndProc(ref m);
        }
        
        private void PreGame_LocationChanged(object sender, EventArgs e)
        {
            UpdateField();
        }

        private void PreGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (EndGame)
            {
                ParentForm.EndGame = true;
                ParentForm.Close();
            }
            else
            {
                Environment.Exit(0);
            }
        }
        
        private void PreGame_Activated(object sender, EventArgs e)
        {
            UpdateField();
        }
    }
}

