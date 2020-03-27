﻿using System;
using Messaging.Enumerators;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameMaster
{
    public class Agent
    {
        public int Id { get; private set; }
        public TeamId Team { get; private set; }
        public Point Position { get; set; }
        public bool IsTeamLeader { get; private set; } = false;
        public double Timeout { get; private set; } = 0;
        public Piece Piece { get; private set; } = null;

        private ExchangeInformationState exchangeInformationState = ExchangeInformationState.None;

        public Agent(int id, TeamId team, Point position, bool isTeamLeader = false)
        {
            Id = id;
            Team = team;
            Position = position;
            IsTeamLeader = isTeamLeader;
        }

        public void Update(double dt)
        {
            Timeout = Math.Max(Timeout - dt, 0.0);
        }

        public void PickUpPiece(Piece p)
        {
            Piece = p;
        }

        public Piece RemovePiece()
        {
            var p = Piece;
            Piece = null;
            return p;
        }

        public bool CanPerformAction()
        {
            return Timeout <= 0;
        }

        public void AddTimeout(double value)
        {
            Timeout += value;
        }

        public void InformationExchangeRequested(bool wasTeamLeader)
        {
            exchangeInformationState = wasTeamLeader ? ExchangeInformationState.Obligated : ExchangeInformationState.Eligible;
        }

        public void ClearExchangeState()
        {
            exchangeInformationState = ExchangeInformationState.None;
        }

        public bool HaveToExchange()
        {
            return exchangeInformationState == ExchangeInformationState.Obligated;
        }

        public bool CanExchange()
        {
            return exchangeInformationState == ExchangeInformationState.Obligated || exchangeInformationState == ExchangeInformationState.Eligible;
        }

        private enum ExchangeInformationState
        {
            None,
            Eligible,
            Obligated
        }
    }
}
