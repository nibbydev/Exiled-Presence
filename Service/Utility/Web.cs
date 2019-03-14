using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using RestSharp;

namespace Service {
    public static class Web {
        private static readonly RestClient Client = new RestClient();
        private const string ReleaseApi = "https://api.github.com/repos/siegrest/Exiled-Presence/releases";
        private const string PoeApi = "https://www.pathofexile.com/character-window/get-characters";

        /// <summary>
        /// Queries all the characters of the user and returns the last active one
        /// </summary>
        public static async Task<Character> GetLastActiveChar(string accountName, string sessId) {
            if (string.IsNullOrEmpty(accountName)) {
                throw new ArgumentException("No account name set!");
            }

            var request = new RestRequest(PoeApi, Method.GET);
            request.AddParameter("accountName", accountName);

            // Add session Id cookie if present
            if (!string.IsNullOrEmpty(sessId)) {
                request.AddCookie("POESESSID", sessId);
            }

            var cancellationTokenSource = new CancellationTokenSource();
            var response = await Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (response.StatusCode == HttpStatusCode.Forbidden) {
                if (string.IsNullOrEmpty(sessId)) {
                    throw new Exception("Profile is private and POESESSID is not set!");
                }

                throw new Exception("Profile is private and POESESSID is invalid!");
            }

            var characters = JsonUtility.Deserialize<Character[]>(response.Content);
            return characters.FirstOrDefault(t => t.LastActive != null);
        }

        /// <summary>
        /// Gets releases from Github
        /// </summary>
        /// <returns>List of ReleaseEntry objects or null on failure</returns>
        public static async Task<Release> GetLatestRelease() {
            var request = new RestRequest(ReleaseApi, Method.GET);
            var cancellationTokenSource = new CancellationTokenSource();
            var response = await Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            var releases = JsonUtility.Deserialize<Release[]>(response.Content);
            if (releases == null || releases.Length == 0) {
                return null;
            }

            return releases.FirstOrDefault(t => !t.prerelease);
        }
    }
}