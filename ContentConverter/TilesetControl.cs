using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WinFormsGraphicsDevice;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using ERAUtils.Enum;

namespace ContentConverter
{
    public partial class TilesetControl :
#if DEBUG
 FakeControl
#else
        GraphicsDeviceControl
#endif
    {
        private Texture2D _blankTexture;
        private Texture2D _tsFlags;
        private Texture2D _selectorBgTexture;
        private Texture2D _selectorOlTexture;
        private Texture2D _tsPriorities;

        /// <summary>
        /// 
        /// </summary>
        public TilesetControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public TilesetControl(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }


        /// <summary>
        /// Spritebatch
        /// </summary>
        protected SpriteBatch SpriteBatch
        {
            get;
            private set;
        }

        /// <summary>
        /// Displaying Texture
        /// </summary>
        public Texture2D TilesetTexture
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Texture2D[] AutotileTextures
        {
            get;
            set;
        }

        /// <summary>
        /// Data holder
        /// </summary>
        internal ERAServer.Data.Tileset Data
        {
            get;
            set;
        }


        /// <summary>
        /// Texture path
        /// </summary>
        public String TilesetTextureName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String[] AutotileTextureNames
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        protected Rectangle SelectorPosition
        {
            get
            {
                MouseState state = Mouse.GetState();
                System.Drawing.Point mouse = new System.Drawing.Point(state.X, state.Y);
                
                return new Rectangle((Int32)this.PointToClient(mouse).X / 32 * 32 , (Int32)this.PointToClient(mouse).Y / 32 * 32, 32, 32);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected Int32 SelectorIndex
        {
            get
            {
                MouseState state = Mouse.GetState();
                System.Drawing.Point mouse = new System.Drawing.Point(state.X, state.Y - 32);
                Int32 x = Math.Max(0, Math.Min(7, (Int32)this.PointToClient(mouse).X / 32));
                Int32 y = Math.Max(0, Math.Min(this.Height / 32, (Int32)this.PointToClient(mouse).Y / 32));

                return x + y * 8;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected Int32 AutoTileIndex
        {
            get
            {
                MouseState state = Mouse.GetState();
                System.Drawing.Point mouse = new System.Drawing.Point(state.X, state.Y);
                Int32 x = Math.Max(0, Math.Min(7, (Int32)this.PointToClient(mouse).X / 32));
                Int32 y = Math.Max(0, Math.Min(this.Height / 32, (Int32)this.PointToClient(mouse).Y / 32));

                if (y > 0 || x == 0)
                    return -1;

                return x + y * 8;
            }
        }

        private List<RenderTarget2D> _overlayTargets;
        private Action _currentAction;
        private Texture2D _arrowLeft, _arrowRight, _arrowUp, _arrowDown;

        /// <summary>
        /// 
        /// </summary>
        protected override void Initialize()
        {
            // Creates spritebatch
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);

            // Loads content
            LoadContent();
            LoadTexture(this.TilesetTextureName);

            if (this.AutotileTextureNames != null)
            for (Int32 i = 0; i < 7; i++)
                LoadTexture(this.AutotileTextureNames[i], i);
            DoAction(_currentAction);

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += delegate { Invalidate(); };
        }

        /// <summary>
        /// Draws a frame
        /// </summary>
        protected override void Draw()
        {
            // Clears the device
            GraphicsDevice.Clear(Color.LightGray);

            // Begins a spritebatch
            this.SpriteBatch.Begin();

            // Draws the selector
            this.SpriteBatch.Draw(_selectorBgTexture, SelectorPosition, Color.White);

            // Draws the tileset
            if (this.AutotileTextures != null)
                for (int i = 0; i < 7; i++)
                {
                    if (this.AutotileTextures[i] != null)
                        this.SpriteBatch.Draw(this.AutotileTextures[i], new Rectangle(i * 32 + 32, 0, 32, 32), new Rectangle(0, 0, 32, 32), Color.White);
                }

            if (this.TilesetTexture != null)
                for (int y = 0; y < this.TilesetTexture.Width / 256; y++)
                {
                    this.SpriteBatch.Draw(this.TilesetTexture,
                        new Rectangle(0, y * this.TilesetTexture.Height + 32, 256, this.TilesetTexture.Height),
                        new Rectangle(256 * y, 0, 256, this.TilesetTexture.Height),
                        Color.White);
                }

            // Draws the overlay
            if (this.Data != null)
            {
                //var all = (_overlayTargets.Count - 1 * 2048 + _overlayTargets[_overlayTargets.Count - 1].Height) / 32;
                for (int i = 0; i < _overlayTargets.Count; i++) {
                    this.SpriteBatch.Draw(_overlayTargets[i],
                        new Rectangle(0, i * 2048, 256, _overlayTargets[i].Height),
                        new Rectangle(256 , 0, 256, _overlayTargets[i].Height),
                        Color.White);
                }
            }

            // Draws the selector
            this.SpriteBatch.Draw(_selectorOlTexture, SelectorPosition, Color.White);
            
            this.SpriteBatch.End();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void HandleInput()
        {
            Int32 cursorIndex = SelectorIndex;
            Int32 autotileIndex = AutoTileIndex;

            if (autotileIndex != -1)
            {
                DoActionAutoTile(autotileIndex);
                return;
            }

            System.Drawing.Point mouse = this.PointToClient(new System.Drawing.Point((Int32)Mouse.GetState().X, (Int32)Mouse.GetState().Y));

            switch (_currentAction)
            {
                case Action.Passages:

                    // 32 x 32 - 16, 16
                    // . W .
                    // A . D
                    // . S .

                    // Set Passages
                    Byte passage = Data.Passages[cursorIndex + 384];
                    Int32 xMod = mouse.X % 32, yMod = mouse.Y % 32;

                    Boolean all = xMod > 16 - 4 && xMod < 16 + 4 && yMod > 16 - 4 && yMod < 16 + 4;
                    Boolean left = xMod < 16 - 4 | all;
                    Boolean right = xMod > 16 + 4 | all;
                    Boolean up = yMod < 16 - 4 | all;
                    Boolean down = yMod > 16 + 4 | all;

                    if (down)
                    {
                        if ((passage & (1 << 0)) == 0)
                            passage = (Byte)(passage | (1 << 0));
                        else
                            passage = (Byte)((~(1 << 0)) & passage);
                    }

                    if (left)
                    {
                        if ((passage & (1 << 1)) == 0)
                            passage = (Byte)(passage | (1 << 1));
                        else
                            passage = (Byte)((~(1 << 1)) & passage);
                    }

                    if (right)
                    {
                        if ((passage & (1 << 2)) == 0)
                            passage = (Byte)(passage | (1 << 2));
                        else
                            passage = (Byte)((~(1 << 2)) & passage);
                    }

                    if (up)
                    {
                        if ((passage & (1 << 3)) == 0)
                            passage = (Byte)(passage | (1 << 3));
                        else
                            passage = (Byte)((~(1 << 3)) & passage);
                    }

                    Data.Passages[384 + cursorIndex] = passage;
                    break;

                case Action.Priorities:
                    if (++Data.Priorities[384 + cursorIndex] > 5)
                        Data.Priorities[384 + cursorIndex] = 0;
                    break;

                case Action.Bush:
                case Action.Counter:
                case Action.Blocking:
                case Action.Inherit:
                case Action.Poison:
                case Action.Hazard:
                case Action.Healing:
                    Byte mask = 0;
                    switch (_currentAction)
                    {
                        case Action.Bush:
                            mask = (Byte)TilesetFlags.Bush;
                            break;
                        case Action.Counter:
                            mask = (Byte)TilesetFlags.Counter;
                            break;
                        case Action.Blocking:
                            mask = (Byte)TilesetFlags.Blocking;
                            break;
                        case Action.Inherit:
                            mask = (Byte)TilesetFlags.TransparantPassability;
                            break;
                        case Action.Poison:
                            mask = (Byte)TilesetFlags.Poisonous;
                            break;
                        case Action.Hazard:
                            mask = (Byte)TilesetFlags.Hazardous;
                            break;
                        case Action.Healing:
                            mask = (Byte)TilesetFlags.Healing;
                            break;
                    }

                    if ((Data.Flags[384 + cursorIndex] & mask) == mask)
                        Data.Flags[384 + cursorIndex] = (Byte)(Data.Flags[384 + cursorIndex] & ~mask);
                    else
                        Data.Flags[384 + cursorIndex] = (Byte)(Data.Flags[384 + cursorIndex] | mask);
                    break;

            }
            DoAction(_currentAction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        internal void DoAction(Action action)
        {
            _currentAction = action;

            if (this.GraphicsDevice == null)
                return;

            if (this.Data == null)
                return;

            //this.Data.Normalize(this.Height / 32);

            switch (action)
            {
                case Action.Passages:
                        
                    Int32 tindex = 0, xPassage = 0, yPassage = 0, zPassage = 0;
                    RenderTarget2D target = _overlayTargets[tindex];

                    this.GraphicsDevice.SetRenderTarget(target);
                    this.GraphicsDevice.Clear(Color.Transparent);

                    this.SpriteBatch.Begin();

                    foreach (var passage in this.Data.Passages)
                    {
                        if (zPassage++ < 384)
                        {
                            if (zPassage % 48 != 0)
                            {
                                continue;
                            }
                        }

                        if (yPassage > 64 * (tindex) + target.Height / 32)
                        {
                            if (tindex == _overlayTargets.Count - 1)
                                break;
                            target = _overlayTargets[++tindex];
                            this.GraphicsDevice.SetRenderTarget(target);
                            this.GraphicsDevice.Clear(Color.Transparent);
                        }

                        if ((passage & 0x02) != 0x02)
                            this.SpriteBatch.Draw(_arrowLeft, new Rectangle(xPassage * 32 + yPassage / 64 * 256, (yPassage % 64) * 32 + 8, 16, 16), Color.White);
                        if ((passage & 0x04) != 0x04)
                            this.SpriteBatch.Draw(_arrowRight, new Rectangle(xPassage * 32 + 16 + yPassage / 64 * 256, (yPassage % 64) * 32 + 8, 16, 16), Color.White);
                        if ((passage & 0x01) != 0x01)
                            this.SpriteBatch.Draw(_arrowDown, new Rectangle(xPassage * 32 + 8 + yPassage / 64 * 256, (yPassage % 64) * 32 + 16, 16, 16), Color.White);
                        if ((passage & 0x08) != 0x08)
                            this.SpriteBatch.Draw(_arrowUp, new Rectangle(xPassage * 32 + 8 + yPassage / 64 * 256, (yPassage % 64) * 32, 16, 16), Color.White);

                        //this.SpriteBatch.Draw(_tsPriorities, new Rectangle(xPassage * 32 + yPassage / 64 * 256, (yPassage % 64) * 32, 32, 32), new Rectangle(0, 0, 32, 32), Color.White);


                        if (xPassage++ >= 7)
                        {
                            xPassage = 0;
                            yPassage++;
                        }

                       

                        
                    }

                    this.SpriteBatch.End();

                    this.GraphicsDevice.SetRenderTarget(null);
                    break;

                case Action.Priorities:
                    Int32 priotindex = 0;
                    RenderTarget2D priotarget = _overlayTargets[priotindex];

                    this.GraphicsDevice.SetRenderTarget(priotarget);
                    this.GraphicsDevice.Clear(Color.Transparent);

                    this.SpriteBatch.Begin();

                    Int32 xPriority = 0, yPriority = 0, zPriority = 0;
                    foreach (var priority in this.Data.Priorities)
                    {
                        if (zPriority++ < 384)
                        {
                            if (zPriority % 48 != 0)
                            {
                                continue;
                            }
                        }

                        if (yPriority > 64 * (priotindex) + priotarget.Height / 32)
                        {
                            if (priotindex == _overlayTargets.Count - 1)
                                break;
                            priotarget = _overlayTargets[++priotindex];
                            this.GraphicsDevice.SetRenderTarget(priotarget);
                            this.GraphicsDevice.Clear(Color.Transparent);
                        }

                        this.SpriteBatch.Draw(_tsPriorities, new Rectangle(xPriority * 32 + yPriority / 64 * 256, (yPriority % 64) * 32, 32, 32), new Rectangle(priority * 32, 0, 32, 32), Color.White);

                        if (xPriority++ >= 7)
                        {
                            xPriority = 0;
                            yPriority++;
                        }

                    }

                    this.SpriteBatch.End();

                    this.GraphicsDevice.SetRenderTarget(null);
                    break;

                case Action.Bush:
                case Action.Counter:
                case Action.Blocking:
                case Action.Inherit:
                case Action.Healing:
                case Action.Hazard:
                case Action.Poison:
                    Int32 flagstindex = 0;
                    RenderTarget2D flagstarget = _overlayTargets[flagstindex];

                    this.GraphicsDevice.SetRenderTarget(flagstarget);
                    this.GraphicsDevice.Clear(Color.Transparent);

                    this.SpriteBatch.Begin();

                    Int32 xFlags = 0, yFlags = 0, zFlags = 0;
                    foreach (var flag in this.Data.Flags)
                    {
                        if (zFlags++ < 384)
                        {
                            if (zFlags % 48 != 0)
                            {
                                continue;
                            }
                        }

                        Int32 flagIndex = 0;
                        Int32 mask = 0;
                        switch(_currentAction)
                        {
                            case Action.Bush:
                                flagIndex = 1;
                                mask = (Int32)TilesetFlags.Bush;
                                break;
                            case Action.Counter:
                                flagIndex = 2;
                                mask = (Int32)TilesetFlags.Counter;
                                break;
                            case Action.Blocking:
                                flagIndex = 3;
                                mask= (Int32)TilesetFlags.Blocking;
                                break;
                            case Action.Inherit:
                                flagIndex = 4;
                                mask = (Int32)TilesetFlags.TransparantPassability;
                                break;
                            case Action.Poison:
                                flagIndex = 5;
                                mask = (Byte)TilesetFlags.Poisonous;
                                break;
                            case Action.Hazard:
                                flagIndex = 6;
                                mask = (Byte)TilesetFlags.Hazardous;
                                break;
                            case Action.Healing:
                                flagIndex = 7;
                                mask = (Byte)TilesetFlags.Healing;
                                break;
                        }

                        if ((flag & mask) != mask)
                            flagIndex = 0;

                        if (yFlags > 64 * (flagstindex) + flagstarget.Height / 32)
                        {
                            if (flagstindex == _overlayTargets.Count - 1)
                                break;
                            target = _overlayTargets[++flagstindex];
                            this.GraphicsDevice.SetRenderTarget(flagstarget);
                            this.GraphicsDevice.Clear(Color.Transparent);
                        }


                        this.SpriteBatch.Draw(_tsFlags, new Rectangle(xFlags * 32 + yFlags / 64 * 256, (yFlags % 64) * 32, 32, 32), new Rectangle(flagIndex * 32, 0, 32, 32), Color.White);

                        if (xFlags++ >= 7)
                        {
                            xFlags = 0;
                            yFlags++;
                        }
                    }

                    this.SpriteBatch.End();

                    this.GraphicsDevice.SetRenderTarget(null);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        internal void DoActionAutoTile(Int32 index)
        {
            switch (_currentAction)
            {
                case Action.Passages:
                    for (int i = index * 48; i < 48 + index * 48; i++)
                    {
                        Byte passage = Data.Passages[i];
                        if ((passage & (1 << 0)) == 0)
                            passage = (Byte)(passage | (1 << 0));
                        else
                            passage = (Byte)((~(1 << 0)) & passage);
                        if ((passage & (1 << 1)) == 0)
                            passage = (Byte)(passage | (1 << 1));
                        else
                            passage = (Byte)((~(1 << 1)) & passage);
                        if ((passage & (1 << 2)) == 0)
                            passage = (Byte)(passage | (1 << 2));
                        else
                            passage = (Byte)((~(1 << 2)) & passage);
                        if ((passage & (1 << 3)) == 0)
                            passage = (Byte)(passage | (1 << 3));
                        else
                            passage = (Byte)((~(1 << 3)) & passage);

                        Data.Passages[i] = passage;
                    }

                    
                    break;
                case Action.Priorities:
                    for (int i = index * 48; i < 48 + index * 48; i++)
                        if (++Data.Priorities[i] > 5)
                            Data.Priorities[i] = 0;
                    break;

                case Action.Bush:
                case Action.Counter:
                case Action.Blocking:
                case Action.Inherit:
                case Action.Poison:
                case Action.Hazard:
                case Action.Healing:
                    Byte mask = 0;
                    switch(_currentAction)
                    {
                        case Action.Bush:
                            mask = (Byte)TilesetFlags.Bush;
                            break;
                        case Action.Counter:
                            mask = (Byte)TilesetFlags.Counter;
                            break;
                        case Action.Blocking:
                            mask = (Byte)TilesetFlags.Blocking;
                            break;
                        case Action.Inherit:
                            mask = (Byte)TilesetFlags.TransparantPassability;
                            break;
                        case Action.Poison:
                            mask = (Byte)TilesetFlags.Poisonous;
                            break;
                        case Action.Hazard:
                            mask = (Byte)TilesetFlags.Hazardous;
                            break;
                        case Action.Healing:
                            mask = (Byte)TilesetFlags.Healing;
                            break;
                    }

                    for (int i = index * 48; i < 48 + index * 48; i++)
                    {
                        if ((Data.Flags[i] & mask) == mask)
                            Data.Flags[i] = (Byte)(Data.Flags[i] & ~mask);
                        else
                            Data.Flags[i] = (Byte)(Data.Flags[i] | mask);
                    }
                                           
                    break;

            }    

            DoAction(_currentAction);
        }

        /// <summary>
        /// Loads content
        /// </summary>
        /// <param name="location"></param>
        internal void LoadTexture(String location)
        {
            this.TilesetTextureName = location;

            if (this.GraphicsDevice == null)
                return;

            UnloadTexture(true, false);

            if (location == null)
            {
                this.TilesetTexture = null;
                return;
            }

            using (FileStream stream = File.OpenRead(TilesetTextureName))
                this.TilesetTexture = Texture2D.FromStream(this.GraphicsDevice, stream);

            this.Width = 256;
            this.Height = Math.Min(4096, this.TilesetTexture.Height + (this.TilesetTexture.Width / 256 * this.TilesetTexture.Height) + 32);

            _overlayTargets = new List<RenderTarget2D>();
            for (Int32 height = this.Height; height > 0; )
            {

                var current = Math.Min(height, 2048);
                var target = new RenderTarget2D(this.GraphicsDevice, this.TilesetTexture.Width, current);
                this.GraphicsDevice.SetRenderTarget(target);
                this.GraphicsDevice.Clear(Color.Transparent);
                this.GraphicsDevice.SetRenderTarget(null);
                height -= current;
                _overlayTargets.Add(target);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="i"></param>
        internal void LoadTexture(String location, Int32 i)
        {
            this.AutotileTextureNames = this.AutotileTextureNames ?? new String[7];
            this.AutotileTextures = this.AutotileTextures ?? new Texture2D[7];
            this.AutotileTextureNames[i] = location;

            if (this.GraphicsDevice == null)
                return;

            if (this.AutotileTextures[i] != null && this.AutotileTextures[i].IsDisposed == false)
                this.AutotileTextures[i].Dispose();

            if (location == null)
            {
                this.AutotileTextures[i] = null;
                return;
            }

            using (FileStream stream = File.OpenRead(AutotileTextureNames[i]))
                this.AutotileTextures[i] = Texture2D.FromStream(this.GraphicsDevice, stream);
        }


        public Int32 TilesetTileCount
        {
            get
            {
                return this.TilesetTexture == null ? 0 : (this.TilesetTexture.Width / 32) * (this.TilesetTexture.Height / 32); // Math.Min(this.Height / 32 - 1, 
                // = Math.Min(4096, this.TilesetTexture.Height + (this.TilesetTexture.Width / 256 * this.TilesetTexture.Height) + 32);
            }
            set
            {
                DoAction(_currentAction);
            }
        }
        
        /// <summary>
        /// Loads base content
        /// </summary>
        internal void LoadContent()
        {
            // Loads Arrows in Memory and writes them to a Texture2D 
            using (MemoryStream stream = new MemoryStream())
            {
                ContentConverter.Properties.Resources.arrow_left.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                _arrowLeft = Texture2D.FromStream(this.GraphicsDevice, stream);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                ContentConverter.Properties.Resources.arrow_right.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                _arrowRight = Texture2D.FromStream(this.GraphicsDevice, stream);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                ContentConverter.Properties.Resources.arrow_down.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                _arrowDown = Texture2D.FromStream(this.GraphicsDevice, stream);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                ContentConverter.Properties.Resources.arrow_up.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                _arrowUp = Texture2D.FromStream(this.GraphicsDevice, stream);
            }

            // Loads graphics
            String basePath = Assembly.GetExecutingAssembly().Location;

            using (FileStream stream = File.OpenRead(Path.Combine(basePath, "../Graphics/tsSelectorBG.png")))
                _selectorBgTexture = Texture2D.FromStream(this.GraphicsDevice, stream); //"tsSelectorBG");
            using (FileStream stream = File.OpenRead(Path.Combine(basePath, "../Graphics/tsSelectorOverlay.png")))
                _selectorOlTexture = Texture2D.FromStream(this.GraphicsDevice, stream); //"tsSelectorOverlay");
            using (FileStream stream = File.OpenRead(Path.Combine(basePath, "../Graphics/blank.png")))
                _blankTexture = Texture2D.FromStream(this.GraphicsDevice, stream);
            using (FileStream stream = File.OpenRead(Path.Combine(basePath, "../Graphics/tsPriorities.png")))
                _tsPriorities = Texture2D.FromStream(this.GraphicsDevice, stream);
            using (FileStream stream = File.OpenRead(Path.Combine(basePath, "../Graphics/tsFlags.png")))
                _tsFlags = Texture2D.FromStream(this.GraphicsDevice, stream);
        }


        /// <summary>
        /// Unloads tileset texture
        /// </summary>
        internal void UnloadTexture(Boolean ts = true, Boolean at = true)
        {
            if (ts && this.TilesetTexture != null && this.TilesetTexture.IsDisposed == false)
                this.TilesetTexture.Dispose();

            if (at && this.AutotileTextures != null)
                foreach (Texture2D autotileTex in this.AutotileTextures)
                    if (autotileTex != null && autotileTex.IsDisposed == false)
                        autotileTex.Dispose();

            this.Width = 256;
            this.Height = 362;
        }
        
        /// <summary>
        /// 
        /// </summary>
        internal void UnloadContent()
        {
            _arrowDown.Dispose();
            _arrowLeft.Dispose();
            _arrowRight.Dispose();
            _arrowUp.Dispose();
            _tsPriorities.Dispose();
            _selectorBgTexture.Dispose();
            _selectorOlTexture.Dispose();
            _blankTexture.Dispose();
            _tsFlags.Dispose();
        }
        

        /// <summary>
        /// 
        /// </summary>
        public enum Action
        {
            None = 0,
            Passages,
            Priorities,
            Bush,
            Terrain,
            Counter,
            Blocking,
            Inherit,
            Poison,
            Hazard,
            Healing,
        }
    }
}
