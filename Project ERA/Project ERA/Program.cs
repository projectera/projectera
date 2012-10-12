using System;

namespace ProjectERA
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            using (Client game = new Client())
            {
                 game.Run();
            }
        }
    }
#endif
}

