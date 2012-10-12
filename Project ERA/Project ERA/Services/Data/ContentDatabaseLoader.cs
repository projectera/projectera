using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using ProjectERA.Services.Data.Serialization;
using ERAUtils.Logger;

namespace ProjectERA.Services.Data
{
    internal static partial class ContentDatabase
    {
#if !NOMULTITHREAD

        /// <summary>
        /// Blocks until contentDatabase is no longer busy (warning: Infinite Sleep)
        /// </summary>
        public static void SleepUntilReleased()
        {
            while (ContentDatabase.IsBusy)
                Thread.Sleep(1);                 //Thread.SpinWait(
        }

        /// <summary>
        /// Spins and returns yield on spin status
        /// </summary>
        /// <returns></returns>
        public static Boolean SpinAndWillYield()
        {
            if (!_spinWait.NextSpinWillYield)
            {
                _spinWait.SpinOnce();
                return false;
            }
            return true;
        }

#endif
      
        internal const Int32 __LOADALLPARTS = 6; // 4 * load + 1 popu + 1 rest

        public static event IntegerEventHandler FinishedLoadingPartial = delegate { };
        public static event EventHandler FinishedLoadingAll = delegate { };
        public static Task LoadTask;
        private static Object _lockObject = new Object();


#if !NOMULTITHREAD
        /// <summary>
        /// Loads all data from file
        /// </summary>
        internal static void LoadAll()
        {
             
            Task task = new Task(() =>
            {
                Interlocked.Increment(ref _asyncOperations);
#else
        internal static void LoadAll()
        {
#endif
                Logger.Info("ContentDatabase commences loading all content");

                LoadWeapons();
                LoadArmors();
                LoadAccessories();
                LoadBattlerModifiers();
                LoadBattlerClasses();

#if !NOMULTITHREAD
                while (_asyncOperations > 1)
                    Thread.Sleep(1);
#endif

                if (FinishedLoadingAll != null)
                    FinishedLoadingAll.Invoke(__LOADALLPARTS, null);
                    Logger.Info("ContentDatabase finished loading all content");
                
#if !NOMULTITHREAD
                    Interlocked.Decrement(ref _asyncOperations);
            });

            lock (_lockObject)
            {
               
                if (LoadTask == null || (TaskStatus.RanToCompletion | TaskStatus.Faulted | TaskStatus.Canceled).HasFlag(LoadTask.Status))
                {
                    LoadTask = task;
                    LoadTask.Start();
                } else {
                    LoadTask.ContinueWith((noresult) => task);
                }
            }
#endif
        }

        /// <summary>
        /// Loads all wapons
        /// </summary>
        internal static void LoadWeapons()
        {
#if !NOMULTITHREAD
            Interlocked.Increment(ref _asyncOperations);

            Task.Factory.StartNew(() =>
            {
                while (IsReading(ContentDatabaseType.Weapon))
                    if (SpinAndWillYield()) break;
#endif
                Logger.Info("ContentDatabase (p:Weapons) loaded " + SerializableWeapons.Deserialize().ToString() + " weapons from [::MACHINE::]");
                FinishedLoadingPartial.Invoke(typeof(ContentDatabase), new IntegerEventArgs(__LOADALLPARTS));
#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Loads all Armors
        /// </summary>
        internal static void LoadArmors()
        {
#if !NOMULTITHREAD
            Interlocked.Increment(ref _asyncOperations);

            Task.Factory.StartNew(() =>
            {
                while (IsReading(ContentDatabaseType.Armor))
                    if (SpinAndWillYield()) break;
#endif
              
                Logger.Info("ContentDatabase (p:Armors) loaded " + SerializableArmors.Deserialize().ToString() + " armors from [::MACHINE::]");
                FinishedLoadingPartial.Invoke(typeof(ContentDatabase), new IntegerEventArgs(__LOADALLPARTS));
#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Loads all Accessories
        /// </summary>
        internal static void LoadAccessories()
        {
#if !NOMULTITHREAD
            Interlocked.Increment(ref _asyncOperations);

            Task.Factory.StartNew(() =>
            {
                while (IsReading(ContentDatabaseType.Accessory))
                    if (SpinAndWillYield()) break;
#endif
                
                Logger.Info("ContentDatabase (p:Accessories) loaded " + SerializableAccessoiries.Deserialize().ToString() + " accessories from [::MACHINE::]");
                FinishedLoadingPartial.Invoke(typeof(ContentDatabase), new IntegerEventArgs(__LOADALLPARTS));
#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Loads all modifiers
        /// </summary>
        internal static void LoadBattlerModifiers()
        {
#if !NOMULTITHREAD
            Interlocked.Increment(ref _asyncOperations);

            Task.Factory.StartNew(() =>
            {
                while (IsReading(ContentDatabaseType.BattlerState) || IsReading(ContentDatabaseType.BattlerBuff))
                    if (SpinAndWillYield()) break;
#endif

                Logger.Info("ContentDatabase (p:BattlerModifiers) loaded " + SerializableBattlerModifiers.Deserialize().ToString() + " battlermodifiers from [::MACHINE::]");
                FinishedLoadingPartial.Invoke(typeof(ContentDatabase), new IntegerEventArgs(__LOADALLPARTS));
#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            }, TaskCreationOptions.AttachedToParent);
#endif
        }

        /// <summary>
        /// Loads all classes
        /// </summary>
        internal static void LoadBattlerClasses()
        {
#if !NOMULTITHREAD
            Interlocked.Increment(ref _asyncOperations);

            Task.Factory.StartNew(() =>
            {
                while (IsReading(ContentDatabaseType.BattlerClass))
                    if (SpinAndWillYield()) break;
#endif

                Logger.Info("ContentDatabase (p:BattlerClass) loaded " + SerializableBattlerClasses.Deserialize().ToString() + " battlerclasses from [::MACHINE::]");
                FinishedLoadingPartial.Invoke(typeof(ContentDatabase), new IntegerEventArgs(__LOADALLPARTS));
#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            }, TaskCreationOptions.AttachedToParent);
#endif
        }
    }
}
