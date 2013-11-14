using System;

namespace Kaibo_Crawler
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
#if NETFX_CORE
        [MTAThread]
#else
        [STAThread]
#endif
        static void Main()
        {
            using (var program = new Kaibo_Crawler())
                program.Run();

        }
    }
}