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
        ModbusRtuProtocol _protocol = new ModbusRtuProtocol();
        ModbusCommunicationParameters currentCommunicationParameters = new ModbusCommunicationParameters(Constants.registryAppNode);
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
            
            if (_protocol.Connect((string)comPorts.SelectedItem, (int)comSpeed.SelectedItem, (Byte)comByteSize.SelectedItem, (StopBits)comStopBits.SelectedItem, (Parity)comParity.SelectedItem, Convert.ToInt32(modbusTimeout.Text)) == false)
            {
                MessageBox.Show("Не удалось подключиться на выбранный СОМ-порт: "+comPorts.Text);
                return;
            }
            _protocol.SilentInterval = Convert.ToInt32(modbusSilentInterval.Text);

            currentCommunicationParameters.DefaultComPort = (string)comPorts.SelectedItem;
            currentCommunicationParameters.DefaultComSpeed = (int)comSpeed.SelectedItem;
            currentCommunicationParameters.DefaultComByteSize = (Byte)comByteSize.SelectedItem;
            currentCommunicationParameters.DefaultComParity = (Parity) comParity.SelectedItem;
            currentCommunicationParameters.DefaultComStopBits = (StopBits) comStopBits.SelectedItem;
            currentCommunicationParameters.DefaultModbusTimeOut = Convert.ToInt32(modbusTimeout.Text);
            currentCommunicationParameters.DefaultModbusSilentInterval = Convert.ToInt32(modbusSilentInterval.Text);
            DialogResult = true;            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ProtocolConfig_Loaded(object sender, RoutedEventArgs e)
        {
            comPorts.ItemsSource = ModbusCommunicationParameters.AvailableComPorts;
            comPorts.SelectedItem = currentCommunicationParameters.DefaultComPort;

            comSpeed.ItemsSource = from speed in ModbusCommunicationParameters.AvailableComSpeeds where speed >=1200 && speed <=57600 select speed;
            comSpeed.SelectedItem = currentCommunicationParameters.DefaultComSpeed; 
            
            comByteSize.ItemsSource = from size in ModbusCommunicationParameters.AvailableComByteSizes where size ==8  select size;
            comByteSize.SelectedItem = currentCommunicationParameters.DefaultComByteSize;

            comParity.ItemsSource = from parity in ModbusCommunicationParameters.AvailableComParities
                where parity >= Parity.None && parity <= Parity.Even
                select parity;
            comParity.SelectedItem = currentCommunicationParameters.DefaultComParity;

            comStopBits.ItemsSource = from bits in ModbusCommunicationParameters.AvailableComStopBits
                where bits == StopBits.One || bits == StopBits.Two
                select bits;
            comStopBits.SelectedItem = currentCommunicationParameters.DefaultComStopBits;

            modbusTimeout.Text = currentCommunicationParameters.DefaultModbusTimeOut.ToString();
            modbusSilentInterval.Text = currentCommunicationParameters.DefaultModbusSilentInterval.ToString();
        }
    }
}
