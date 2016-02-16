using System;
using System.Windows;
using MIOConfig.PresentationLayer;

namespace MIOConfigurator.Windows
{
    /// <summary>
    /// Логика взаимодействия для DevicesFinderConfigWindow.xaml
    /// </summary>
    public partial class DevicesFinderConfigWindow : Window
    {
        DeviceFinder _finder;
        public DevicesFinderConfigWindow()
        {
            InitializeComponent();
        }

        public DevicesFinderConfigWindow(DeviceFinder finder)
        {
            InitializeComponent();
            _finder = finder;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchAllAdresses.IsChecked == true)
            {
                _finder.StartSlaveAddress = 1;
                _finder.EndSlaveAddress = 247;                
            }
            else if (SearchConcreteAdress.IsChecked == true)
            {
                _finder.StartSlaveAddress = Convert.ToByte((string) SlaveAddress.Text);
                _finder.EndSlaveAddress = Convert.ToByte((string) SlaveAddress.Text);                                
            }
            else if (SearchByAdresses.IsChecked == true)            
            {
                _finder.StartSlaveAddress = Convert.ToByte((string) StartSlaveAddress.Text);
                _finder.EndSlaveAddress = Convert.ToByte((string) EndSlaveAddress.Text);    
            }
            if (SerchOnlyConcreteVersion.IsChecked == true)
            {
                _finder.TargetVersion = Convert.ToUInt16((string) ConcreteVersion.Text);
            }
            DialogResult = true;       
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
