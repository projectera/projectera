using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.IO;

namespace ProjectERA.Services.Data.Serialization
{
    public class SerializableDatabaseContent<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        protected void Serialize(String path)
        {
            using (System.IO.IsolatedStorage.IsolatedStorageFile isf = System.IO.IsolatedStorage.IsolatedStorageFile.GetMachineStoreForDomain())
            {
                // Create Directories if needed
                if (!isf.DirectoryExists(@"Data/Database"))
                {
                    isf.CreateDirectory(@"Data/Database");
                }

                // Save settings
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                // Save to file
                using (System.IO.IsolatedStorage.IsolatedStorageFileStream isfs = isf.OpenFile(path, FileMode.Create))
                {

                    using (XmlWriter writer = XmlWriter.Create(isfs, settings))
                    {
                        IntermediateSerializer.Serialize(writer, this, null);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected static T Deserialize(String path)
        {
            T result = default(T);

            using (System.IO.IsolatedStorage.IsolatedStorageFile isf = System.IO.IsolatedStorage.IsolatedStorageFile.GetMachineStoreForDomain())
            {
                // Create Directories if needed
                if (!isf.DirectoryExists(@"Data/Database"))
                {
                    isf.CreateDirectory(@"Data/Database");
                }

                // Load file
                if (isf.FileExists(path))
                {

                    // Read from file
                    using (System.IO.IsolatedStorage.IsolatedStorageFileStream isfs = isf.OpenFile(path, FileMode.Open))
                    {

                        using (XmlReader reader = XmlReader.Create(isfs))
                        {
                            try
                            {
                                result = IntermediateSerializer.Deserialize<T>(reader, null);
                            }
                            catch(Exception
#if DEBUG && NOFAILSAFE                                
                                e) {

                                throw e;
#else
                                 ) {
#endif
                            }
                        } 
                    }
                }
      
            }

            return result;
        }


    }
}
