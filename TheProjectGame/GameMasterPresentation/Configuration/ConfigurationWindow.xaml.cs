using System;
using System.ComponentModel;
using System.Linq;
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
            Config = configuration;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ContentGrid.Children.Add(new ConfigurationView());
        }

        private void ConfigurationWindow_Closing(object sender, CancelEventArgs e)
        {
            var editWindows = ContentGrid.Children.OfType<ConfigurationEdit>();
            if (editWindows.Any())
            {
                if (editWindows.First().ConfigCopy != Config)
                {
                    var result = MessageBox.Show("Do you want to exit without saving?", Constants.MessageBoxName, MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }
    }
}