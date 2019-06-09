using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace PasswordCracker.Service
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICrackerCallback))]
    public interface ICracker
    {
        [OperationContract(IsOneWay = true)]
        void Connect();

        [OperationContract]
        int GetWorkMode();

        [OperationContract]
        List<string> GetPasswords();

        [OperationContract]
        Stream GetPasswordsFile();

        [OperationContract]
        string GetPasswordsFileName();

        [OperationContract(IsOneWay = true)]
        void SendResult(Dictionary<string, string> result, TimeSpan elapsed);

        // --------------------------------
        // Dictionary mode methods
        [OperationContract]
        bool IsDictionaryUpToDate(string md5);

        [OperationContract]
        List<string> GetDictionary();

        [OperationContract]
        Stream GetDictionaryFile();

        [OperationContract]
        Dictionary<int, int> GetDictionaryRange();

        [OperationContract]
        string GetDictionaryFileName();

        // --------------------------------
        // Brute Force mode methods
        [OperationContract]
        string GetBFAlphabet();

        [OperationContract]
        string GetBFNextPrefix();

        [OperationContract]
        Dictionary<int, int> GetBFGenerationRange();
    }

    public interface ICrackerCallback
    {
        [OperationContract(IsOneWay = true)]
        void DisplayMessage(string message);

        [OperationContract(IsOneWay = true)]
        void UpdatePasswordsList(string password);
    }
}
