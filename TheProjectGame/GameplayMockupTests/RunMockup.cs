using Agent;
using GameMasterPresentation;
using IntegrationTests;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace GameplayMockupTests
{
    public class Tests
    {
        const int agentsInTeam = 1;
        const int agentSleepMs = 16;

        //[Ignore("This test is made only for running and debugging game setup with all components")]
        [Test]
        public void RunMockup()
        {
            var gmThread = CreateGmThread();
            var csThread = IntegrationTestsHelper.CreateCsThread();
            
            csThread.Start();
            gmThread.Start();

            Thread.Sleep(3000); //time for connecting gm with cs

            var agents = IntegrationTestsHelper.CreateAgents(agentsInTeam);
            foreach (var agent in agents)
            {
                var agentThread = new Thread(RunAgent);
                agentThread.IsBackground = true; //background threads for termination at test exit
                agentThread.Start(agent);
            }

            gmThread.Join();
            Environment.Exit(Environment.ExitCode); //second nail for preventing thread garbage
        }

        private Thread CreateGmThread()
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
            return gmThread;
        }

        private void RunAgent(object o)
        {
            var agent = o as Agent.Agent;
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
        }
    }
}