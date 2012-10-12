using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace ERAServer.Data
{
    /// <summary>
    /// Page of dialogue
    /// </summary>
    internal class DialoguePage
    {
        public const Int32 PageSize = 25;

        /// <summary>
        /// Dialogue page
        /// </summary>
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Dialogue it belongs to
        /// </summary>
        public ObjectId DialogueId
        {
            get;
            private set;
        }

        /// <summary>
        /// Contents of page
        /// </summary>
        public List<DialogueMessage> Contents
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ObjectId? FollowUp
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="dialogueMessage"></param>
        /// <returns></returns>
        internal static DialoguePage Generate(ObjectId dialog, DialogueMessage dialogueMessage)
        {
            DialoguePage result = new DialoguePage();
            result.Id = ObjectId.GenerateNewId();
            result.DialogueId = dialog;
            result.Contents = new List<DialogueMessage>(PageSize) { dialogueMessage };
            result.FollowUp = null;
            dialogueMessage.Parent = result;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialogueMessage"></param>
        /// <returns></returns>
        internal Boolean AddMessage(DialogueMessage dialogueMessage)
        {
            if (this.Contents.Count >= PageSize)
                return false;

            lock (this.Contents)
            {
                this.Contents.Add(dialogueMessage);
                dialogueMessage.Parent = this;

                // Updates message
                Dialogue.GetCollection().Update(Query.EQ("Pages._id", this.Id), Update.AddToSet("Pages.$.Contents", dialogueMessage.ToBsonDocument<DialogueMessage>()));
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
        {
            msg.Write(this.Id.ToByteArray()); // 12
            msg.Write(this.Contents.Count); // 16

            foreach (var message in this.Contents)
                msg = message.Pack(ref msg);// 16 + x * (33 || 44 + y)

            return msg;
        }
    }
}
