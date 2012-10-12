using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectERA.Data;
using ERAUtils.Enum;
using ProjectERA.Data.Enum;
using ProjectERA.Data.Update;
using ProjectERA.Services.Data;

namespace ProjectERA.Logic
{
    /// <summary>
    /// 
    /// </summary>
    internal static class GameAvatar
    {
        /// <summary>
        /// Source facing
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="direction">Direction facing</param>
        internal static void Turn(Interactable source, Direction direction)
        {
            source.AddChange(() => { source.Appearance.MapDir = (Byte)direction; });
        }

        /// <summary>
        /// Tries to move source from position in direction
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
        /// <param name="direction">Direction</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveFrom(Interactable source, MapData mapData, Int32 x, Int32 y, Direction direction)
        {
            if (!mapData.IsValid(x, y))
                return Direction.None;

            Int32 restoreX = source.MapX;
            Int32 restoreY = source.MapY;

            // not Thread safe
            source.MapX = x;
            source.MapY = y;

            Direction result = TryMove(source, mapData, direction);

            source.MapX = restoreX;
            source.MapY = restoreY;

            return result;
        }

        /// <summary>
        /// Tries to move source in direction
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <param name="direction">Direction</param>
        /// <returns>Executed</returns>
        internal static Direction TryMove(Interactable source, MapData mapData, Direction direction)
        {
            switch(direction)
            {
                case Direction.East:
                    return TryMoveRight(source, mapData);
                case Direction.West:
                    return TryMoveLeft(source, mapData);
                case Direction.North:
                    return TryMoveUp(source, mapData);
                case Direction.South:
                    return TryMoveDown(source, mapData);
                case Direction.NorthEast:
                    return TryMoveUpperRight(source, mapData);
                case Direction.NorthWest:
                    return TryMoveUpperLeft(source, mapData);
                case Direction.SouthEast:
                    return TryMoveLowerRight(source, mapData);
                case Direction.SouthWest:
                    return TryMoveLowerLeft(source, mapData);
            }

            return Direction.None;
        }

        /// <summary>
        /// Tries to move right
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveRight(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.East);

            if (GameAvatar.IsPassable(source, mapData, Direction.East))
            {
                GameInteractable.MoveRight(source);
                return Direction.East;
            }

            return Direction.None;
        }

        /// <summary>
        /// Tries to move left
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveLeft(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.West);

            if (GameAvatar.IsPassable(source, mapData, Direction.West))
            {
                GameInteractable.MoveLeft(source);
                return Direction.West;
            }

            return Direction.None;
        }

        /// <summary>
        /// Tries to move up
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveUp(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.North);

            if (GameAvatar.IsPassable(source, mapData, Direction.North))
            {
                GameInteractable.MoveUp(source);
                return Direction.North;
            }

            return Direction.None;
        }

        /// <summary>
        /// Tries to move down
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveDown(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.South);

            if (GameAvatar.IsPassable(source, mapData, Direction.South))
            {
                GameInteractable.MoveDown(source);
                return Direction.South;
            }

            return Direction.None;
        }

        /// <summary>
        /// Tries to move down-left
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveLowerLeft(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.South);

            if ((GameAvatar.IsPassable(source, mapData, Direction.South) && GameAvatar.IsPassable(source, mapData, source.MapX, source.MapY + 1, Direction.West)) ||
                (GameAvatar.IsPassable(source, mapData, Direction.West) && GameAvatar.IsPassable(source, mapData, source.MapX - 1, source.MapY, Direction.South)))
            {
                GameInteractable.MoveDown(source);
                GameInteractable.MoveLeft(source);
                return Direction.SouthWest;
            }
            else
            {
                Direction first = TryMoveDown(source, mapData);
                if (first == Direction.None)
                    return TryMoveLeft(source, mapData);
                return first;
            }
        }

        /// <summary>
        /// Tries to move up-left
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveUpperLeft(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.North);

            if ((GameAvatar.IsPassable(source, mapData, Direction.North) && GameAvatar.IsPassable(source, mapData, source.MapX, source.MapY - 1, Direction.West)) ||
               (GameAvatar.IsPassable(source, mapData, Direction.West) && GameAvatar.IsPassable(source, mapData, source.MapX - 1, source.MapY, Direction.North)))
            {
                GameInteractable.MoveLeft(source);
                GameInteractable.MoveUp(source);
                return Direction.NorthWest;
            }
            else
            {
                Direction first = TryMoveUp(source, mapData);
                if (first == Direction.None)
                    return TryMoveLeft(source, mapData);
                return first;
            }
        }
        
        /// <summary>
        /// Tries to move down-right
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <returns>Succesfull</returns>
        internal static Direction TryMoveLowerRight(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.South);


            if ((GameAvatar.IsPassable(source, mapData, Direction.South) && GameAvatar.IsPassable(source, mapData, source.MapX, source.MapY + 1, Direction.East)) ||
               (GameAvatar.IsPassable(source, mapData, Direction.East) && GameAvatar.IsPassable(source, mapData, source.MapX + 1, source.MapY, Direction.South)))
            {
                GameInteractable.MoveRight(source);
                GameInteractable.MoveDown(source);
                return Direction.SouthEast;
            }
            else
            {
                Direction first = TryMoveDown(source, mapData);
                if (first == Direction.None)
                    return TryMoveRight(source, mapData);
                return first;
            }
        }

        /// <summary>
        /// Tries to move up-right
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">Source</param>
        /// <returns>Executed</returns>
        internal static Direction TryMoveUpperRight(Interactable source, MapData mapData)
        {
            GameAvatar.Turn(source, Direction.North);

            if ((GameAvatar.IsPassable(source, mapData, Direction.North) && GameAvatar.IsPassable(source, mapData, source.MapX, source.MapY - 1, Direction.East)) ||
               (GameAvatar.IsPassable(source, mapData, Direction.East) && GameAvatar.IsPassable(source, mapData, source.MapX + 1, source.MapY, Direction.North)))
            {
                GameInteractable.MoveUp(source);
                GameInteractable.MoveRight(source);
                return Direction.NorthEast;
            }
            else
            {
                Direction first = TryMoveUp(source, mapData);
                if (first == Direction.None)
                    return TryMoveRight(source, mapData);
                return first;
            }
        }

        /// <summary>
        /// Returns passability flag for avatar and direction
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <param name="direction">Direction</param>
        /// <returns>Passable</returns>
        internal static Boolean IsPassable(Interactable source, MapData mapData, Direction direction)
        {
            return GameInteractable.IsPassable(source, mapData, source.MapX, source.MapY, direction);
        }

        /// <summary>
        /// Returns passabillity flag for avatar at position and direction
        /// </summary>
        /// <param name="source">Avatar</param>
        /// <param name="mapData">MapData</param>
        /// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
        /// <param name="direction">Direction</param>
        /// <returns>Passable</returns>
        internal static Boolean IsPassable(Interactable source, MapData mapData, Int32 x, Int32 y, Direction direction)
        {
            return GameInteractable.IsPassable(source, mapData, x, y, direction);
        }  
    }
}
