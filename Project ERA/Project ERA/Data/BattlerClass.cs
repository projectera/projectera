using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERAUtils;
using Microsoft.Xna.Framework.Content;

namespace ProjectERA.Data
{
    [Serializable]
    internal class BattlerClass
    {
        /// <summary>
        /// Id
        /// </summary>
        public MongoObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Content Id
        /// </summary>
        [ContentSerializer(ElementName="Id")]
        public Int32 DatabaseId
        {
            get;
            private set;
        }

        /// <summary>
        /// Class Name
        /// </summary>
        [ContentSerializer()]
        public Int32 ParentId
        {
            get;
            private set;
        }

        /// <summary>
        /// Description
        /// </summary>
        [ContentSerializer()]
        public String Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Class Name
        /// </summary>
        [ContentSerializer()]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [ContentSerializer()]
        public List<TalentTree> TalentTrees
        {
            get;
            set;
        }

        private BattlerClass()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static BattlerClass Generate(Int32 id, String name, Int32 parentId, List<TalentTree> talentTree = null)
        {
            return Generate(id, name, String.Empty, parentId, talentTree);
        }

        /// <summary>
        /// Generates a new class
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static BattlerClass Generate(Int32 id, String name, String description, Int32 parentId, List<TalentTree> talentTree = null)
        {
            BattlerClass result = new BattlerClass();
            result.DatabaseId = id;
            result.Name = name;
            result.Description = description;
            result.ParentId = parentId;
            result.TalentTrees = talentTree ?? new List<TalentTree>();

            return result;
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(String description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(params String[] description)
        {
            SetDescription(String.Join(" ", description));
        }
    }
}
