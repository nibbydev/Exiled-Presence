using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Newtonsoft.Json;
using RestSharp;

namespace Service {
    public static class Web {
        private static readonly RestClient Client = new RestClient( "https://www.pathofexile.com");
        public static string SessId { private get; set; }

        public static async Task<Character> GetLastActiveChar(string account) {
            if (string.IsNullOrEmpty(account)) {
                throw new ArgumentException();
            }
            
            var request = new RestRequest("character-window/get-characters", Method.GET);
            request.AddParameter("accountName", account);
            request.AddCookie("POESESSID", SessId);
            
            var cancellationTokenSource = new CancellationTokenSource();
            var response = await Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (response.StatusCode == HttpStatusCode.Forbidden) {
                if (string.IsNullOrEmpty(SessId)) {
                    throw new Exception("Profile is private and POESESSID is not set!");
                }
                
                throw new Exception("Profile is private and POESESSID is invalid!");
            }
            
            var characters = Deserialize<Character[]>(response.Content);
            return characters.FirstOrDefault(t => t.LastActive != null);
        }

        private static T Deserialize<T>(string json) {
            var s = new JsonSerializer();
            return s.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }
    }
}