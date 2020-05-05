using GameMaster.Interfaces;
using Messaging.Communication;
using Messaging.Contracts;
using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameMaster
{
    public class GameMaster
    {
        public BoardLogicComponent BoardLogic { get; private set; }
        public ConnectionLogicComponent ConnectionLogic { get; private set; }
        public GameLogicComponent GameLogic { get; private set; }
        public List<Agent> Agents { get; private set; } = new List<Agent>();

        public GMLogger Logger { get; private set; } = new GMLogger();
        public ScoreComponent ScoreComponent { get; private set; }
        public GameMasterConfiguration Configuration { get; private set; }
        public PresentationComponent PresentationComponent { get; private set; }
        public INetworkComponent NetworkComponent { get; private set; }

        public GameMasterState state { get; private set; } = GameMasterState.Configuration;
        private IMessageProcessor currentMessageProcessor = null;

        public GameMaster(GameMasterConfiguration configuration)
        {
            Logger.Get().Info("[GM] Creating GameMaster");

            Configuration = configuration;
            ConnectionLogic = new ConnectionLogicComponent(this);
            GameLogic = new GameLogicComponent(this);
            ScoreComponent = new ScoreComponent(this);
            BoardLogic = new BoardLogicComponent(this);
            PresentationComponent = new PresentationComponent(this);
        }

        public void SetConfiguration(GameMasterConfiguration configuration)
        {
            if (state == GameMasterState.Configuration)
            {
                Configuration = configuration;
                ScoreComponent.LoadNewConfiguration();
                BoardLogic.LoadNewConfiguration();
            }
            else
            {
                Logger.Get().Error("[GM] Cannot Set Configuration, because GM is not in configuration state.");
            }
        }

        public void ConnectToCommunicationServer()
        {
            NetworkComponent = new ClientNetworkComponent(Configuration.CsIP, Configuration.CsPort);
            if (!NetworkComponent.Connect(ClientType.GameMaster))
                throw new ApplicationException("Unable to connect to CS");

            Logger.Get().Info("[GM] Connected to Communication Server");
            state = GameMasterState.ConnectingAgents;
            currentMessageProcessor = ConnectionLogic;
        }

        public bool StartGame()
        {
            if (!ConnectionLogic.CanStartGame())
            {
                Logger.Get().Error("[GM] Start game conditions not met!");
                return false;
            }

            Agents = ConnectionLogic.FlushLobby();
            state = GameMasterState.InGame;
            currentMessageProcessor = GameLogic;
            BoardLogic.StartGame();

            Logger.Get().Info("[GM] Starting game with {count} agents", Agents.Count);
            var messages = GameLogic.GetStartGameMessages();
            foreach (var m in messages)
                SendMessage(m);

            return true;
        }

        public void PauseGame()
        {
            state = GameMasterState.Paused;

            //TODO: send
            Logger.Get().Info("[GM] Pausing game");
            GameLogic.GetPauseMessages();
        }

        public void ResumeGame()
        {
            state = GameMasterState.InGame;

            //TODO: send
            Logger.Get().Info("[GM] Resuming game");
            GameLogic.GetResumeMessages();
        }

        //called from window system each frame, updates all components
        public void Update(double dt)
        {
            if (state != GameMasterState.Configuration && NetworkComponent?.Exception != null)
                throw NetworkComponent.Exception;

            if (state == GameMasterState.Configuration || state == GameMasterState.Summary)
                return;

            foreach (var agent in Agents)
                agent.Update(dt);

            var messages = GetIncomingMessages();
            if (messages.Count > 0)
            {
                Logger.Get().Info("[GM] Processing {n} messages", messages.Count);
                NLog.NestedDiagnosticsContext.Push("    ");
                foreach (var message in messages)
                {
                    var response = currentMessageProcessor.ProcessMessage(message);
                    SendMessage(response);
                }
                NLog.NestedDiagnosticsContext.Pop();
            }

            var result = ScoreComponent.GetGameResult();
            if (result != Enums.GameResult.None)
            {
                state = GameMasterState.Summary;

                Logger.Get().Info("[GM] Ending game");
                var resultMessages = GameLogic.GetEndGameMessages(result == Enums.GameResult.BlueWin ? TeamId.Blue : TeamId.Red);
                foreach (var m in resultMessages)
                    SendMessage(m);
            }
        }

        public Agent GetAgent(int agentId)
        {
            return Agents.FirstOrDefault(a => a.Id == agentId);
        }

        public void OnDestroy()
        {
            try
            {
                NetworkComponent?.Disconnect();
            }
            catch (Exception ex)
            {
                Logger.Get().Error("[GM] {error}", ex.Message);
            }

            Logger.OnDestroy();
        }

        public void SendMessage(BaseMessage message)
        {
            try
            {
                NetworkComponent.SendMessage(message);
            }
            catch (CommunicationErrorException e)
            {
                if (e.Type == CommunicationExceptionType.InvalidSocket)
                {
                    // TODO: Should terminate
                }

                Console.WriteLine(e.Message);
            }
        }

        //TODO (#IO-57): Move to mocked tests
#if DEBUG
        private List<BaseMessage> injectedMessages = new List<BaseMessage>();

        public void InjectMessage(BaseMessage message)
        {
            injectedMessages.Add(message);
        }

#endif

        private List<BaseMessage> GetIncomingMessages()
        {
            var clone = new List<BaseMessage>(injectedMessages);
            injectedMessages.Clear();

            //TODO: refactor
            return clone.Concat(NetworkComponent.GetIncomingMessages().ToList()).ToList();
        }
    }
}