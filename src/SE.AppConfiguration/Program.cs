using SE.AppConfiguration.Core;
using System;

namespace SE.AppConfiguration
{
    public class Program
    {
        public static int Main(string[] args)
        {
            SatelliteResolver.RegisterResolveEventHandler();

            try
            {
                var start = new Startup();
                return start.RunParser(args);
            }
            finally
            {
                SatelliteResolver.UnregisterResolveEventHandler();
                Console.WriteLine("Hello World!");
            }
        }
    }
}
