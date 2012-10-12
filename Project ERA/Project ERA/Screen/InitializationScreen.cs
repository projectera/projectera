using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Data;
using System.Diagnostics;
using System.Threading;
using ERAUtils.Logger;
using ProjectERA.Services.Network;
using ERAUtils;

namespace ProjectERA.Screen
{
    internal partial class InitializationScreen : ProgressLoadingScreen
    {
        /// <summary>
        /// 
        /// </summary>
        public InitializationScreen()
            : base()
        {
            this.IsPopup = false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();
            
//#if DEBUG

            // LOAD AND POPULATE
            ContentDatabase.Populate();

            ContentDatabase.FinishedLoadingPartial += new IntegerEventHandler(ContentDatabase_FinishedLoadingPartial);
            ContentDatabase.FinishedLoadingAll += new EventHandler(ContentDatabase_FinishedLoadingAll);
            ContentDatabase.LoadAll();

            try
            {
                #if !NOMULTITHREAD
                ContentDatabase.LoadTask.ContinueWith((prev) =>
                {
                #endif
                    ContentDatabase.SaveAll();

                    Pool<Data.Equipment>.Initialize(25000);
                    Pool<Data.Interactable>.Initialize(250);
                    Pool<Data.Player>.Initialize(10);

                    /*ProgressBy(-1);
                    while (Progress != 0)
                        System.Threading.Thread.Sleep(1);

                    _networkManager.OnConnectingStatusChanged += new NetworkManager.ConnectingStatusChangedEventHandler(_networkManager_OnConnectingStatusChanged);
                    _networkManager.OnHandShakeCompleted += new EventHandler(_networkManager_OnHandShakeCompleted);
                    _networkManager.OnHandShakeFailed += new EventHandler(_networkManager_OnHandShakeFailed);
                    _networkManager.OnHandShakeNoResponse += new BooleanEventHandler(_networkManager_OnHandShakeNoResponse);
                    _networkManager.AsyncConnect("Derk-Jan", "Password");*/

                    FinishProgress();

                #if !NOMULTITHREAD
                });
                #endif
            }
            catch (Exception e)
            {
                throw e;
            }

            #if !NOMULTITHREAD
            if (ContentDatabase.LoadTask.Status == System.Threading.Tasks.TaskStatus.Created)
                ContentDatabase.LoadTask.Start();
            #endif
            
//            #else
            /*
            ContentDatabase.LoadAll();
            ContentDatabase.Populate();

            FinishProgress();

            #if !NOMULTITHREAD
                ContentDatabase.LoadTask.ContinueWith((prev) =>
                {
            #endif
                    ContentDatabase.SaveAll();

                    Pool<Data.Equipment>.Initialize(25000);
                    Pool<Data.Interactable>.Initialize(250);
                    Pool<Data.Player>.Initialize(10);
                    FinishProgress();

            #if !NOMULTITHREAD
                });
            #endif

            #if !NOMULTITHREAD
            if (ContentDatabase.LoadTask.Status == System.Threading.Tasks.TaskStatus.Created)
                ContentDatabase.LoadTask.Start();
            #endif

            #endif*/
        }

        #region Events
        /*
        #region HandShakeEvents
        /// <summary>
        /// Event: No Response Handshake
        /// </summary>
        /// <param name="sender">Source</param>
        /// <param name="e">BooleanEventArgs that holds value if reconnecting is taking place</param>
        private void _networkManager_OnHandShakeNoResponse(object sender, BooleanEventArgs e)
        {
            if (e.Value == false)
            {
                UnregisterHandShakeEvents();
                Logger.Notice("Authentication No Response");
                ProgressBy(-1);
            }
            else
            {
                Logger.Notice("Authentication No Response - Reconnecting");
                ProgressBy(-2, 6);
            }   
        }

        /// <summary>
        /// Event: Failed Handshake
        /// </summary>
        /// <param name="sender">Source</param>
        /// <param name="e"></param>
        private void _networkManager_OnHandShakeFailed(object sender, EventArgs e)
        {
            UnregisterHandShakeEvents();
            Logger.Notice("Authentication Failed");
            ProgressBy(-1);
        }

        /// <summary>
        /// Event: Completed HandShake
        /// </summary>
        /// <param name="sender">Source</param>
        /// <param name="e"></param>
        private void _networkManager_OnHandShakeCompleted(object sender, EventArgs e)
        {
            UnregisterHandShakeEvents();
            FinishProgress();
        }

        /// <summary>
        /// Unregisters all events on the handshake
        /// </summary>
        private void UnregisterHandShakeEvents()
        {
            _networkManager.OnHandShakeCompleted -= _networkManager_OnHandShakeCompleted;
            _networkManager.OnHandShakeFailed -= _networkManager_OnHandShakeFailed;
            _networkManager.OnHandShakeNoResponse -= _networkManager_OnHandShakeNoResponse;
            _networkManager.OnConnectingStatusChanged -= _networkManager_OnConnectingStatusChanged;
        }

        #endregion

        
        /// <summary>
        /// Event: ConnectingStatus Changes
        /// </summary>
        /// <param name="sender">Source</param>
        /// <param name="e"></param>
        private void _networkManager_OnConnectingStatusChanged(object sender, NetworkManager.ConnectingStatusChangedEventArgs e)
        {
            ProgressBy(1, 6);
            Logger.Info(e.Status.ToString());
        }*/

        /// <summary>
        /// Event: Loaded ContentDatabase partial
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentDatabase_FinishedLoadingPartial(object sender, IntegerEventArgs e)
        {
            ProgressBy(1, e.Value);
        }

        /// <summary>
        /// Event: Finished Loading ContentDatabase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentDatabase_FinishedLoadingAll(object sender, EventArgs e)
        {
            ContentDatabase.FinishedLoadingAll -= ContentDatabase_FinishedLoadingAll;
            ContentDatabase.FinishedLoadingPartial -= ContentDatabase_FinishedLoadingPartial;
        }
        #endregion

        #region Tests

        [Conditional("DEBUG")]
        private void Tests()
        {
            System.Threading.Thread.Sleep(1000);

            // Undo contentdatabase loading
            ProgressBy(-1);
            while (Progress != 0)
                System.Threading.Thread.Sleep(1);

            RecycleTest();


            FinishProgress();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RecycleTest()
        {
            ProgressBy(-1);

            while (Progress != 0)
                System.Threading.Thread.Sleep(1);

            /*
            Data.Interactable a = Pool<Data.Interactable>.Fetch();
            ContentDatabase.GetEquipmentArmor(2).Clone(a.Equipment.ArmLeft);
            ContentDatabase.GetEquipmentArmor(1).Clone(a.Equipment.Bottom);
            Debug.WriteLine(a.Equipment.ArmLeft.ItemId);
            a.Equipment.ArmRight.Set(ContentDatabase.GetEquipmentWeapon(2));
            Debug.WriteLine(a.Equipment.ArmRight.Name);
            Pool<Data.Interactable>.Recycle(a);
            Debug.WriteLine(a.Equipment.ArmLeft.ItemId);
            Debug.WriteLine(a.Equipment.ArmRight.Name);*/
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        internal override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
