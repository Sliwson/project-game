using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LogEntry(string text)
        {
            Message = text;
        }
    }
}
