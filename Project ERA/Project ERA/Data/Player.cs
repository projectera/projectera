using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Data.Update;
using ERAUtils;

namespace ProjectERA.Data
{
    internal class Player : Changable, IResetable
    {

        #region Private fields

        private MongoObjectId _id;
        private UInt16 _forumId;
        private String _name;
        private String _email;
        private List<Interactable> _avatars;
        private Interactable _activeAvatar;

        private Dialogue _privateDialogue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets Player Username
        /// </summary>
        internal String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets/Sets Player ID
        /// </summary>
        internal MongoObjectId UserId
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Gets/Sets Player Forum ID
        /// </summary>
        internal UInt16 ForumId
        {
            get { return _forumId; }
            set { _forumId = value; }
        }

        /// <summary>
        /// Gets/Sets Player Email
        /// </summary>
        internal String Email
        {
            get { return _email; }
            set { _email = value; }
        }

        /// <summary>
        /// Gets/Sets Available Characters
        /// </summary>
        internal List<Interactable> Avatars
        {
            get { return _avatars; }
            set { _avatars = value; }
        }

        /// <summary>
        /// Gets/Sets Active Character
        /// </summary>
        internal Interactable ActiveAvatar
        {
            set { _activeAvatar = value; }
            get { return _activeAvatar; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal Dialogue PrivateDialogue
        {
            get { return _privateDialogue; }
            set { _privateDialogue = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Player()
        {
            this.Avatars = new List<Interactable>(3);
            this.PrivateDialogue = new Dialogue();
        }

        /// <summary>
        /// Clear all Data
        /// </summary>
        public void Clear()
        {
            // Cleans basic values
            this.UserId = MongoObjectId.Empty;
            this.ForumId = 0;
            this.Name = String.Empty;
            this.Email = String.Empty;

            // Cleans avatars
            for (Int32 i = 0; i < this.Avatars.Count; i++)
            {
                Pool<Interactable>.Recycle(this.Avatars[i]);
            }

            // Cleans list
            this.Avatars.Clear();
            this.PrivateDialogue.Clear();
        }
    }
}