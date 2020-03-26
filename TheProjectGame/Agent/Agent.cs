using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public Point boardSize;

        public int goalAreaSize;

        public Point position;

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

        public void Move(Direction direction) { /*if distance=0 send pick up request*/ }

        public void Put() { }

        public void BegForInfo() { /*teammate that was not asked before or last asked*/ }

        public void GiveInfo() { /*to first waiting player*/ }

        public void CheckPiece() { /*if sham send destroy request*/ }

        public void Discover() { }

        public void AcceptMessage() { }

        public void MakeDecisionFromStrategy() { }

    }
}
