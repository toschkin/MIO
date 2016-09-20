using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EnumExtension;
using MIOConfig;
using Modbus.Core;
using TextFileLogger;

namespace MIOConfigurator.Windows
{
    /// <summary>
    /// Логика взаимодействия для DeviceStatuses.xaml
    /// </summary>
    public partial class DeviceStatusesWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
       
        public ModbusReaderSaver ModbusReader { get; set; }

        public bool SaveToLog { get; set; }
      
        private DeviceStatuses _currentStatuses;
        private TextLogger _logger;
        public DeviceStatuses CurrentStatuses
        {
            get { return _currentStatuses; }
            set
            {
                _currentStatuses = value;
                NotifyPropertyChanged("CurrentStatuses");
                PortsStatusesDataGrid.Items.Refresh();
            }
        }

        public ReadOnlyCollection<DeviceUartPortStatus> UartPortStatuses
        {
            get { return new ReadOnlyCollection<DeviceUartPortStatus>(CurrentStatuses.UartPortStatuses); }
        }

        public DeviceStatusesWindow()
        {            
            InitializeComponent();
            _currentStatuses = new DeviceStatuses();
            _logger = new TextLogger();
            _logger.LogFilePath = AppDomain.CurrentDomain.BaseDirectory + "DeviceStatuses_Log.txt";
        }

        private BackgroundWorker windowBackgroundWorker = new BackgroundWorker();

        private void ReadStatusesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is DeviceStatuses)
            {
                CurrentStatuses = (DeviceStatuses) e.UserState;                
            }
            ExchangeStatus.Text = ((ReaderSaverErrors)e.ProgressPercentage).GetDescription();
            if (SaveToLog)
            {
                _logger.LogTextMessage("Request statuses:\t"+ExchangeStatus.Text);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            windowBackgroundWorker.WorkerReportsProgress = true;
            windowBackgroundWorker.DoWork += ReadStatuses;            
            windowBackgroundWorker.ProgressChanged += ReadStatusesProgressChanged;
            windowBackgroundWorker.WorkerSupportsCancellation = true;
            windowBackgroundWorker.RunWorkerAsync();
        }
        private void ReadStatuses(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            ReaderSaverErrors retCode = ReaderSaverErrors.CodeOk;
            while (!worker.CancellationPending)
            {
                DeviceStatuses temporaryDeviceStatuses = CurrentStatuses;                
                retCode = ModbusReader.ReadModuleRegisters(temporaryDeviceStatuses);               
                worker.ReportProgress((int)retCode, temporaryDeviceStatuses);
                Thread.Sleep(1000);
            }             
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            windowBackgroundWorker.CancelAsync();
        }
    }
}
