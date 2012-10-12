using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace ProjectERA.Data
{
    [Serializable]
    internal class BattlerRace
    {
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
        /// Description
        /// </summary>
        [ContentSerializer()]
        public String Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Name
        /// </summary>
        [ContentSerializer()]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Effects
        /// </summary>
        [ContentSerializer()]
        public BattlerValues EquipmentBase
        {
            get;
            private set;
        }

        private BattlerRace()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="equipmentBase"></param>
        /// <returns></returns>
        internal static BattlerRace Generate(Int32 id, String name, BattlerValues equipmentBase)
        {
            return Generate(id, name, String.Empty, equipmentBase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="equipmentBase"></param>
        /// <returns></returns>
        internal static BattlerRace Generate(Int32 id, String name, String description, BattlerValues equipmentBase)
        {
            BattlerRace result = new BattlerRace();
            result.DatabaseId = id;
            result.Name = name;
            result.Description = description;
            result.EquipmentBase = equipmentBase;

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
