using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using Lidgren.Network;
using ERAServer.Services;
using System.Threading;

namespace ERAServer.Data
{
    /// <summary>
    /// Dialogue
    /// </summary>
    [BsonIgnoreExtraElements]
    internal class Dialogue
    {
        public const Int32 DialogueSize = 100; // 100 page/dialogue * 25 msg/page = 2500 msg/dialogue

        /// <summary>
        /// Dialogue Id
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Active participants
        /// </summary>
        [BsonRequired]
        public HashSet<ObjectId> Participants
        {
            get;
            private set;
        }

        /// <summary>
        /// Pages in the dialogue
        /// </summary>
        [BsonRequired]
        public List<DialoguePage> Pages
        {
            get;
            private set;
        }

        /// <summary>
        /// Dialogue Creation
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
        public ObjectId? FollowUp
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a new Dialogue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static Dialogue Generate(ObjectId sender, ObjectId receiver, String message)
        {
            DialogueMessage dialogueMessage = DialogueMessage.Generate(sender, message);

            Dialogue result = new Dialogue();
            result.Id = ObjectId.GenerateNewId();
            result.Participants = new HashSet<ObjectId>() { receiver, sender };
            result.Participants.Remove(ObjectId.Empty);
            result.Pages = new List<DialoguePage>() { DialoguePage.Generate(result.Id, dialogueMessage) };
            result.FollowUp = null;

            return result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="participants"></param>
        /// <returns></returns>
        internal static Dialogue Generate(DialogueMessage message, HashSet<ObjectId> participants)
        {
            Dialogue result = new Dialogue();
            result.Id = ObjectId.GenerateNewId();
            result.Participants = participants;
            result.Pages = new List<DialoguePage>() { DialoguePage.Generate(result.Id, message) };
            result.FollowUp = null;

            return result;
        }

        /// <summary>
        /// Adds a message to the conversation
        /// </summary>
        /// <param name="message"></param>
        internal void AddMessage(DialogueMessage message)
        {
            lock (this.Pages)
            {
                if (this.Pages[this.Pages.Count - 1].AddMessage(message))
                    return;
                //}

                // Page limit reached? Page count can only increase so no need to lock
                if (this.Pages.Count() >= DialogueSize)
                {
                    if (this.FollowUp == null)
                    {
                        ArchiveDialogue(message);
                        return;
                    }

                    // Get FollowUp and append
                    Dialogue.Get(this.FollowUp ?? ObjectId.Empty).
                        ContinueWith(result =>
                        {
                            if (result != null) { result.Result.AddMessage(message); }
                        });
                    return;
                }

                this.Pages.Add(DialoguePage.Generate(this.Id, message));

                // Adds page
                GetCollection().Update(Query.EQ("_id", this.Id), Update.AddToSet("Pages", this.Pages[this.Pages.Count - 1].ToBsonDocument<DialoguePage>())); 
            
            }

            /*DialoguePage lastPage;
            FindAndModifyResult atomicResult;

            // First create a new page with the message
            DialoguePage newPage = DialoguePage.Generate(this.Id, message);

            GetCollection().Update(Query.EQ("_id", this.Id), 
                Update.AddToSet("Pages", newPage.ToBsonDocument<DialoguePage>()));     

            // Now mark this page and try to put it as follow up page
            lock(this.Pages)
            {
                lastPage = this.Pages[this.Pages.Count - 1];
            
                atomicResult = Dialogue.GetCollection().FindAndModify(
                    Query.And(Query.EQ("_id", this.Id), Query.EQ("Pages._id", lastPage.Id), Query.EQ("Pages.$.FollowUp", BsonNull.Value)), 
                    SortBy.Null, Update.Set("Pages.$.FollowUp", newPage.Id));
            }

            // If follow up page was already set
            if (!atomicResult.Ok)
            {
                // Remove the local follow up page
                Dialogue.GetCollection().Update(Query.EQ("_id", this.Id), 
                    Update.Pull("Pages._id", newPage.Id));

                // Get pages from the database
                lock (this.Pages)
                {
                    this.Pages = Dialogue.GetCollection().FindOneById(this.Id).Pages;
                    lastPage = this.Pages[this.Pages.Count - 1];
                }

                // Add message to assumed last page (recursively when failure)
                lastPage.AddMessage(message);
            }
            else
            {
                lock (this.Pages)
                {
                    // Follow up page now updates, so set it
                    lastPage.FollowUp = newPage.Id;
                    this.Pages.Add(newPage);
                }
            }

            // Adds page
            //GetCollection().Update(Query.EQ("_id", this.Id), Update.AddToSet("Pages", this.Pages[this.Pages.Count - 1].ToBsonDocument<DialoguePage>())); 
             */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        internal void AddMessage(ObjectId sender, String message)
        {
            AddMessage(DialogueMessage.Generate(sender, message));
        }

        /// <summary>
        /// Adds a participant to the conversation
        /// </summary>
        /// <param name="id"></param>
        internal void AddParticipant(ObjectId id)
        {
            // Updates collection
            lock (this.Participants)
            {
                this.Participants = (GetCollection().
                    FindAndModify(
                        Query.EQ("_id", this.Id), 
                        SortBy.Null, Update.AddToSet("Participants", id), true).
                        GetModifiedDocumentAs<Dialogue>()
                    ).Participants;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        internal void ArchiveDialogue(DialogueMessage message)
        {
            Dialogue replacement = Dialogue.Generate(message, this.Participants );
            replacement.Put();

            FindAndModifyResult atomicResult = Dialogue.GetCollection().FindAndModify(
                Query.And(Query.EQ("_id", this.Id), Query.EQ("FollowUp", BsonNull.Value)), 
                SortBy.Null, Update.Set("FollowUp", replacement.Id));

            if (!atomicResult.Ok)
            {
                Dialogue.GetCollection().Remove(Query.EQ("_id", replacement.Id));
                Dialogue.Get(this.Id).
                    ContinueWith(result =>
                    {
                        this.FollowUp = result.Result.FollowUp;
                        this.AddMessage(message);
                    });
            }
            else
            {
                this.FollowUp = replacement.Id;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialogueMessageId"></param>
        /// <param name="readerId"></param>
        /// <returns></returns>
        internal void MarkAsRead(ObjectId dialogueMessageId, ObjectId readerId, Boolean asRead = true)
        {
            DialoguePage[] pagesCopy = null;

            lock (this.Pages)
            {
                pagesCopy = new DialoguePage[this.Pages.Count];
                this.Pages.CopyTo(pagesCopy);
            }

            foreach (DialoguePage page in pagesCopy)
            {
                // Read whole page
                if (page.Id == dialogueMessageId)
                {
                    foreach (DialogueMessage message in page.Contents)
                    {
                        if (asRead)
                        {
                            message.MarkAsRead(readerId);
                        }
                        else
                        {
                            message.MarkAsUnread(readerId);
                        }
                    }

                    break;
                }

                // Read single message
                DialogueMessage readMessage = page.Contents.Find(message => message.Id == dialogueMessageId);

                if (readMessage != null)
                {
                    if (asRead)
                    {
                        readMessage.MarkAsRead(readerId);
                    }
                    else
                    {
                        readMessage.MarkAsUnread(readerId);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialogueMessageId"></param>
        /// <param name="readerId"></param>
        internal void MarkAsUnread(ObjectId dialogueMessageId, ObjectId readerId)
        {
            MarkAsRead(dialogueMessageId, readerId, false);
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
        public static Task<Dialogue> Get(ObjectId id)
        {
            return Task<Dialogue>.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Task<Dialogue[]> GetFor(ObjectId id)
        {
            return Task<Dialogue[]>.Factory.StartNew(() => { return GetBlockingFor(id); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Dialogue GetBlocking(ObjectId id)
        {
            Dialogue result = GetCollection().FindOneById(id);

            foreach (DialoguePage page in result.Pages)
                foreach (DialogueMessage message in page.Contents)
                    message.Parent = page;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Dialogue[] GetBlockingFor(ObjectId id, Boolean archived = false, Boolean unread = true)
        {
            QueryComplete query = Query.EQ("Participants", id);

            if (!archived)
            {
                QueryComplete innerQuery = Query.EQ("FollowUp", BsonNull.Value);

                if (unread)
                {
                    innerQuery = Query.Or(innerQuery, Query.ElemMatch("Pages", Query.NotIn("Contents.Read", id)));
                }

                query = Query.And(query, innerQuery);
            }

            Dialogue[] result = GetCollection().Find(query).ToArray();

            foreach (Dialogue dialogue in result)
                foreach (DialoguePage page in dialogue.Pages)
                    foreach (DialogueMessage message in page.Contents)
                        message.Parent = page;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Dialogue> GetCollection()
        {
            return DataManager.Database.GetCollection<Dialogue>("Players.Dialogues");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public NetOutgoingMessage Pack(ref NetOutgoingMessage msg)
        {
            msg.Write(this.Id.ToByteArray()); // 12
            msg.Write(this.Participants.Count); // 16
            foreach (var participant in this.Participants)
                msg.Write(participant.ToByteArray()); // 16 + 12 * x
            msg.Write(this.Pages.Count()); // 20 + 12 * x
            foreach (var page in this.Pages)
                msg.Write(page.Id.ToByteArray()); // 20 + 12 * x + 12 * y
            msg.Write(this.TimeStamp.ToBinary()); // 28 + 12 * x + 12 * y

            return msg;
        }
    }
}
