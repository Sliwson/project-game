﻿using GameMaster.Interfaces;
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

        private GameMasterState state = GameMasterState.Configuration;
        private IMessageProcessor currentMessageProcessor = null;

        public GameMaster(GameMasterConfiguration configuration = null)
        {
            Logger.Get().Info("[GM] Creating GameMaster");

            if (configuration == null)
                LoadDefaultConfiguration();
            else
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
                Logger.Get().Error("[GM] Cannot Set Configuration");
            }
        }

        public void ApplyConfiguration()
        {
            NetworkComponent = new ClientNetworkComponent(Configuration.CsIP, Configuration.CsPort);

            //we should connect to cs after setting configuration
            //try to connect to communciation server (if connection is not successful throw exception)

            ConnectToCommunicationServer();

            //if ok start accepting agents
            state = GameMasterState.ConnectingAgents;
            currentMessageProcessor = ConnectionLogic;
        }

        public void ConnectToCommunicationServer()
        {
            if (!NetworkComponent.Connect(ClientType.GameMaster))
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
            BoardLogic.StartGame();

            Logger.Get().Info("[GM] Starting game with {count} agents", Agents.Count);
            var messages = GameLogic.GetStartGameMessages();
            foreach (var m in messages)
                NetworkComponent.SendMessage(m);
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
                    NetworkComponent.SendMessage(response);
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
                    NetworkComponent.SendMessage(m);
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

        //TODO (#IO-39): move to messaging system
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