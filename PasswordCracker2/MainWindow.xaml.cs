using Microsoft.Win32;
using PasswordCracker.Service;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PasswordCracker2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _mainFrame.Navigate(new SelectWorkMode());
        }
    }
}
