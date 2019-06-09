using PasswordCracker2.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace PasswordCracker2
{
    public partial class ApplicationConsole : Page
    {
        private WCFHost wcfhost;
        private CallbackHandler callbackHandler;
        private CrackerClient client;

        [CallbackBehavior(UseSynchronizationContext = false)]
        public class CallbackHandler : ApplicationConsole, ICrackerCallback
        {
            public event EventHandler<string> PingReplyReceived;

            public void DisplayMessage(string message)
            {
                var evt = this.PingReplyReceived;
                evt(this, message);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LogMessage(message);
                }, DispatcherPriority.ContextIdle);
            }

            public void UpdatePasswordsList(string password){}
        }

        public ApplicationConsole()
        {
            InitializeComponent();
        }

        public ApplicationConsole(string passwordsFile, string dictionaryFile, int packageSize) : this()
        {
            LogMessage("Starting PasswordCracker service...");
            LogMessage("Selected work mode => DICTIONARY");
            wcfhost = new WCFHost(passwordsFile, dictionaryFile, packageSize);
            LogMessage("PasswordCracker service started!");

            callbackHandler = new CallbackHandler();
            callbackHandler.PingReplyReceived += (sender, message) =>
            {
                Dispatcher.Invoke(() => LogMessage(message), DispatcherPriority.ContextIdle);
            };
            client = new CrackerClient(new InstanceContext(callbackHandler));
            client.Open();
            client.Connect();
        }

        public ApplicationConsole(string passwordsFile, string bruteForceAlphabet, Dictionary<int, int> genPasswordsLength) : this()
        {
            LogMessage("Starting PasswordCracker service...");
            LogMessage("Selected work mode => Brute Force");
            wcfhost = new WCFHost(passwordsFile, bruteForceAlphabet, genPasswordsLength);
            LogMessage("PasswordCracker service started!");

            callbackHandler = new CallbackHandler();
            callbackHandler.PingReplyReceived += (sender, message) =>
            {
                Dispatcher.Invoke(() => LogMessage(message), DispatcherPriority.ContextIdle);
            };
            client = new CrackerClient(new InstanceContext(callbackHandler));
            client.Open();
            client.Connect();
        }

        private void btnStopService_Click(object sender, RoutedEventArgs e)
        {
            client.Close();

            LogMessage("Stopping PasswordCracker service...");
            this.wcfhost.Stop();
            this.wcfhost = null;
            LogMessage("PasswordCracker service stopped!");

            btnStopService.IsEnabled = false;
            btnBack.IsEnabled = true;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public void LogMessage(string message)
        {
            consoleLog.Items.Add(DateTime.Now.ToString("H:mm:ss") + " : " + message);
        }
    }
}
