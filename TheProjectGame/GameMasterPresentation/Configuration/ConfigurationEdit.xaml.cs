using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace GameMasterPresentation.Configuration
{
    /// <summary>
    /// Interaction logic for ConfigurationEdit.xaml
    /// </summary>
    public partial class ConfigurationEdit : UserControl, INotifyPropertyChanged
    {
        private ConfigurationWindow parentWindow;
        private Configuration _config;

        public Configuration Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConfigurationEdit()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this) as ConfigurationWindow;
            if (parentWindow == null)
                ;//TODO: error something
        }
    }
}