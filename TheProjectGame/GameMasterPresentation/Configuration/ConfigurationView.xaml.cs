using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            parentWindow.ContentGrid.Children.Add(new ConfigurationEdit());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this) as ConfigurationWindow;
            if (parentWindow == null)
                ;//TODO: error something
        }
    }
}
