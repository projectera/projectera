using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;
using ProjectERA.Services.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using System.Threading;
using ProjectERA.Services.Data;
using ERAUtils.Logger;

namespace ProjectERA.Screen
{
    /// <summary>
    /// 
    /// </summary>
    internal class LoginScreen : GameScreen
    {
        private TextInputComponent _usernameInput, _passwordInput;
        private LoginAction _cursorAction;
        private List<String> _status;
        private Boolean _firstLogin;

        private VariableLoadingPopup _actionStatus;
        private ProjectERA.Services.Network.Registration _registration;

        /// <summary>
        /// Constructor
        /// </summary>
        internal LoginScreen()
        {
            // Set the transition time
            this.TransitionOnTime = TimeSpan.FromSeconds(1);
            this.TransitionOffTime = TimeSpan.FromSeconds(1);

            // Set the type
            this.IsPopup = false;

            _status = new List<String>();

            this.Exited += new EventHandler(LoginScreen_Exited);
            this.Exiting += new EventHandler(LoginScreen_Exiting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginScreen_Exiting(object sender, EventArgs e)
        {
            if (_actionStatus != null)
                _actionStatus.FinishProgress();
        }

        /// <summary>
        /// Event: Loginscreen exited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginScreen_Exited(object sender, EventArgs e)
        {
            // Remove components
            this.Game.Components.Remove(_usernameInput);
            this.Game.Components.Remove(_passwordInput);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        internal override void Initialize()
        {
            // Initialize GameScreen
            base.Initialize();

            _usernameInput = new TextInputComponent(this.Game);
            _passwordInput = new TextInputComponent(this.Game);

            // Edit options
            _usernameInput.SpacesEnabled = false;
            _passwordInput.SpacesEnabled = false;

            // Set current active
            EnableInput();

            // Add components
            this.Game.Components.Add(_usernameInput);
            this.Game.Components.Add(_passwordInput);

            if (this.NetworkManager.IsAuthenticated || this.NetworkManager.IsAuthenticating || this.NetworkManager.IsConnected || this.NetworkManager.IsConnecting)
                this.NetworkManager.Disconnect("Client logout. Goodbye!");

            this.NetworkManager.OnAuthenticated += new Services.Network.NetworkManager.AuthenticationSucces(NetworkManager_OnAuthenticated);
            this.NetworkManager.OnAuthenticationFailed += new Services.Network.NetworkManager.AuthenticationFailure(NetworkManager_OnAuthenticationFailed);
            this.NetworkManager.OnAuthenticationStep += new Services.Network.NetworkManager.AuthenticationProgress(NetworkManager_OnAuthenticationStep);
            this.NetworkManager.OnAuthenticationDenied += new Services.Network.NetworkManager.AuthenticationFailure(NetworkManager_OnAuthenticationDenied);
            this.NetworkManager.OnAuthenticationTimeout += new Services.Network.NetworkManager.AuthenticationFailure(NetworkManager_OnAuthenticationTimeout);
        }

        /// <summary>
        /// Enables input
        /// </summary>
        /// <remarks>Puts cursor on username input</remarks>
        private void EnableInput()
        {
            _usernameInput.Enabled = true;
            _passwordInput.Enabled = false;

            _cursorAction = LoginAction.UsernameInput;
        }

        /// <summary>
        /// Disables input
        /// </summary>
        private void DisableInput()
        {
            _usernameInput.Enabled = false;
            _passwordInput.Enabled = false;
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values.</param>
        /// <param name="otherScreenHasFocus">Other screen has focus flag</param>
        /// <param name="coveredByOtherScreen">Other gameScreen over this</param>
        internal override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !this.IsExiting && !coveredByOtherScreen)
                UpdateInput();

            // Update transition
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Updates input
        /// </summary>
        private void UpdateInput()
        {
            // When pressed enter
            if (!_networkManager.IsAuthenticating && !_networkManager.IsAuthenticated && !_networkManager.IsConnecting && !_networkManager.IsConnected
                && this.InputManager.Keyboard.IsKeyTriggerd(Keys.Enter))
            {
                switch (_cursorAction)
                {
                    case LoginAction.UsernameInput:
                    case LoginAction.PasswordInput:
                        NextAction();
                        break;
                    case LoginAction.InstantPlay:
                        InstantPlay();
                        break;
                    case LoginAction.InstantRegister:
                        InstantRegister();
                        break;

                    case LoginAction.Login:
                        Login();
                        break;
                    case LoginAction.Register:
                        Register();
                        break;

                    case LoginAction.Exit:
                        _hasCalledExited = true;
                        ExitScreen();
                        break;
                }
            }

            // When pressed escape
            if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Escape))
            {
                switch(_cursorAction)
                {
                    case LoginAction.UsernameInput:
                    case LoginAction.PasswordInput:
                    case LoginAction.Exit:
                        _hasCalledExited = true;
                        ExitScreen();
                        break;

                    case LoginAction.InstantPlay:
                    case LoginAction.Login:
                        CancelLogin();
                        break;
                }
            }

            // Movement of cursor
            if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Up))
                if (!_networkManager.IsAuthenticating && !_networkManager.IsAuthenticated && !_networkManager.IsConnecting && !_networkManager.IsConnected)
                    PreviousAction();

            if (this.InputManager.Keyboard.IsKeyTriggerd(Keys.Down) | this.InputManager.Keyboard.IsKeyTriggerd(Keys.Tab))
                if (!_networkManager.IsAuthenticating && !_networkManager.IsAuthenticated && !_networkManager.IsConnecting && !_networkManager.IsConnected)
                    NextAction();

            if (this._inputManager.Keyboard.IsKeyTriggerd(Keys.Left))
            {
                switch (_cursorAction)
                {
                    case LoginAction.PasswordInput:
                        _passwordInput.CursorIndex = Math.Max(0, _passwordInput.CursorIndex - 1);
                        break;
                    case LoginAction.UsernameInput:
                        _usernameInput.CursorIndex = Math.Max(0, _usernameInput.CursorIndex - 1);
                        break;
                }
            }

            if (this._inputManager.Keyboard.IsKeyTriggerd(Keys.Right))
            {
                switch (_cursorAction)
                {
                    case LoginAction.PasswordInput:
                        _passwordInput.CursorIndex = Math.Min(_passwordInput.Text.Length - 1, _passwordInput.CursorIndex + 1);
                        break;
                    case LoginAction.UsernameInput:
                        _usernameInput.CursorIndex = Math.Min(_usernameInput.Text.Length - 1, _usernameInput.CursorIndex + 1);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InstantPlay()
        {
            String username = String.Empty, password = String.Empty;
            if ((!InstantPlayManager.Exists(this.Game) || !InstantPlayManager.Load(this.Game, out username, out password)) && (_registration == null || _registration.IsBusy == false))
            {
                if (_actionStatus != null && _actionStatus.IsActive)
                    _actionStatus.ExitScreen();
                _actionStatus = new VariableLoadingPopup(new Vector2(1280 / 2, 720 / 2));
                this.ScreenManager.AddScreen(_actionStatus);

                _registration = Services.Network.Registration.Generate(this.Game);
                _actionStatus.DisplayText = "Looking for server";

                _registration.OnStatusChanged += new EventHandler<Services.Network.Registration.StatusChangedEventArgs>(_registration_OnStatusChanged);
                _registration.Register(
                    (u, p) =>
                    {
                        Logger.Info("Registeration succeeded!");
                        _actionStatus.DisplayText = "Saving credentials";

                        InstantPlayManager.Save(this.Game, u, p);
                        _firstLogin = true;
                        InstantPlay();
                    },
                    (e) =>
                    {
                        Logger.Error("Registeration failed: " + e.Message);
                        PushStatus("Registration failed: " + e.Message);
                        _actionStatus.FinishProgress();
                    });
                return;
            }

            _usernameInput.Text = username;
            _passwordInput.Text = password;
            Login(true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InstantRegister()
        {
            String username = String.Empty, password = String.Empty;
            
            if (_actionStatus != null && _actionStatus.IsActive)
                _actionStatus.ExitScreen();
            _actionStatus = new VariableLoadingPopup(new Vector2(1280 / 2, 720 / 2));
            this.ScreenManager.AddScreen(_actionStatus);

            _registration = Services.Network.Registration.Generate(this.Game);
            _actionStatus.DisplayText = "Looking for server";

            _registration.OnStatusChanged += new EventHandler<Services.Network.Registration.StatusChangedEventArgs>(_registration_OnStatusChanged);
            _registration.Register(
                (u, p) =>
                {
                    Logger.Info("Registeration succeeded!");
                    _actionStatus.DisplayText = "Saving credentials";

                    InstantPlayManager.Save(this.Game, u, p);
                    _firstLogin = true;
                    InstantPlay();
                },
                (e) =>
                {
                    Logger.Error("Registeration failed: " + e.Message);
                    PushStatus("Registration failed: " + e.Message);
                    _actionStatus.FinishProgress();
                });
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Register()
        {
            String username = _usernameInput.Text, password = _usernameInput.Text;

            if (_actionStatus != null && _actionStatus.IsActive)
                _actionStatus.ExitScreen();
            _actionStatus = new VariableLoadingPopup(new Vector2(1280 / 2, 720 / 2));
            this.ScreenManager.AddScreen(_actionStatus);

            _registration = Services.Network.Registration.Generate(this.Game, username, password);
            _actionStatus.DisplayText = "Looking for server";

            _registration.OnStatusChanged += new EventHandler<Services.Network.Registration.StatusChangedEventArgs>(_registration_OnStatusChanged);
            _registration.Register(
                (u, p) =>
                {
                    Logger.Info("Registeration succeeded!");
                    _actionStatus.DisplayText = "Saving credentials";
                    _firstLogin = true;
                    Login();
                },
                (e) =>
                {
                    Logger.Error("Registeration failed: " + e.Message);
                    PushStatus("Registration failed: " + e.Message);
                    _actionStatus.FinishProgress();
                });
            return;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _registration_OnStatusChanged(object sender, Services.Network.Registration.StatusChangedEventArgs e)
        {
            switch (e.Current)
            {
                case Services.Network.Registration.Status.Succes:
                    PushStatus("Registration completed");
                    _actionStatus.DisplayText = "Registration completed";
                    break;

                case Services.Network.Registration.Status.Failure:
                    //PushStatus("Registration failed");
                    _actionStatus.DisplayText = "Registration failed";
                    break;

                case Services.Network.Registration.Status.None:
                    break;

                case Services.Network.Registration.Status.RegistrationGenerated:
                    PushStatus("Registration initialized");
                    break;

                default:
                    if (e.Current.HasFlag(Services.Network.Registration.Status.Anonymous))
                        e.Current = e.Current & ~Services.Network.Registration.Status.Anonymous;

                    switch (e.Current)
                    {
                        case Services.Network.Registration.Status.ServerReceived:
                            _actionStatus.DisplayText = "Connecting";
                            break;
                        case Services.Network.Registration.Status.Connected:
                            _actionStatus.DisplayText = "Authenticating";
                            break;
                        case Services.Network.Registration.Status.Secured:
                            _actionStatus.DisplayText = "Authenticated";
                            break;
                        case Services.Network.Registration.Status.RequestSent:
                            _actionStatus.DisplayText = "Registering";
                            break;
                    }

                    PushStatus("In progress... (" + e.Current + ")");
                    break;
            }
        }

        /// <summary>
        /// Moves cursor to previous action
        /// </summary>
        private void PreviousAction()
        {
            // NOTE clean up switches (make function call?)
            switch (_cursorAction)
            {
                case LoginAction.UsernameInput:
                    _usernameInput.Enabled = false;
                    break;
                case LoginAction.PasswordInput:
                    _passwordInput.Enabled = false;
                    break;
            }

            if (_cursorAction == 0)
                _cursorAction = (LoginAction)(Enum.GetNames(typeof(LoginAction)).Length - 4);
            else
                _cursorAction = (LoginAction)((Byte)_cursorAction - 1);

            switch (_cursorAction)
            {
                case LoginAction.UsernameInput:
                    _usernameInput.Enabled = true;
                    break;
                case LoginAction.PasswordInput:
                    _passwordInput.Enabled = true;
                    break;
            }
        }

        /// <summary>
        /// Moves cursor to next action
        /// </summary>
        private void NextAction()
        {
            switch (_cursorAction)
            {
                case LoginAction.UsernameInput:
                    _usernameInput.Enabled = false;
                    break;
                case LoginAction.PasswordInput:
                    _passwordInput.Enabled = false;
                    break;
            }

            _cursorAction = (LoginAction)(((Byte)_cursorAction + 1) % Enum.GetNames(typeof(LoginAction)).Length);

            switch (_cursorAction)
            {
                case LoginAction.UsernameInput:
                    _usernameInput.Enabled = true;
                    break;
                case LoginAction.PasswordInput:
                    _passwordInput.Enabled = true;
                    break;
            }
        }

        /// <summary>
        /// Starts login
        /// </summary>
        private void Login(Boolean fromInstant = false)
        {
            _firstLogin = (!fromInstant ? false : _firstLogin);
            _networkManager.CancelConnect();

            DisableInput();          

            Task.Factory.StartNew(() =>
                {
                    while (_networkManager.IsConnected)
                    {
                        Thread.Sleep(1);
                        Thread.MemoryBarrier(); // We don't want a cached connectionstatus
                    }

                    if (!_firstLogin)
                    {
                        if (_actionStatus != null && _actionStatus.IsActive)
                            _actionStatus.ExitScreen();

                        _actionStatus = new VariableLoadingPopup(new Vector2(1280 / 2, 720 / 2));
                        this.ScreenManager.AddScreen(_actionStatus);
                    }

                    // Connect when not connected
                    _networkManager.AsyncConnect(_usernameInput.Text, _passwordInput.Text);
                });
        }

        /// <summary>
        /// Cancels Login and restores input
        /// </summary>
        private void CancelLogin()
        {
            //lock (_cancelCall)
            //{
                _networkManager.CancelConnect();
                EnableInput();
            //}

            if (!_firstLogin)
                _actionStatus.FinishProgress();
        }

        #region NetworkManager Events
        void NetworkManager_OnAuthenticationStep(Services.Network.NetworkManager.AuthenticationStatus step)
        {
            switch (step)
            {
                // Connecting not possible
                case Services.Network.NetworkManager.AuthenticationStatus.NoServerConnection:
                case Services.Network.NetworkManager.AuthenticationStatus.NoServerFound:
                    PushStatus("Server not available.");
                    _actionStatus.DisplayText = "Server offline";
                    CancelLogin();
                    break;

                // Still connecting
                case Services.Network.NetworkManager.AuthenticationStatus.ServerFound:
                case Services.Network.NetworkManager.AuthenticationStatus.FindServer:
                case Services.Network.NetworkManager.AuthenticationStatus.ServerConnection:
                case Services.Network.NetworkManager.AuthenticationStatus.HandshakeData:
                case Services.Network.NetworkManager.AuthenticationStatus.HandshakeVerification:
                    PushStatus("In progress... (" + step + ")");

                    switch (step)
                    {
                        case Services.Network.NetworkManager.AuthenticationStatus.ServerFound:
                            _actionStatus.DisplayText = "Server found";
                            break;
                        case Services.Network.NetworkManager.AuthenticationStatus.ServerConnection:
                            _actionStatus.DisplayText = "Server Connected"; // //ing";
                            break;
                        case Services.Network.NetworkManager.AuthenticationStatus.FindServer:
                            _actionStatus.DisplayText = "Looking for server";
                            break;
                        case Services.Network.NetworkManager.AuthenticationStatus.HandshakeData:
                            _actionStatus.DisplayText = "Establishing secure connection";
                            break;
                        case Services.Network.NetworkManager.AuthenticationStatus.HandshakeVerification:
                            _actionStatus.DisplayText = "Verifying secure connection";
                            break;
                    }
                    break;

                // These are handled by the other events/functions
                case Services.Network.NetworkManager.AuthenticationStatus.Cancelled:
                case Services.Network.NetworkManager.AuthenticationStatus.None:
                case Services.Network.NetworkManager.AuthenticationStatus.HandshakeExpired:
                case Services.Network.NetworkManager.AuthenticationStatus.HandshakeFailed:
                case Services.Network.NetworkManager.AuthenticationStatus.HandshakeDenied:
                    PushStatus(step.ToString());
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        void NetworkManager_OnAuthenticationFailed(String reason)
        {
            System.Threading.Thread.MemoryBarrier();

            PushStatus(reason); //"Wrong username and/or password");
            _actionStatus.DisplayText = reason ==  "Could not connect" ? "Server Error" : reason;

            if (_firstLogin)
            {
                _actionStatus.DisplayText = "Registration error";
                InstantPlayManager.Delete(this.Game);
                _firstLogin = false;
                CancelLogin();
                InstantPlay();
                return;
            }

            CancelLogin();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        void NetworkManager_OnAuthenticationTimeout(string reason)
        {
            System.Threading.Thread.MemoryBarrier();

            PushStatus(reason); //"Wrong username and/or password");
            _actionStatus.DisplayText =  "Server offline!";
            CancelLogin();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
        void NetworkManager_OnAuthenticationDenied(string reason)
        {
            System.Threading.Thread.MemoryBarrier();

            PushStatus("Wrong username and/or password");
            _actionStatus.DisplayText = reason ==  "Could not connect" ? "Server offline" : "Wrong credentials";

            if (_firstLogin)
            {
                _actionStatus.DisplayText = "Registration error";
                InstantPlayManager.Delete(this.Game);
                _firstLogin = false;
                CancelLogin();
                InstantPlay();
                return;
            }

            CancelLogin();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        void NetworkManager_OnAuthenticated(Connection connection)
        {
            PushStatus("Authentication complete!");
            _actionStatus.DisplayText = "Authentication complete!";

            _hasCalledExited = true;
            this.Next = new AvatarSelectionScreen();
            ExitScreen();
        }


        #endregion

        /// <summary>
        /// Pushes a new status message
        /// </summary>
        /// <param name="message">Message to push</param>
        private void PushStatus(String message)
        {
            lock (_status)
                _status.Add(message);
        }

        /// <summary>
        /// Draw frame
        /// </summary>
        /// <param name="gameTime">Snapshot of timing values.</param>
        internal override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            String instant = "InstantPlay";
            String instant2 = "Register anonymously";
            String register = "Register";
            String username = "username: " + _usernameInput.Text;
            String password = "password: " + _passwordInput.Text; // NOTE use a * mask with password length
            String login = "Login";
            String exit = "Exit";

            this.ScreenManager.SpriteBatch.Begin();

            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], instant, new Vector2(21, 41), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], instant, new Vector2(20, 40), _cursorAction == LoginAction.InstantPlay ? Color.LightGreen : Color.White);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], instant2, new Vector2(21, 51), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], instant2, new Vector2(20, 50), _cursorAction == LoginAction.InstantRegister ? Color.LightGreen : Color.White);

            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], username, new Vector2(21, 71), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], username, new Vector2(20, 70), _usernameInput.Enabled ? (_cursorAction == LoginAction.UsernameInput ? Color.LightGreen : Color.White) : Color.Gray);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], password, new Vector2(21, 81), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], password, new Vector2(20, 80), _passwordInput.Enabled ? (_cursorAction == LoginAction.PasswordInput ? Color.LightGreen : Color.White) : Color.Gray);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], login, new Vector2(21, 91), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], login, new Vector2(20, 90), _cursorAction == LoginAction.Login ? Color.LightGreen : Color.White);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], register, new Vector2(21, 101), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], register, new Vector2(20, 100), _cursorAction == LoginAction.Register ? Color.LightGreen : Color.White);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], exit, new Vector2(21, 121), Color.Black);
            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], exit, new Vector2(20, 120), _cursorAction == LoginAction.Exit ? Color.LightGreen : Color.White);

            String[] status;

            lock(_status)
            {
                status = new String[_status.Count];
                _status.CopyTo(status);
            }

            StringBuilder statustxt = new StringBuilder();

            for (int i = status.Length-1; i >= 0; i--)
            {
                statustxt.AppendLine(status[i]);
            }

            this.ScreenManager.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], statustxt.ToString(), new Vector2(20, 140), Color.White);
            this.ScreenManager.SpriteBatch.End();

            // Draw the black fading graphic
            if (this.IsTransitioning)
                this.ScreenManager.FadeBackBufferToBlack((Byte)(255 - TransitionAlpha));
        }

        /// <summary>
        /// 
        /// </summary>
        private enum LoginAction : byte
        {
            InstantPlay = 0,
            InstantRegister = 1,
            UsernameInput = 2,
            PasswordInput = 3,
            Login = 4,
            Register = 5,
            Exit = 6,
        }
    }
}
