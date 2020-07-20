using System;
using Polly;
using Polly.Retry;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace RenameBranch
{
    internal sealed class Rest
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _policy;

        public Rest(string token)
        {
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", "Rename Branch");
            this._policy = this.CreatePolicy();
        }

        public async Task<T> Get<T>(string url)
        {
            return await this._policy.ExecuteAsync(async () =>
            {
                var response = await this._httpClient.GetAsync(url);
                return await this.GetContent<T>(response);
            });
        }

        public async Task<TResponse> Post<TResponse>(string url, object request)
        {
            return await this._policy.ExecuteAsync(async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json);
                var response = await this._httpClient.PostAsync(url, content);
                return await this.GetContent<TResponse>(response);
            });
        }

        public async Task Patch(string url, object request)
        {
            await this._policy.ExecuteAsync(async () =>
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json);
                var response = await this._httpClient.PatchAsync(url, content);
                response.EnsureSuccessStatusCode();
            });
        }

        public async Task Delete(string url)
        {
            await this._policy.ExecuteAsync(async () =>
            {
                var response = await this._httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
            });
        }

        private async Task<T> GetContent<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json);
            return result;
        }

        private AsyncRetryPolicy CreatePolicy(int retryCount = 3, int factor = 1)
            => Policy.Handle<Exception>(e => true)
                     .WaitAndRetryAsync(retryCount: retryCount,
                                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt * factor))
               );
    }
}