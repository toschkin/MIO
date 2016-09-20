using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using TextFileLogger;

namespace MIOConfigurator.Windows
{
    /// <summary>
    /// Логика взаимодействия для UserRegistersMonitor.xaml
    /// </summary>
    public partial class UserRegistersMonitor : Window
    {        
        public ModbusReaderSaver ModbusReader { get; set; }

        private ObservableCollection<DeviceUserRegister> _currentValues;
        public ObservableCollection<DeviceUserRegister> CurrentValues
        {
            get { return _currentValues; }
            set
            {
                _currentValues = value;                
                UserRegsDataGrid.Items.Refresh();
            }
        }

        private TextLogger _logger;
        public UserRegistersMonitor()
        {
            InitializeComponent();
            _currentValues = new ObservableCollection<DeviceUserRegister>();
            _logger = new TextLogger();
            _logger.LogFilePath = AppDomain.CurrentDomain.BaseDirectory + "UserRegs_Monitor_LOG.txt";
        }

        public bool SaveToLog { get; set; }

        private BackgroundWorker windowBackgroundWorker = new BackgroundWorker();

        private void ReadStatusesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is List<DeviceUserRegister>)
            {
                CurrentValues = new ObservableCollection<DeviceUserRegister>((List<DeviceUserRegister>)e.UserState);
            }
            ExchangeStatus.Text = ((ReaderSaverErrors)e.ProgressPercentage).GetDescription();
            if (SaveToLog)
            {
                _logger.LogTextMessage("Request statuses:\t" + ExchangeStatus.Text);
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
                List<DeviceUserRegister> temporaryDeviceStatuses = CurrentValues.ToList();               
                retCode = ModbusReader.ReadUserRegisters(ref temporaryDeviceStatuses);
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
