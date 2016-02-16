using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EnumExtension;
using Microsoft.Win32;
using MIOConfig;
using MIOConfig.PresentationLayer;
using Modbus.Core;

namespace MIOConfigurator.Windows
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

        private Dispatcher _dispatcher;
        private bool _supressMBox;
        private ModbusRtuProtocol _modbusRtuProtocol;
        public ObservableCollection<Device> Devices { get; set; }
        private Byte[] _deviceSnapshotBefore;        
        private Device _selectedDevice;
        private DeviceValidator _selectedDeviceValidator;
        public DeviceValidator SelectedDeviceValidator 
        { 
            get { return _selectedDeviceValidator; }
            set
            {
                _selectedDeviceValidator = value;
                NotifyPropertyChanged("SelectedDeviceValidator"); 
            }
        }

        public Device SelectedDevice 
        {
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
                    SelectedDeviceValidator = new DeviceValidator(_selectedDevice);                    
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

        private DeviceModbusMasterQuery _selectedModbusQuery;
        public DeviceModbusMasterQuery SelectedModbusQuery
        {
            get { return _selectedModbusQuery; }
            set
            {
                _selectedModbusQuery = value;
                NotifyPropertyChanged("SelectedModbusQuery");
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
            foreach (var device in Devices)
            {
                if (device.ModbusAddress == _deviceReaderSaver.SlaveAddress)
                {
                    worker.ReportProgress(1);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        retCode = device.ReadConfiguration(_deviceReaderSaver);    
                    }));                                    
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
                DeviceValidator dv= new DeviceValidator(SelectedDevice);

                if (dv.IsFullDeviceValid() == false)
                {
                    MessageBox.Show("В конфигурции устройства обнаружены ошибки!\nКонфигурация не может быть записана.\n" + dv.ToString(), Constants.messageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
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
            DrawTree();
        }

        private void DrawEmptySpace()
        {
            NoItemsTextBlock.Visibility = Visibility.Visible;
            ConfigurationTabs.Visibility = Visibility.Collapsed;
        }

        private void DrawTree()
        {
            ModbusQueriesTreeView.Items.Clear();
            if (SelectedDevice != null)
            {
                if (SelectedDevice.ModuleModbusMasterPresent)
                {                    
                    int portNumber = 0;
                    foreach (var port in SelectedDevice.ModbusMasterPortsQueries)
                    {
                        if (SelectedDevice.UartPortsConfigurations[portNumber].PortProtocolType ==
                            Definitions.MODBUS_MASTER_PROTOCOL)
                        {
                            TreeViewItem portItem = new TreeViewItem();
                            portItem.Tag = port;
                            portItem.Header = String.Format("Порт №{0}", portNumber+1);
                            int queryNumber = 0;
                            foreach (var query in port)
                            {
                                TreeViewItem queryItem = new TreeViewItem();
                                queryItem.Tag = query;
                                queryItem.Header = String.Format("Запрос №{0}", queryNumber+1);
                                if (query.QueryConfigured == false)
                                    queryItem.Foreground = Brushes.Gray;
                                else
                                    queryItem.Foreground = Brushes.Black;
                                portItem.Items.Add(queryItem);
                                queryNumber++;
                            }
                            ModbusQueriesTreeView.Items.Add(portItem);                              
                        }                        
                        portNumber++;
                    }                    
                }                
            }           
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
            SelectedModbusQuery = null;
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
            SelectedModbusQuery = null;
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
            SelectedModbusQuery = null;
            DrawEmptySpace();
            Devices.Clear();
        }
        private void DelDeviceFromList()
        {
            Devices.Remove(DevicesList.SelectedItem as Device);
            SelectedDevice = null;
            SelectedPortConfiguration = null;
            SelectedModbusQuery = null;
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
            SelectedModbusQuery = null;
            _deviceReaderSaver = new ModbusReaderSaver(_modbusRtuProtocol);
            _foundDevicesCount = 0;
            _deviceSnapshotBefore = null;
            _supressMBox = false;
            _dispatcher = Dispatcher.CurrentDispatcher;
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
                if (device.ModbusAddress == Convert.ToByte((string) SlaveConcreteAddress.Text))
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
            _deviceFinder.StartSlaveAddress = Convert.ToByte((string) SlaveConcreteAddress.Text);
            _deviceFinder.EndSlaveAddress = Convert.ToByte((string) SlaveConcreteAddress.Text);
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
                port.PortModbusAddress = Convert.ToByte((string) PortModbusAddress.Text);
            }
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
               
        public static RoutedCommand AddRouteCommand = new RoutedCommand();
        public static RoutedCommand DelRouteCommand = new RoutedCommand();

        private void AddRouteCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DeviceRoutingTableElement addedElement = new DeviceRoutingTableElement();
            SelectedDevice.RoutingMap.Add(addedElement);
            RoutingMapGrid.ScrollIntoView(SelectedDevice.RoutingMap.Last());
            //RoutingMapGrid.RowValidationRules[0].Validate(addedElement,CultureInfo.CurrentCulture);
        }
        private void DelRouteCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (RoutingMapGrid.SelectedItem is DeviceRoutingTableElement)
                SelectedDevice.RoutingMap.Remove((DeviceRoutingTableElement)RoutingMapGrid.SelectedItem);
        }

        private bool IsSlectedDeviceConfigurationValid()
        {
            //routing
            /*if (RoutingMapGrid != null)
            {
                if (RoutingMapGrid.Items != null)
                {
                    for (int i = 0; i < RoutingMapGrid.Items.Count; i++)
                    {
                        DataGridRow row = GetRow(RoutingMapGrid, i);
                        if (row != null && Validation.GetHasError(row))
                        {
                            return false;
                        }
                    }   
                }                
            }*/
            //modbusquery
            if (ModbusQueryConfigurationGrid != null)
            {
                if (TextBoxContainerHasErrors(ModbusQueryConfigurationGrid))
                    return false;   
            }            
            return true;      
        }

        private bool TextBoxContainerHasErrors(DependencyObject obj)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(obj))
            {
                TextBox element = child as TextBox;
                if (element == null) continue;

                if (Validation.GetHasError(element))
                {
                    return true;
                }

                if (TextBoxContainerHasErrors(element))
                    return true;
            }
            return false;
        }

        private void RouteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = IsSlectedDeviceConfigurationValid();
            e.CanExecute = true;
        }
        public static DataGridRow GetRow(DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }


        /* private void RouteToTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           if (String.IsNullOrEmpty(((TextBox) sender).Text))
                return;
            DeviceValidator validator = new DeviceValidator(SelectedDevice);
            DeviceRoutingTableElement elementToValidate = (DeviceRoutingTableElement)RoutingMapGrid.SelectedItem;
            elementToValidate.RouteTo = Convert.ToUInt16(((TextBox)sender).Text);
            if (validator.ValidateRoutingMapElement(elementToValidate) == false)
            {
                ((TextBox)sender).t
            }
        }
       
        private void RoutingMapGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
           
            if (e.EditAction == DataGridEditAction.Commit)
            {
                DeviceValidator validator = new DeviceValidator(SelectedDevice);
                DeviceRoutingTableElement elementToValidate = ((DeviceRoutingTableElement) e.Row.Item);
                
                if (e.Column.DisplayIndex == 0)
                    elementToValidate.RouteTo = Convert.ToUInt16(((DataGridCell)e.EditingElement).Content);
               
                if (e.Column.DisplayIndex == 1)
                    elementToValidate.RouteFrom = Convert.ToUInt16(((TextBlock)e.EditingElement).Text);

                if (validator.ValidateRoutingMapElement(elementToValidate) == false)
                {
                    e.EditingElement.ToolTip = validator.ToString();
                    e.Cancel = true;
                }
            } 
        }*/

        /*private int errorCount;
        private void RoutingMapGrid_OnError(object sender, ValidationErrorEventArgs e)
        {
            switch (e.Action)
            {
                case ValidationErrorEventAction.Added:
                    {
                        errorCount++; break;
                    }
                case ValidationErrorEventAction.Removed:
                    {
                        errorCount--; break;
                    }
            }
            AddRoute.IsEnabled = errorCount == 0;
            DelRoute.IsEnabled = errorCount == 0;
        }*/

        private void SaveCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SaveDeviceConfiguration();
        }

        private void ModbusQueriesTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem)
            {                
                if (((TreeViewItem) e.NewValue).Tag is DeviceModbusMasterQuery)
                {
                    Debug.WriteLine("Before");
                    PrintModbusQueriesTreeView();

                    SelectedModbusQuery = (DeviceModbusMasterQuery) ((TreeViewItem) e.NewValue).Tag;
                    PerformFullValidation();
                    
                    Debug.WriteLine("After");
                    PrintModbusQueriesTreeView();
                }
                else
                {
                    SelectedModbusQuery = null;
                }
            }            
        }

        private void QueryConfiguredCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            if (ModbusQueriesTreeView.SelectedItem is TreeViewItem)
            {
                if (QueryConfiguredCheckBox.IsChecked == false)
                    ((TreeViewItem)ModbusQueriesTreeView.SelectedItem).Foreground = Brushes.Gray;
                else
                    ((TreeViewItem)ModbusQueriesTreeView.SelectedItem).Foreground = Brushes.Black;
            }
        }

        private void PortProtocolType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            DrawTree();
        }

        private void CmdReadFromFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MIO configuration file|*.mio";
            openFileDialog.Title = "Прочитать конфигурацию";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                FileReaderSaver reader = new FileReaderSaver(openFileDialog.FileName);
                if (SelectedDevice != null && SelectedDevice.ConfigurationReadFromDevice)
                {
                    ReaderSaverErrors Result = reader.ReadDeviceConfiguration(SelectedDevice);
                    if (Result != ReaderSaverErrors.CodeOk)
                    {
                        СurrentlyProcessed.Text = "Чтение конфигурации завершено c ошибкой: " + Result.GetDescription();
                    }
                    else
                    {
                        UartPots.SelectedIndex = 0;
                        DrawConfigurationTabs();

                        NotifyPropertyChanged("SelectedDevice");                        
                        NotifyPropertyChanged("SelectedPortConfiguration");             
                        NotifyPropertyChanged("SelectedModbusQuery");

                        СurrentlyProcessed.Text = "Чтение конфигурации завершено успешно";
                    }
                }
            }            
        }

        private void CmdWriteToFile_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "MIO configuration file|*.mio";
            saveFileDialog.Title = "Сохранить конфигурацию";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog.FileName != "")
            {                
                FileReaderSaver saver = new FileReaderSaver(saveFileDialog.FileName);
                if (SelectedDevice != null && SelectedDevice.ConfigurationReadFromDevice)
                {
                    ReaderSaverErrors Result = saver.SaveDeviceConfiguration(SelectedDevice);
                    if (Result != ReaderSaverErrors.CodeOk)
                    {
                        СurrentlyProcessed.Text = "Запись конфигурации завершена c ошибкой: " + Result.GetDescription();
                    }
                    else
                    {                        
                        СurrentlyProcessed.Text = "Запись конфигурации завершена успешно";
                    }
                }                    
            }
        }

        private void PerformFullValidation()
        {
            /*foreach (var rule in RoutingMapGrid.RowValidationRules)
            {
               rule.Validate() 
            }
            
            BindingExpression be = RoutingMapGrid.GetBindingExpression(DataGrid.it);
            //if (be != null)
                //be.ValidateWithoutUpdate();
            //be.UpdateTarget();
            /*be = StartAddressOfModbusQueryTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null)
                be.UpdateSource();
            be = StatusAddressOfModbusQueryTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null)
                be.UpdateSource();
            be = RegistersCountInModbusQueryTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null)
                be.UpdateSource();*/
        }

        private void ConfigurationTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PerformFullValidation();
        }

        private void RoutingMapGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            PerformFullValidation();          
        }

        private void StatusAddressOfModbusQueryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PerformFullValidation();           
        }

        private void StartAddressOfModbusQueryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PerformFullValidation();
        }

        private void RegistersCountInModbusQueryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PerformFullValidation();
        }

        private void PrintModbusQueriesTreeView()
        {
            foreach (TreeViewItem portView in ModbusQueriesTreeView.Items)
            {
                foreach (TreeViewItem queryView in ((TreeViewItem)(portView)).Items)
                {
                    Debug.WriteLine(portView.Header + " " + queryView.Header + "\t" + ((DeviceModbusMasterQuery)(queryView.Tag)));                    
                }
            }
            
        }

        private void StatusesMonitor_OnClick(object sender, RoutedEventArgs e)
        {
            DeviceStatusesWindow statusesWindowMonitor = new DeviceStatusesWindow();
            statusesWindowMonitor.Owner = this;
            statusesWindowMonitor.CurrentStatuses.UartPortStatuses.Clear();
            _deviceReaderSaver.SlaveAddress = ((Device)DevicesList.SelectedItem).ModbusAddress;
            foreach (var port in SelectedDevice.UartPortsConfigurations)
            {
                statusesWindowMonitor.CurrentStatuses.UartPortStatuses.Add(new DeviceUartPortStatus());
            }
            statusesWindowMonitor.ModbusReader = _deviceReaderSaver;
            statusesWindowMonitor.ShowDialog();
        }
    }
}
