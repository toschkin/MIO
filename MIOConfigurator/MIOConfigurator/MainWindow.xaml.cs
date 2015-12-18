using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
    public partial class MainWindow : Window ,INotifyPropertyChanged
    {
        private delegate void AfterWorkerCompletedAction();

        private AfterWorkerCompletedAction _workerCompletedAction;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private bool _supressMBox;
        private ModbusRtuProtocol _modbusRtuProtocol;
        public ObservableCollection<Device> Devices { get; set; }
        private Byte[] _deviceSnapshotBefore;        
        private Device _selectedDevice;
        public Device SelectedDevice {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                //taking snapshot
                if (_selectedDevice != null)
                {
                    MemoryStream buffer = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();                    
                    formatter.Serialize(buffer, _selectedDevice);                   
                    _deviceSnapshotBefore = buffer.ToArray();
    
                }
                else                
                    _deviceSnapshotBefore = null;
                
                NotifyPropertyChanged("SelectedDevice");               
            }
        }
        private DeviceUARTPortConfiguration _selectedPortConfiguration;
        public DeviceUARTPortConfiguration SelectedPortConfiguration
        {
            get { return _selectedPortConfiguration; }
            set
            {
                _selectedPortConfiguration = value;
                NotifyPropertyChanged("SelectedPortConfiguration");
            }
        }

        private BackgroundWorker mainWindowBackgroundWorker = new BackgroundWorker();

        public bool DeviceConfigurationChanged
        {
            get
            {
                if (_selectedDevice != null)
                {
                    MemoryStream buffer = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(buffer, _selectedDevice);
                    Byte[] deviceSnapshotAfter = buffer.ToArray();
                    if(deviceSnapshotAfter.Length != _deviceSnapshotBefore.Length)
                        return true;
                    for (int i = 0; i < _deviceSnapshotBefore.Length; i++)
                    {
                        if (deviceSnapshotAfter[i] != _deviceSnapshotBefore[i])
                            return true;
                    }                    
                }
                return false;
            }
        }

        private bool _workerInProgress;
        public bool WorkerInProgress {
            get { return _workerInProgress; }
            set
            {
                _workerInProgress = value;
                NotifyPropertyChanged("WorkerInProgress");
                NotifyPropertyChanged("WorkerStopped"); } 
        }
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
            mainWindowBackgroundWorker.ProgressChanged -= ReadConfigurationProgressChanged;

            if (((Device) DevicesList.SelectedItem).ConfigurationReadFromDevice)
            {
                СurrentlyProcessed.Text = "Чтение конфигурации завершено успешно";
                SelectedDevice = (Device) DevicesList.SelectedItem;
                UartPots.SelectedIndex = 0;
                DrawConfigurationTabs();
            }
            else
            {
                СurrentlyProcessed.Text = "Чтение конфигурации завершено c ошибкой: " + ((ReaderSaverErrors)e.Result).GetDescription();
                if (SelectedDevice == null) //no item was previously selected
                    DevicesList.UnselectAll();
                else
                {
                    DevicesList.SelectedItem = SelectedDevice;//returning to previously selected item 
                    UartPots.SelectedIndex = 0;
                }                    
            }
            if (_workerCompletedAction != null)
            {
                _workerCompletedAction();
                _workerCompletedAction = null;
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

        private void ReadDeviceConfiguration(bool forceRead=false)
        {
            if (DevicesList.SelectedItem != null)
            {
                if (AskUserToSaveDeviceConfiguration())
                {
                    if (forceRead)
                        _workerCompletedAction += ForcedReadConfig;
                    else
                        _workerCompletedAction += ReadConfig;
                    SaveDeviceConfiguration();
                }
                else
                {
                    if (forceRead)
                        ForcedReadConfig();
                    else
                        ReadConfig();
                }
            }                              
        }
        private void SaveDeviceConfiguration()
        {
            if (DevicesList.SelectedItem != null)
            {
                _deviceReaderSaver.SlaveAddress = ((Device)DevicesList.SelectedItem).ModbusAddress;
                WorkerInProgress = true;
                CmdFindDevices.IsEnabled = false;
                CmdAddDeviceToList.IsEnabled = false;
                CmdCancelSearchDevices.IsEnabled = false;
                DevicesList.IsEnabled = false;
                СurrentlyProcessed.Text = "Запись конфигурации в устройство...";
                ProcessingProgress.Minimum = 0;
                ProcessingProgress.Maximum = 2;
                ProcessingProgress.Value = 0;
                mainWindowBackgroundWorker.WorkerReportsProgress = true;
                mainWindowBackgroundWorker.DoWork += SaveConfiguration;
                mainWindowBackgroundWorker.RunWorkerCompleted += SaveConfigurationCompleted;
                mainWindowBackgroundWorker.ProgressChanged += SaveConfigurationProgressChanged;
                mainWindowBackgroundWorker.WorkerSupportsCancellation = false;
                mainWindowBackgroundWorker.RunWorkerAsync();                   
            }
        }
        private bool AskUserToSaveDeviceConfiguration(bool forceSaving = false)
        {
            if (DeviceConfigurationChanged)
            {
                if (MessageBoxResult.Yes ==
                    MessageBox.Show("Сохранить конфигурацию устройства?", Constants.messageBoxTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question))
                {
                    return true;
                }
            }
            return false;
        }
        private void SaveConfigurationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerInProgress = false;
            CmdFindDevices.IsEnabled = true;
            CmdCancelSearchDevices.IsEnabled = false;
            CmdAddDeviceToList.IsEnabled = true;
            DevicesList.IsEnabled = true;
            ProcessingProgress.Value = 0;
            mainWindowBackgroundWorker.DoWork -= SaveConfiguration;
            mainWindowBackgroundWorker.RunWorkerCompleted -= SaveConfigurationCompleted;
            mainWindowBackgroundWorker.ProgressChanged -= SaveConfigurationProgressChanged;

            if (e.Result is ReaderSaverErrors)
            {
                if ((ReaderSaverErrors)e.Result == ReaderSaverErrors.CodeOk)
                {
                    SelectedDevice = SelectedDevice;//wow!!! the propertys advantage
                    СurrentlyProcessed.Text = "Запись конфигурации завершена успешно";                    
                }
                else
                {
                    СurrentlyProcessed.Text = "Запись конфигурации завершена c ошибкой: " + ((ReaderSaverErrors)e.Result).GetDescription();                    
                }
            }            
            if (_workerCompletedAction != null)
            {
                _workerCompletedAction();
                _workerCompletedAction = null;
            }
        }

        private void SaveConfigurationProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessingProgress.Value = e.ProgressPercentage;
        }

        private void SaveConfiguration(object sender, DoWorkEventArgs e)
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
                    retCode = device.SaveConfiguration(_deviceReaderSaver);
                    break;
                }
            }
            worker.ReportProgress(2);
            e.Result = retCode;
        }

        private void RestartCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerInProgress = false;
            CmdFindDevices.IsEnabled = true;
            CmdCancelSearchDevices.IsEnabled = false;
            CmdAddDeviceToList.IsEnabled = true;
            DevicesList.IsEnabled = true;
            ProcessingProgress.Value = 0;
            mainWindowBackgroundWorker.DoWork -= Restart;
            mainWindowBackgroundWorker.RunWorkerCompleted -= RestartCompleted;
            mainWindowBackgroundWorker.ProgressChanged -= RestartProgressChanged;

            if (e.Result is ReaderSaverErrors)
            {
                if ((ReaderSaverErrors)e.Result == ReaderSaverErrors.CodeOk)
                {
                    СurrentlyProcessed.Text = "Команда перезагрузки устройства отправлена успешно";
                }
                else
                {
                    СurrentlyProcessed.Text = "Ошибка при отправке команды перезагрузки устройства: " + ((ReaderSaverErrors)e.Result).GetDescription();
                }
            }
            if (_workerCompletedAction != null)
            {
                _workerCompletedAction();
                _workerCompletedAction = null;
            }
        }

        private void RestartProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessingProgress.Value = e.ProgressPercentage;
        }

        private void Restart(object sender, DoWorkEventArgs e)
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
                    retCode = device.RestartDevice(_deviceReaderSaver);
                    break;
                }
            }
            worker.ReportProgress(2);
            e.Result = retCode;
        }       
        #endregion
        
        #region Search                
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
            if (_workerCompletedAction != null)
            {
                _workerCompletedAction();
                _workerCompletedAction = null;
            }            
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
        //afterworker actions
        private void Disconnect()
        {            
            if (_modbusRtuProtocol.IsConnected)
                _modbusRtuProtocol.Disconnect();
            CmdConnect.IsEnabled = true;
            CmdDisconnect.IsEnabled = false;
            СonnectionStatus.Text = "Отключено";
            СurrentlyProcessed.Text = "";            
            Devices.Clear();            
            SelectedDevice = null;
            SelectedPortConfiguration = null;
            DrawEmptySpace();
        }
        private void CloseWindow()
        {
            _supressMBox = true;
            Close();
        }
        private void ReadConfig()
        {
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
                    MessageBoxResult result = MessageBox.Show("Для данного устройства не считана конфигурация. Прочитать?",
                        Constants.messageBoxTitle, MessageBoxButton.YesNo,
                        MessageBoxImage.Question);                   
                    if (MessageBoxResult.Yes == result)
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
        private void ForcedReadConfig()
        {            
            if (DevicesList.SelectedItem is Device)
            {               
                if (WorkerInProgress)
                {
                    if (SelectedDevice == null)//no item was previously selected
                        DevicesList.UnselectAll();
                    else
                        DevicesList.SelectedItem = SelectedDevice;//returning to previously selected item        
                    return;
                }                    
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
            
        }
        private void FindDevices()
        {
            Devices.Clear();
            DrawEmptySpace();
            SelectedDevice = null;
            SelectedPortConfiguration = null;
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
        private void ClearDevicesList()
        {
            SelectedDevice = null;
            SelectedPortConfiguration = null;
            DrawEmptySpace();
            Devices.Clear();
        }
        private void DelDeviceFromList()
        {
            Devices.Remove(DevicesList.SelectedItem as Device);
            SelectedDevice = null;
            SelectedPortConfiguration = null;
            DrawEmptySpace();
        }
        private void RestartDevice()
        {
            if (DevicesList.SelectedItem != null)
            {
                _deviceReaderSaver.SlaveAddress = ((Device)DevicesList.SelectedItem).ModbusAddress;
                WorkerInProgress = true;
                CmdFindDevices.IsEnabled = false;
                CmdAddDeviceToList.IsEnabled = false;
                CmdCancelSearchDevices.IsEnabled = false;
                DevicesList.IsEnabled = false;
                СurrentlyProcessed.Text = "Отправка команды перезагрузки устройства...";
                ProcessingProgress.Minimum = 0;
                ProcessingProgress.Maximum = 2;
                ProcessingProgress.Value = 0;
                mainWindowBackgroundWorker.WorkerReportsProgress = true;
                mainWindowBackgroundWorker.DoWork += Restart;
                mainWindowBackgroundWorker.RunWorkerCompleted += RestartCompleted;
                mainWindowBackgroundWorker.ProgressChanged += RestartProgressChanged;
                mainWindowBackgroundWorker.WorkerSupportsCancellation = false;
                mainWindowBackgroundWorker.RunWorkerAsync();
            }
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
            SelectedDevice = null;
            SelectedPortConfiguration = null;
            _deviceReaderSaver = new ModbusReaderSaver(_modbusRtuProtocol);
            _foundDevicesCount = 0;
            _deviceSnapshotBefore = null;
            _supressMBox = false;
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
            {
                _workerCompletedAction += Disconnect;
            }                
            else
            {
                if (!WorkerInProgress)
                {
                    if (AskUserToSaveDeviceConfiguration())
                    {
                        _workerCompletedAction += Disconnect;
                        SaveDeviceConfiguration();
                    }
                    else
                        Disconnect();
                }                    
            }            
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_supressMBox)
                return;
            if (MessageBoxResult.No ==
                MessageBox.Show("Выйти из программы?", Constants.messageBoxTitle, MessageBoxButton.YesNo,
                    MessageBoxImage.Question))
                e.Cancel = true;
            else
            {
                if (WorkerInProgress)
                {
                    mainWindowBackgroundWorker.CancelAsync();
                    e.Cancel = true;
                    _workerCompletedAction += Disconnect;
                    _workerCompletedAction += CloseWindow;
                }                    
                else
                {
                    if (AskUserToSaveDeviceConfiguration())
                    {
                        _workerCompletedAction += Disconnect;
                        _workerCompletedAction += CloseWindow;
                        SaveDeviceConfiguration();
                    }
                    else
                        Disconnect();
                }                                                    
            }
                
        }

        private void CmdFindDevices_OnClick(object sender, RoutedEventArgs e)
        {            
            DevicesFinderConfigWindow finderConfigWindow = new DevicesFinderConfigWindow(_deviceFinder);
            finderConfigWindow.Owner = this;
            if (finderConfigWindow.ShowDialog() == true)
            {
                if (AskUserToSaveDeviceConfiguration())
                {
                    _workerCompletedAction += FindDevices;
                    SaveDeviceConfiguration();                    
                }
                else
                {
                    FindDevices();
                }
            }
        }        

        private void CmdCancelSearchDevices_OnClick(object sender, RoutedEventArgs e)
        {
            IsUserCancelSearch();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DrawEmptySpace();
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
            foreach (var device in Devices)
            {
                if (device.ModbusAddress == Convert.ToByte(SlaveConcreteAddress.Text))
                {
                    DevicesList.ScrollIntoView(device);
                    return;
                }
            }
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
            if (AskUserToSaveDeviceConfiguration())
            {
                _workerCompletedAction += ClearDevicesList;
                SaveDeviceConfiguration();
            }
            else
            {
                ClearDevicesList();
            }                 
        }        

        private void CmdDelDeviceFromList_OnClick(object sender, RoutedEventArgs e)
        {
            if (DevicesList.SelectedItem is Device)
            {
                if (AskUserToSaveDeviceConfiguration())
                {
                    _workerCompletedAction += DelDeviceFromList;
                    SaveDeviceConfiguration();     
                }
                else
                {
                    DelDeviceFromList();
                }                
            }
        }       

        private void DevicesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)            
        {
            if(e.RemovedItems.Count == 0)
                ReadDeviceConfiguration();
        }

        private void CmdReadConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            ReadDeviceConfiguration(true);
        }

        private void UartPots_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (UartPots.SelectedIndex >= 0 && UartPots.SelectedIndex < SelectedDevice.UartPortsConfigurations.Count)
                SelectedPortConfiguration = SelectedDevice.UartPortsConfigurations[UartPots.SelectedIndex];
        }

        private void PortModbusAddress_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(PortModbusAddress.Text))
                return;
            foreach (var port in SelectedDevice.UartPortsConfigurations)
            {
                port.PortModbusAddress = Convert.ToByte(PortModbusAddress.Text);
            }
        }

        private void CmdSaveConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            SaveDeviceConfiguration();
        }

        private void CmdRestart_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedDevice != null)
            {
                if (AskUserToSaveDeviceConfiguration())
                {
                    _workerCompletedAction += RestartDevice;
                    SaveDeviceConfiguration();
                }
                RestartDevice();
            }
        }        
    }
}
