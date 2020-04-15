using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GameMasterPresentation.Configuration
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window, INotifyPropertyChanged
    {
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

        public ConfigurationWindow(Configuration configuration)
        {
            InitializeComponent();
            Config = configuration;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ContentGrid.Children.Add(new ConfigurationView());
        }
    }
}