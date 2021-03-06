﻿using System;
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
    /// Логика взаимодействия для ModuleAIMonitor.xaml
    /// </summary>
    public partial class ModuleAIMonitor : Window, INotifyPropertyChanged
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

        private DeviceAIModule _currentStatus;        
        
        private TextLogger _logger;
        public DeviceAIModule CurrentModuleStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                NotifyPropertyChanged("CurrentModuleStatus");
                NotifyPropertyChanged("StatusDescription");
            }
        }

        public string StatusDescription
        {
            get
            {
                switch (_currentStatus.ModuleStatus.StatusRegister)
                {
                    case 0: { return "Модуль функционирует нормально"; }
                    case 1: { return "Модуль выключен"; }
                    case 2: { return "Измеренный ток меньше порога детекции тока"; }
                    case 3: { return "Измеренный ток больше макс. значения калибровки"; }
                }
                return "<неизвестное значение регистра статуса>";
            }           
        }
     
        public ModuleAIMonitor()
        {
            InitializeComponent();
            _currentStatus = new DeviceAIModule();
            _logger = new TextLogger();
            _logger.LogFilePath = AppDomain.CurrentDomain.BaseDirectory + "TI_ModuleMonitor_LOG.txt";
        }

        public bool SaveToLog { get; set; }

        private BackgroundWorker windowBackgroundWorker = new BackgroundWorker();

        private void ReadStatusesProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is DeviceAIModule)
            {
                CurrentModuleStatus = (DeviceAIModule)e.UserState;
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
                DeviceAIModule temporaryModuleStatus = CurrentModuleStatus;                
                retCode = ModbusReader.ReadModuleRegisters(temporaryModuleStatus);
                worker.ReportProgress((int)retCode, temporaryModuleStatus);
                Thread.Sleep(1000);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            windowBackgroundWorker.CancelAsync();
        }
    }
}
