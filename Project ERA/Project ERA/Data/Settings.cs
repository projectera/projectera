using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services;
using ERAUtils.Logger;

namespace ProjectERA.Data
{
    /// <summary>
    /// Settings class for player
    /// </summary>
    [Serializable]
    internal struct Settings
    {
        private Int32 _magic;

        /// <summary>
        /// 
        /// </summary>
        public Settings(Int32 magic)
        {
            _magic = magic; 

            this.LogSeverity = GameSettings.LogSeverity;
            this.TriggerKeyReactivationTime = GameSettings.TriggerKeyReactivationTime;
            this.TriggerKeyPressTime = GameSettings.TriggerKeyPressTime;
            this.MotionBlurEnabled = GameSettings.MotionBlurEnabled;
            this.BloomEnabled = GameSettings.BloomEnabled;
        }

        /// <summary>
        /// 
        /// </summary>
        public Severity LogSeverity;
        public Double TriggerKeyReactivationTime;
        public Double TriggerKeyPressTime;
        public Boolean MotionBlurEnabled;
        public Boolean BloomEnabled;
    }
}
