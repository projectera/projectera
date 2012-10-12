using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Data.Enum;
using Microsoft.Xna.Framework;
using System.Threading;
using ERAUtils;

namespace ProjectERA.Data
{
    internal class AvatarSynths : IResetable
    {
        private Int32 _fireSynths, _waterSynths, _airSynths, _earthSynths;
        private Int32 _enduranceSynths, _agilitySynths, _mindSynths, _strengthSynths;
        private Int32 _lightSynths, _darkSynths, _spiritSynths;

        /// <summary>
        /// Returns (number of) Fire synths
        /// </summary>
        public Int32 Fire
        {
            get { return _fireSynths; }
        }

        /// <summary>
        /// Returns (number of) Water synths
        /// </summary>
        public Int32 Water
        {
            get { return _waterSynths; }
        }

        /// <summary>
        /// Returns (number of) Air synths
        /// </summary>
        public Int32 Air
        {
            get { return _airSynths; }
        }

        /// <summary>
        /// Returns (number of) Earth synths
        /// </summary>
        public Int32 Earth
        {
            get { return _earthSynths; }
        }

        /// <summary>
        /// Returns (number of) Endurance synths
        /// </summary>
        public Int32 Endurance
        {
            get { return _enduranceSynths; }
        }

        /// <summary>
        /// Returns (number of) Agility synths
        /// </summary>
        public Int32 Agility
        {
            get { return _agilitySynths; }
        }

        /// <summary>
        /// Returns (number of) Mind synths
        /// </summary>
        public Int32 Mind
        {
            get { return _mindSynths; }
        }

        /// <summary>
        /// Returns (number of) Strength synths
        /// </summary>
        public Int32 Strength
        {
            get { return _strengthSynths; }
        }

        /// <summary>
        /// Returns (number of) Spirit synths
        /// </summary>
        public Int32 Spirit
        {
            get { return _spiritSynths; }
        }

        /// <summary>
        /// Returns (number of) Light synths
        /// </summary>
        public Int32 Light
        {
            get { return _lightSynths; }
        }

        /// <summary>
        /// Returns (number of) Dark synths
        /// </summary>
        public Int32 Dark
        {
            get { return _darkSynths; }
        }

        /// <summary>
        /// Returns (number of) Heat synths
        /// </summary>
        public Int32 Heat
        {
            get { return Math.Min(this.Fire, this.Water); }
        }

        /// <summary>
        /// Returns (number of) Nature synths
        /// </summary>
        public Int32 Nature
        {
            get { return Math.Min(this.Water, this.Earth); }
        }

        /// <summary>
        /// Returns (number of) Searing synths
        /// </summary>
        public Int32 Searing
        {
            get { return Math.Min(this.Air, this.Fire); }
        }

        /// <summary>
        /// Returns (number of) Destruction synths
        /// </summary>
        public Int32 Destruction
        {
            get { return Math.Min(this.Light, this.Dark); }
        }

        /// <summary>
        /// Returns (number of) Domination synths
        /// </summary>
        public Int32 Domination
        { 
            get { return Math.Min(this.Light, this.Spirit); }
        }

        /// <summary>
        /// Returns (number of) Reduction synths
        /// </summary>
        public Int32 Reduction
        {
            get { return Math.Min(this.Dark, this.Spirit); }
        }

        /// <summary>
        /// Get synths
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Int32 Get(ElementType type)
        {
            switch (type)
            {
                case ElementType.Fire:
                    return this.Fire;
                case ElementType.Water:
                    return this.Water;
                case ElementType.Air:
                    return this.Air;
                case ElementType.Earth:
                    return this.Earth;
                case ElementType.Endurance:
                    return this.Endurance;
                case ElementType.Mind:
                    return this.Mind;
                case ElementType.Strength:
                    return this.Strength;
                case ElementType.Agility:
                    return this.Agility;
                case ElementType.Spirit:
                    return this.Spirit;
                case ElementType.Light:
                    return this.Light;
                case ElementType.Dark:
                    return this.Dark;

                case ElementType.Heat:
                    return this.Heat;
                case ElementType.Nature:
                    return this.Nature;
                case ElementType.Searing:
                    return this.Searing;
                case ElementType.Reduction:
                    return this.Reduction;
                case ElementType.Domination:
                    return this.Domination;
                case ElementType.Destruction:
                    return this.Destruction;
            }

            throw new ArgumentException("There is no such synth", "type");
        }

        /// <summary>
        /// Modify synths
        /// </summary>
        /// <param name="type"></param>
        /// <param name="modifier"></param>
        private void Modify(ElementType type, Int32 modifier)
        {
            switch (type)
            {
                case ElementType.Fire:
                    Add(ref _fireSynths, modifier);
                    break;
                case ElementType.Water:
                    Add(ref _waterSynths, modifier);
                    break;
                case ElementType.Air:
                    Add(ref _airSynths, modifier);
                    break;
                case ElementType.Earth:
                    Add(ref _earthSynths, modifier);
                    break;
                case ElementType.Endurance:
                    Add(ref _enduranceSynths, modifier);
                    break;
                case ElementType.Mind:
                    Add(ref _mindSynths, modifier);
                    break;
                case ElementType.Strength:
                    Add(ref _strengthSynths, modifier);
                    break;
                case ElementType.Agility:
                    Add(ref _agilitySynths, modifier);
                    break;
                case ElementType.Dark:
                    Add(ref _darkSynths, modifier);
                    break;
                case ElementType.Light:
                    Add(ref _lightSynths, modifier);
                    break;
                case ElementType.Spirit:
                    Add(ref _spiritSynths, modifier);
                    break;

                case ElementType.Heat:
                case ElementType.Nature:
                case ElementType.Searing:
                case ElementType.Reduction:
                case ElementType.Destruction:
                case ElementType.Domination:
                    throw new NotSupportedException("The element type " + type + " is has more than one component and is thus read only.");

                default:
                    throw new ArgumentException("There is no such synth", "type");
            }
        }

        /// <summary>
        /// Modify synths
        /// </summary>
        /// <param name="type"></param>
        /// <param name="modifier"></param>
        private Boolean TryModify(ElementType type, Int32 modifier, Int32 snapshot)
        {
            switch (type)
            {
                case ElementType.Fire:
                    return TryAdd(ref _fireSynths, modifier, snapshot);
                case ElementType.Water:
                    return TryAdd(ref _waterSynths, modifier, snapshot);
                case ElementType.Air:
                    return TryAdd(ref _airSynths, modifier, snapshot);
                case ElementType.Earth:
                    return TryAdd(ref _earthSynths, modifier, snapshot);
                case ElementType.Endurance:
                    return TryAdd(ref _enduranceSynths, modifier, snapshot);
                case ElementType.Mind:
                    return TryAdd(ref _mindSynths, modifier, snapshot);
                case ElementType.Strength:
                    return TryAdd(ref _strengthSynths, modifier, snapshot);
                case ElementType.Agility:
                    return TryAdd(ref _agilitySynths, modifier, snapshot);
                case ElementType.Dark:
                    return TryAdd(ref _darkSynths, modifier, snapshot);
                case ElementType.Light:
                    return TryAdd(ref _lightSynths, modifier, snapshot);
                case ElementType.Spirit:
                    return TryAdd(ref _spiritSynths, modifier, snapshot);

                case ElementType.Heat:
                case ElementType.Nature:
                case ElementType.Searing:
                case ElementType.Reduction:
                case ElementType.Destruction:
                case ElementType.Domination:
                    throw new NotSupportedException("The element type " + type + " is has more than one component and is thus read only.");

                default:
                    throw new ArgumentException("There is no such synth", "type");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private Boolean TryModifyWhile(ElementType type, Int32 modifier)
        {
            switch (type)
            {
                case ElementType.Fire:
                    return TryAddWhile(ref _fireSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Water:
                    return TryAddWhile(ref _waterSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Air:
                    return TryAddWhile(ref _airSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Earth:
                    return TryAddWhile(ref _earthSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Endurance:
                    return TryAddWhile(ref _enduranceSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Mind:
                    return TryAddWhile(ref _mindSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Strength:
                    return TryAddWhile(ref _strengthSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Agility:
                    return TryAddWhile(ref _agilitySynths, modifier, 0, Int32.MaxValue);
                case ElementType.Dark:
                    return TryAddWhile(ref _darkSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Light:
                    return TryAddWhile(ref _lightSynths, modifier, 0, Int32.MaxValue);
                case ElementType.Spirit:
                    return TryAddWhile(ref _spiritSynths, modifier, 0, Int32.MaxValue);

                case ElementType.Heat:
                case ElementType.Nature:
                case ElementType.Searing:
                case ElementType.Reduction:
                case ElementType.Destruction:
                case ElementType.Domination:
                    throw new NotSupportedException("The element type " + type + " is has more than one component and is thus read only.");

                default:
                    throw new ArgumentException("There is no such synth", "type");
            }
        }

        /// <summary>
        /// Lock-free adding of value to field
        /// </summary>
        /// <param name="field">referenced field</param>
        /// <param name="value">value to add</param>
        private void Add(ref Int32 field, Int32 value)
        {
            SpinWait spinner = new SpinWait();
            while (true)
            {
                Int32 snapshot = field;
                Int32 updated = snapshot + value;
                Int32 snapshot2 = Interlocked.CompareExchange(ref field, updated, snapshot);
                if (snapshot == snapshot2)
                    return;
                spinner.SpinOnce();
            }   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Boolean TryAdd(ref Int32 field, Int32 value, Int32 snapshot)
        {
            Int32 updated = snapshot + value;
            Int32 snapshot2 = Interlocked.CompareExchange(ref field, updated, snapshot);
            return updated == snapshot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        private Boolean TryAddWhile(ref Int32 field, Int32 value, Int32 lowerBound, Int32 upperBound)
        {
            SpinWait spinner = new SpinWait();
            while (true)
            {
                Int32 snapshot = field;
                Int32 updated = snapshot + value;

                if (updated < lowerBound || updated > upperBound)
                    return false;
                
                //if (TryAdd(ref field, value, field))
                //  return true;

                Int32 snapshot2 = Interlocked.CompareExchange(ref field, updated, snapshot);
                if (snapshot == snapshot2)
                    return true;

                spinner.SpinOnce();
            }
        }

        /// <summary>
        /// Consume synths
        /// </summary>
        /// <param name="type">element type to consume</param>
        /// <param name="number">number of synths to consume</param>
        public void Consume(ElementType type, Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", number, "Consuming less than 0 synths is not allowed");
            if (number > Get(type))
                throw new ArgumentOutOfRangeException("number", number, "Consuming more than available is not allowed");

            Modify(type, -number);
        }

        /// <summary>
        /// Tries consuming number of type while snapshot is valid
        /// </summary>
        /// <param name="type">element type to consume</param>
        /// <param name="number">number of synths to consume</param>
        /// <param name="snapshot">snapshot of type (Get(type))</param>
        /// <returns>Succeeds</returns>
        public Boolean TryConsume(ElementType type, Int32 number, Int32 snapshot)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", number, "Consuming less than 0 synths is not allowed");
            if (number > snapshot)
                throw new ArgumentOutOfRangeException("number", number, "Consuming more than available synths is not allowed"); 

            return TryModify(type, -number, snapshot);
        }

        /// <summary>
        /// Tries consuming number of type while in bounds
        /// </summary>
        /// <param name="type">element type to consume</param>
        /// <param name="number">number of synths to consume</param>
        /// <returns>Succeeds</returns>
        public Boolean TryConsumeWhile(ElementType type, Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", number, "Consuming less than 0 synths is not allowed");

            return TryModifyWhile(type, -number);
        }

        /// <summary>
        /// Store number of type
        /// </summary>
        /// <param name="type">element type to store</param>
        /// <param name="number">number of synths to store</param>
        public void Store(ElementType type, Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", number, "Storing less than 0 synths is not allowed");
            if (number + Get(type) > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("number", number, "Storing more than max synths is not allowed");

            Modify(type, number);
        }

        /// <summary>
        /// Tries storing number of type while snapshot is valid
        /// </summary>
        /// <param name="type">element type to store</param>
        /// <param name="number">number of synths to store</param>
        /// <param name="snapshot">snapshot of type (Get(type))</param>
        /// <returns>Succeeds</returns>
        public Boolean TryStore(ElementType type, Int32 number, Int32 snapshot)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", number, "Storing less than 0 synths is not allowed");
            if (snapshot + number > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("number", number, "Storing more than max synths is not allowed");

            return TryModify(type, number, snapshot);
        }

        /// <summary>
        /// Tries storing number of type while in bounds
        /// </summary>
        /// <param name="type">element type to store</param>
        /// <param name="number">number of synths to store</param>
        /// <returns>Succeeds</returns>
        public Boolean TryStoreWhile(ElementType type, Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", number, "Consuming less than 0 synths is not allowed");

            return TryModifyWhile(type, number);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AvatarSynths()
        {
         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gametime"></param>
        internal void Update(GameTime gametime)
        {
            // updates internal regeneration of some types
        }

        /// <summary>
        /// Clears this instance
        /// </summary>
        public void Clear()
        {
            _fireSynths = 0;
            _airSynths = 0;
            _waterSynths = 0;
            _earthSynths = 0;

            _mindSynths = 0;
            _agilitySynths = 0;
            _strengthSynths = 0;
            _enduranceSynths = 0;

            _darkSynths = 0;
            _lightSynths = 0;
            _spiritSynths = 0;
        }
    }
}
