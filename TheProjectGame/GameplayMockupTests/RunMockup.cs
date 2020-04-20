using CommunicationServer;
using GameMasterPresentation;
using NUnit.Framework;
using System.Threading;
using System.Windows;

namespace GameplayMockupTests
{
    public class Tests
    {
        private static string csConfigFilePath = @"communicationServerConfig.json";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RunMockup()
        {
            var gmThread = new Thread(() =>
            {
                new App(); //this is magic, but absolutely neceessary because it sets Application.Current
                var window = new MainWindow();
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();

                System.Windows.Threading.Dispatcher.Run();
            });

            gmThread.SetApartmentState(ApartmentState.STA);


            var csThread = new Thread(() =>
            {
                CommunicationServer.CommunicationServer server = new CommunicationServer.CommunicationServer(csConfigFilePath);
                server.Run();
            });

            csThread.Start();
            gmThread.Start();

            gmThread.Join();
        }
    }
}