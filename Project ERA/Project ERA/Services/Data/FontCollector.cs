using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// This Component loads and keeps track of all the fonts. It will
    /// allow us to only once load a spritefont instead of many times,
    /// which will be handy, since every screen has it's own instanced
    /// contentManager, so we wouldn't be able to crossreference fonts.
    /// </summary>
    internal class FontCollector : DrawableGameComponent
    {
        #region Private fields

        private ContentManager _contentManager;
        private Dictionary<String, SpriteFont> _spritefontDictionairy;

        #endregion

        /// <summary>
        /// Constructor for the FontCollector
        /// </summary>
        /// <param name="game"></param>
        internal FontCollector(Game game)
            : base(game)
        {
            _contentManager = game.Content;

            // Create the Dictionairy
            _spritefontDictionairy = new Dictionary<String, SpriteFont>();

            // Add this as service to the services container
            game.Services.AddService(typeof(FontCollector), this);
        }

        /// <summary>
        /// Load a Font
        /// </summary>
        internal void LoadFont(String key, String path)
        {
            if (_spritefontDictionairy.ContainsKey(key))
                return;

            // Load Default font, and assign them
            _spritefontDictionairy[key] = _contentManager.Load<SpriteFont>(path);
        }

        /// <summary>
        /// Load All Content
        /// </summary>
        protected override void LoadContent()
        {
            // Load DefaultSprite
            _spritefontDictionairy["Default"] = _contentManager.Load<SpriteFont>(@"Common\defaultFont");
            _spritefontDictionairy["Names"] = _contentManager.Load<SpriteFont>(@"Common\defaultSmallHeading");

            // Base Loading
            base.LoadContent();
        }

        /// <summary>
        /// Get a Spritefont
        /// </summary>
        /// <param name="fontName">key</param>
        /// <returns>Font or Default Font</returns>
        internal SpriteFont this[String fontName]
        {
            get
            {
                SpriteFont font;
                if (_spritefontDictionairy.TryGetValue(fontName, out font))
                    return font;
                return _spritefontDictionairy["Default"];
            }
        }
    }
}
