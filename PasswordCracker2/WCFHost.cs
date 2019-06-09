using PasswordCracker.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordCracker2
{
    public class WCFHost : ApplicationConsole
    {
        private ServiceHost selftHost;

        public WCFHost() {}

        public WCFHost(string passwordsFile, string dictionaryFile, int packageSize, string address = "http://localhost:8000/PasswordCracker/")
        {
            var instance = new Cracker(passwordsFile, dictionaryFile, packageSize);
            this.selftHost = new ServiceHost(instance, new Uri(address));

            this.Run();
        }

        public WCFHost(string passwordsFile, string alphabet, Dictionary<int, int> genPasswordsLength, string address = "http://localhost:8000/PasswordCracker/")
        {
            var instance = new Cracker(passwordsFile, alphabet, genPasswordsLength);
            this.selftHost = new ServiceHost(instance, new Uri(address));

            this.Run();
        }

        private void Run()
        {
            try
            {
                WSDualHttpBinding binding = new WSDualHttpBinding();
                binding.OpenTimeout = new TimeSpan(1, 0, 0);
                binding.CloseTimeout = new TimeSpan(1, 0, 0);
                binding.SendTimeout = new TimeSpan(1, 0, 0);
                binding.ReceiveTimeout = new TimeSpan(1, 0, 0);
                selftHost.AddServiceEndpoint(typeof(ICracker), binding, "PasswordCracker");

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;

                selftHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
                selftHost.Description.Behaviors.Add(smb);

                selftHost.Open();
            }
            catch (CommunicationException ce)
            {
                MessageBoxResult result = MessageBox.Show(ce.Message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                selftHost.Abort();
                System.Windows.Application.Current.Shutdown();
            }
        }

        public void Stop()
        {
            try
            {
                selftHost.Close();
            }
            catch (CommunicationException ce)
            {
                MessageBoxResult result = MessageBox.Show(ce.Message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                selftHost.Abort();
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
