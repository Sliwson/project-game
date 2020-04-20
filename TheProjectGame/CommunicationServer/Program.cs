using System;
using System.IO;

namespace CommunicationServer
{
    class Program
    {
        private static string configFilePath = @"communicationServerConfig.json";

        static void Main(string[] args)
        {
            ConsoleKey loadConfigChoice;
            while(true)
            {
                Console.Write($"Do you want to load configuration from file: {Path.GetFullPath(configFilePath)}? [y]/[n]: ");
                loadConfigChoice = Console.ReadKey().Key;
                Console.WriteLine("\n");

                if (loadConfigChoice == ConsoleKey.N)
                {
                    configFilePath = null;
                    break;
                }
                else if (loadConfigChoice == ConsoleKey.Y)
                {
                    break;
                }
            }

            CommunicationServer server = new CommunicationServer(configFilePath);
            server.Run();
        }
    }
}
