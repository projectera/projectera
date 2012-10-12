using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Threading.Tasks;
using ERAServer.Services;
using MongoDB.Driver.Builders;

namespace ERAServer.Data
{
    /// <summary>
    /// Dialogue message
    /// </summary>
    internal class DialogueMessage
    {
        /// <summary>
        /// Message id
        /// </summary>
        [BsonRequired]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Sender of message
        /// </summary>
        [BsonRequired]
        public ObjectId Sender
        {
            get;
            private set;
        }

        /// <summary>
        /// Contents of message
        /// </summary>
        [BsonRequired]
        public String Contents
        {
            get;
            private set;
        }

        /// <summary>
        /// Timestamp of message
        /// </summary>
        [BsonIgnore]
        public DateTime TimeStamp
        {
            get
            {
                return Id.CreationTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public ObjectId? Attachment
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public HashSet<ObjectId> Read
        {
            get;
            private set;
        }

        [BsonIgnore]
        public DialoguePage Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Attachment interface
        /// </summary>
        [BsonDiscriminator(RootClass = true)]
        [BsonKnownTypes(typeof(AchievementAttachment), typeof(EffortAttachment), typeof(ItemAttachment), typeof(LocationAttachment))]
        internal abstract class IAttachment
        {
            /// <summary>
            /// 
            /// </summary>
            [BsonId]
            public ObjectId Id
            {
                get;
                protected set;
            }


            /// <summary>
            /// Attachment id
            /// </summary>
            [BsonRequired]
            public ObjectId Sender
            {
                get;
                protected set;
            }

            /// <summary>
            /// 
            /// </summary>
            [BsonIgnore]
            public DateTime TimeStamp
            {
                get { return Id.CreationTime; }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Put()
            {
                Put(SafeMode.False);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sm"></param>
            /// <returns></returns>
            public SafeModeResult Put(SafeMode sm)
            {
                return GetCollection().Save(this, sm);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static Task<IAttachment> Get(ObjectId id)
            {
                return Task<IAttachment>.Factory.StartNew(() => { return GetBlocking(id); });
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static IAttachment GetBlocking(ObjectId id)
            {
                return GetCollection().FindOneById(id);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static MongoCollection<IAttachment> GetCollection()
            {
                return DataManager.Database.GetCollection<IAttachment>("Players.Attachments");
            }
        }

        /// <summary>
        /// Attachment in message to show an interactables location
        /// </summary>
        internal class LocationAttachment : IAttachment
        {
            private const Byte AttachmentType = 1;

            /// <summary>
            /// Map id
            /// </summary>
            [BsonRequired]
            public ObjectId MapId
            {
                get;
                private set;
            }

            /// <summary>
            /// Map x coord
            /// </summary>
            [BsonRequired]
            public Int32 MapX
            {
                get;
                private set;
            }

            /// <summary>
            /// Map y coord
            /// </summary>
            [BsonRequired]
            public Int32 MapY
            {
                get;
                private set;
            }

            /// <summary>
            /// Generates location attachment
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public static IAttachment Generate(ObjectId sender, Interactable source)
            {
                LocationAttachment result = new LocationAttachment();
                result.Id = ObjectId.GenerateNewId();
                result.Sender = sender;
                result.MapId = source.MapId;
                result.MapX = source.MapX;
                result.MapY = source.MapY;

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            public NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
            {
                msg.Write(AttachmentType); // 1
                msg.Write(this.Id.ToByteArray()); // 13
                msg.Write(this.MapId.ToByteArray()); // 25
                msg.Write(this.MapX); // 29
                msg.Write(this.MapY); // 33
                msg.Write(this.Sender.ToByteArray()); // 45
                msg.Write(this.TimeStamp.ToBinary()); // 53
                return msg;
            }
        }

        /// <summary>
        /// Attachment in message to show an item
        /// </summary>
        internal class ItemAttachment : IAttachment
        {
            private const Byte AttachmentType = 2;

            /// <summary>
            /// Item id
            /// </summary>
            [BsonRequired]
            public ObjectId ItemId
            {
                get;
                private set;
            }

            /// <summary>
            /// Blueprint id
            /// </summary>
            public Int32 BlueprintId
            {
                get;
                private set;
            }

            /// <summary>
            /// Generates item attachment
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public static IAttachment Generate(ObjectId sender, InteractableItem item)
            {
                ItemAttachment result = new ItemAttachment();
                result.Id = ObjectId.GenerateNewId();
                result.ItemId = item.Id;
                result.Sender = sender;
                result.BlueprintId = item.BlueprintId;

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            public NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
            {
                msg.Write(AttachmentType); // 1
                msg.Write(this.Id.ToByteArray()); // 13
                msg.Write(Data.Blueprint.Item.GetBlocking(this.BlueprintId).Id); // 17
                msg.Write(this.Sender.ToByteArray()); // 29
                msg.Write(this.TimeStamp.ToBinary()); // 37
                return msg;
            }
        }

        /// <summary>
        /// Attachment in message to show an achievement
        /// </summary>
        internal class AchievementAttachment : IAttachment
        {
            private const Byte AttachmentType = 3;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            public NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
            {
                msg.Write(AttachmentType); // 1
                msg.Write(this.Id.ToByteArray()); // 13
                msg.Write(this.Sender.ToByteArray()); // 25
                msg.Write(this.TimeStamp.ToBinary()); // 33
                return msg;
            }
        }

        /// <summary>
        /// Attachment in message to show an effort
        /// </summary>
        internal class EffortAttachment : IAttachment
        {
            private const Byte AttachmentType = 3;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            public NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
            {
                msg.Write(AttachmentType); // 1
                msg.Write(this.Id.ToByteArray()); // 13
                msg.Write(this.Sender.ToByteArray()); // 25
                msg.Write(this.TimeStamp.ToBinary()); // 33
                return msg;
            }
        }

        /// <summary>
        /// Generates a new dialog message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static DialogueMessage Generate(ObjectId sender, String message)
        {
            DialogueMessage result = new DialogueMessage();
            result.Id = ObjectId.GenerateNewId();
            result.Sender = sender;
            result.Contents = message;
            result.Read = new HashSet<ObjectId> { sender };
            result.Read.Remove(ObjectId.Empty);
            result.Parent = null;

            return result;
        }

        /// <summary>
        /// Attaches attachment to a message
        /// </summary>
        /// <param name="attachment"></param>
        internal void Attach(DialogueMessage.IAttachment attachment)
        {
            this.Attachment = attachment.Id;
            attachment.Put();

            Dialogue.GetCollection().FindAndModify(Query.EQ("Pages._id", Parent.Id), 
                SortBy.Null, Update.Set("Pages.$", this.Parent.ToBsonDocument()));
                    //AddToSet("Pages.$.Contents", this.ToBsonDocument<DialogueMessage>()));*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receipientId"></param>
        internal void MarkAsRead(ObjectId readerId)
        {
            if (readerId != ObjectId.Empty && this.Read.Add(readerId))
            {
                Dialogue.GetCollection().FindAndModify(Query.EQ("Pages._id", Parent.Id), 
                    SortBy.Null, Update.Set("Pages.$", this.Parent.ToBsonDocument()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readerId"></param>
        internal void MarkAsUnread(ObjectId readerId)
        {
            if (this.Read.Remove(readerId))
            {
                Dialogue.GetCollection().FindAndModify(Query.EQ("Pages._id", Parent.Id), 
                    SortBy.Null, Update.Set("Pages.$", this.Parent.ToBsonDocument()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
        {
            msg.Write(this.Id.ToByteArray()); // 12
            msg.Write(this.Contents); // 12 + x
            msg.Write(this.Sender.ToByteArray()); // 24 + x
            msg.Write(this.TimeStamp.ToBinary()); // 32 + x
            msg.Write(this.Attachment.HasValue); // 33 + x
            msg.Write(this.Attachment.GetValueOrDefault().ToByteArray()); // (33 || 45) + x

            return msg;
        }
    }

}
