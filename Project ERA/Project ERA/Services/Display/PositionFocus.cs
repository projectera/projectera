using Microsoft.Xna.Framework;

namespace ProjectERA.Services.Display
{
    /// <summary>
    /// 
    /// </summary>
    public class PositionFocus : IFocusable
    {
        public Vector3 Position
        {
            get;
            private set;
        }

        public PositionFocus(Vector3 focus)
        {
            this.Position = focus;
        }
    }

}
