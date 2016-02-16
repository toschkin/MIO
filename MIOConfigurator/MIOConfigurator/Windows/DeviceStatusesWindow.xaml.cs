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
    /// Логика взаимодействия для DeviceStatuses.xaml
    /// </summary>
    public partial class DeviceStatusesWindow : Window
    {
        public ModbusReaderSaver ModbusReader { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private DeviceStatuses _currentStatuses;
        public DeviceStatuses CurrentStatuses
        {
            get { return _currentStatuses; }
            set
            {
                _currentStatuses = value;
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
        }

        private BackgroundWorker windowBackgroundWorker = new BackgroundWorker();

        private void ReadStatusesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is DeviceStatuses)
            {
                CurrentStatuses = (DeviceStatuses) e.UserState;                
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
                DeviceStatuses temporaryDeviceStatuses = CurrentStatuses;
                ModbusReader.RegisterReadAddressOffset = Definitions.DEVICE_STATE_OFFSET;
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
