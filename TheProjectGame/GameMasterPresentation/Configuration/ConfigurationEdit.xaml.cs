using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Linq;
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
            InitializeComponent();

            ConfigCopy = configuration.Clone();
        }

        private bool Validate()
        {
            return ValidateGrid(BoardGrid) &&
                ValidateGrid(GameGrid) &&
                ValidateGrid(NetworkGrid) &&
                ValidateGrid(PenaltiesGrid);
        }

        private bool ValidateGrid(Grid grid)
        {
            foreach (var textbox in grid.Children.OfType<TextBox>())
            {
                if (Validation.GetHasError(textbox))
                    return false;
            }
            return true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this) as ConfigurationWindow;
            if (parentWindow == null)
                ;//TODO: error something
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                if (ConfigCopy.Validate())
                {
                    ConfigCopy.FileName = "";
                    parentWindow.Config.CopyFrom(ConfigCopy);
                    MessageBox.Show("Configuration saved!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Form contains errors!\n2 * GoalAreaHeight cannot be larger than BoardHeight!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Form contains errors!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void SaveToFileButtonClick(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                string configurationPath = Constants.ConfigurationDirectoryPath;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON File|*.json";
                saveFileDialog.Title = "Save Game Master Configuration File";
                saveFileDialog.FileName = "Untitled";
                saveFileDialog.InitialDirectory = System.IO.Path.GetFullPath(configurationPath);
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (saveFileDialog.FileName != "")
                    {
                        if (ConfigCopy.SaveToFile(saveFileDialog.FileName) == true)
                        {
                            MessageBox.Show("Configuration saved!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("There was problem saving configuration!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Form contains errors!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            if (ConfigCopy != parentWindow.Config)
            {
                var result = MessageBox.Show(parentWindow,"Do you want to exit without saving?", Constants.MessageBoxName, MessageBoxButton.YesNo, MessageBoxImage.Information);
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