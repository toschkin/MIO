using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Modbus.Core;

namespace MIOConfigurator
{    
    /// <summary>
    /// Логика взаимодействия для ProtocolConfigWindow.xaml
    /// </summary>
    public partial class ProtocolConfigWindow : Window
    {
        readonly ModbusRtuProtocol _protocol = new ModbusRtuProtocol();
        readonly ModbusCommunicationParameters currentCommunicationParameters = new ModbusCommunicationParameters(Constants.registryAppNode);
        public ProtocolConfigWindow()
        {
            InitializeComponent();            
        }

        public ProtocolConfigWindow(ModbusRtuProtocol protocol)
        {
            InitializeComponent();
            _protocol = protocol;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            if (_protocol.Connect((string)ComPorts.SelectedItem, (int)ComSpeed.SelectedItem, (Byte)ComByteSize.SelectedItem, (StopBits)ComStopBits.SelectedItem, (Parity)ComParity.SelectedItem, Convert.ToInt32(ModbusTimeout.Text)) == false)
            {
                MessageBox.Show("Не удалось подключиться на выбранный СОМ-порт: "+ComPorts.Text);
                return;
            }
            _protocol.SilentInterval = Convert.ToInt32(ModbusSilentInterval.Text);

            currentCommunicationParameters.DefaultComPort = (string)ComPorts.SelectedItem;
            currentCommunicationParameters.DefaultComSpeed = (int)ComSpeed.SelectedItem;
            currentCommunicationParameters.DefaultComByteSize = (Byte)ComByteSize.SelectedItem;
            currentCommunicationParameters.DefaultComParity = (Parity) ComParity.SelectedItem;
            currentCommunicationParameters.DefaultComStopBits = (StopBits) ComStopBits.SelectedItem;
            currentCommunicationParameters.DefaultModbusTimeOut = Convert.ToInt32(ModbusTimeout.Text);
            currentCommunicationParameters.DefaultModbusSilentInterval = Convert.ToInt32(ModbusSilentInterval.Text);
            DialogResult = true;            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ProtocolConfig_Loaded(object sender, RoutedEventArgs e)
        {
            ComPorts.ItemsSource = ModbusCommunicationParameters.AvailableComPorts;
            ComPorts.SelectedItem = currentCommunicationParameters.DefaultComPort;

            ComSpeed.ItemsSource = from speed in ModbusCommunicationParameters.AvailableComSpeeds where speed >= 1200 && speed <= 57600 select speed;
            ComSpeed.SelectedItem = currentCommunicationParameters.DefaultComSpeed; 
            
            ComByteSize.ItemsSource = from size in ModbusCommunicationParameters.AvailableComByteSizes where size ==8  select size;
            ComByteSize.SelectedItem = currentCommunicationParameters.DefaultComByteSize;
            
            ComParity.ItemsSource = from parity in ModbusCommunicationParameters.AvailableComParities
                where parity >= Parity.None && parity <= Parity.Even
                select parity;
            ComParity.SelectedItem = currentCommunicationParameters.DefaultComParity;

            ComStopBits.ItemsSource = from bits in ModbusCommunicationParameters.AvailableComStopBits
                where bits == StopBits.One || bits == StopBits.Two
                select bits;
            ComStopBits.SelectedItem = currentCommunicationParameters.DefaultComStopBits;

            ModbusTimeout.Text = currentCommunicationParameters.DefaultModbusTimeOut.ToString();
            ModbusSilentInterval.Text = currentCommunicationParameters.DefaultModbusSilentInterval.ToString();
        }
    }
}
