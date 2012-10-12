using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;

namespace ProjectERA.Data
{
    internal class Dialogue : IResetable
    {
        /// <summary>
        /// 
        /// </summary>
        internal MongoObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal List<MongoObjectId> Participants
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal List<DialoguePage> Pages
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Dialogue()
        {
            this.Participants = new List<MongoObjectId>();
            this.Pages = new List<DialoguePage>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.Id = MongoObjectId.Empty;

            if (this.Participants != null)
                this.Participants.Clear();

            if (this.Pages != null)
            {
                foreach (DialoguePage page in Pages)
                    Pool<DialoguePage>.Recycle(page);

                this.Pages.Clear();
            }
        }
    }

    internal class DialoguePage : IResetable
    {
        internal MongoObjectId Id
        {
            get;
            private set;
        }

        internal List<DialogueMessage> Entries
        {
            get;
            private set;
        }

        public void Clear()
        {
            this.Id = MongoObjectId.Empty;

            foreach (DialogueMessage entry in Entries)
                Pool<DialogueMessage>.Recycle(entry);

            Entries.Clear();
        }
    }

    internal class DialogueMessage : IResetable
    {
        internal MongoObjectId Id
        {
            get;
            private set;
        }

        internal MongoObjectId Sender
        {
            get;
            private set;
        }

        internal String Contents
        {
            get;
            private set;
        }

        internal DateTime TimeStamp
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Sender"></param>
        /// <param name="Contents"></param>
        /// <returns></returns>
        public static DialogueMessage Generate(MongoObjectId Id, MongoObjectId Sender, String Contents)
        {
            DialogueMessage result = new DialogueMessage();
            result.Id = Id;
            result.Sender = Sender;
            result.Contents = Contents;
            result.TimeStamp = Id.CreationTime;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Contents"></param>
        /// <returns></returns>
        public static DialogueMessage Generate(String Contents)
        {
            DialogueMessage result = new DialogueMessage();
            result.Contents = Contents;
            result.TimeStamp = DateTime.Now;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.Id = MongoObjectId.Empty;
            this.Sender = MongoObjectId.Empty;
            this.Contents = String.Empty;
            this.TimeStamp = DateTime.MinValue;
        }
    }
}
