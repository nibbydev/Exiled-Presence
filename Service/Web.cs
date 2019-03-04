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
        private static readonly RestClient Client = new RestClient("https://www.pathofexile.com");

        public static async Task<Character> GetLastActiveChar() {
            if (string.IsNullOrEmpty(Config.Settings.AccountName)) {
                Console.WriteLine("No accountname set!");
                return null;
            }

            var request = new RestRequest("character-window/get-characters", Method.GET);
            request.AddParameter("accountName", Config.Settings.AccountName);
            request.AddCookie("POESESSID", Config.Settings.PoeSessionId);

            var cancellationTokenSource = new CancellationTokenSource();
            var response = await Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (response.StatusCode == HttpStatusCode.Forbidden) {
                if (string.IsNullOrEmpty(Config.Settings.PoeSessionId)) {
                    Console.WriteLine("Profile is private and POESESSID is not set!");
                    return null;
                }

                Console.WriteLine("Profile is private and POESESSID is invalid!");
                return null;
            }

            var characters = JsonUtility.Deserialize<Character[]>(response.Content);
            return characters.FirstOrDefault(t => t.LastActive != null);
        }
    }
}