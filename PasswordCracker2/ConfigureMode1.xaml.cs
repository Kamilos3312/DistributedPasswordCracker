using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PasswordCracker2
{
    /// <summary>
    /// Interaction logic for ConfigureMode1.xaml
    /// </summary>
    public partial class ConfigureMode1 : Page
    {
        private FileDialog passwordsFile;
        private FileDialog dictionaryFile;

        public ConfigureMode1()
        {
            InitializeComponent();
        }

        private void btnBrowsePasswordsFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.passwordsFile = openFileDialog;
                passwordsFilePath.Text = openFileDialog.SafeFileName;
                var lineCount = File.ReadLines(openFileDialog.FileName).Count();
                if (lineCount > 50)
                    passwordsFilePreview.Text = "Preview disabled.\nFile is too big to process.";
                else
                    passwordsFilePreview.Text = File.ReadAllText(openFileDialog.FileName);
            }

            newPassword.IsEnabled = true;
            btnAddNewPassword.IsEnabled = true;
        }

        private void btnAddNewPassword_Click(object sender, RoutedEventArgs e)
        {
            if (newPassword.Text.ToString().Trim() == "")
                return;

            var password = Util.CreateMD5(newPassword.Text);
            File.AppendAllText(passwordsFile.FileName, Environment.NewLine + password);

            var lineCount = File.ReadLines(passwordsFile.FileName).Count();
            if (lineCount > 50)
                passwordsFilePreview.Text = "Preview disabled.\nFile is too big to process.";
            else
                passwordsFilePreview.Text = File.ReadAllText(passwordsFile.FileName);

            newPassword.Text = "";
        }

        private void btnBrowseDictionaryFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                dictionaryFilePath.Text = openFileDialog.SafeFileName;
            }
            this.dictionaryFile = openFileDialog;
        }

        private void btnStartService_Click(object sender, RoutedEventArgs e)
        {
            var applicationConsole = new ApplicationConsole(passwordsFile.SafeFileName, dictionaryFile.SafeFileName, Convert.ToInt32(packageSize.Text));

            NavigationService.Navigate(applicationConsole);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
