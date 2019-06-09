using PasswordCrackerClient.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PasswordCrackerClient
{
    /// <summary>
    /// Interaction logic for ApplicationConsole.xaml
    /// </summary>
    public partial class ApplicationConsole : Page
    {
        private CrackerClient client;
        private CallbackHandler callbackHandler;
        private List<string> hashedPasswords = new List<string>();

        [CallbackBehavior(UseSynchronizationContext = false)]
        public class CallbackHandler : ApplicationConsole, ICrackerCallback
        {
            public event EventHandler<string> PingReplyReceived;

            public void DisplayMessage(string message)
            {
                var evt = this.PingReplyReceived;
                evt(this, message);

                Application.Current.Dispatcher.Invoke(() => LogMessage(message), DispatcherPriority.ContextIdle);
            }

            public void UpdatePasswordsList(string password)
            {
                var evt = this.PingReplyReceived;
                evt(this, password);

                Application.Current.Dispatcher.Invoke(() => UpdatePasswords(password), DispatcherPriority.ContextIdle);
            }
        }

        public ApplicationConsole()
        {
            InitializeComponent();
        }

        public ApplicationConsole(string serviceUrl = "http://localhost:8000/PasswordCracker/") : this()
        {
            this.callbackHandler = new CallbackHandler();
            this.callbackHandler.PingReplyReceived += (sender, message) => Dispatcher.Invoke(() => LogMessage(message), DispatcherPriority.ContextIdle);
            this.callbackHandler.PingReplyReceived += (sender, message) => Dispatcher.Invoke(() => UpdatePasswords(message), DispatcherPriority.ContextIdle);
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("[INFO] Disconnecting from server...");
            btnDisconnect.IsEnabled = false;
            btnBack.IsEnabled = true;
            LogMessage("[INFO] Disconnected!");
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public void UpdatePasswords(string password)
        {
            passwordsToCrack.Items.Remove(password);
            hashedPasswords.Remove(password);
        }

        public void LogMessage(string message)
        {
            consoleLog.Items.Add(DateTime.Now.ToString("H:mm:ss") + " : " + message);
        }

        private void WindowLauncher(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => RunApplication()), DispatcherPriority.ContextIdle, null);
        }

        private void RunApplication()
        {
            this.client = new CrackerClient(new InstanceContext(callbackHandler));
            //this.client.Endpoint.Address = new EndpointAddress(serviceUrl);
            client.Open();
            LogMessage("[INFO] Connecting to server...");
            client.Connect();
            LogMessage("[INFO] Connected!");

            Stopwatch stopwatch;
            Generator gen = new Generator();
            LogMessage("[INFO] Fetching server work mode...");

            // 0 - RNG / 1 - Dictionary
            var mode = client.GetWorkMode();

            LogMessage("[INFO] Downloading passwords to crack...");
            this.hashedPasswords = client.GetPasswords();
            var data = new List<string>();

            if (mode == 0)
            {
                LogMessage("[INFO] Fetching brute force alphabet...");
                gen = new Generator(client.GetBFAlphabet());
            }

            LogMessage("[INFO] Processing now...");
            stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();
            do
            {
                if (mode == 0)
                {
                    LogMessage("[INFO] Fetching data for brute force...");
                    var prefix = client.GetBFNextPrefix();
                    var range = client.GetBFGenerationRange();
                    if (range.Count() <= 0 || prefix.Trim() == "")
                        break;

                    LogMessage("[INFO] Generating new batch of passwords...");
                    data = gen.Generate(prefix, Convert.ToInt32(range.First().Value));
                }
                else
                {
                    LogMessage("[INFO] Fetching dictionary...");
                    data = client.GetDictionary();
                    if (!data.Any())
                        break;
                }

                if (!stopwatch.IsRunning)
                    stopwatch.Start();

                for (int i = 0; i < data.Count(); i++)
                {
                    var passwordHashed = Util.CreateMD5(data[i]);
                    for (int j = 0; j < hashedPasswords.Count(); j++)
                    {
                        if (passwordHashed.ToLower() == hashedPasswords[j].ToLower())
                        {
                            stopwatch.Stop();
                            var result = new Dictionary<string, string> { { passwordHashed, data[i] } };

                            client.SendResult(result, stopwatch.Elapsed);
                            LogMessage(String.Format("[INFO] Password found! {0} => {1}", passwordHashed, data[i]));
                            hashedPasswords.RemoveAt(j);
                        }
                    }
                }
            } while (hashedPasswords.Count() > 0);

            LogMessage("[INFO] Finished!");
            LogMessage(String.Format("[INFO] Elapsed {0}min {1}s {2}ms", stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds, stopwatch.Elapsed.Milliseconds));
        }
    }
}
