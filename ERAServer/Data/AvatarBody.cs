using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data
{
    [Serializable]
    internal class AvatarBody
    {

        public Byte Opacity, GraphicHue;
        public Byte SkintoneId,
            HairtypeId, HaircolorId,
            EyetypeId, EyecolorId;
        public String GraphicName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static AvatarBody Generate()
        {
            AvatarBody result = new AvatarBody();
            result.Opacity = 255;
            result.GraphicHue = 0;
            result.SkintoneId = 0;
            result.HairtypeId = 0;
            result.HaircolorId = 0;
            result.EyetypeId = 0;
            result.EyecolorId = 0;
            result.GraphicName = String.Empty;

            return result;
        }
    }
}
