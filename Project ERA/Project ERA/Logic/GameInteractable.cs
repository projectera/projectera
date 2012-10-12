using System;
using Microsoft.Xna.Framework;
using ProjectERA.Data;
using ERAUtils.Enum;
using ProjectERA.Services.Data;
using ProjectERA.Data.Enum;

namespace ProjectERA.Logic
{
    internal static class GameInteractable
    {
        /// <summary>
        /// Move Interactable down
        /// </summary>
        /// <param name="source"></param>
        internal static void MoveDown(Interactable source)
        {
            source.AddChange(() => 
            { 
                source.MapY++; 
            });
        }

        /// <summary>
        /// Move Interactable Up
        /// </summary>
        /// <param name="source"></param>
        internal static void MoveUp(Interactable source)
        {
            source.AddChange(() => 
            { 
                source.MapY--; 
            });
        }

        /// <summary>
        /// Move Interactable Left
        /// </summary>
        /// <param name="source"></param>
        internal static void MoveLeft(Interactable source)
        {
            source.AddChange(() => 
            {
                source.MapX--;
            });
        }

        /// <summary>
        /// Move Interactable Right
        /// </summary>
        /// <param name="source"></param>
        internal static void MoveRight(Interactable source)
        {
            source.AddChange(() => 
            { 
                source.MapX++; 
            } );
        }

        /// <summary>
        /// Increase steps
        /// </summary>
        /// <param name="source"></param>
        internal static void IncreaseSteps(Graphics.Sprite.Interactable source)
        {
            source.StopCount = 0;
        }

        /// <summary>
        /// Determine if Passable
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="d">direction (0,2,4,6,8)
        /// 0 = Determines if all directions are impassable (for jumping)
        /// </param>
        /// <returns></returns>
        internal static Boolean IsPassable(Interactable source, MapData mapData, Int32 x, Int32 y, Direction d)
        {
            // Get New Coords
            Int32 newX = (d == Direction.East ? 1 : (d == Direction.West ? -1 : 0)); newX += x;
            Int32 newY = (d == Direction.South ? 1 : (d == Direction.North ? -1 : 0)); newY += y;

            // If Coords Are Outside the Map
            if (mapData.IsValid(newX, newY) == false)
                // Impassable
                return false;

            // If Trough is On
            if (source.StateFlags.HasFlag(InteractableStateFlags.Through))
                // Passable
                return true;

            // If unable to leave first move tile in designated direction
            if (mapData.IsPassable(x, y, (Byte)d, source) == false)
                // Impassable
                return false;

            // If unable to enter move tile in designated direction
            if (mapData.IsPassable(newX, newY, (Byte)(10 - (Byte)d)) == false)
                // Impassable
                return false;
            
            // Passable
            return true;
        }

        /// <summary>
        /// Update sprite
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        internal static void UpdateSprite(Interactable source, Graphics.Sprite.Interactable destination, GameTime gameTime)
        {
            String message; ProjectERA.Services.Display.ScreenManager screenManager = null; 
            while (source.RemoveMessage(out message))
            {
                screenManager = screenManager ?? (ProjectERA.Services.Display.ScreenManager)(destination.Game.Services.GetService(typeof(ProjectERA.Services.Display.ScreenManager)));
                if (screenManager != null)
                {
                    String messageCopy = String.Copy(message);
                    destination.AddChange(() => destination.Messages.Add(new Graphics.Sprite.Message(messageCopy, screenManager)));
                }
            }

            if (!source.HasAppearance || !source.HasMovement)
            {
                if (source.StateFlags.HasFlag(InteractableStateFlags.Moving) | source.StateFlags.HasFlag(InteractableStateFlags.Teleporting))
                {
                    Vector3 newPosition = Vector3.Zero;
                    newPosition.X = source.MapX;
                    newPosition.Y = source.MapY + .5F;

                    destination.AddChange(() => 
                        destination.Position = newPosition
                    );

                    source.AddChange(() => { source.StateFlags &= ~InteractableStateFlags.Moving; });
                
                }

                return;
            }

            if (source.HasAppearance && source.Appearance.MapDir != (Byte)destination.Direction)
                destination.AddChange(() => { destination.Direction = (Direction)source.Appearance.MapDir; });

            if (source.StateFlags.HasFlag(InteractableStateFlags.Moving))
            {
                UpdateSpriteMoving(source, destination, gameTime);

                GameInteractable.IncreaseSteps(destination);
            }
            else
            {
                if (source.StateFlags.HasFlag(InteractableStateFlags.Teleporting))
                {
                    Vector3 newPosition = Vector3.Zero;
                    newPosition.X = source.MapX;
                    newPosition.Y = source.MapY + .5F;

                    destination.AddChange(() =>
                        destination.Position = newPosition
                    );
                    destination.AddChange(() =>
                        destination.Direction = (Direction)source.Appearance.MapDir
                    );
                    

                    source.AddChange(() => { source.StateFlags &= ~InteractableStateFlags.Teleporting; });
                }
                else
                {
                    UpdateSpriteStopped(source, destination, gameTime);
                }
            }
        }

        /// <summary>
        /// Update sprite moving animation
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="gameTime"></param>
        private static void UpdateSpriteMoving(Interactable source, Graphics.Sprite.Interactable destination, GameTime gameTime)
        {
            if (!source.HasAppearance || !source.HasMovement)
                return;

            InteractableMovement movement = source.Movement;

            Single yshift = .5f; ///-0.5f;

            Double dx = (source.MapX - destination.Position.X);
            Double dy = (source.MapY - destination.Position.Y + yshift);

            if (dx == 0 && dy == 0)
            {
                source.AddChange(() => { source.StateFlags &= ~InteractableStateFlags.Moving; });
                return;
            }
            else if (source.HasMovement == false || dx > 5 || dy > 5)
            {
                Vector3 newPosition = Vector3.Zero;
                newPosition.X = source.MapX;
                newPosition.Y = source.MapY + yshift;
                destination.AddChange(() => { destination.Position = newPosition; });
            }
            else
            {

                Double totalMovement = Math.Sqrt(dx * dx + dy * dy);
                Double frameMovement = gameTime.ElapsedGameTime.TotalSeconds * movement.MoveSpeed * 1.5f;
                dx = frameMovement * (dx / totalMovement);
                dy = frameMovement * (dy / totalMovement);

                Vector3 newPosition = Vector3.Zero;
                newPosition.X = MathHelper.Clamp((Single)(destination.Position.X + dx), (dx > 0 ? destination.Position.X : source.MapX), (dx > 0 ? source.MapX : destination.Position.X));
                newPosition.Y = MathHelper.Clamp((Single)(destination.Position.Y + dy), (dy > 0 ? destination.Position.Y : source.MapY + yshift), (dy > 0 ? source.MapY + yshift : destination.Position.Y));
                
                // Snap to Grid
                /*dx = (source.MapX - newPosition.X);
                dy = (source.MapY - newPosition.Y + yshift);
                if (Math.Abs(dx) < 1 / 16 && Math.Abs(dy) < 1 / 16)
                {
                    source.AddChange(() => { source.IsMoving = false; });
                    newPosition.X = source.MapX;
                    newPosition.Y = source.MapY + yshift;
                }*/

                destination.AddChange(() => { destination.Position = newPosition; }); 

                Single newAnimationCount = (Single)(destination.AnimationCount + gameTime.ElapsedGameTime.TotalSeconds * 1.5f * ((movement.MoveFrequency > 0 ? movement.MoveFrequency : movement.StopFrequency) * 10f));
                destination.AddChange(() => { destination.AnimationCount = newAnimationCount; }); 
                destination.StopCount = 0;                    
            }
        }

        /// <summary>
        /// Update sprite stop animation
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="gameTime"></param>
        private static void UpdateSpriteStopped(Interactable source, Graphics.Sprite.Interactable destination, GameTime gameTime)
        {
            if (!source.HasMovement)
                return;

            InteractableMovement movement = source.Movement;

            Single newAnimationCount = (Single)(destination.AnimationCount + gameTime.ElapsedGameTime.TotalSeconds * 1.5f * ((movement.StopFrequency > 0 ? movement.StopFrequency : movement.MoveFrequency) * 10f));
            Single newStopCount = (Single)(destination.StopCount + gameTime.ElapsedGameTime.TotalSeconds * 30f);

            // If stop animation is ON or stop animation is OFF, but current pattern is different from original
            if (movement.StopFrequency > 0 || destination.AnimationFrame != source.Appearance.AnimationFrame)
                destination.AddChange(() => destination.AnimationCount = newAnimationCount); 

            // When waiting for event execution, or not locked
            // * If lock deals with event execution coming to a halt
            // When waiting for event execution or locked
            if (!source.StateFlags.HasFlag(InteractableStateFlags.EventRunning) && !source.StateFlags.HasFlag(InteractableStateFlags.Locked))
                destination.StopCount = newStopCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveInteractable"></param>
        /// <param name="moveToX"></param>
        /// <param name="moveToY"></param>
        internal static void MoveTo(Interactable moveInteractable, Int32 moveToX, Int32 moveToY, Byte moveToD)
        {
            moveInteractable.AddChange(() => { 
                moveInteractable.MapX = moveToX; 
                moveInteractable.MapY = moveToY;

                if (moveInteractable.HasAppearance)
                    moveInteractable.Appearance.MapDir = moveToD; 

                moveInteractable.StateFlags |= InteractableStateFlags.Moving; 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveInteractable"></param>
        /// <param name="moveToX"></param>
        /// <param name="moveToY"></param>
        internal static void TeleportTo(Interactable moveInteractable, Int32 moveToX, Int32 moveToY, Byte moveToD)
        {
            moveInteractable.AddChange(() =>
            {
                moveInteractable.MapX = moveToX;
                moveInteractable.MapY = moveToY;

                if (moveInteractable.HasAppearance)
                    moveInteractable.Appearance.MapDir = moveToD;

                moveInteractable.StateFlags |= InteractableStateFlags.Teleporting;
            });
        }
    }

}
