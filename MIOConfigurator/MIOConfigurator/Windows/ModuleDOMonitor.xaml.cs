using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для ModuleDOMonitor.xaml
    /// </summary>
    public partial class ModuleDOMonitor : Window, INotifyPropertyChanged
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

        private DeviceDOModule _currentStatus;
        public DeviceDOModule CurrentModuleStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                NotifyPropertyChanged("CurrentModuleStatus");
            }
        }

        private TextLogger _logger;
        public ModuleDOMonitor()
        {
            InitializeComponent();
            _currentStatus = new DeviceDOModule();
            _needToForceCoil = false;
            _coilState = false;
            _coilAddress = 0;
            _logger = new TextLogger();
            _logger.LogFilePath = AppDomain.CurrentDomain.BaseDirectory + "TU_ModuleMonitor_LOG.txt";
        }

        private bool _needToForceCoil;
        private UInt16 _coilAddress;
        private bool _coilState;
        private BackgroundWorker windowBackgroundWorker = new BackgroundWorker();

        public bool SaveToLog { get; set; }

        private void ReadStatusesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is DeviceDOModule)
            {
                CurrentModuleStatus = (DeviceDOModule)e.UserState;
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
            windowBackgroundWorker.RunWorkerCompleted += ReadDOModuleStatusesCompleted;
            windowBackgroundWorker.RunWorkerAsync();
        }

        private void ReadDOModuleStatusesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_needToForceCoil)
            {
                ReaderSaverErrors retCode = ModbusReader.ForceCoil(_coilAddress, _coilState);
                ExchangeStatus.Text = retCode.GetDescription();
                if (retCode != ReaderSaverErrors.CodeOk)
                {
                    MessageBox.Show("Ошибка: " + retCode.GetDescription());
                }
                _needToForceCoil = false;
                windowBackgroundWorker.RunWorkerAsync();
            }
        }

        private void ReadStatuses(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            ReaderSaverErrors retCode = ReaderSaverErrors.CodeOk;
            while (!worker.CancellationPending)
            {
                DeviceDOModule temporaryModuleStatus = CurrentModuleStatus;                
                retCode = ModbusReader.ReadModuleRegisters(temporaryModuleStatus);
                worker.ReportProgress((int)retCode, temporaryModuleStatus);
                Thread.Sleep(1000);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            windowBackgroundWorker.CancelAsync();
        }


        private void GridCoilStates_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Byte coilOffset = 5;
            if (e.Source.Equals(SetCoil1))            
                _coilState = OnStateCoil1.IsChecked == true;

            /*if (e.Source.Equals(SetCoil2))
            {
                _coilState = OnStateCoil2.IsChecked == true;
                coilOffset = 6;
            }*/
            if (e.Source.Equals(SetCoil3))
            {
                _coilState = OnStateCoil3.IsChecked == true;
                coilOffset = 7;
            }
            if (e.Source.Equals(SetCoil4))
            {
                _coilState = OnStateCoil4.IsChecked == true;
                coilOffset = 8;
            }
            if (e.Source.Equals(SetCoil1) || /*e.Source.Equals(SetCoil2) ||*/ e.Source.Equals(SetCoil3) ||
                e.Source.Equals(SetCoil4))
            {
                _needToForceCoil = true;
                _coilAddress = (UInt16)(ModbusReader.RegisterReadAddressOffset + coilOffset);
                windowBackgroundWorker.CancelAsync();
            }
        }
    }
}
