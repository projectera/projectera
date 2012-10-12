using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading.Tasks;
using System.Net.Mail;
using ERAUtils.Enum;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using ERAServer.Properties;
using ERAUtils;
using ERAServer.Services;
using Lidgren.Network.Authentication;

namespace ERAServer.Data
{
    [Serializable]
    internal class Player : IResetable// Struct gives VerificationError Operation Could destabilize w/e
    {
        /// <summary>
        /// Player ID (readonly)
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Forum ID
        /// </summary>
        public UInt16 ForumId
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Player Username
        /// </summary>
        /// <remarks>Can be changed through ChangeUsername()</remarks>
        [BsonRequired]
        public String Username
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Player Logon Verifier
        /// </summary>
        [BsonRequired]
        public Byte[] Verifier;

        /// <summary>
        /// Player Logon Salt
        /// </summary>
        [BsonRequired]
        public Byte[] Salt;

        /// <summary>
        /// Players Email Adress
        /// <remarks>System.Net.MailAddress is not Serializble</remarks>
        /// </summary>
        [BsonRequired]
        public String EmailAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Players Permission Group
        /// </summary>
        public PermissionGroup PermissionGroup
        {
            get;
            private set;
        }

        /// <summary>
        /// List of avatar ids for player
        /// </summary>
        public HashSet<ObjectId> AvatarIds
        {
            get;
            private set;
        }

        /// <summary>
        /// List of dialogues
        /// </summary>
        [BsonIgnore]
        public Dictionary<ObjectId, Dialogue> Dialogues
        {
            get;
            private set;
        }

        /// <summary>
        /// Banned Reason (not banned if empty)
        /// </summary>
        [BsonIgnoreIfNull]
        public String BannedReason
        {
            get;
            private set;
        }

        /// <summary>
        /// Banned Flag (not stored)
        /// </summary>
        [BsonIgnore]
        public Boolean IsBanned
        {
            get { return !String.IsNullOrEmpty(this.BannedReason); }
        }

        /// <summary>
        /// Friends
        /// </summary>
        [BsonRequired]
        public HashSet<Friend> Friends
        {
            get;
            private set;
        }

        /// <summary>
        /// Registration Time
        /// </summary>
        [BsonIgnore]
        public DateTime RegistrationDate
        {
            get { return this.Id.CreationTime; }
        }


        public Int32 ValidationLinkCode;
        public Int32 ValidationMailCode;

        /// <summary>
        /// Generates a player
        /// </summary>
        /// <remarks>To store the newly created player call Put() on the player object</remarks>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <param name="mailAdress">mailadress</param>
        /// <returns>Player object</returns>
        internal static Player Generate(String username, String password, String mailAdress)
        {
            Player result = Player.GetBlocking(username) ?? new Player();
            if (result.Id.Equals(ObjectId.Empty) == false)
                throw new Exception("User already exists");

            // Set player values
            result.Id = ObjectId.GenerateNewId();
            result.ForumId = 0;
            result.Username = username;
            result.EmailAddress = mailAdress;

            // Set default values
            result.PermissionGroup = PermissionGroup.Registered;
            result.AvatarIds = new HashSet<ObjectId>();
            result.Friends = new HashSet<Friend>();
            result.BannedReason = null;
            result.Dialogues = new Dictionary<ObjectId, Dialogue>();

            // Generate Salt and Verifier
            result.Verifier = Handshake.PasswordVerifier(result.Username.ToLower(), password, Settings.Default.SRP6Keysize, out result.Salt).ToByteArray(); //NetPeer.PasswordVerifier(result.Username.ToLower(), password, Settings.Default.SRP6Keysize, out result.Salt).ToByteArray();

            return result;
        }

        internal static Player Generate(String username, Byte[] verifier, Byte[] salt)
        {
            Player result = Player.GetBlocking(username) ?? new Player();
            if (result.Id.Equals(ObjectId.Empty) == false)
                throw new Exception("User already exists");

            // Set player values
            result.Id = ObjectId.GenerateNewId();
            result.ForumId = 0;
            result.Username = username;
            result.EmailAddress = String.Empty;

            // Set default values
            result.PermissionGroup = PermissionGroup.None;
            result.AvatarIds = new HashSet<ObjectId>();
            result.Friends = new HashSet<Friend>();
            result.BannedReason = null;
            result.Dialogues = new Dictionary<ObjectId, Dialogue>();

            // Generate Salt and Verifier
            result.Verifier = verifier;
            result.Salt = salt;

            return result;
        }

        /// <summary>
        /// Bans user with reason
        /// </summary>
        /// <param name="reason"></param>
        /// <returns>Update task</returns>
        internal Task<Boolean> Ban(String reason)
        {
            // Update locally
            if (String.IsNullOrEmpty(reason))
                reason = "No reason specified";
            this.BannedReason = reason;

            // Update remotely
            ObjectId updateId = this.Id;
            
            return Task.Factory.StartNew(() =>
            {
                // Second, try updating the username to the new username
                SafeModeResult sfr = GetCollection().Update(Query.EQ("_id", updateId), Update.Set("BannedReason", reason), UpdateFlags.None, SafeMode.True);
                // Return succession flag
                return sfr.UpdatedExisting;

            });
        }

        /// <summary>
        /// Changes Username of player
        /// </summary>
        /// <remarks>Even though locally changes always succeeded, you should inspect the Task.Result value to see if the update succeeded remotely</remarks>
        /// <param name="username">new username</param>
        /// <returns>Update Task</returns>
        internal Task<Boolean> ChangeUsername(String username)
        {
            // Copy old value and update (local) value
            String oldname = this.Username;
            this.Username = username;

            // Start updating
            return Task.Factory.StartNew(() =>
            {
                // First, find a player with the new username
                Player player = GetCollection().FindOne(Query.EQ("Username", username));
                // If we found one, we can not change to this username
                if (!player.Id.Equals(ObjectId.Empty))
                    return false;
                // Second, try updating the username to the new username
                SafeModeResult sfr = GetCollection().Update(Query.EQ("Username", oldname), Update.Set("Username", username), SafeMode.True);
                // Return succession flag
                return sfr.UpdatedExisting;
            });
        }

        /// <summary>
        /// Changes Email of Player
        /// </summary>
        /// <param name="email">new Email</param>
        /// <returns></returns>
        internal Task<Boolean> ChangeEmail(String email)
        {
            // Copy old value and update (local) value
            String oldmail = this.EmailAddress;
            this.EmailAddress = email;

            ObjectId updateId = this.Id;

            // Start updating
            return Task.Factory.StartNew(() =>
            {
                ValidationMailCode = NetRandom.Instance.NextInt();

                FindAndModifyResult fmr = GetCollection().FindAndModify(
                    Query.And(
                        Query.EQ("_id", updateId), 
                        Query.EQ("EmailAddress", oldmail)), 
                    SortBy.Null, 
                    Update.Set("EmailAddress", email).Set("ValidationMailCode", ValidationMailCode), 
                    true, false);
                return fmr.Ok;
            }).ContinueWith<Boolean>((task) =>
            {
                try
                {
                    MailMessage message = new MailMessage(new MailAddress("derk-jan@projectera.org", "Derk-Jan [ProjectERA]"), new MailAddress(this.EmailAddress, this.Username + " [ProjectERA]"));
                    message.IsBodyHtml = true;
                    message.Subject = "Email address linked to ProjectERA account";

                    // HTML Body (remove HTML tags for plain text).
                    message.Body = "<HTML><BODY><B>Hello World! " + ValidationMailCode + "</B></BODY></HTML>";

                    SmtpClient smtp = new SmtpClient("mail.direct-adsl.nl");
                    //smtp.SendCompleted += new SendCompletedEventHandler(smtp_SendCompleted);
                    smtp.SendAsync(message, this.Id);
                }
                catch (Exception)
                {

                }

                return task.Result;
            });

            // TODO: Sent Confirmation Email
        }

        /// <summary>
        /// Links Forum account with Player
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        internal Task<Boolean> LinkAccount(UInt16 forumId)
        {
            // Copy old value and update (local) value
            ObjectId updateId = this.Id;

            // Start updating
            return Task.Factory.StartNew(() =>
            {
                ValidationLinkCode = NetRandom.Instance.NextInt();

                FindAndModifyResult fmr = GetCollection().FindAndModify(
                    Query.EQ("_id", updateId),
                    SortBy.Null,
                    Update.Set("ForumId", forumId).Set("ValidationLinkCode", ValidationLinkCode),
                    true, false);
                return fmr.Ok;
            }).ContinueWith<Boolean>((task) =>
            {
                try
                {
                    MailMessage message = new MailMessage(new MailAddress("derk-jan@projectera.org", "Derk-Jan [ProjectERA]"), new MailAddress(this.EmailAddress, this.Username + " [ProjectERA]"));
                    message.IsBodyHtml = true;
                    message.Subject = "Forum account linked to ProjectERA account";

                    // HTML Body (remove HTML tags for plain text).
                    message.Body = "<HTML><BODY><B>Hello World! " + ValidationLinkCode + "</B></BODY></HTML>";

                    SmtpClient smtp = new SmtpClient("mail.direct-adsl.nl");
                    //smtp.SendCompleted += new SendCompletedEventHandler(smtp_SendCompleted);
                    smtp.SendAsync(message, this.Id);
                }
                catch (Exception)
                {

                }

                return task.Result;
            });
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smtp_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            
        }

        /// <summary>
        /// Elevates the player to another permissionGroup
        /// </summary>
        /// <param name="permissionGroup">new permissiongroup</param>
        /// <returns>Update task</returns>
        internal Task<Boolean> Elevate(PermissionGroup permissionGroup)
        {
            this.PermissionGroup = permissionGroup;

            // Update remotely
            ObjectId updateId = this.Id;

            return Task<Boolean>.Factory.StartNew(() =>
            {
                // Second, try updating the username to the new username
                SafeModeResult sfr = GetCollection().Update(Query.EQ("_id", updateId), Update.Set("PermissionGroup", permissionGroup), UpdateFlags.None, SafeMode.True);
                // Return succession flag
                return sfr.UpdatedExisting;

            });
        }

        /// <summary>
        /// Gets a player from the db
        /// </summary>
        /// <param name="id">id of player to get</param>
        /// <returns></returns>
        internal static Task<Player> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a player from the db and recursive data
        /// </summary>
        /// <param name="id">id of player to get</param>
        /// <returns></returns>
        internal static Task<Player> GetRecursive(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); }).ContinueWith<Player>((task) =>
                {
                    task.Result.Dialogues = new Dictionary<ObjectId, Dialogue>();
                    Dialogue[] retrieved = Dialogue.GetBlockingFor(task.Result.Id);
                    foreach (var dialogue in retrieved)
                        task.Result.Dialogues.Add(dialogue.Id, dialogue);

                    return task.Result;
                });
        }

        /// <summary>
        /// Gets a player from the db
        /// </summary>
        /// <param name="username">username of player to get</param>
        /// <returns></returns>
        internal static Task<Player> Get(String username)
        {
            return Task.Factory.StartNew(()=> { return GetBlocking(username); });
        }

        /// <summary>
        /// Gets a player from the db,  blocks while retrieving
        /// </summary>
        /// <param name="id">id of player to get</param>
        /// <returns></returns>
        internal static Player GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Player;
        }

        /// <summary>
        /// Gets a player from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">username of player to get</param>
        /// <returns></returns>
        internal static Player GetBlocking(String username)
        {
            return GetCollection().FindOneAs<Player>(Query.Matches("Username", new BsonRegularExpression("^(?i)"+username+"$"))) as Player;
        }

        /// <summary>
        /// Gets the players collection
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Player> GetCollection()
        {
            return DataManager.Database.GetCollection<Player>("Players");
        }

        /// <summary>
        /// Puts a player to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a player to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Player>(this, safemode);
        }

        /// <summary>
        /// Clears for pool
        /// </summary>
        public void Clear()
        {
            this.Id = ObjectId.Empty;
            this.ForumId = 0;
            this.Username = String.Empty;
            this.EmailAddress = String.Empty;

            this.PermissionGroup = 0;
            this.AvatarIds.Clear();

            this.BannedReason = null;
            this.Verifier = null;
            this.Salt = null;
        }
    }
}
