using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using MongoDB.Bson;
using ERAUtils.Enum;

namespace ERAServer.Data
{
    internal partial class Interactable : IResetable
    {
        #region Avatar
        /// <summary>
        /// Generates an avatar (interactable controlled by player)
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        internal static Interactable GenerateAvatar(Player destination)
        {
            Interactable result = new Interactable();
            result.Id = ObjectId.GenerateNewId();
            result.Name = Generators.LanguageConfluxer.Run("Generators/Celtic-m.txt", 1)[0] + Generators.LanguageConfluxer.Run("Generators/Celtic-m.txt", 1)[0]; //"Avatar " + DateTime.Now.ToShortTimeString() + " number " + result.Id.Increment;

            // This is an avatar
            result.StateFlags = InteractableStateFlags.Visible;

            // HACK: testmap
            result.MapId = Map.GetCollection().FindAll().Select(a => a.Id).OrderBy(a => Lidgren.Network.NetRandom.Instance.Next()).First(); 
            //new ObjectId(new Byte[] { 78, 26, 78, 76, 106, 239, 98, 26, 32, 188, 2, 83 }); 

            Generate(result);

            result.AddComponent(AI.InteractableAppearance.Generate(result));
            result.AddComponent(AI.InteractableBattler.Generate(result, Blueprint.BattlerClass.GetBlocking(1).Id, Blueprint.BattlerRace.GetBlocking(1).Id));
            result.AddComponent(AI.InteractableMovement.Generate(result));

            // Add equipment
            AI.InteractableBattler battler = result.GetComponent(typeof(AI.InteractableBattler)) as AI.InteractableBattler;
            battler.Equip(InteractableEquipment.Generate(1, result.Id)); //Data.Blueprint.Equipment.SearchBlocking("Stick").First().ContentId, result.Id));
            battler.Equip(InteractableEquipment.Generate(2, result.Id)); //Data.Blueprint.Equipment.SearchBlocking("Sloppy Shirt").First().ContentId, result.Id));
            battler.Equip(InteractableEquipment.Generate(3, result.Id)); //Data.Blueprint.Equipment.SearchBlocking("Sloppy Trousers").First().ContentId, result.Id));

            // HACK: testcoords
            result.MapX = 20;
            result.MapY = 30;

            // Save the interactable
            destination.AvatarIds.Add(result.Id);

            return result;
        }
        #endregion

        #region Monster
        /// <summary>
        /// Generates a monster
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        internal static Interactable GenerateMonster()
        {
            Interactable result = new Interactable();
            result.Id = ObjectId.GenerateNewId();
            result.Name = "Monster " + DateTime.Now.ToShortTimeString();

            // This is a monster
            //result.Type = InteractableType.DefaultMonster;
            //result.InputFlags = InteractableInputFlags.DefaultMonster;
            result.StateFlags = InteractableStateFlags.Visible;

            Generate(result);

            result.AddComponent(AI.InteractableAppearance.Generate(result, "Sahagin-01"));
            result.AddComponent(AI.InteractableBattler.Generate(result, 0, 0));
            result.AddComponent(AI.InteractableMovement.Generate(result));

            return result;
        }

        /// <summary>
        /// Generates a monster with race
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Interactable GenerateMonster(Int32 type)//GameMap destination)
        {
            Interactable result = GenerateMonster();

            AI.InteractableBattler component = (AI.InteractableBattler)result.GetComponent(typeof(AI.InteractableBattler));
            component.RaceId = type;

            return result;
        }
        #endregion
    }
}
