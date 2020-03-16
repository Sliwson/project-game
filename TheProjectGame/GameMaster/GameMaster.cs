using System;
using System.Drawing;
using System.Collections.Generic;

namespace GameMaster
{
    public class GameMaster
    {
        private GameMasterState state = GameMasterState.Configuration;
        private List<Agent> agents = new List<Agent>();
        private BoardLogicComponent boardLogicComponent;

        public GameMaster()
        {
            LoadDefaultConfiguration();

            //create board with deafult parameters
            boardLogicComponent = new BoardLogicComponent(new Point(5, 10));

            //try to connect to communciation server
        }

        public void SetNetworkConfiguration(/*network configuration*/) { }
        public void SetBoardConfiguration(/*board configuration*/) { }
        public void SetAgentsConfiguartion(/*agents configuration*/) { }

        public void ApplyConfiguration()
        {
            //if ok start accepting agents
            state = GameMasterState.ConnectingAgents;
        }

        public void StartGame()
        {
            state = GameMasterState.InGame;
        }

        public void PauseGame()
        {
            state = GameMasterState.Paused;
        }

        //called from window system each frame, updates all components
        public void Update()
        {

        }

        private void LoadDefaultConfiguration()
        {
            
        }
    }
}
