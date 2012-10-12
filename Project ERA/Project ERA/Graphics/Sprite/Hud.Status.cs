using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;
using Microsoft.Xna.Framework;
using ERAUtils.Enum;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectERA.Graphics.Sprite
{
    internal partial class HeadsUpDisplay : DrawableComponent
    {
        internal class StatusWidget : Widget
        {
            private readonly String _assetBarBackground = @"Graphics\Interface\hud_status_bar_background";
            private readonly String _assetBarForegrounds = @"Graphics\Interface\hud_status_bar_forgrounds";
            private readonly String _assetJoint = @"Graphics\Interface\hud_status_joint";

            private readonly IconsetVx _assetIconHealth = IconsetVx.Death;
            private readonly IconsetVx _assetIconConcentration = IconsetVx.Concentration;

            private Data.Interactable _source;
            private Icon _healthIcon;
            private Icon _concentrationIcon;

            private Texture2D _textureBarBackground;
            private Texture2D _textureBarForegrounds;
            private Texture2D _textureJoint;

            private Double _displayHealth;
            private Double _displayConcentration;

            private Vector2 _displayPosition;

            /// <summary>
            /// Position Health Bar
            /// </summary>
            private Vector2 PositionHealth
            {
                get { return this.Position + Vector2.UnitY * 21 ;}
            }

            /// <summary>
            /// Position Health Icon
            /// </summary>
            private Vector2 PositionHealthIcon
            {
                get { return this.Position + Vector2.UnitY * 17; }
            }

            /// <summary>
            /// Position Concentration Bar
            /// </summary>
            private Vector2 PositionConcentration
            {
                get { return this.Position + Vector2.UnitY * 43; }
            }

            /// <summary>
            /// Position Concentration Icon
            /// </summary>
            private Vector2 PositionConcentrationIcon
            {
                get { return this.Position + Vector2.UnitX * 50 + Vector2.UnitY * 33; }
            }

            /// <summary>
            /// SourceRect for HealthBar
            /// </summary>
            private Rectangle SourceRectHealthBar
            {
                get { return new Rectangle(0, 0, (Int32)(75 * _displayHealth), 8);  }
            }

            /// <summary>
            /// SourceRect for ConcentrationBar
            /// </summary>
            private Rectangle SourceRectConcentrationBar
            {
                get { return new Rectangle(0, 8, (Int32)(75 * _displayConcentration), 8); }
            }

            /// <summary>
            /// Constructor for status widget
            /// </summary>
            /// <param name="game"></param>
            /// <param name="camera"></param>
            /// <param name="source"></param>
            internal StatusWidget(Game game, Camera3D camera, Data.Interactable source)
                : base(game, camera)
            {
                _source = source;

                _healthIcon = Icon.Generate(_assetIconHealth);
                _concentrationIcon = Icon.Generate(_assetIconConcentration);
            }

            /// <summary>
            /// Initializes widget
            /// </summary>
            internal override void Initialize()
            {
                _healthIcon.Initialize(this.TextureManager);
                _concentrationIcon.Initialize(this.TextureManager);

                _healthIcon.Position = this.PositionHealth;
                _concentrationIcon.Position = this.PositionConcentration;
                this.Position = Vector2.UnitX * 200 + Vector2.UnitY * 83;
                _displayPosition = this.Position;

                _displayHealth = 0;
                _displayConcentration = 0;

                this.Surface = Rectangle.Empty;
            }

            /// <summary>
            /// Loads widget content
            /// </summary>
            /// <param name="contentManager"></param>
            internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
            {
                _healthIcon.LoadContent(contentManager);
                _concentrationIcon.LoadContent(contentManager);

                _textureBarBackground = this.TextureManager.LoadStaticTexture(_assetBarBackground, contentManager);
                _textureBarForegrounds = this.TextureManager.LoadStaticTexture(_assetBarForegrounds, contentManager);
                _textureJoint = this.TextureManager.LoadStaticTexture(_assetJoint, contentManager);
            }

            /// <summary>
            /// Unloads widget content
            /// </summary>
            internal override void UnloadContent()
            {
                _healthIcon.UnloadContent();
                _concentrationIcon.UnloadContent();

                this.TextureManager.ReleaseStaticTexture(_assetBarBackground);
                this.TextureManager.ReleaseStaticTexture(_assetBarForegrounds);
                this.TextureManager.ReleaseStaticTexture(_assetJoint);
            }

            /// <summary>
            /// Draws widget
            /// </summary>
            /// <param name="gameTime"></param>
            /// <param name="drawTransparent"></param>
            internal override void Draw(GameTime gameTime, bool drawTransparent)
            {
                if (!drawTransparent)
                    return;

                this.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

                this.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Status", this.Position + Vector2.One, Color.Black);
                this.SpriteBatch.DrawString(this.ScreenManager.SpriteFonts["Default"], "Status", this.Position, Color.White);

                this.SpriteBatch.Draw(_textureBarBackground, this.PositionHealth, Color.White);
                this.SpriteBatch.Draw(_textureBarForegrounds, this.PositionHealth, this.SourceRectHealthBar, Color.White);
                this.SpriteBatch.Draw(_textureBarBackground, this.PositionConcentration, Color.White);
                this.SpriteBatch.Draw(_textureBarForegrounds, this.PositionConcentration, this.SourceRectConcentrationBar, Color.White);
                this.SpriteBatch.Draw(_healthIcon.Texture, this.PositionHealthIcon + Vector2.One, _healthIcon.SourceRect, Color.Black * ((_displayHealth < 0.25f && Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8) > 0) ? 0.25f * 0.8f : 0.75f * 0.8f));
                this.SpriteBatch.Draw(_healthIcon.Texture, this.PositionHealthIcon, _healthIcon.SourceRect, Color.White * ((_displayHealth < 0.25f && Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8) > 0) ? 0.25f : 0.75f));
                this.SpriteBatch.Draw(_concentrationIcon.Texture, this.PositionConcentrationIcon + Vector2.One, _concentrationIcon.SourceRect, Color.Black * ((_displayConcentration < 0.25f && Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8) > 0) ? 0.25f * 0.8f : 0.75f * 0.8f));
                this.SpriteBatch.Draw(_concentrationIcon.Texture, this.PositionConcentrationIcon, _concentrationIcon.SourceRect, Color.White * ((_displayConcentration < 0.25f && Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8) > 0) ? 0.25f : 0.75f));

                this.SpriteBatch.End();
            }

            /// <summary>
            /// Updates widget
            /// </summary>
            /// <param name="gameTime"></param>
            internal override void Update(GameTime gameTime)
            {
#if DEBUG
                Double someHealth = InputManager.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) ? 0.18f : _source.Battler.HealthPoints;
                Double someConcentration = InputManager.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) ? 0.45f : _source.Battler.ConcentrationPoints;
#else
                Double someHealth = _source.Battler.HealthPoints;
                Double someConcentration = _source.Battler.ConcentrationPoints;
#endif

                if (_displayHealth != someHealth)
                {
                    // Lerp between current and goto
                    Double newHealth = _displayHealth;
                    newHealth = MathHelper.Lerp((Single)newHealth, (Single)someHealth, (Single)(gameTime.ElapsedGameTime.TotalSeconds * 16));

                    // Set current to goto if close
                    if (Math.Abs(someHealth - newHealth) < 0.001f)
                        newHealth = someHealth;

                    // Changes
                    this.AddChange(() => _displayHealth = newHealth);
                }

                if (_displayConcentration != someConcentration)
                {
                    // Lerp between current and goto
                    Double newConcentration = _displayConcentration;
                    newConcentration = MathHelper.Lerp((Single)newConcentration, (Single)someConcentration, (Single)(gameTime.ElapsedGameTime.TotalSeconds * 16));

                    // Set current to goto if close
                    if (Math.Abs(someHealth - newConcentration) < 0.001f)
                        newConcentration = someConcentration;

                    // Changes
                    this.AddChange(() => _displayConcentration = newConcentration);
                }
            }

            /// <summary>
            /// Handles widget input
            /// </summary>
            internal override void HandleInput()
            {
                return;
            }
        }
    }
}
