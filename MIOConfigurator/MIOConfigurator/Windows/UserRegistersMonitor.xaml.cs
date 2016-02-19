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
        
        public UserRegistersMonitor()
        {
            InitializeComponent();
            _currentValues = new ObservableCollection<DeviceUserRegister>();
        }

        private BackgroundWorker windowBackgroundWorker = new BackgroundWorker();

        private void ReadStatusesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is List<DeviceUserRegister>)
            {
                CurrentValues = new ObservableCollection<DeviceUserRegister>((List<DeviceUserRegister>)e.UserState);
            }
            ExchangeStatus.Text = ((ReaderSaverErrors)e.ProgressPercentage).GetDescription();
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
