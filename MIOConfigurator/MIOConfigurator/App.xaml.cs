﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MIOConfigurator
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            // Setup Quick Converter.
            // Add the System namespace so we can use primitive types (i.e. int, etc.).
            QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
            QuickConverter.EquationTokenizer.AddNamespace(typeof(UInt16));
            QuickConverter.EquationTokenizer.AddNamespace(typeof(Parity));
            // Add the System.Windows namespace so we can use Visibility.Collapsed, etc.
            QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));
        }
    }
}
