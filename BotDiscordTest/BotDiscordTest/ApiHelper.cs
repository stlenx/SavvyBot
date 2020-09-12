using System.Net.Http;
using System.Net.Http.Headers;

namespace BotDiscordTest
{
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; private set; }

        public static void InitializeClient()
        {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}