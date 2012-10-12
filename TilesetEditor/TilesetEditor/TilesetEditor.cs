using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ProjectERA.Services.Data;
using ProjectERA.Services.Display;
using ProjectERA.Services.Input;
using ERAUtils.Logger;

namespace ProjectERA.Editors.TilesetEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TilesetEditor : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        TilesetManager _tilesetManager;
        TextureManager _textureManager;
        InputManager _inputManager;

        Texture2D _tilesetTexture;
        Texture2D _selectorBgTexture;
        Texture2D _selectorOlTexture;
        Texture2D _backgroundTexture;
        SpriteFont _font;

        Int32 _tilesetIndex;
        Int32 _cursorIndex;
        //Int32 _actionIndex;
        Int32 _maxId;
        Int32 _oldHashCode;
        List<TilesetData> _saveThese;

        SelectorState _selectorState;

        /// <summary>
        /// Selector Positie
        /// </summary>
        private Vector2 SelectorPosition
        {
            get
            {
                return new Vector2(128 + _cursorIndex % 8 * 32, 128 + 48);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TilesetEditor()
        {
            Logger.Initialize(Severity.Info);

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 512;
            _graphics.PreferredBackBufferHeight = 512;
            _graphics.ApplyChanges();

            this.Content.RootDirectory = "Content/Editor";

            _textureManager = new TextureManager(this);
            _tilesetManager = new TilesetManager(this, String.Empty);
            _inputManager   = new InputManager(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.Components.Add(_textureManager);
            this.Components.Add(_tilesetManager);
            this.Components.Add(_inputManager);

            _tilesetIndex = 0;
            _selectorState = SelectorState.SelectTileset;
            _saveThese = new List<TilesetData>();

            _inputManager.Enabled = true;

            base.Initialize();
        }

        private List<TilesetData> _content;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Loads Tilesets
            _content = _tilesetManager.LoadRange(1, 20);
            Logger.Info("Loaded " + _content.Count + " tilesets in the range 1-20.");

            // Load editor tools
            _selectorBgTexture = this.Content.Load<Texture2D>("tsSelectorBG");
            _selectorOlTexture = this.Content.Load<Texture2D>("tsSelectorOverlay");
            _backgroundTexture = this.Content.Load<Texture2D>("backgroundTSSelec");
            _font = this.Content.Load<SpriteFont>("defaultFont");

            // Load first tileset
            LoadContent(_content[Math.Min(_tilesetIndex, _content.Count-1)]);
        }

        private Rectangle _bgSourceRect;

        /// <summary>
        /// Load Content for a specific tileset
        /// </summary>
        /// <param name="source">source tileset</param>
        internal void LoadContent(TilesetData source)
        {
            _tilesetTexture = _tilesetManager.LoadGraphic(source);
            _bgSourceRect = _tilesetTexture.Bounds;
            _bgSourceRect.Width = Math.Min(_bgSourceRect.Width, 8 * 32);
            _bgSourceRect.Height = Math.Min(_bgSourceRect.Height, _graphics.PreferredBackBufferHeight - 64);
            _cursorIndex = 0;
            _maxId = source.Passages != null ? source.Passages.Length : 0;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        private Single _yShift = 0;
        private Single _invertor = 1;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Branch by innerstate
            switch (_selectorState)
            {
                // Select tileset
                case SelectorState.SelectTileset:

                    // Shift tileset
                    _yShift += (Single)(gameTime.ElapsedGameTime.TotalSeconds * 64) * _invertor;

                    if (_yShift > 2048 - 8 * 32)
                        _invertor = -1;
                    else if (_yShift < 48)
                        _invertor = 1;

                    // Next tileset
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Down))
                    {
                        _tilesetIndex = (_tilesetIndex + 1) % (_content.Count + 1);
                        
                        if (_tilesetIndex < _content.Count)
                            LoadContent(_content[_tilesetIndex]);
                    }
                    // Previous tileset
                    else if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Up))
                    {
                        _tilesetIndex = _tilesetIndex - 1;
                        if (_tilesetIndex < 0)
                            _tilesetIndex = _content.Count;
                        else
                            LoadContent(_content[_tilesetIndex]);
                    }
                    // Select tileset
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Enter))
                    {
                        _selectorState = SelectorState.SelectTile;

                        if (_tilesetIndex == _content.Count)
                            AddNewTileSet();
                        else
                            _oldHashCode = _content[_tilesetIndex].GetHashCode();

                        _yShift = 0;
                    }
                    // Save and exit
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Escape))
                    {
                        foreach (TilesetData ts in _saveThese)
                        {
                            ts.Save();
                        }

                        this.Exit();
                        return;
                    }
                    break;

                // Select tile
                case SelectorState.SelectTile:

                    // Move Cursor
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Down))
                        _cursorIndex = Math.Min(_cursorIndex + 8, _maxId - 385); 
                    else if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Up))
                        _cursorIndex = Math.Max(_cursorIndex - 8, 0); 
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Right))
                        _cursorIndex = Math.Min(_cursorIndex + 1, _maxId - 385); 
                    else if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Left))
                        _cursorIndex = Math.Max(_cursorIndex - 1, 0); 

                    // Set Priority
                    if (_inputManager.Keyboard.IsKeyPressed(Keys.D0))
                        _content[_tilesetIndex].Priorities[_cursorIndex + 384] = 0;
                    else if (_inputManager.Keyboard.IsKeyPressed(Keys.D1))
                        _content[_tilesetIndex].Priorities[_cursorIndex + 384] = 1;
                    else if (_inputManager.Keyboard.IsKeyPressed(Keys.D2))
                        _content[_tilesetIndex].Priorities[_cursorIndex + 384] = 2;
                    else if (_inputManager.Keyboard.IsKeyPressed(Keys.D3))
                        _content[_tilesetIndex].Priorities[_cursorIndex + 384] = 3;
                    else if (_inputManager.Keyboard.IsKeyPressed(Keys.D4))
                        _content[_tilesetIndex].Priorities[_cursorIndex + 384] = 4;
                    else if (_inputManager.Keyboard.IsKeyPressed(Keys.D5))
                        _content[_tilesetIndex].Priorities[_cursorIndex + 384] = 5;

                    // Set Passages
                    Boolean toggler = _inputManager.Keyboard.IsKeyPressed(Keys.Q);
                    Byte passage = _content[_tilesetIndex].Passages[_cursorIndex + 384];
                    if (toggler || _inputManager.Keyboard.IsKeyPressed(Keys.S))
                    {
                        if ((passage & (1 << 0)) == 0)
                            passage = (Byte)(passage | (1 << 0));
                        else
                            passage = (Byte)((~(1 << 0)) & passage);
                    }

                    if (toggler || _inputManager.Keyboard.IsKeyPressed(Keys.A))
                    {
                        if ((passage & (1 << 1)) == 0)
                            passage = (Byte)(passage | (1 << 1));
                        else
                            passage = (Byte)((~(1 << 1)) & passage);
                    }

                    if (toggler || _inputManager.Keyboard.IsKeyPressed(Keys.D))
                    {
                        if ((passage & (1 << 2)) == 0)
                            passage = (Byte)(passage | (1 << 2));
                        else
                            passage = (Byte)((~(1 << 2)) & passage);
                    }

                    if (toggler || _inputManager.Keyboard.IsKeyPressed(Keys.W))
                    {
                        if ((passage & (1 << 3)) == 0)
                            passage = (Byte)(passage | (1 << 3));
                        else
                            passage = (Byte)((~(1 << 3)) & passage);
                    }

                    _content[_tilesetIndex].Passages[384 + _cursorIndex] = passage;
                

                    // Flags
                    TilesetFlags flags = (TilesetFlags)_content[_tilesetIndex].Flags[_cursorIndex + 384];
                    if (_inputManager.Keyboard.IsKeyPressed(Keys.B))
                    {
                        if (flags.HasFlag(TilesetFlags.Bush))
                            flags = (flags & ~(TilesetFlags.Bush));
                        else
                            flags |= TilesetFlags.Bush;
                    }

                    if (_inputManager.Keyboard.IsKeyPressed(Keys.C))
                    {
                        if (flags.HasFlag(TilesetFlags.Counter))
                            flags = flags & ~(TilesetFlags.Counter);
                        else
                            flags |= TilesetFlags.Counter;
                    }

                    if (_inputManager.Keyboard.IsKeyPressed(Keys.X))
                    {
                        if (flags.HasFlag(TilesetFlags.Blocking))
                            flags &= ~(TilesetFlags.Blocking);
                        else
                            flags |= TilesetFlags.Blocking;
                    }

                    if (_inputManager.Keyboard.IsKeyPressed(Keys.T))
                    {
                        if (flags.HasFlag(TilesetFlags.TransparantPassability))
                            flags &= ~(TilesetFlags.TransparantPassability);
                        else
                            flags |= TilesetFlags.TransparantPassability;
                    }

                    if (_inputManager.Keyboard.IsKeyPressed(Keys.H))
                    {
                        if (flags.HasFlag(TilesetFlags.Hazardous))
                            flags &= ~(TilesetFlags.Hazardous);
                        else
                            flags |= TilesetFlags.Hazardous;
                    }

                    if (_inputManager.Keyboard.IsKeyPressed(Keys.R))
                    {
                        if (flags.HasFlag(TilesetFlags.Healing))
                            flags &= ~(TilesetFlags.Healing);
                        else
                            flags |= TilesetFlags.Healing;
                    }

                    if (_inputManager.Keyboard.IsKeyPressed(Keys.P))
                    {
                        if (flags.HasFlag(TilesetFlags.Poisonous))
                            flags &= ~(TilesetFlags.Poisonous);
                        else
                            flags |= TilesetFlags.Poisonous;
                    }

                    _content[_tilesetIndex].Flags[_cursorIndex + 384] = (Byte)flags;

                   
                    // Return to tileset selection
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Back) || _inputManager.Keyboard.IsKeyTriggerd(Keys.Escape))
                    {
                        _selectorState = SelectorState.SelectTileset;
                        if (_oldHashCode != _content[_tilesetIndex].GetHashCode())
                        {
                            //_saveThese.Remove(_content[_tilesetIndex]);
                            _saveThese.Add(_content[_tilesetIndex]);
                        }
                    }

                    // Save
                    if (_inputManager.Keyboard.IsKeyTriggerd(Keys.Enter))
                    {
                        _selectorState = SelectorState.SelectAction;
                    }
                    break;

                // Saves Tileset
                case SelectorState.SelectAction:
                    _oldHashCode = _content[_tilesetIndex].GetHashCode();
                    _saveThese.Remove(_content[_tilesetIndex]);
                    _content[_tilesetIndex].Save();
                    _selectorState = SelectorState.SelectTile;
                    break;
            }

            base.Update(gameTime);
        }

        private Vector2 _bgPosition = new Vector2(4, 64);

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Draw background
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            if (_tilesetTexture != null)
            {
                switch(_selectorState)
                {
                    // Draw Selection List and preview of tileset
                    case SelectorState.SelectTileset:

                        if (_tilesetIndex != _content.Count)
                        {
                            _spriteBatch.Draw(_tilesetTexture,
                                new Vector2(128,
                                    128),
                                new Rectangle((Int32)(_cursorIndex / 512 * (8 * 32)),
                                    Math.Min(2048 - 8 * 32, Math.Max(0, (Int32)(_yShift + ((_cursorIndex % 512) / 8 * 32) - 48))),
                                    (8 * 32),
                                    (8 * 32)),
                                new Color(180, 180, 220, 255)); 
                        }

                        for (Int32 i = 0; i < _content.Count; i++)
                        {
                            Single xindent = 128 + 4;

                            // Write id and name
                            String listing = String.Format("{0} : {1}", _content[i].TilesetId, _content[i].Name);
                            _spriteBatch.DrawString(_font, listing,
                                new Vector2(xindent, 32 + i * 14), _tilesetIndex == i ? Color.DarkRed : Color.DarkBlue);

                            // Write abnormal status
                            String status = "";
                            if (_content[i].LoadResult.HasFlag(Data.Enum.DataLoadResult.NotValidated))
                            {
                                status = "Invalid Hash";

                                _spriteBatch.DrawString(_font, status,
                                    new Vector2(xindent + _font.MeasureString(listing).X + 4, 32 + i * 14), Color.Red);
                            }

                            // Loaded from title
                            if (_content[i].LoadResult.HasFlag(Data.Enum.DataLoadResult.FromTitle))
                            {
                                _spriteBatch.DrawString(_font, "from TitleContainer",
                                    new Vector2(xindent + _font.MeasureString(listing).X + 4 + _font.MeasureString(status).X + 4, 32 + i * 14), Color.Green);

                                status += "from TitleContainer";
                            }
                            else
                            {
                                _spriteBatch.DrawString(_font, "from IsolatedStorage",
                                    new Vector2(xindent + _font.MeasureString(listing).X + 4 + _font.MeasureString(status).X + 4, 32 + i * 14), Color.Gray);

                                status += "from IsolatedStorage";
                            }

                            // Need Saving
                            if (_saveThese.Contains(_content[i]))
                                _spriteBatch.DrawString(_font, "unsaved changes",
                                    new Vector2(xindent + _font.MeasureString(listing).X + 4 + _font.MeasureString(status).X + 8, 32 + i * 14), Color.Red);


                        }
   
                        _spriteBatch.DrawString(_font, String.Format("{0} : {1}", "X" , "<New Tileset>"),
                            new Vector2(128 + 4, 32 + _content.Count * 14), _tilesetIndex == _content.Count ? Color.DarkRed : Color.DarkBlue);

                        break;
                         
                    // Draw tileset at position and cursors
                    case SelectorState.SelectTile:

                        // Tile Id
                        String tileidString = String.Format("Tile {0}", _cursorIndex + 384);
                        _spriteBatch.DrawString(_font, tileidString,
                                new Vector2(128, 32), Color.DarkBlue);

                        // Tile Data
                        Byte passage = _content[_tilesetIndex].Passages[_cursorIndex + 384];
                        TilesetFlags flags = (TilesetFlags)_content[_tilesetIndex].Flags[_cursorIndex + 384];
                        String data = String.Format("Priority {0}\n{1} {2}{3}{4}{5}\nFlags {6}",
                            _content[_tilesetIndex].Priorities[_cursorIndex + 384],
                            passage != 15 ? "Passable" : "Blocked",
                            (passage & (1 << 1)) == (1 << 1) ? String.Empty : "L",
                            (passage & (1 << 2)) == (1 << 2) ? String.Empty : "R",
                            (passage & (1 << 3)) == (1 << 3) ? String.Empty : "U",
                            (passage & (1 << 0)) == (1 << 0) ? String.Empty : "D",
                            flags);
                        _spriteBatch.DrawString(_font, data,    
                            new Vector2(128 + _font.MeasureString(tileidString).X + 4, 32), Color.DarkGreen);

                        // Options
                        String help = "Passages all: Q | single: WASD\nPriorities set: 012345\nFlags toggle: BCTXPHR";
                        _spriteBatch.DrawString(_font, help,
                                new Vector2(128, 32 + _font.MeasureString(data).Y + 4), Color.DarkGray);

                        // Selector
                        _spriteBatch.Draw(_selectorBgTexture, SelectorPosition, Color.White);
                        
                        /// THIS SHOULD BE ABLE IN ONE BRANCH?
                        if (_cursorIndex % 512 >= 464)
                        {
                            _spriteBatch.Draw(_tilesetTexture,
                                new Vector2(128,
                                    128 + ((Int32)(((_cursorIndex % 512) / 8 * 32) - 48) < 0 ? -(Int32)(((_cursorIndex % 512) / 8 * 32) - 48) : 0)),
                                new Rectangle((Int32)(_cursorIndex / 512 * (8 * 32)),
                                    Math.Max(0, (Int32)(((_cursorIndex % 512) / 8 * 32) - 48)),
                                    (8 * 32),
                                    (Int32)(2048 - (((_cursorIndex % 512) / 8 * 32) - 48))),
                                new Color(180, 180, 220, 255));

                            _spriteBatch.Draw(_tilesetTexture,
                                new Vector2(128,
                                    128 + (2048 - (((_cursorIndex % 512) / 8 * 32) - 48))),
                                new Rectangle((Int32)((_cursorIndex / 512) + 1 * (8 * 32)),
                                    0,
                                    (8 * 32),
                                    (Int32)(8 * 32 - (2048 - (((_cursorIndex % 512) / 8 * 32) - 48)))),
                                new Color(180, 180, 220, 255));   
                        }
                        else
                        {
                            _spriteBatch.Draw(_tilesetTexture,
                                new Vector2(128,
                                    128 + ((Int32)(((_cursorIndex % 512) / 8 * 32) - 48) < 0 ? -(Int32)(((_cursorIndex % 512) / 8 * 32) - 48) : 0)),
                                new Rectangle((Int32)(_cursorIndex / 512 * (8 * 32)),
                                    Math.Max(0, (Int32)(((_cursorIndex % 512) / 8 * 32) - 48)),
                                    (8 * 32),
                                    _cursorIndex < 16 ? (6 + (_cursorIndex / 8)) * 32 + 16 : 8 * 32),
                                new Color(180, 180, 220, 255));
                        }

                        _spriteBatch.Draw(_tilesetTexture, new Vector2(128, 128 + 48), 
                            new Rectangle((Int32)(_cursorIndex / 512 * (8 * 32)), 
                                (Int32)((_cursorIndex % 512) / 8 * 32), 256, 32), 
                            Color.White);                        

                        _spriteBatch.Draw(_selectorOlTexture, SelectorPosition - Vector2.One * 2, Color.White);
                        
                        break;
                
                    // Don't draw!
                    case SelectorState.SelectAction:
                        break;
                }   
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Add a new tileset
        /// </summary>
        private void AddNewTileSet()
        {
            _selectorState = SelectorState.None;

            String fullFileName = "";
            String fileName = "";
            String name = "Tileset " + (_content[_content.Count-1].TilesetId + 1);
            String num = "0";
            List<String> autotileAssets = new List<String>();

            // Find Tileset Graphic
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Filter = "Tileset Graphics (*.png; *.jpg; *.gif)|*.png; *.jpg; *.gif"; 
                ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                ofd.Title = "Select the tileset graphic file";
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.AddExtension = true;
                ofd.AutoUpgradeEnabled = true;
                ofd.DefaultExt = "png";
                ofd.RestoreDirectory = true;
            
                if (!ofd.ShowDialog().HasFlag(System.Windows.Forms.DialogResult.OK))
                {
                    _selectorState = SelectorState.SelectTileset;
                    return;
                }

                fullFileName = ofd.FileName;
                fileName = ofd.SafeFileName;

                if (!InputBox("Tileset Creation", "Enter tileset name:", ref name).HasFlag(System.Windows.Forms.DialogResult.OK))
                {
                    _selectorState = SelectorState.SelectTileset;
                    return;
                }

                if (!InputBox("Tileset Creation", "Enter number of autotiles:", ref num).HasFlag(System.Windows.Forms.DialogResult.OK))
                {
                    _selectorState = SelectorState.SelectTileset;
                    return;
                }

                Int32 times = Int32.Parse(num);

                ofd.Filter = "Autotile Graphics (*.png; *.jpg; *.gif)|*.png; *.jpg; *.gif";

                while (times-- > 0)
                {
                    ofd.Title = "Select the autotile graphic file (" + times + " remaining)";
                    if (ofd.ShowDialog().HasFlag(System.Windows.Forms.DialogResult.OK))
                    {
                        autotileAssets.Add(ofd.FileName);
                    }
                }
            }

            // Autotiles
            foreach (String fn in autotileAssets)
            {
                using (Stream f = new FileStream(fn, FileMode.Open, FileAccess.Read))
                {

                    using (Texture2D autoTex = Texture2D.FromStream(GraphicsDevice, f))
                    {
                        autoTex.Name = fn.Substring(fn.LastIndexOf(@"\") + 1);
                        _textureManager.SaveStaticTexure(@"Graphics\Autotiles\" + autoTex.Name, autoTex);
                    }
                }
            }

            // Completion & Tilesettexture
            using (Stream fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read))
            {
                using (Texture2D tsTexture = Texture2D.FromStream(GraphicsDevice, fs))
                {
                    tsTexture.Name = fileName;
                    _textureManager.SaveStaticTexure(@"Graphics\Tilesets\" + tsTexture.Name, tsTexture);    
                    System.Threading.Thread.Sleep(100); // Release file
                    Texture2D loadedTexture = _textureManager.LoadStaticTexture(@"Graphics\Tilesets\" + tsTexture.Name, null);
                    System.Diagnostics.Debug.WriteLine(loadedTexture.Name);
                    _content.Add(TilesetData.CreateFromGraphicFile((UInt16)(_content[_content.Count - 1].TilesetId + 1), name, 
                        tsTexture.Name, loadedTexture.Width, loadedTexture.Height, autotileAssets));
                        
                }
                // Load it
                LoadContent(_content[_content.Count-1]);
                _oldHashCode = 0; // Mark to be saved
                _selectorState = SelectorState.SelectTile;
            }              
            
        }

        /// <summary>
        /// Creates an InputBox
        /// </summary>
        /// <param name="title"></param>
        /// <param name="promptText"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static System.Windows.Forms.DialogResult InputBox(string title, string promptText, ref string value)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Button buttonOk = new System.Windows.Forms.Button();
            System.Windows.Forms.Button buttonCancel = new System.Windows.Forms.Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | System.Windows.Forms.AnchorStyles.Right;
            buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            System.Windows.Forms.DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }

    /// <summary>
    /// SelectorState
    /// </summary>
    internal enum SelectorState
    {
        None = 0,
        SelectTileset = 1,
        SelectTile = 2,
        SelectAction = 3
    }
}
