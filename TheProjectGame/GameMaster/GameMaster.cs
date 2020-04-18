using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Messaging.Contracts;
using GameMaster.Interfaces;
using Messaging.Enumerators;
using Messaging.Communication;

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

        private GameMasterState state = GameMasterState.Configuration;
        private IMessageProcessor currentMessageProcessor = null;

        public GameMaster()
        {
            Logger.Get().Info("[GM] Creating GameMaster");
            LoadDefaultConfiguration();

            ConnectionLogic = new ConnectionLogicComponent(this);
            GameLogic = new GameLogicComponent(this);
            ScoreComponent = new ScoreComponent(this);
            BoardLogic = new BoardLogicComponent(this, new Point(Configuration.BoardX, Configuration.BoardY));
            PresentationComponent = new PresentationComponent(this);
            NetworkComponent = new ClientNetworkComponent(Configuration.CsIP, Configuration.CsPort);
        }

        public void SetNetworkConfiguration(/*network configuration*/) { }
        public void SetBoardConfiguration(/*board configuration*/) { }
        public void SetAgentsConfiguartion(/*agents configuration*/) { }

        public void ApplyConfiguration()
        {
            //if ok start accepting agents
            state = GameMasterState.ConnectingAgents;
            currentMessageProcessor = ConnectionLogic;
        }

        public void ConnectToCommunicationServer()
        {
            if (!NetworkComponent.Connect())
                throw new ApplicationException("Unable to connect to CS");
        }

        public void StartGame()
        {
            if (!ConnectionLogic.CanStartGame())
            {
                Logger.Get().Error("[GM] Start game conditions not met!");
                return;
            }

            Agents = ConnectionLogic.FlushLobby();
            state = GameMasterState.InGame;
            currentMessageProcessor = GameLogic;
            BoardLogic.GenerateGoals();

            //TODO: send
            Logger.Get().Info("[GM] Starting game with {count} agents", Agents.Count);
            GameLogic.GetStartGameMessages();
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
            if (state == GameMasterState.Configuration || state == GameMasterState.Summary)
                return;

            if (state == GameMasterState.InGame)
                BoardLogic.Update(dt);
            
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
                    //TODO: send response
                }
                NLog.NestedDiagnosticsContext.Pop();
            }

            var result = ScoreComponent.GetGameResult();
            if (result != Enums.GameResult.None)
            {
                state = GameMasterState.Summary;

                //TODO: send
                Logger.Get().Info("[GM] Ending game");
                GameLogic.GetEndGameMessages(result == Enums.GameResult.BlueWin ? TeamId.Blue : TeamId.Red);
            }
        }

        public Agent GetAgent(int agentId)
        {
            return Agents.FirstOrDefault(a => a.Id == agentId);
        }

        public void OnDestroy()
        {
            Logger.OnDestroy();
            NetworkComponent.Disconnect();
        }

        //TODO: move to messaging system
#if DEBUG
        private List<BaseMessage> injectedMessages = new List<BaseMessage>();

        public void InjectMessage(BaseMessage message)
        {
            injectedMessages.Add(message);
        }
#endif

        private List<BaseMessage> GetIncomingMessages()
        {
#if DEBUG
            var clone = new List<BaseMessage>(injectedMessages);
            injectedMessages.Clear();
            return clone;
#endif
            return NetworkComponent.GetIncomingMessages().ToList();
        }

        private void LoadDefaultConfiguration()
        {
            Logger.Get().Info("[GM] Loading default configuration");

            var configurationProvider = new MockConfigurationProvider();
            Configuration = configurationProvider.GetConfiguration();
        }
    }
}
