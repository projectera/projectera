using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data
{
    // HOLDS battler values of equipment
    [Serializable]
    public struct BattlerValues
    {
        public static readonly BattlerValues None = new BattlerValues();

        /// <summary>
        /// Strength multiplier of the equipment
        /// </summary>
        public Single Strength;
        /// <summary>
        /// Defense multiplier of the equipment
        /// </summary>
        public Single Defense;
        /// <summary>
        /// Magical Strength multiplier of the equipment
        /// </summary>
        public Single MagicalStrength;
        /// <summary>
        /// Magic Defense multiplier of the equipment
        /// </summary>
        public Single MagicalDefense;

        /// <summary>
        /// Concentration multiplier of the equipment
        /// </summary>
        public Single Concentration;
        /// <summary>
        /// Haste multiplier of the equipment
        /// </summary>
        public Single Haste;
        /// <summary>
        /// Endurance multiplier of the equipment
        /// </summary>
        public Single Endurance;
        /// <summary>
        /// Essense multiplier of the equipment
        /// </summary>
        public Single Essence;

        /// <summary>
        /// Generates Battler Values
        /// </summary>
        /// <param name="str">Power % over hands</param>
        /// <param name="def">Defensive % over body/hands</param>
        /// <param name="mStr">Power % over mind/spirit</param>
        /// <param name="mDef">Defensive % over mind/spirit</param>
        /// <param name="con">Focus % over body/mind</param>
        /// <param name="has">Agility % over body</param>
        /// <param name="end">Stamina % over body</param>
        /// <param name="ess">Ability to use synths % over body</param>
        /// <returns></returns>
        internal static BattlerValues Generate(Single str, Single def, Single mStr, Single mDef,
            Single con, Single has, Single end, Single ess)
        {
            BattlerValues result = new BattlerValues();

            result.Strength = str;
            result.Defense = def;
            result.MagicalStrength = mStr;
            result.MagicalDefense = mDef;
            result.Concentration = con;
            result.Haste = has;
            result.Endurance = end;
            result.Essence = ess;

            return result;
        }

        /// <summary>
        /// Declare which operator to overload (+), the types 
        /// that can be added (two BattlerValues objects), and the 
        /// return type (BattlerValues):
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static BattlerValues operator +(BattlerValues c1, BattlerValues c2)
        {
            return BattlerValues.Generate(
                CombineValues(c1.Strength, c2.Strength),
                CombineValues(c1.Defense, c2.Defense),
                CombineValues(c1.MagicalStrength, c2.MagicalStrength),
                CombineValues(c1.MagicalDefense, c2.MagicalDefense),
                CombineValues(c1.Concentration, c2.Concentration),
                CombineValues(c1.Haste, c2.Haste),
                CombineValues(c1.Endurance, c2.Endurance),
                CombineValues(c1.Essence, c2.Essence)
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c2"></param>
        public void Add(BattlerValues c2)
        {
            this.Strength = CombineValues(this.Strength, c2.Strength);
            this.Defense = CombineValues(this.Defense, c2.Defense);
            this.MagicalStrength = CombineValues(this.MagicalStrength, c2.MagicalStrength);
            this.MagicalDefense = CombineValues(this.MagicalDefense, c2.MagicalDefense);
            this.Concentration = CombineValues(this.Concentration, c2.Concentration);
            this.Haste = CombineValues(this.Haste, c2.Haste);
            this.Endurance = CombineValues(this.Endurance, c2.Endurance);
            this.Essence = CombineValues(this.Essence, c2.Essence);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c2"></param>
        public void Subtract(BattlerValues c2)
        {
            this.Strength = SeperateValues(this.Strength, c2.Strength);
            this.Defense = SeperateValues(this.Defense, c2.Defense);
            this.MagicalStrength = SeperateValues(this.MagicalStrength, c2.MagicalStrength);
            this.MagicalDefense = SeperateValues(this.MagicalDefense, c2.MagicalDefense);
            this.Concentration = SeperateValues(this.Concentration, c2.Concentration);
            this.Haste = SeperateValues(this.Haste, c2.Haste);
            this.Endurance = SeperateValues(this.Endurance, c2.Endurance);
            this.Essence = SeperateValues(this.Essence, c2.Essence);
        }

        /// <summary>
        /// Declare which operator to overload (-), the types 
        /// that can be added (two BattlerValues objects), and the 
        /// return type (BattlerValues):
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static BattlerValues operator -(BattlerValues c1, BattlerValues c2)
        {
            return BattlerValues.Generate(
                SeperateValues(c1.Strength, c2.Strength),
                SeperateValues(c1.Defense, c2.Defense),
                SeperateValues(c1.MagicalStrength, c2.MagicalStrength),
                SeperateValues(c1.MagicalDefense, c2.MagicalDefense),
                SeperateValues(c1.Concentration, c2.Concentration),
                SeperateValues(c1.Haste, c2.Haste),
                SeperateValues(c1.Endurance, c2.Endurance),
                SeperateValues(c1.Essence, c2.Essence)
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">Base</param>
        /// <param name="b">Combine with</param>
        /// <returns>Combined values</returns>
        private static Single CombineValues(Single a, Single b)
        {
            // (1 + a/100) * (1 + b/100) = c + 1
            return ((100 + a) * b + (100 * a)) / 100;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">Combined</param>
        /// <param name="b">Seperate from</param>
        /// <returns>Rest</returns>
        private static Single SeperateValues(Single c, Single b)
        {
            // c - b - ab/100 = a
            return ((100 * c) - (100 * b)) / (b + 100);
        }

        /// <summary>
        ///  Strength multiplier
        /// </summary>
        /// <param name="str">Power % over hands</param>
        /// <returns></returns>
        internal static BattlerValues GenerateStr(Single str)
        {
            return Generate(str, 0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// Defense multiplier
        /// </summary>
        /// <param name="def">Focus % over body/mind</param>
        /// <returns></returns>
        internal static BattlerValues GenerateDef(Single def)
        {
            return Generate(0, def, 0, 0, 0, 0, 0, 0);

        }

        /// <summary>
        /// Magical Strength multiplier
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static BattlerValues GenerateMStr(Single str)
        {
            return Generate(0, 0, str, 0, 0, 0, 0, 0);

        }

        /// <summary>
        /// Magical Defense multiplier
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        internal static BattlerValues GenerateMDef(Single def)
        {
            return Generate(0, 0, 0, def, 0, 0, 0, 0);

        }

        /// <summary>
        /// Concentration multiplier
        /// </summary>
        /// <param name="con">Focus % over body/mind</param>
        /// <returns></returns>
        internal static BattlerValues GenerateCon(Single con)
        {
            return Generate(0, 0, 0, 0, con, 0, 0, 0);

        }

        /// <summary>
        /// Haste multiplier
        /// </summary>
        /// <param name="has">Agility % over body</param>
        /// <returns></returns>
        internal static BattlerValues GenerateHas(Single has)
        {
            return Generate(0, 0, 0, 0, 0, has, 0, 0);
        }

        /// <summary>
        /// Essence multiplier
        /// </summary>
        /// <param name="con">Ability to use synths % over body</param>
        /// <returns></returns>
        internal static BattlerValues GenerateEss(Single ess)
        {
            return Generate(0, 0, 0, 0, 0, 0, 0, ess);
        }

        /// <summary>
        /// Clears all values
        /// </summary>
        internal void Clear()
        {
            System.Diagnostics.Debug.WriteLine("Clearing battler equipment");

            Strength = 0;
            Defense = 0;
            MagicalDefense = 0;
            MagicalStrength = 0;
            Concentration = 0;
            Haste = 0;
            Endurance = 0;
            Essence = 0;
        }
    }
}
