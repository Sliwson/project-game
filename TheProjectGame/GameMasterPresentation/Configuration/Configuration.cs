using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

namespace GameMasterPresentation.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration : INotifyPropertyChanged
    {
        //board
        private int _boardX;

        [JsonProperty(PropertyName = "boardX", Order = 8)]
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

        [JsonProperty(PropertyName = "boardY", Order = 9)]
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

        [JsonProperty(PropertyName = "goalAreaHeight", Order = 10)]
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

        [JsonProperty(PropertyName = "numberOfGoals", Order = 11)]
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

        [JsonProperty(PropertyName = "teamSize", Order = 13)]
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

        [JsonProperty(PropertyName = "numberOfPieces", Order = 12)]
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

        [JsonProperty(PropertyName = "shamPieceProbability", Order = 14)]
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

        [JsonProperty(PropertyName = "CsIP", Order = 0)]
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

        [JsonProperty(PropertyName = "CsPort", Order = 1)]
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

        [JsonProperty(PropertyName = "movePenalty", Order = 2)]
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

        [JsonProperty(PropertyName = "informationExchangePenalty", Order = 3)]
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

        [JsonProperty(PropertyName = "discoveryPenalty", Order = 4)]
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

        [JsonProperty(PropertyName = "putPenalty", Order = 5)]
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

        [JsonProperty(PropertyName = "checkForShamPenalty", Order = 6)]
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

        [JsonProperty(PropertyName = "destroyPiecePenalty", Order = 7)]
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

        public void CopyFrom(Configuration conf)
        {
            BoardX = conf.BoardX;
            BoardY = conf.BoardY;
            GoalAreaHeight = conf.GoalAreaHeight;
            NumberOfGoals = conf.NumberOfGoals;
            TeamSize = conf.TeamSize;
            NumberOfPieces = conf.NumberOfPieces;
            ShamProbability = conf.ShamProbability;
            CSAddress = conf.CSAddress;
            CSPort = conf.CSPort;
            MovePenalty = conf.MovePenalty;
            InformationExchangePenalty = conf.InformationExchangePenalty;
            DiscoveryPenalty = conf.DiscoveryPenalty;
            PutPenalty = conf.PutPenalty;
            CheckForShamPenalty = conf.CheckForShamPenalty;
            DestroyPiecePenalty = conf.DestroyPiecePenalty;
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

        public bool Validate()
        {
            if (BoardX < 1)
                return false;
            if (BoardY < 1)
                return false;
            if (2 * GoalAreaHeight >= BoardY)
                return false;
            if (NumberOfGoals < 1)
                return false;
            if (TeamSize < 1)
                return false;
            if (NumberOfPieces < 1)
                return false;
            if (ShamProbability < 0 || ShamProbability > 1)
                return false;
            if (IPAddress.TryParse(CSAddress, out _) == false)
                return false;
            if (CSPort < 49152 || CSPort > 65535)
                return false;
            if (MovePenalty < 0)
                return false;
            if (InformationExchangePenalty < 0)
                return false;
            if (DiscoveryPenalty < 0)
                return false;
            if (PutPenalty < 0)
                return false;
            if (CheckForShamPenalty < 0)
                return false;
            if (DestroyPiecePenalty < 0)
                return false;
            return true;
        }

        public GameMaster.GameMasterConfiguration ConvertToGMConfiguration()
        {
            var conf = new GameMaster.GameMasterConfiguration()
            {
                BoardX = this.BoardX,
                BoardY = this.BoardY,
                GoalAreaHeight = this.GoalAreaHeight,
                NumberOfGoals = this.NumberOfGoals,
                TeamSize = this.TeamSize,
                NumberOfPieces = this.NumberOfPieces,
                ShamProbability = this.ShamProbability,
                CsIP = this.CSAddress,
                CsPort = this.CSPort,
                MovePenalty = TimeSpan.FromMilliseconds(this.MovePenalty),
                InformationExchangePenalty = TimeSpan.FromMilliseconds(this.InformationExchangePenalty),
                DiscoveryPenalty = TimeSpan.FromMilliseconds(this.DiscoveryPenalty),
                PutPenalty = TimeSpan.FromMilliseconds(this.PutPenalty),
                CheckForShamPenalty = TimeSpan.FromMilliseconds(this.CheckForShamPenalty),
                DestroyPiecePenalty = TimeSpan.FromMilliseconds(this.DestroyPiecePenalty)
            };
            return conf;
        }

        public bool SaveToFile(string path)
        {
            // serialize JSON directly to a file
            try
            {
                using (StreamWriter file = File.CreateText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, this);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static Configuration ReadFromFile(string path)
        {
            //TODO: validate using schema
            //var resolver = new JSchemaUrlResolver();
            //var schema = JSchema.Parse(ConfigurationSchema.GetConfigurationSchema(), resolver);
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception)
            {
                return null;
            }
            //JObject jObject = JObject.Parse(json);
            //bool isValid = jObject.IsValid(schema);
            //if(isValid==false)
            //{
            //    return null;
            //}

            Configuration conf = JsonConvert.DeserializeObject<Configuration>(json);
            //if (conf.Validate() == false)
            //    return null;
            return conf;
        }

        public static bool operator ==(Configuration a, Configuration b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (ReferenceEquals(a, null))
            {
                return false;
            }
            if (ReferenceEquals(b, null))
            {
                return false;
            }

            if (a.BoardX != b.BoardX)
                return false;
            if (a.BoardY != b.BoardY)
                return false;
            if (a.GoalAreaHeight != b.GoalAreaHeight)
                return false;
            if (a.NumberOfGoals != b.NumberOfGoals)
                return false;
            if (a.TeamSize != b.TeamSize)
                return false;
            if (a.NumberOfPieces != b.NumberOfPieces)
                return false;
            if (a.ShamProbability != b.ShamProbability)
                return false;
            if (a.CSAddress != b.CSAddress)
                return false;
            if (a.CSPort != b.CSPort)
                return false;
            if (a.MovePenalty != b.MovePenalty)
                return false;
            if (a.InformationExchangePenalty != b.InformationExchangePenalty)
                return false;
            if (a.DiscoveryPenalty != b.DiscoveryPenalty)
                return false;
            if (a.PutPenalty != b.PutPenalty)
                return false;
            if (a.CheckForShamPenalty != b.CheckForShamPenalty)
                return false;
            if (a.DestroyPiecePenalty != b.DestroyPiecePenalty)
                return false;
            return true;
        }

        public static bool operator !=(Configuration a, Configuration b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var conf = obj as Configuration;
            if (conf == null) //this won't be null
                return false;
            return this == conf;
        }

        public override int GetHashCode()
        {
            //maybe think of sth better
            return base.GetHashCode();
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
                CSPort = 50000,
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