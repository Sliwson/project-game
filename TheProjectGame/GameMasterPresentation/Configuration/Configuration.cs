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
        private string _CSAddress;

        public string CSAddress
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

        //penalties in milliseconds
        private int _movePenalty;

        public int MovePenalty
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

        private int _informationExchangePenalty;

        public int InformationExchangePenalty
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

        private int _discoveryPenalty;

        public int DiscoveryPenalty
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

        private int _putPenalty;

        public int PutPenalty
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

        private int _checkForShamPenalty;

        public int CheckForShamPenalty
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

        private int _destroyPiecePenalty;

        public int DestroyPiecePenalty
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

        public Configuration Clone()
        {
            var conf = new Configuration()
            {
                BoardX = this.BoardX,
                BoardY = this.BoardY,
                GoalAreaHeight = this.GoalAreaHeight,
                NumberOfGoals = this.NumberOfGoals,
                TeamSize = this.TeamSize,
                NumberOfPieces = this.NumberOfPieces,
                ShamProbability = this.ShamProbability,
                CSAddress = this.CSAddress,
                CSPort = this.CSPort,
                MovePenalty = this.MovePenalty,
                InformationExchangePenalty = this.InformationExchangePenalty,
                DiscoveryPenalty = this.DiscoveryPenalty,
                PutPenalty = this.PutPenalty,
                CheckForShamPenalty = this.CheckForShamPenalty,
                DestroyPiecePenalty = this.DestroyPiecePenalty
            };
            return conf;
        }

        public static Configuration ReadFromFile(string path)
        {
            return null;
        }

        //TODO: delete
        public static Configuration MockConfiguration()
        {
            var conf = new Configuration()
            {
                BoardX = 5,
                BoardY = 10,
                GoalAreaHeight = 3,
                NumberOfGoals = 4,
                TeamSize = 2,
                NumberOfPieces = 10,
                ShamProbability = 0.3f,
                CSAddress = "192.168.1.1",
                CSPort = 10023,
                MovePenalty = 1000,
                InformationExchangePenalty = 700,
                DiscoveryPenalty = 600,
                PutPenalty = 500,
                CheckForShamPenalty = 400,
                DestroyPiecePenalty = 300
            };
            return conf;
        }
    }
}