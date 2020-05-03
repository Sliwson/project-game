using System;
using System.IO;

namespace CommunicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = CommunicationServerConfiguration.GetDefault();

            Console.WriteLine("Do you want to load configuration from file? [Y]/[*]");
            ConsoleKey key = Console.ReadKey().Key;
            Console.WriteLine();

            if (key == ConsoleKey.Y)
            {
                Console.WriteLine("Enter path: ");
                var line = Console.ReadLine();
                var newConfig = CommunicationServerConfiguration.LoadFromFile(line);
                if (newConfig != null)
                    config = newConfig;
            }

            CommunicationServer server = new CommunicationServer(config);
            server.Run();
        }
    }
}
