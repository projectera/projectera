using System;

namespace ProjectERA.Editors.TilesetEditor
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            using (TilesetEditor game = new TilesetEditor())
            {
                game.Run();
            }
        }
    }
#endif
}

