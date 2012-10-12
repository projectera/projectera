using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Display;
using Microsoft.Xna.Framework;

namespace ProjectERA.Graphics.Sprite
{
    internal partial class HeadsUpDisplay : DrawableComponent
    {
        internal class EquipmentWidget : Widget
        {
            internal EquipmentWidget(Game game, Camera3D camera, Data.Interactable source)
                : base(game, camera)
            {

            }

            internal override void HandleInput()
            {
                throw new NotImplementedException();
            }

            internal override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
            {
                throw new NotImplementedException();
            }

            internal override void UnloadContent()
            {
                throw new NotImplementedException();
            }

            internal override void Initialize()
            {
                throw new NotImplementedException();
            }

            internal override void Draw(GameTime gameTime, bool drawTransparent)
            {
                throw new NotImplementedException();
            }

            internal override void Update(GameTime gameTime)
            {
                throw new NotImplementedException();
            }
        }
    }
}
