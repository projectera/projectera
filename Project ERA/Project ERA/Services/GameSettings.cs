using System;
using System.IO;
using System.IO.IsolatedStorage;
using ERAUtils.Logger;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using ProjectERA.Services.Data.Storage;
using Microsoft.Xna.Framework;

namespace ProjectERA.Services
{
    [Serializable]
    public class GameSettings
    {        
        /// <summary>
        /// All log messages lower than this severity won't get written to the log
        /// </summary>
        [ContentSerializer]
        public static Severity LogSeverity { get; set; }

        /// <summary>
        /// Finetune for input. Press time is the time before triggerkey status becomes PressedActive after Active.
        /// </summary>
        [ContentSerializer]
        public static Double TriggerKeyPressTime { get; set; }

        /// <summary>
        /// Finetune for input. Reactivation time it the time before triggerkey status becomes PressedActive after PressedInactive.
        /// </summary>
        [ContentSerializer]
        public static Double TriggerKeyReactivationTime { get; set; }

        /// <summary>
        /// Enables motion blur on some rendertargets
        /// </summary>
        [ContentSerializer]
        public static bool MotionBlurEnabled { get; set; }

        /// <summary>
        /// Enables motion blur on some rendertargets
        /// </summary>
        [ContentSerializer]
        public static bool BloomEnabled { get; set; }

        /// <summary>
        /// Reads user or machine config
        /// </summary>
        internal static void Initialize()
        {
            // Try to load user config
            ProjectERA.Data.Settings gs;

            SharedStorageDevice localStorage = new SharedStorageDevice();
            localStorage.Initialize();
            localStorage.PromptForDevice();
            localStorage.Update(null);

            PlayerStorageDevice localStorageP = new PlayerStorageDevice(PlayerIndex.One);
            localStorageP.Initialize();
            localStorageP.PromptForDevice();
            localStorageP.Update(null);

            if (TryLoad(localStorageP, out gs) ||
                TryLoad(localStorage, out gs))
            {
                Process(gs);
                Logger.Info("Loaded Settings (f:[::ISOLATED::]Settings.xml)");
            } else {
                try
                {
                    using (Stream s = Microsoft.Xna.Framework.TitleContainer.OpenStream(@"Content\Data\Settings.xml"))
                    {
                        using (XmlReader reader = XmlReader.Create(s))
                        {
                            Process(IntermediateSerializer.Deserialize<ProjectERA.Data.Settings>(reader, "Settings.xml"));
                        }
                    }

                    Logger.Info("Loaded Settings (f:[::TITLE::]Settings.xml)");
                }
                catch (FileNotFoundException)
                {
                    Logger.Error("Settings could not be loaded");

#if DEBUG && NOFAILSAFE
                        throw (new FileNotFoundException("Settings could not be loaded"));
#else
                    Logger.Notice("Failsafe is using default settings");

                    // Set default values.
                    SetDefault();
#endif
                }
                catch (InvalidContentException)
                {

                    Logger.Error("Settings could not be loaded");

#if DEBUG && NOFAILSAFE
                        throw (new FileNotFoundException("Settings could not be loaded"));
#else
                    Logger.Notice("Failsafe is using default settings");

                    // Set default values.
                    SetDefault();
#endif
                }
               
            }

            Save();
        }

        /// <summary>
        /// Set default GameSettings
        /// </summary>
        private static void SetDefault()
        {
            LogSeverity = Severity.Info;
            TriggerKeyPressTime = 300;
            TriggerKeyReactivationTime = 15;
            MotionBlurEnabled = true;
            BloomEnabled = false;
        }

        /// <summary>
        /// Read settings
        /// </summary>
        /// <param name="source"></param>
        internal static void Process(ProjectERA.Data.Settings source)
        {
            LogSeverity = source.LogSeverity;
            TriggerKeyPressTime = source.TriggerKeyPressTime;
            TriggerKeyReactivationTime = source.TriggerKeyReactivationTime;
            MotionBlurEnabled = source.MotionBlurEnabled;
            BloomEnabled = source.BloomEnabled;
        }

        /// <summary>
        /// Saves the config to user storage
        /// </summary>
        internal static void Save()
        {
            GameSettings.Save(new ProjectERA.Data.Settings(Lidgren.Network.NetRandom.Instance.NextInt()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gs"></param>
        internal static void Save(ProjectERA.Data.Settings gs)
        {
            PlayerStorageDevice localStorage = new PlayerStorageDevice(PlayerIndex.One);
            localStorage.Initialize();
            localStorage.PromptForDevice();
            localStorage.Update(null);
 
            //IsolatedStorageFile localStorage = IsolatedStorageFile.GetUserStoreForDomain();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            localStorage.Save(".", "Settings.xml", (s) => 
            {
                using (XmlWriter writer = XmlWriter.Create(s, settings))
                {
                    IntermediateSerializer.Serialize(writer, gs, "Settings.xml");
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localStorage"></param>
        /// <param name="gs"></param>
        /// <returns></returns>
        internal static Boolean TryLoad(IAsyncStorageDevice localStorage, out ProjectERA.Data.Settings gs)
        {
            gs = new ProjectERA.Data.Settings();

            if (localStorage.FileExists(".", "Settings.xml"))
            {
                try
                {
                    ProjectERA.Data.Settings inner = new ProjectERA.Data.Settings();
                    localStorage.Load(".", "Settings.xml", (s) =>
                    {
                        using (XmlReader reader = XmlReader.Create(s))
                        {
                            inner = IntermediateSerializer.Deserialize<ProjectERA.Data.Settings>(reader, "Settings.xml");
                        }
                    });

                    gs = inner;

                    Logger.Info("Loaded Settings (f:[::USER::]Settings.xml)");

                    return true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (InvalidContentException)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
