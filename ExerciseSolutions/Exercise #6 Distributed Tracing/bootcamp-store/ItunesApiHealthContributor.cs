using System;
using System.Net.Http;
using System.Threading.Tasks;
using Steeltoe.Common.HealthChecks;

namespace bootcamp_store
{
    public class ItunesApiHealthContributor : IHealthContributor
    {
        public string Id => "itunesApiHealthContributor";
        private readonly HttpClient _httpClient;

        public ItunesApiHealthContributor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HealthCheckResult Health()
        {
            var result = new HealthCheckResult
            {
                Status = HealthStatus.UP,
                Description = "Current Status of Itunes API Integration!"
            };

            var response = string.Empty;
            try
            {
                // Ideally this would be called asynchronously
                response = QueryItunesHubApiAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result.Status = HealthStatus.DOWN;
                result.Details.Add("status", "DOWN");
            }

            result.Details.Add("endpoint", "itunes.apple.com/search");
            result.Details.Add("status", "UP");
            return result;
        }

        public async Task<string> QueryItunesHubApiAsync()
        {
            return await _httpClient.GetStringAsync("https://itunes.apple.com/search");
        }
    }
}