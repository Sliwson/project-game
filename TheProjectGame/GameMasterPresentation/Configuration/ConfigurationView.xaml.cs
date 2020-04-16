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

        }
    }
}