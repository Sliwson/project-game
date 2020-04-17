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

        public Configuration ConfigCopy
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

        public ConfigurationEdit(Configuration configuration)
        {
            ConfigCopy = configuration.Clone();
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this) as ConfigurationWindow;
            if (parentWindow == null)
                ;//TODO: error something
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SaveToFileButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            if (ConfigCopy != parentWindow.Config)
            {
                var result = MessageBox.Show("Do you want to exit without saving?", "Configuration", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            parentWindow.ContentGrid.Children.Clear();
            parentWindow.ContentGrid.Children.Add(new ConfigurationView());
        }
    }
}