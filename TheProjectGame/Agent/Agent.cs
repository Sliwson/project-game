using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Agent
{
    public class Agent
    {
        private int id;

        private ISender sender;

        private IStrategy strategy;

        public int penaltyTime;

        public Team team;

        public bool isLeader;

        public Field[,] board;

        public (int, int) boardSize;

        public int goalAreaSize;

        public (int, int) position;

        public List<int> waitingPlayers;

        public int[] teamMates;

        public Piece piece;

        public Agent() { }

        private void Communicate() { }

        private void Penalty() 
        {
            Thread.Sleep(penaltyTime);
        }

        public void JoinTheGame() { }

        public void Start() { }

        public void Stop() { }

        public void Move(Direction direction) { }

        public void Put() { }

        public void BegForInfo() { }

        public void GiveInfo() { }

        public void CheckPiece() { }

        public void Discover() { }

        public void AcceptMessage() { }

        public void MakeDecisionFromStrategy() { }

    }
}
