using ConsoleClient.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        [CallbackBehavior(UseSynchronizationContext = false)]
        public class CallbackHandler : ICrackerCallback
        {
            public void DisplayMessage(string message)
            {
                Console.WriteLine(message);
            }

            public void UpdatePasswordsList(string password) {}
        }


        static void Main(string[] args)
        {
            var client = new CrackerClient(new InstanceContext(new CallbackHandler()));
            client.Open();
            Console.WriteLine("Connecting to server...");
            client.Connect();

            Stopwatch stopwatch;
            Generator gen = new Generator();
            var hashedPasswords = new List<string>();
            var data = new List<string>();
            string PASSWORDS_FILE;
            string DICTIONARY_FILE;

            Console.WriteLine("Fetching server work mode...");
            // 0 - RNG / 1 - Dictionary
            var mode = client.GetWorkMode();

            Console.WriteLine("Fetching passwords to crack...");
            //hashedPasswords = client.GetPasswords();
            PASSWORDS_FILE = client.GetPasswordsFileName();
            DICTIONARY_FILE = client.GetDictionaryFileName();
            using (Stream file = File.OpenWrite(PASSWORDS_FILE))
            {
                client.GetPasswordsFile().CopyTo(file);
            }
            hashedPasswords = System.IO.File.ReadLines(PASSWORDS_FILE).ToList();

            if (mode == 0)
            {
                Console.WriteLine("Fetching brute force alphabet...");
                gen = new Generator(client.GetBFAlphabet());
            }

            if (mode == 1 && !client.IsDictionaryUpToDate(Util.CalculateFileMD5(DICTIONARY_FILE)))
            {
                using (Stream file = File.OpenWrite(DICTIONARY_FILE))
                {
                    client.GetDictionaryFile().CopyTo(file);
                }
            }

            stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();
            do
            {
                if (mode == 0)
                {
                    Console.WriteLine("Fetching data for brute force...");
                    var prefix = client.GetBFNextPrefix();
                    var range = client.GetBFGenerationRange();
                    if (range.Count() <= 0 || prefix.Trim() == "")
                    {
                        hashedPasswords.Clear();
                        break;
                        Environment.Exit(0);
                    }

                    //Console.WriteLine("Generating new batch of passwords...");
                    data = gen.Generate(prefix, Convert.ToInt32(range.First().Value));
                }
                else
                {
                    Console.WriteLine("Fetching dictionary...");
                    var range = client.GetDictionaryRange();
                    if (range.Count() <= 0)
                    {
                        hashedPasswords.Clear();
                        break;
                        Environment.Exit(0);
                    }

                    var diff = range.First().Value - range.First().Key;
                    data = System.IO.File.ReadLines(DICTIONARY_FILE).Skip(range.First().Key).Take(diff).ToList();
                    if (!data.Any())
                        break;
                }

                Console.WriteLine("Processing now...");
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
                            Console.WriteLine(String.Format("Password found! {0} => {1}", passwordHashed, data[i]));
                            hashedPasswords.RemoveAt(j);
                        }
                    }
                }
                data.Clear();
            } while (hashedPasswords.Count() > 0);

            client.Close();
            
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}
