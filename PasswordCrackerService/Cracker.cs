using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Windows.Controls;

namespace PasswordCracker.Service
{
    public enum Mode { RNG, DICTIONARY };

    public class Client
    {
        public string ipaddress { get; set; }
        public string port { get; set; }
        public Stopwatch stopwatch { get; set; }
        public ICrackerCallback callback { get; set; }
        public Client() => this.stopwatch = Stopwatch.StartNew();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Cracker : ICracker
    {
        private string DICTIONARY_FILE;
        private string PASSWORDS_FILE;
        private string ALPHA_NUM;
        private Dictionary<int, int> generatedPasswordsLength = new Dictionary<int, int>();
        private int PACKAGE_SIZE;

        private Mode mode;
        private List<Client> clients = new List<Client>();
        private int currentIndex;

        public Cracker() {}

        public Cracker(string passwordsFile, string dictionaryFile, int packageSize)
        {
            this.currentIndex = 0;
            this.PASSWORDS_FILE = passwordsFile;

            this.mode = Mode.DICTIONARY;
            this.DICTIONARY_FILE = dictionaryFile;
            this.PACKAGE_SIZE = packageSize;
        }

        public Cracker(string passwordsFile, string alphabet, Dictionary<int, int> genPasswordsLength)
        {
            this.currentIndex = 0;
            this.PASSWORDS_FILE = passwordsFile;

            this.mode = Mode.RNG;
            this.ALPHA_NUM = alphabet;
            this.generatedPasswordsLength = genPasswordsLength;
        }

        public void Connect()
        {
            clients.Add(new Client() {
                ipaddress = incConnection.Address,
                port = incConnection.Port.ToString(),
                callback = Callback
            });

            if (clients.Count > 1)
            {
                clients.First().callback.DisplayMessage(String.Format("New client connected from {0}:{1}.", incConnection.Address, incConnection.Port));
                clients.First().callback.DisplayMessage(String.Format("Total connected clients {0}", clients.Count() - 1));
                Callback.DisplayMessage("Connected!");
            }
        }

        public int GetWorkMode() => (int)this.mode;

        public List<string> GetPasswords() => File.ReadAllLines(PASSWORDS_FILE).ToList();

        public Stream GetPasswordsFile() => new FileStream(PASSWORDS_FILE, FileMode.Open, FileAccess.Read);

        public string GetPasswordsFileName() => PASSWORDS_FILE;

        public void SendResult(Dictionary<string, string> result, TimeSpan elapsed)
        {
            var client = clients.FirstOrDefault(x => x.ipaddress == incConnection.Address && x.port == incConnection.Port.ToString());
            var workTime = client.stopwatch.Elapsed - elapsed;
            clients.First().callback.DisplayMessage(String.Format("[{0}:{1}] {2} = {3}", incConnection.Address, incConnection.Port, result.First().Key, result.First().Value));
            clients.First().callback.DisplayMessage(String.Format("Elapsed: {0}m {1}s {2}ms", workTime.Minutes, workTime.Seconds, workTime.Milliseconds));
        }

        // --------------------------------
        // Dictionary mode methods
        public bool IsDictionaryUpToDate(string md5)
        {
            if (CalculateMD5(DICTIONARY_FILE) != md5)
                return false;

            return true;
        }

        public List<string> GetDictionary()
        {
            var lineCount = File.ReadLines(DICTIONARY_FILE).Count();
            List<string> lines = new List<string>();
            if (lineCount < currentIndex + PACKAGE_SIZE)
            {
                lines = File.ReadLines(DICTIONARY_FILE).Skip(Convert.ToInt32(currentIndex)).Take(PACKAGE_SIZE).ToList();
                currentIndex += PACKAGE_SIZE;
            }
            else
            {
                var take = lineCount - currentIndex;
                lines = File.ReadLines(DICTIONARY_FILE).Skip(Convert.ToInt32(currentIndex)).Take(take).ToList();
                currentIndex += take;
            }
            
            return lines;
        }

        public Stream GetDictionaryFile() => new FileStream(DICTIONARY_FILE, FileMode.Open, FileAccess.Read);

        public string GetDictionaryFileName() => DICTIONARY_FILE;

        public Dictionary<int, int> GetDictionaryRange()
        {
            var client = clients.FirstOrDefault(x => x.ipaddress == incConnection.Address && x.port == incConnection.Port.ToString());
            if (!client.stopwatch.IsRunning)
                client.stopwatch.Restart();

            var lineCount = File.ReadLines(DICTIONARY_FILE).Count();
            var range = new Dictionary<int, int>();
            if (currentIndex >= lineCount)
                return range;

            if (lineCount < currentIndex + PACKAGE_SIZE)
            {
                var diff = lineCount - currentIndex;
                range.Add(currentIndex, currentIndex + diff);
                currentIndex = currentIndex + diff;
            }
            else
            {
                range.Add(currentIndex, currentIndex + PACKAGE_SIZE);
                currentIndex = currentIndex + PACKAGE_SIZE;
            }

            return range;
        }

        // --------------------------------
        // Brute Force mode methods
        public string GetBFAlphabet() => ALPHA_NUM;

        public string GetBFNextPrefix()
        {
            if (currentIndex >= ALPHA_NUM.Length)
                return "\0";

            char c = ALPHA_NUM[currentIndex];
            currentIndex++;
            return Convert.ToString(c);
        }

        public Dictionary<int, int> GetBFGenerationRange() => generatedPasswordsLength;

        // ----------------------------------------------------------------

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        ICrackerCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<ICrackerCallback>();
            }
        }

        RemoteEndpointMessageProperty incConnection
        {
            get
            {
                return OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            }
        }
    }
}
