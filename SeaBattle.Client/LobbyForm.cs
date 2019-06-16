using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SeaBattle.Client.Misc;
using SeaBattle.Client.Properties;
using Microsoft.VisualBasic;
using SeaBattle.Lib.Messaging;
using SeaBattle.Lib.Networking;
using SeaBattle.Lib.Threading;

namespace SeaBattle.Client
{
    /// <summary>
    /// Menu form
    /// </summary>
    public partial class LobbyForm : Form
    {
        public LoginForm ParentForm;
        public bool EndGame;

        private readonly SeaBattleDrawerClass _drawerClass;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly PrivateFontCollection _pfc;

        public string Token;
        public string ServerIp;

        private readonly INetworkWriter<MessageDuel> _server;
        private readonly UdpMessageListener<MessageDuel> _listener;

        //Controls
        private readonly ImageObj _buttonPlay;

        //Resources
        public Dictionary<string, Font> Fonts = new Dictionary<string, Font>();

        private PreGame _gameForm;

        public LobbyForm(string token, string serverIp)
        {
            EndGame = false;

            //SERVER
            Token = token;
            ServerIp = serverIp;
            _server = NetworkingFactory.UdpWriter<MessageDuel>(IPAddress.Parse(ServerIp), Ports.DuelPort);
            
            _listener = new UdpMessageListener<MessageDuel>(Ports.DuelAnswerPort);
            _listener.IncomingMessage += OnIncomingDuelMessage;
            _listener.Start();

            InitializeComponent();

            //Load resources
            //Graphics
            var graphics = FieldPanel.CreateGraphics();
            _drawerClass = new SeaBattleDrawerClass(FieldPanel, graphics);

            //Fonts
            _pfc = new PrivateFontCollection();
            _pfc.AddFontFile(Directory.GetCurrentDirectory() + "\\Resources\\AnimeFont.ttf");
            Fonts.Add("Anime Font 30", new Font(_pfc.Families[0], 20, FontStyle.Regular));

            //Controls
            _buttonPlay = new ImageObj(Resources.Button, (this.Width - Resources.Button.Width) / 2, 218, Resources.Button.Width, Resources.Button.Height);
            
            //Draw controls
            var tmp = new List<ImageObj>
            {
                _buttonPlay,
            };

            _drawerClass.DrawItems(tmp);

            UpdateField();
        }

        /// <summary>
        /// Defines behavior on incoming duel message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIncomingDuelMessage(object sender, IncomingMessageEventArgs<MessageDuel> e)
        {
            if (e.Message.IsSucceed && e.Message.Type == 4)
            {
                GoToGame(Token);
            }
            else if(e.Message.Type == 4)
            {
                //TODO: Clear
                MessageBox.Show(@"Оппонент отказался", @"Error", MessageBoxButtons.OK);
            }
            else if (e.Message.Type == 2)
            {
                var dialogResult = MessageBox.Show(String.Format("Вас вызвал на дуэль игрок {0}[{1}]", e.Message.Nickname, e.Message.Rating), @"Вызов на дуэль", MessageBoxButtons.YesNo);
                _server.Write(new MessageDuel(Token, e.Message.Nickname, 3, dialogResult == DialogResult.Yes));
            }
            else
            {
                MessageBox.Show(@"Тип сообщения не распознан", @"Error", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Switch room to editor
        /// </summary>
        /// <param name="token">Our token</param>
        private void GoToGame(string token)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    GoToGame(token);
                }));
            }
            else
            {
                Hide();
                _gameForm = new PreGame(Fonts, token, this.ServerIp);
                _gameForm.ParentForm = this;
                _gameForm.Show();
                _listener.Dispose();
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
                        if (_buttonPlay.Contains(relativePoint))
                        {
                            string nickname = Interaction.InputBox("Введите ник игрока, которого Вы хотите вызвать на дуэль:", "Дуэль", "");
                            if (nickname != "")
                            {
                                var duelMessage = new MessageDuel(Token, nickname);
                                _server.Write(duelMessage);
                                return;
                            }
                            //TODO:
                        }
                        break;
                    case MouseButtons.Right:
                        break;
                }
            }
        }

        /// <summary>
        /// Refresh graphic manually
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugRefreshButton_Click(object sender, EventArgs e)
        {
            UpdateField();
        }

        /// <summary>
        /// Refresh graphic
        /// </summary>
        private void UpdateField()
        {
            _drawerClass.ChangeTexts(GetRefreshedCountTexts());
            _drawerClass.RedrawAll();
        }

        /// <summary>
        /// Returns updated texts for this form
        /// </summary>
        /// <returns>List of text objects</returns>
        List<TextObj> GetRefreshedCountTexts()
        {
            return new List<TextObj>
            {
                new TextObj("Дуэль", Fonts["Anime Font 30"], new SolidBrush(Color.FromArgb(62, 51, 98)), _buttonPlay.Rectangle.X + _buttonPlay.Rectangle.Width / 3, _buttonPlay.Rectangle.Y + _buttonPlay.Rectangle.Height / 3),
            };
        }
        
        private void LobbyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (EndGame)
            {
                ParentForm.EndGame = true;
                ParentForm.refreshCheckTimer.Start();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void LobbyForm_LocationChanged(object sender, EventArgs e)
        {
            UpdateField();
        }
        
        private void LobbyForm_Activated(object sender, EventArgs e)
        {
            UpdateField();
        }
        
    }
}
