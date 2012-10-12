using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ERAUtils.Enum;
using System.Threading;
using MongoDB.Bson.Serialization.Attributes;
using ERAUtils;
using MongoDB.Bson;
using ProjectERA.Protocols;
using ERAServer.Services;

namespace ERAServer.Data.AI
{
    [Serializable]
    internal partial class InteractableAppearance : InteractableComponent, IEnumerable, IResetable
    {
        [BsonElement("BodyParts")]
        private List<InteractableBodyPart> _bodyParts;
        private ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// Returns the body part with type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal InteractableBodyPart this[BodyPart type]
        {
            get
            {
                try
                {
                    _rwLock.EnterReadLock();
                    return _bodyParts.First(bp => bp.Type == type);
                } finally {
                    _rwLock.ExitReadLock();
                }
            }
            set
            {
                _rwLock.EnterWriteLock();
                if (_bodyParts.Any(bp => bp.Type == type))
                    _bodyParts[_bodyParts.FindIndex(bp => bp.Type == type)] = value;
                else
                    _bodyParts.Add(value);

                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the body part at index
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal InteractableBodyPart this[Int32 index]
        {
            get 
            { 
                try
                {
                    _rwLock.EnterReadLock();
                    return _bodyParts[index]; 
                } finally {
                    _rwLock.ExitReadLock();
                }
            }
            set 
            { 
                _rwLock.EnterWriteLock();
                _bodyParts[index] = value; 
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the number of body parts
        /// </summary>
        internal Int32 Count
        {
            get { return _bodyParts.Count; }
        }

        /// <summary>
        /// Map Dir coord
        /// </summary>
        [BsonRequired]
        public Byte MapDir
        {
            get;
            set;
        }

        /// <summary>
        /// Initial Animation Frame
        /// </summary>
        public Byte AnimationFrame
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public Byte Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte Hue
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractableAppearance()
        {
            _rwLock = new ReaderWriterLockSlim();
            _bodyParts = new List<InteractableBodyPart>();
        }

        /// <summary>
        /// Generates default body
        /// </summary>
        /// <returns></returns>
        internal static InteractableAppearance Generate()
        {
            InteractableAppearance result = new InteractableAppearance();
            result.AddPart(InteractableBodyPart.GenerateSkin(DataManager.Random.SkinId));
            result.AddPart(InteractableBodyPart.GenerateEyes(DataManager.Random.EyesId, DataManager.Random.ColorId));
            result.AddPart(InteractableBodyPart.GenerateHair(DataManager.Random.HairId, DataManager.Random.ColorId));

            result.MapDir = 2;
            result.AnimationFrame = 0;
            result.Opacity = 255;
            result.Hue = 0;

            result.Sort();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static InteractableAppearance Generate(Interactable root)
        {
            InteractableAppearance result = Generate();
            result.Root = root;
            return result;
        }

        /// <summary>
        /// Generates body with one asset
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        internal static InteractableAppearance Generate(String graphic)
        {
            InteractableAppearance result = new InteractableAppearance();
            result.AddPart(InteractableBodyPart.Generate(BodyPart.Asset, graphic, 255, 0, 10));

            result.MapDir = 2;
            result.AnimationFrame = 0;
            result.Opacity = 255;
            result.Hue = 0;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static InteractableAppearance Generate(Interactable root, String graphic)
        {
            InteractableAppearance result = Generate(graphic);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// Generates a body with skin, eyes, hair
        /// </summary>
        /// <param name="skintone"></param>
        /// <param name="eyeType"></param>
        /// <param name="eyeColor"></param>
        /// <param name="hairType"></param>
        /// <param name="hairColor"></param>
        /// <returns></returns>
        internal static InteractableAppearance Generate(Byte skintone, Byte eyeType, Byte eyeColor, Byte hairType, Byte hairColor)
        {
            InteractableAppearance result = new InteractableAppearance();
            result.AddPart(InteractableBodyPart.GenerateSkin(skintone));
            result.AddPart(InteractableBodyPart.GenerateEyes(eyeType, eyeColor));
            result.AddPart(InteractableBodyPart.GenerateHair(hairType, hairColor));

            result.MapDir = 2;
            result.AnimationFrame = 0;
            result.Opacity = 255;
            result.Hue = 0;

            result.Sort();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static InteractableAppearance Generate(Interactable root, Byte skintone, Byte eyeType, Byte eyeColor, Byte hairType, Byte hairColor)
        {
            InteractableAppearance result = Generate(skintone, eyeType, eyeColor, hairType, hairColor);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// Adds a part
        /// </summary>
        /// <param name="part"></param>
        internal void AddPart(InteractableBodyPart part)
        {
            _rwLock.EnterWriteLock();
            _bodyParts.Add(part);
            _rwLock.ExitWriteLock();

            Sort();
        }

        /// <summary>
        /// Removes a part
        /// </summary>
        /// <param name="part"></param>
        internal void RemovePart(InteractableBodyPart part)
        {
            _rwLock.EnterWriteLock();
            _bodyParts.Remove(part);
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Removes a part by type
        /// </summary>
        /// <param name="type"></param>
        internal void RemovePart(BodyPart type)
        {
            _rwLock.EnterWriteLock();
            InteractableBodyPart pending = _bodyParts.First(bp => bp.Type == type);
            _bodyParts.Remove(pending);
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Sorts the body parts by priority
        /// </summary>
        internal void Sort()
        {
 	        _bodyParts.Sort(InteractableBodyPart.CompareByPriority);
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _bodyParts.GetEnumerator();
        }

        /// <summary>
        /// Clears this component
        /// </summary>
        public void Clear()
        {
            _rwLock.EnterWriteLock();
            _bodyParts.Clear();
            _rwLock.ExitWriteLock();

            this.Root = null;
        }

        /// <summary>
        /// Recycles this component and sub-components
        /// </summary>
        internal override void Expire()
        {
            this.Clear();
        }

        /// <summary>
        /// 6
        /// </summary>
        /// <param name="msg"></param>
        internal override void Pack(ref Lidgren.Network.NetOutgoingMessage msg)
        {
            // Write header
            msg.WriteRangedInteger(0, (Int32)(InteractableAction.Max), (Int32)InteractableAction.Appearance);
            
            // Write map appearance
            msg.Write(this.MapDir);
            msg.Write(this.AnimationFrame);

            // Write avatar appearance
            lock (_bodyParts)
            {
                msg.Write(this.Count);
                foreach (InteractableBodyPart part in this)
                {
                    msg.Write((Byte)part.Type);
                    msg.Write(part.Opacity);
                    msg.Write(part.Hue);
                    msg.Write(part.Priority);

                    if (part.Type == BodyPart.Skin) // 0 value
                    {
                        msg.Write(part.ByteValue ?? (Byte)1); // 1 Byte
                    }
                    else if (BodyPart.Integer.HasFlag(part.Type))
                    {
                        msg.Write(part.IntegerValue ?? 2); // 1 Int32
                    }
                    else if (BodyPart.ByteArr.HasFlag(part.Type))
                    {
                        msg.Write(part.ArrayByteValue ?? new Byte[] { 2, 2 }); // 2 Bytes;
                    }
                    else if (BodyPart.Byte.HasFlag(part.Type))
                    {
                        msg.Write(part.ByteValue ?? (Byte)1); // 1 Byte
                    }
                    else if (BodyPart.String.HasFlag(part.Type))
                    {
                        msg.Write(part.StringValue ?? String.Empty); // 1 String
                    }
                    else
                    {
                        ERAUtils.Logger.Logger.Warning("Bodypart has no value!");
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        internal InteractableBodyPart RemoveWeapon(Int32 id)
        {
            _rwLock.EnterWriteLock();
            InteractableBodyPart pending = _bodyParts.First(bp => bp.Type == BodyPart.Weapon && bp.IntegerValue == id);
            _bodyParts.Remove(pending);
            _rwLock.ExitWriteLock();

            return pending;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        internal InteractableBodyPart RemoveArmor(Int32 id)
        {
            _rwLock.EnterWriteLock();
            InteractableBodyPart pending = _bodyParts.First(bp => bp.Type == BodyPart.Armor && bp.IntegerValue == id);
            _bodyParts.Remove(pending);
            _rwLock.ExitWriteLock();

            return pending;
        }
    }
}
