using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace ProjectERA.Services.Data.Storage
{
    public class TitleStorageDevice : IStorageDevice
    {

        #region IStorageDevice Members

        /// <summary>
        /// 
        /// </summary>
        public bool IsReady
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="saveAction"></param>
        public void Save(String containerName, string fileName, FileAction saveAction)
        {
            throw new NotSupportedException("Can not save to the title container.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="loadAction"></param>
        public void Load(String containerName, String fileName, FileAction loadAction)
        {
            loadAction.Invoke(TitleContainer.OpenStream(String.Join(@"\", containerName, fileName)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        public void Delete(String containerName, String fileName)
        {
            throw new NotSupportedException("Can not delete from the title container.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Boolean FileExists(String containerName, String fileName)
        {
            try
            {
                using (Stream stream = TitleContainer.OpenStream(String.Join(@"\", containerName, fileName)))
                {
                    return true;
                }
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public String[] GetFiles(String containerName)
        {
            throw new NotSupportedException("Can not access directory listings in the title container.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public String[] GetFiles(String containerName, String pattern)
        {
            throw new NotImplementedException("Can not access directory listings in the title container.");
        }

        #endregion
    }
}
