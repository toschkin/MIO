using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MIOConfig.PresentationLayer;

namespace MIOConfigurator
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
                _finder.StartSlaveAddress = Convert.ToByte(SlaveAddress.Text);
                _finder.EndSlaveAddress = Convert.ToByte(SlaveAddress.Text);                                
            }
            else if (SearchByAdresses.IsChecked == true)            
            {
                _finder.StartSlaveAddress = Convert.ToByte(StartSlaveAddress.Text);
                _finder.EndSlaveAddress = Convert.ToByte(EndSlaveAddress.Text);    
            }
            if (SerchOnlyConcreteVersion.IsChecked == true)
            {
                _finder.TargetVersion = Convert.ToUInt16(ConcreteVersion.Text);
            }
            DialogResult = true;       
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
