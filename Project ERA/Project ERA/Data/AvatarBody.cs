using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;

namespace ProjectERA.Data
{
    internal class AvatarBody : IResetable
    {
        #region Private fields

        private Byte _opacity, _graphicHue;
        private Byte _skintoneId,
            _hairtypeId, _haircolorId,
            _eyetypeId, _eyecolorId;
        private String _graphicName;

        #endregion

        #region Properties

        /// <summary>
        /// Skintone Id
        /// </summary>
        internal Byte SkintoneId
        {
            get { return _skintoneId; }
            set { _skintoneId = value; }
        }

        /// <summary>
        /// Hair Type Id
        /// </summary>
        internal Byte HairtypeId
        {
            get { return _hairtypeId; }
            set { _hairtypeId = value; }
        }

        /// <summary>
        /// Hair Color Id
        /// </summary>
        internal Byte HaircolorId
        {
            get { return _haircolorId; }
            set { _haircolorId = value; }
        }

        /// <summary>
        /// Eye Type id
        /// </summary>
        internal Byte EyetypeId
        {
            get { return _eyetypeId; }
            set { _eyetypeId = value; }
        }

        /// <summary>
        /// Eye Color id
        /// </summary>
        internal Byte EyecolorId
        {
            get { return _eyecolorId; }
            set { _eyecolorId = value; }
        }

        /// <summary>
        /// Body Opacity
        /// </summary>
        internal Byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        /// <summary>
        /// Graphics Hue
        /// </summary>
        internal Byte GraphicHue
        {
            get { return _graphicHue; }
            set { _graphicHue = value; }
        }

        /// <summary>
        /// Graphics Assetname
        /// </summary>
        internal String GraphicName
        {
            get { return _graphicName; }
            set { _graphicName = value; }
        }

        #endregion

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public AvatarBody()
        {

        }

        /// <summary>
        /// Constructor with predefined data
        /// </summary>
        /// <param name="skintoneId">Skin tone ID</param>
        /// <param name="hairtypeId">Hair type ID</param>
        /// <param name="haircolorId">Hair color ID</param>
        /// <param name="eyetypeId">Eye type ID</param>
        /// <param name="eyecolorId">Eye color ID</param>
        /// <param name="opacity">Opacity</param>
        /// <param name="graphicsHue">Graphics Hue</param>
        /// <param name="graphicsName">Graphics Name</param>
        internal AvatarBody(Byte skintoneId, Byte hairtypeId, Byte haircolorId, Byte eyetypeId, Byte eyecolorId,
            Byte opacity, Byte graphicsHue, String graphicsName)
        {
            this.SkintoneId = skintoneId;
            this.HairtypeId = hairtypeId;
            this.HaircolorId = haircolorId;
            this.EyecolorId = eyecolorId;
            this.EyetypeId = eyetypeId;
            this.Opacity = opacity;
            this.GraphicHue = graphicsHue;
            this.GraphicName = graphicsName;
        }

        /// <summary>
        /// Reset Body
        /// </summary>
        public void Clear()
        {
            this.SkintoneId = 0;
            this.HairtypeId = 0;
            this.HaircolorId = 0;
            this.EyetypeId = 0;
            this.EyecolorId = 0;
            this.Opacity = 0;
            this.GraphicHue = 0;
            this.GraphicName = String.Empty;
        }
    }
}
