using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;

namespace GameMasterPresentation.Configuration
{
    public class Configuration : INotifyPropertyChanged
    {
        //board
        private int _boardX;

        public int BoardX
        {
            get
            {
                return _boardX;
            }
            set
            {
                _boardX = value;
                NotifyPropertyChanged();
            }
        }

        private int _boardY;

        public int BoardY
        {
            get
            {
                return _boardY;
            }
            set
            {
                _boardY = value;
                NotifyPropertyChanged();
            }
        }

        private int _goalAreaHeight;

        public int GoalAreaHeight
        {
            get
            {
                return _goalAreaHeight;
            }
            set
            {
                _goalAreaHeight = value;
                NotifyPropertyChanged();
            }
        }

        private int _numberOfGoals;

        public int NumberOfGoals
        {
            get
            {
                return _numberOfGoals;
            }
            set
            {
                _numberOfGoals = value;
                NotifyPropertyChanged();
            }
        }

        //game
        private int _teamSize;

        public int TeamSize
        {
            get
            {
                return _teamSize;
            }
            set
            {
                _teamSize = value;
                NotifyPropertyChanged();
            }
        }

        private int _numberOfPieces;

        public int NumberOfPieces
        {
            get
            {
                return _numberOfPieces;
            }
            set
            {
                _numberOfPieces = value;
                NotifyPropertyChanged();
            }
        }

        private float _shamProbability;

        public float ShamProbability
        {
            get
            {
                return _shamProbability;
            }
            set
            {
                _shamProbability = value;
                NotifyPropertyChanged();
            }
        }

        //network
        private IPAddress _CSAddress;

        public IPAddress CSAddress
        {
            get
            {
                return _CSAddress;
            }
            set
            {
                _CSAddress = value;
                NotifyPropertyChanged();
            }
        }

        private int _CSPort;

        public int CSPort
        {
            get
            {
                return _CSPort;
            }
            set
            {
                _CSPort = value;
                NotifyPropertyChanged();
            }
        }

        //penalties
        private TimeSpan _movePenalty;

        public TimeSpan MovePenalty
        {
            get
            {
                return _movePenalty;
            }
            set
            {
                _movePenalty = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _informationExchangePenalty;

        public TimeSpan InformationExchangePenalty
        {
            get
            {
                return _informationExchangePenalty;
            }
            set
            {
                _informationExchangePenalty = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _discoveryPenalty;

        public TimeSpan DiscoveryPenalty
        {
            get
            {
                return _discoveryPenalty;
            }
            set
            {
                _discoveryPenalty = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _putPenalty;

        public TimeSpan PutPenalty
        {
            get
            {
                return _putPenalty;
            }
            set
            {
                _putPenalty = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _checkForShamPenalty;

        public TimeSpan CheckForShamPenalty
        {
            get
            {
                return _checkForShamPenalty;
            }
            set
            {
                _checkForShamPenalty = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _destroyPiecePenalty;

        public TimeSpan DestroyPiecePenalty
        {
            get
            {
                return _destroyPiecePenalty;
            }
            set
            {
                _destroyPiecePenalty = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}