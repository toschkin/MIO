using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using EnumExtension;
using Microsoft.Win32;
using MIOConfig;
using MIOConfig.PresentationLayer;
using Modbus.Core;
using MIOConfigurator;

namespace MIOConfigurator
{    
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        private ModbusRtuProtocol _modbusRtuProtocol;
        public ObservableCollection<Device> Devices { get; set; }
        public Device SelectedDevice;
        private BackgroundWorker mainWindowBackgroundWorker = new BackgroundWorker();
        private bool _deviceConfigurationChanged;                  

        public bool WorkerInProgress { get; set; }
        public bool WorkerStopped { get { return !WorkerInProgress; } }

        #region Reading and Saving

        private ModbusReaderSaver _deviceReaderSaver;
        
        private void ReadConfigurationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerInProgress = false;
            CmdFindDevices.IsEnabled = true;
            CmdCancelSearchDevices.IsEnabled = false;
            CmdAddDeviceToList.IsEnabled = true;
            DevicesList.IsEnabled = true;
            ProcessingProgress.Value = 0;
            mainWindowBackgroundWorker.DoWork -= ReadConfiguration;
            mainWindowBackgroundWorker.RunWorkerCompleted -= ReadConfigurationCompleted;

            if (((Device) DevicesList.SelectedItem).ConfigurationReadFromDevice)
            {
                СurrentlyProcessed.Text = "Чтение конфигурации завершено успешно";
                SelectedDevice = (Device) DevicesList.SelectedItem;
                DrawConfigurationTabs();
            }
            else
            {
                СurrentlyProcessed.Text = "Чтение конфигурации завершено c ошибкой: " + ((ReaderSaverErrors)e.Result).GetDescription();
                if (SelectedDevice == null)//no item was previously selected
                    DevicesList.UnselectAll();
                else
                    DevicesList.SelectedItem = SelectedDevice;//returning to previously selected item 
            }                                        
        }
        private void ReadConfigurationProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessingProgress.Value = e.ProgressPercentage; 
        }  
        private void ReadConfiguration(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            ReaderSaverErrors retCode = ReaderSaverErrors.CodeOk;
            foreach (Device device in Devices)
            {
                if (device.ModbusAddress == _deviceReaderSaver.SlaveAddress)
                {
                    worker.ReportProgress(1);
                    retCode = device.ReadConfiguration(_deviceReaderSaver);                    
                    break;
                }                    
            }
            worker.ReportProgress(2);
            e.Result = retCode;
        }

        private void OnReadConfiguration()
        {
            if (SelectedDevice != null)
                AskUserToSaveDeviceConfiguration();

            if (DevicesList.SelectedItem is Device)
            {
                if (((Device)DevicesList.SelectedItem).ConfigurationReadFromDevice)
                {
                    SelectedDevice = (Device)DevicesList.SelectedItem;
                    DrawConfigurationTabs();
                }
                else
                {
                    if (WorkerInProgress)
                    {
                        if (SelectedDevice == null)//no item was previously selected
                            DevicesList.UnselectAll();
                        else
                            DevicesList.SelectedItem = SelectedDevice;//returning to previously selected item        
                        return;
                    }
                    if (MessageBoxResult.Yes ==
                        MessageBox.Show("Для данного устройства не считана конфигурация. Прочитать?",
                            Constants.messageBoxTitle, MessageBoxButton.YesNo,
                            MessageBoxImage.Question))
                    {
                        _deviceReaderSaver.SlaveAddress = ((Device)DevicesList.SelectedItem).ModbusAddress;
                        WorkerInProgress = true;
                        CmdFindDevices.IsEnabled = false;
                        CmdAddDeviceToList.IsEnabled = false;
                        CmdCancelSearchDevices.IsEnabled = false;
                        DevicesList.IsEnabled = false;
                        СurrentlyProcessed.Text = "Чтение конфигурации устройства...";
                        ProcessingProgress.Minimum = 0;
                        ProcessingProgress.Maximum = 2;
                        ProcessingProgress.Value = 0;
                        mainWindowBackgroundWorker.WorkerReportsProgress = true;
                        mainWindowBackgroundWorker.DoWork += ReadConfiguration;
                        mainWindowBackgroundWorker.RunWorkerCompleted += ReadConfigurationCompleted;
                        mainWindowBackgroundWorker.ProgressChanged += ReadConfigurationProgressChanged;
                        mainWindowBackgroundWorker.WorkerSupportsCancellation = false;
                        mainWindowBackgroundWorker.RunWorkerAsync();
                    }
                    else
                    {
                        if (SelectedDevice == null)//no item was previously selected
                            DevicesList.UnselectAll();
                        else
                            DevicesList.SelectedItem = SelectedDevice;//returning to previously selected item                                                   
                    }
                }
            }  
        }
        private void SaveDeviceConfiguration()
        {
            _deviceConfigurationChanged = false;
            if (SelectedDevice != null)
            {
                //todo save configuration
            }
        }

        private void AskUserToSaveDeviceConfiguration()
        {
            if (_deviceConfigurationChanged)
            {
                if (MessageBoxResult.Yes ==
                    MessageBox.Show("Сохранить конфигурацию устройства?", Constants.messageBoxTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question))
                    SaveDeviceConfiguration();
                else
                    _deviceConfigurationChanged = false;
            }
        }
        #endregion
        
        #region Search        
        private bool _needToDisconnectOnSearchCompleted;        
        private DeviceFinder _deviceFinder;
        private int _foundDevicesCount;

        private void SearchDevices(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            _foundDevicesCount = 0;            
            for (Byte address = _deviceFinder.StartSlaveAddress; address <= _deviceFinder.EndSlaveAddress; address++)
            {                
                if (worker.CancellationPending)
                    break;
                Device device = _deviceFinder.FindDevice(address);
                /*/todo unrem for test 
                device = new Device();
                device.ModbusAddress = address;                
                /////////////////////*/
                worker.ReportProgress(address, device);                
            }            
        }

        void SearchProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessingProgress.Value = e.ProgressPercentage;            
            if (e.UserState is Device)
            {
                _foundDevicesCount++;
                Devices.Add((Device) e.UserState);                
                DevicesList.ScrollIntoView((Device)e.UserState);
                СurrentlyProcessed.Text = String.Format("Поиск устройств... (найдено {0})", _foundDevicesCount);               
            }
        }

        private void SearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerInProgress = false;
            CmdFindDevices.IsEnabled = true;
            CmdCancelSearchDevices.IsEnabled = false;
            CmdAddDeviceToList.IsEnabled = true;
            ProcessingProgress.Value = 0;
            СurrentlyProcessed.Text = String.Format("Поиск устройств окончен. (найдено {0})", _foundDevicesCount);
            mainWindowBackgroundWorker.DoWork -= SearchDevices;
            mainWindowBackgroundWorker.RunWorkerCompleted -= SearchCompleted;
            mainWindowBackgroundWorker.ProgressChanged -= SearchProgressChanged;
            if(_needToDisconnectOnSearchCompleted)
                Disconnect();
        }

        private bool IsUserCancelSearch()
        {
            if (WorkerInProgress)
            {
                if (MessageBoxResult.Yes ==
                    MessageBox.Show("Выполняется поиск устройств.\r\nОстановить процесс поиска?", Constants.messageBoxTitle, MessageBoxButton.YesNo,
                        MessageBoxImage.Question))
                {
                    mainWindowBackgroundWorker.CancelAsync();
                    return true;
                }
            }
            return false;
        }   
        #endregion

        private void DrawConfigurationTabs()
        {
            NoItemsTextBlock.Visibility = Visibility.Collapsed;
            ConfigurationTabs.Visibility = Visibility.Visible;
        }

        private void DrawEmptySpace()
        {
            NoItemsTextBlock.Visibility = Visibility.Visible;
            ConfigurationTabs.Visibility = Visibility.Collapsed;
        }

        private void Disconnect()
        {            
            if (_modbusRtuProtocol.IsConnected)
                _modbusRtuProtocol.Disconnect();
            CmdConnect.IsEnabled = true;
            CmdDisconnect.IsEnabled = false;
            СonnectionStatus.Text = "Отключено";
            СurrentlyProcessed.Text = "";            
            Devices.Clear();
            _needToDisconnectOnSearchCompleted = false;
            SelectedDevice = null;
            DrawEmptySpace();
        }        

        public MainWindow()
        {
            InitializeComponent();   
            if(Registry.CurrentUser.OpenSubKey(Constants.registryAppNode) == null)
                Registry.CurrentUser.CreateSubKey(Constants.registryAppNode);
            _modbusRtuProtocol = new ModbusRtuProtocol();
            _deviceFinder = new DeviceFinder(_modbusRtuProtocol);  
            Devices = new ObservableCollection<Device>();
            WorkerInProgress = false;
            _needToDisconnectOnSearchCompleted = false;
            _deviceConfigurationChanged = false;
            SelectedDevice = null;
            _deviceReaderSaver = new ModbusReaderSaver(_modbusRtuProtocol);
            _foundDevicesCount = 0;
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
                if (!WorkerInProgress)
                {
                    AskUserToSaveDeviceConfiguration();
                    Disconnect();
                }                    
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
                if(WorkerInProgress)
                    mainWindowBackgroundWorker.CancelAsync();
                else                
                    AskUserToSaveDeviceConfiguration();
                
                Disconnect();
            }
                
        }

        private void CmdFindDevices_OnClick(object sender, RoutedEventArgs e)
        {            
            DevicesFinderConfigWindow finderConfigWindow = new DevicesFinderConfigWindow(_deviceFinder);
            finderConfigWindow.Owner = this;
            if (finderConfigWindow.ShowDialog() == true)
            {
                AskUserToSaveDeviceConfiguration();                
                Devices.Clear();
                DrawEmptySpace();
                SelectedDevice = null;
                WorkerInProgress = true;
                CmdFindDevices.IsEnabled = false;
                CmdAddDeviceToList.IsEnabled = false;
                CmdCancelSearchDevices.IsEnabled = true;
                СurrentlyProcessed.Text = "Поиск устройств...";
                ProcessingProgress.Minimum = _deviceFinder.StartSlaveAddress;
                ProcessingProgress.Maximum = _deviceFinder.EndSlaveAddress;
                mainWindowBackgroundWorker.WorkerReportsProgress = true;
                mainWindowBackgroundWorker.DoWork += SearchDevices;
                mainWindowBackgroundWorker.RunWorkerCompleted += SearchCompleted;
                mainWindowBackgroundWorker.ProgressChanged += SearchProgressChanged;
                mainWindowBackgroundWorker.WorkerSupportsCancellation = true;                
                mainWindowBackgroundWorker.RunWorkerAsync();                   
            }
        }

        private void CmdCancelSearchDevices_OnClick(object sender, RoutedEventArgs e)
        {
            IsUserCancelSearch();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {       
                
        }
        
        private void SlaveConcreteAddress_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (MainMenu.IsKeyboardFocusWithin)
            {
                e.Handled = true;
            }
        }

        private void CmdAddDeviceToList_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(SlaveConcreteAddress.Text))
                return;
            WorkerInProgress = true;
            CmdFindDevices.IsEnabled = false;
            CmdAddDeviceToList.IsEnabled = false;
            CmdCancelSearchDevices.IsEnabled = true;
            СurrentlyProcessed.Text = "Поиск устройства...";
            _deviceFinder.StartSlaveAddress = Convert.ToByte(SlaveConcreteAddress.Text);
            _deviceFinder.EndSlaveAddress = Convert.ToByte(SlaveConcreteAddress.Text);
            _deviceFinder.TargetVersion = 0;
            ProcessingProgress.Minimum = 0;
            ProcessingProgress.Maximum = 1;
            mainWindowBackgroundWorker.WorkerReportsProgress = true;
            mainWindowBackgroundWorker.DoWork += SearchDevices;
            mainWindowBackgroundWorker.RunWorkerCompleted += SearchCompleted;
            mainWindowBackgroundWorker.ProgressChanged += SearchProgressChanged;
            mainWindowBackgroundWorker.WorkerSupportsCancellation = true;
            mainWindowBackgroundWorker.RunWorkerAsync();            
        }

        private void CmdDelAllDevicesFromList_Click(object sender, RoutedEventArgs e)
        {
            AskUserToSaveDeviceConfiguration();
            SelectedDevice = null;
            DrawEmptySpace();
            Devices.Clear();            
        }

        private void CmdDelDeviceFromList_OnClick(object sender, RoutedEventArgs e)
        {
            if (DevicesList.SelectedItem is Device)
            {
                AskUserToSaveDeviceConfiguration();                
                Devices.Remove(DevicesList.SelectedItem as Device);
                SelectedDevice = null; 
                DrawEmptySpace();
            }
        }

        private void DevicesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnReadConfiguration();
        }

        private void CmdReadConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            OnReadConfiguration();
        }                                           
    }
}
