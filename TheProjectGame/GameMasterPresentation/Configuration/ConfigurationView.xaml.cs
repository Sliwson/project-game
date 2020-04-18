using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GameMasterPresentation.Configuration
{
    /// <summary>
    /// Interaction logic for ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : UserControl
    {
        private ConfigurationWindow parentWindow;

        public ConfigurationView()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.ContentGrid.Children.Clear();
            parentWindow.ContentGrid.Children.Add(new ConfigurationEdit(parentWindow.Config));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this) as ConfigurationWindow;
            if (parentWindow == null)
                ;//TODO: error something
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Game Master Configuration File";
            openFileDialog.Filter = "JSON Files (*.json)|*.json";

            string configurationPath = Constants.ConfigurationDirectoryPath;

            openFileDialog.InitialDirectory = Path.GetFullPath(configurationPath);
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                var conf = Configuration.ReadFromFile(filename);
                if(conf == null)
                {
                    MessageBox.Show("There was problem reading json file!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    MessageBox.Show("Configuration read successfully!", Constants.MessageBoxName, MessageBoxButton.OK, MessageBoxImage.Information);
                    parentWindow.Config = conf;
                }
            }
        }
    }
}