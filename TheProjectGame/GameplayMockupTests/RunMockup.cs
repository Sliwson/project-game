using GameMasterPresentation;
using NUnit.Framework;
using System.Threading;

namespace GameplayMockupTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RunMockup()
        {
            var gmThread = new Thread(() =>
            {
                var window = new MainWindow();
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();

                System.Windows.Threading.Dispatcher.Run();
            });

            gmThread.SetApartmentState(ApartmentState.STA);
            gmThread.Start();

            gmThread.Join();
        }
    }
}