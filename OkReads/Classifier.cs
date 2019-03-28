using Newtonsoft.Json;
using OkReads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace OkReads
{
    public class Classifier
    {
        class ClassifierResponse
        {
            public string Genre { get; set; }
        }

        private static Classifier instance = null;
        private string url;
        private HttpClient httpClient = new HttpClient();

        public static void Initialize(string url)
        {
            if (instance != null)
            {
                throw new Exception("Classifier service already initialized");
            }
            instance = new Classifier(url);
        }

        public static void Deinitialize()
        {
            Classifier.instance = null;
        }

        private Classifier(string url)
        {
            this.url = url;
        }

        ~Classifier()
        {
            instance.httpClient.Dispose();
        }

        public static async Task<string> PredictAsnyc(string description)
        {
            string responseBody = await instance.httpClient.GetStringAsync(instance.url + "?description=" + description);
            ClassifierResponse jsonResponse = JsonConvert.DeserializeObject<ClassifierResponse>(responseBody);
            return jsonResponse.Genre;
        }

        public static string Predict(string description)
        {
            Task<string> getTask = instance.httpClient.GetStringAsync(instance.url + "?description=" + description);
            getTask.Wait();
            string responseBody = getTask.Result;
            ClassifierResponse jsonResponse = JsonConvert.DeserializeObject<ClassifierResponse>(responseBody);
            return jsonResponse.Genre;
        }
    }
}