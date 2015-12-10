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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MIOConfig.PresentationLayer;
using Modbus.Core;
using MIOConfigurator;

namespace MIOConfigurator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModbusRtuProtocol protocol;
        private DeviceFinder finder;
        public MainWindow()
        {
            InitializeComponent();   
            if(Registry.CurrentUser.OpenSubKey(Constants.registryAppNode) == null)
                Registry.CurrentUser.CreateSubKey(Constants.registryAppNode);
            protocol = new ModbusRtuProtocol();
            finder = new DeviceFinder(protocol);            
        }

        private void CmdConnect_OnClick(object sender, RoutedEventArgs e)
        {
            ProtocolConfigWindow connectionConfigWindow = new ProtocolConfigWindow(protocol);            
            connectionConfigWindow.Owner = this;
            if (connectionConfigWindow.ShowDialog() == true)
            {                
                cmdConnect.IsEnabled = false;
                cmdDisconnect.IsEnabled = true;               
                СonnectionStatus.Text = "Подключено: "+protocol.GetConnectionParametersString();
            }            
        }

        private void CmdDisconnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (protocol.IsConnected)
                protocol.Disconnect();
            cmdConnect.IsEnabled = true;
            cmdDisconnect.IsEnabled = false;
            СonnectionStatus.Text = "Отключено";            
            //Todo need to reset list
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBoxResult.No ==
                MessageBox.Show("Выйти из программы?", Constants.messageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question))
                e.Cancel = true;
        }

        private void CmdFindDevices_OnClick(object sender, RoutedEventArgs e)
        {
            DevicesFinderConfigWindow finderConfigWindow = new DevicesFinderConfigWindow(finder);
            finderConfigWindow.Owner = this;
            if (finderConfigWindow.ShowDialog() == true)
            {
                //todo start searching
            }
        }
    }
}
