using Agent;
using CommunicationServer;
using GameMasterPresentation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace GameplayMockupTests
{
    public class Tests
    {
        private static string csConfigFilePath = @"communicationServerConfig.json";
        const int agentsInTeam = 1;
        const int agentSleepMs = 16;

        [Test]
        public void RunMockup()
        {
            //configure gm thread
            var gmThread = new Thread(() =>
            {
                new App(); //this is magic, but absolutely neceessary because it sets Application.Current
                var window = new MainWindow();
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();

                System.Windows.Threading.Dispatcher.Run();
            });
            gmThread.SetApartmentState(ApartmentState.STA);

            //configure cs thread
            var csThread = new Thread(() =>
            {
                CommunicationServer.CommunicationServer server = new CommunicationServer.CommunicationServer(csConfigFilePath);
                server.Run();
            });

            var agentsThreads = new List<Thread>();
            for (int i = 0; i < agentsInTeam * 2; i++)
            {
                agentsThreads.Add(new Thread(() => {
                    var agent = new Agent.Agent(new AgentConfiguration {
                        CsIP = "127.0.0.1",
                        CsPort = 54321,
                        TeamID = i < agentsInTeam ? "red" : "blue",
                        WantsToBeTeamLeader = i % agentsInTeam == 0
                    });

                    agent.ConnectToCommunicationServer();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    ActionResult actionResult = ActionResult.Continue;
                    while (actionResult == ActionResult.Continue)
                    {
                        stopwatch.Stop();
                        var timeElapsed = stopwatch.Elapsed.TotalSeconds;
                        stopwatch.Reset();
                        stopwatch.Start();

                        actionResult = agent.Update(timeElapsed);
                        Thread.Sleep(agentSleepMs);
                    }

                    agent.OnDestroy();
                }));
            }

            csThread.Start();
            gmThread.Start();

            Thread.Sleep(3000);
            foreach (var t in agentsThreads)
                t.Start();

            gmThread.Join();
        }
    }
}