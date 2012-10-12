using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using ERAUtils.Logger;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// The contentdatabase has functions to store collected data.
    /// </summary>
    internal static partial class ContentDatabase
    {
        /// <summary>
        /// Saves all data
        /// </summary>
        internal static void SaveAll()
        {
#if !NOMULTITHREAD
            // Wait for database to be released
            SleepUntilReleased();

            Interlocked.Increment(ref _asyncOperations);

            // Save all
            Task.Factory.StartNew(() =>
            {
#endif
                ContentDatabase.SaveColors();
                ContentDatabase.SaveWeapons();
                ContentDatabase.SaveArmors();
                ContentDatabase.SaveAccessories();
                ContentDatabase.SaveBattlerModifiers();
                ContentDatabase.SaveBattlerClasses();
                ContentDatabase.SaveBattlerRaces();

#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            });
#endif
        }

        /// <summary>
        /// Saves colors to file
        /// </summary>
        internal static void SaveColors()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.Color))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:Colors) saved " + ContentDatabase.GetSerializableColors().Serialize().ToString() + " weapons to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Saves weapons to file
        /// </summary>
        internal static void SaveWeapons()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.Weapon))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:Weapons) saved " + ContentDatabase.GetSerializableWeapons().Serialize().ToString() + " weapons to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Saves armors to file
        /// </summary>
        internal static void SaveArmors()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.Armor))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:Armors) saved " + ContentDatabase.GetSerializableArmors().Serialize().ToString() + " armors to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Saves accessories to file
        /// </summary>
        internal static void SaveAccessories()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.Accessory))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:Accessoiries) saved " + ContentDatabase.GetSerializableAccessoiries().Serialize().ToString() + " accessoiries to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Saves battler modifiers to file
        /// </summary>
        internal static void SaveBattlerModifiers()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.BattlerBuff) || IsWriting(ContentDatabaseType.BattlerState))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:BattlerModifiers) saved " + ContentDatabase.GetSerializableBattlerModifiers().Serialize().ToString() + " battlermodifiers to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Saves battler classes to file
        /// </summary>
        internal static void SaveBattlerClasses()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.BattlerClass))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:BattlerClass) saved " + ContentDatabase.GetSerializableBattlerClasses().Serialize().ToString() + " battlerclasses to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Saves battler classes to file
        /// </summary>
        internal static void SaveBattlerRaces()
        {
#if !NOMULTITHREAD
            Task.Factory.StartNew(() =>
            {
                while (IsWriting(ContentDatabaseType.BattlerRace))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:BattlerRace) saved " + ContentDatabase.GetSerializableBattlerRaces().Serialize().ToString() + " battlerraces to [::MACHINE::]");
#if !NOMULTITHREAD
            }, TaskCreationOptions.AttachedToParent);
#endif
        }
    }
}
