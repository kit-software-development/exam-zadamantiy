using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SeaBattle.Lib.Messaging;
using SeaBattle.Lib.Networking;
using SeaBattle.Lib.Threading;

namespace SeaBattle.Client
{
    /// <summary>
    /// Login form
    /// </summary>
    public partial class LoginForm : Form
    {
        public bool EndGame;
        public string Token;

        private MD5 md5 = MD5.Create();

        private readonly UdpMessageListener<AnswerAuth> _listener;
        private static INetworkWriter<MessageAuth> _server;
        private static bool _isRegister;

        private LobbyForm _lobbyForm;

        public LoginForm()
        {
            EndGame = false;

            _listener = new UdpMessageListener<AnswerAuth>(Ports.AuthAnswerPort);
            _listener.IncomingMessage += OnIncomingAnswer;
            _listener.Start();

            InitializeComponent();

            PasswordTextbox.KeyDown += new KeyEventHandler(PasswordEnter);
        }

        /// <summary>
        /// On "enter" pressed in the password field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PasswordEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _isRegister = false;
                GetToken(LoginTextbox.Text, PasswordTextbox.Text, _isRegister);
            }
        }

        /// <summary>
        /// Defines behaviour on incoming auth answer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIncomingAnswer(object sender, IncomingMessageEventArgs<AnswerAuth> e)
        {
            if (e.Message.IsSucceed)
            {
                Token = e.Message.Token;
                GoToLobby();
            }
            else
            {
                MessageBox.Show(
                    _isRegister ? @"Such username already in use." : @"Login-Password pair is incorrect.",
                    @"Error",
                    MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Switch form to menu
        /// </summary>
        public void GoToLobby()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    GoToLobby();
                }));
            }
            else
            {
                Hide();
                //_gameForm = new Game {Token = token};
                _lobbyForm = new LobbyForm(Token, ServerIPTextbox.Text) {ParentForm = this};
                _lobbyForm.Show();
                _listener.Dispose();
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            _isRegister = false;
            GetToken(LoginTextbox.Text, PasswordTextbox.Text, _isRegister);
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            _isRegister = true;
            GetToken(LoginTextbox.Text, PasswordTextbox.Text, _isRegister);
        }

        /// <summary>
        /// Gets token from server by login-password
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <param name="isRegister">Is it register or login</param>
        private void GetToken(string login, string password, bool isRegister)
        {
            _server = NetworkingFactory.UdpWriter<MessageAuth>(IPAddress.Parse(ServerIPTextbox.Text), Ports.AuthPort);
            var authMessage = new MessageAuth(login, Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(password))), isRegister);
            _server.Write(authMessage);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            MaximumSize = Size;
            MinimumSize = Size;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Event that handles returning to lobby after match end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshCheckTimer_Tick(object sender, EventArgs e)
        {
            refreshCheckTimer.Stop();
            _lobbyForm = new LobbyForm(Token, ServerIPTextbox.Text) {ParentForm = this};
            _lobbyForm.Show();
        }
    }
}
