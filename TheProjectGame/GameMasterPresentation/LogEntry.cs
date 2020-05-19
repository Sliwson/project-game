using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace GameMasterPresentation
{
    public class LogEntry : INotifyPropertyChanged
    {
        private string _Message;

        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                _Message = value;
                NotifyPropertyChanged();
            }
        }

        private SolidColorBrush _color;

        public SolidColorBrush Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LogEntry(string text)
        {
            Message = text;
            var trimmedText = Message.Trim();
            string substr = "";
            if (trimmedText.Length >= 19)
                substr = trimmedText.Substring(15, 4);
            switch (substr)
            {
                case "Warn":
                    Color = new SolidColorBrush(Colors.Orange);
                    return;

                case "Erro":
                    Color = new SolidColorBrush(Colors.Red);
                    return;

                default:
                    Color = new SolidColorBrush(Colors.Black);
                    break;
            }
            substr = trimmedText.Substring(trimmedText.IndexOf('-') + 1);
            substr = substr.Trim();
            if (substr.Length >= 4)
                substr = substr.Substring(0, 4);
            switch (substr)
            {
                case "[GM]":
                    Color = new SolidColorBrush(Colors.Black);
                    break;

                case "[CS]":
                    Color = new SolidColorBrush(Colors.Purple);
                    break;

                case "[Con"://Connection
                    Color = new SolidColorBrush(Colors.DodgerBlue);
                    break;

                case "[Age"://Agent
                    Color = new SolidColorBrush(Colors.LightSlateGray);
                    break;

                case "[Hos"://HostMapping
                    Color = new SolidColorBrush(Colors.DarkMagenta);
                    break;

                case "[Net"://NetworkComponent
                    Color = new SolidColorBrush(Colors.LightSkyBlue);
                    break;

                case "[Cli"://ClientNetworkComponent
                    Color = new SolidColorBrush(Colors.LightSkyBlue);
                    break;

                case "[Boa"://Board
                    Color = new SolidColorBrush(Colors.Navy);
                    break;

                case "[Sco"://Score
                    Color = new SolidColorBrush(Colors.ForestGreen);
                    break;

                case "[Log"://Logic
                    Color = new SolidColorBrush(Colors.Teal);
                    break;
            }
        }
    }
}