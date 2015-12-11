﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using MIOConfig;
using MIOConfig.PresentationLayer;
using Modbus.Core;
using MIOConfigurator;
using MIOConfigurator.Data;

namespace MIOConfigurator
{    
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModbusRtuProtocol _modbusRtuProtocol;
        private DeviceFinder _deviceFinder;
        private List<Device> _devices;
        private BackgroundWorker searchBackgroundWorker = new BackgroundWorker();

        private bool _needToDisconnectOnSearchCompleted;
        #region Search
        public bool SearchInProgress { get; set; }
        private void SearchDevices(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            
            _devices.Clear();            
            for (Byte address = _deviceFinder.StartSlaveAddress; address <= _deviceFinder.EndSlaveAddress; address++)
            {
                worker.ReportProgress(address);
                if (worker.CancellationPending)
                    break;
                Device device = _deviceFinder.FindDevice(address);
                //todo unrem for test 
                device = new Device();
                device.ModbusAddress = address;
                ///////////////////////
                if (device != null)                                              
                    worker.ReportProgress(address, device);                
            }
        }

        void SearchProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessingProgress.Value = e.ProgressPercentage;            
            if (e.UserState is Device)
            {
                _devices.Add((Device) e.UserState);
                СurrentlyProcessed.Text = String.Format("Поиск устройств... (найдено {0})",_devices.Count);               
            }
        }

        private void SearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchInProgress = false;
            CmdFindDevices.IsEnabled = true;
            CmdCancelSearchDevices.IsEnabled = false;
            CmdAddDeviceToList.IsEnabled = true;
            ProcessingProgress.Value = 0;
            СurrentlyProcessed.Text = String.Format("Поиск устройств окончен. (найдено {0})",_devices.Count);
            if(_needToDisconnectOnSearchCompleted)
                Disconnect();
        }

        private bool IsUserCancelSearch()
        {
            if (SearchInProgress)
            {
                if (MessageBoxResult.Yes ==
                    MessageBox.Show("Выполняется поиск устройств.\r\nОстановить процесс поиска?", Constants.messageBoxTitle, MessageBoxButton.YesNo,
                        MessageBoxImage.Question))
                {
                    searchBackgroundWorker.CancelAsync();
                    return true;
                }
            }
            return false;
        }   
        #endregion

        private void Disconnect()
        {
            if (_modbusRtuProtocol.IsConnected)
                _modbusRtuProtocol.Disconnect();
            CmdConnect.IsEnabled = true;
            CmdDisconnect.IsEnabled = false;
            СonnectionStatus.Text = "Отключено";
            СurrentlyProcessed.Text = "";
            _needToDisconnectOnSearchCompleted = false;
            //Todo need to reset list
        }
        public MainWindow()
        {
            InitializeComponent();   
            if(Registry.CurrentUser.OpenSubKey(Constants.registryAppNode) == null)
                Registry.CurrentUser.CreateSubKey(Constants.registryAppNode);
            _modbusRtuProtocol = new ModbusRtuProtocol();
            _deviceFinder = new DeviceFinder(_modbusRtuProtocol);  
            _devices = new List<Device>();
            SearchInProgress = false;
            _needToDisconnectOnSearchCompleted = false;
        }

        private void CmdConnect_OnClick(object sender, RoutedEventArgs e)
        {
            ProtocolConfigWindow connectionConfigWindow = new ProtocolConfigWindow(_modbusRtuProtocol);            
            connectionConfigWindow.Owner = this;
            if (connectionConfigWindow.ShowDialog() == true)
            {                
                CmdConnect.IsEnabled = false;
                CmdDisconnect.IsEnabled = true;               
                СonnectionStatus.Text = "Подключено: "+_modbusRtuProtocol.GetConnectionParametersString();
            }            
        }
        
        private void CmdDisconnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsUserCancelSearch())
                _needToDisconnectOnSearchCompleted = true;
            else
            {
                if (!SearchInProgress)
                    Disconnect();
            }            
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBoxResult.No ==
                MessageBox.Show("Выйти из программы?", Constants.messageBoxTitle, MessageBoxButton.YesNo,
                    MessageBoxImage.Question))
                e.Cancel = true;
            else
            {
                if(SearchInProgress)
                    searchBackgroundWorker.CancelAsync();
                Disconnect();
            }
                
        }

        private void CmdFindDevices_OnClick(object sender, RoutedEventArgs e)
        {            
            DevicesFinderConfigWindow finderConfigWindow = new DevicesFinderConfigWindow(_deviceFinder);
            finderConfigWindow.Owner = this;
            if (finderConfigWindow.ShowDialog() == true)
            {                
                //todo ask user to save all changes if present
                SearchInProgress = true;
                CmdFindDevices.IsEnabled = false;
                CmdAddDeviceToList.IsEnabled = false;
                CmdCancelSearchDevices.IsEnabled = true;
                СurrentlyProcessed.Text = "Поиск устройств...";
                ProcessingProgress.Minimum = _deviceFinder.StartSlaveAddress;
                ProcessingProgress.Maximum = _deviceFinder.EndSlaveAddress;
                searchBackgroundWorker.WorkerReportsProgress = true;
                searchBackgroundWorker.DoWork += SearchDevices;
                searchBackgroundWorker.RunWorkerCompleted += SearchCompleted;
                searchBackgroundWorker.ProgressChanged += SearchProgressChanged;
                searchBackgroundWorker.WorkerSupportsCancellation = true;                
                searchBackgroundWorker.RunWorkerAsync();                   
            }
        }

        private void CmdCancelSearchDevices_OnClick(object sender, RoutedEventArgs e)
        {
            IsUserCancelSearch();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {       
                
        }                             
    }
}
