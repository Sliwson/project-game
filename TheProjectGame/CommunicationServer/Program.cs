using System;

namespace CommunicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            CommunicationServer server = new CommunicationServer();

            server.Run();
        }
    }
}
