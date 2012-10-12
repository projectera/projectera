using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Me.Components
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ScreenshotComponent : GameComponent
    {
        #region Fields
        private Microsoft.Xna.Framework.Input.Keys _screenshotKey = Microsoft.Xna.Framework.Input.Keys.F13;
        //private String _screenshotPath = Application.StartupPath;
        private String _screenshotDirectory = "Screenshots";
        private String _screenshotPrefix = "screenshot";
        private Boolean _screenshotTaken;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public Microsoft.Xna.Framework.Input.Keys ScreenshotKey
        {
            get { return _screenshotKey; }
            set { _screenshotKey = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /*public String ScreenshotPath
        {
            get { return _screenshotPath; }
            set { _screenshotPath = value; }
        }*/
        /// <summary>
        /// 
        /// </summary>
        public String ScreenshotDirectory
        {
            get { return _screenshotDirectory; }
            set { _screenshotDirectory = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public String ScreenshotPrefix
        {
            get { return _screenshotPrefix; }
            set { _screenshotPrefix = value; }
        }
     
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        public ScreenshotComponent(Game game)
            : base(game)
        {
            UpdateOrder = Int32.MinValue+10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(ScreenshotKey) && !_screenshotTaken)
            {
                _screenshotTaken = true;

                GraphicsDevice device = this.Game.GraphicsDevice;

                byte[] screenData;

                screenData = new byte[device.PresentationParameters.BackBufferWidth * device.PresentationParameters.BackBufferHeight * 4];

                device.GetBackBufferData<byte>(screenData);

                Texture2D t2d = new Texture2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, false, device.PresentationParameters.BackBufferFormat);

                t2d.SetData<byte>(screenData);

                String fileName = "";

                if (!System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForDomain().DirectoryExists(ScreenshotDirectory)) //ScreenshotPath + "\\" +
                    System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForDomain().CreateDirectory(ScreenshotDirectory);

                String[] files = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForDomain().GetFileNames("*.png");

                if (files.Length > 0)
                {
                    //get last element and increment
                    fileName = files[files.Length - 1].Substring(0, files[files.Length - 1].Length - 5);
                    fileName += (String)(files.Length + 1).ToString() + ".png";

                }
                else
                {
                    fileName = ScreenshotDirectory + "\\" + ScreenshotPrefix + "1.png";
                }

               
                using (FileStream fs = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForDomain().CreateFile(fileName))
                {
                    t2d.SaveAsPng(fs, t2d.Width, t2d.Height);
                }
            }
            else if (_screenshotTaken && Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyUp(ScreenshotKey))
            {
                _screenshotTaken = false;
            }

            base.Update(gameTime);
        }
    }
}