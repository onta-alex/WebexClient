using Newtonsoft.Json;
using Prom.WebEx.Client.Models;
using Prom.WebEx.Client.Responses;

namespace Prom.WebEx.Client
{

    public interface IWebExHttpClient
    {
        Task<List<RoomItem>> GetAllRoomsAsync();
        Task<RoomDetails> RoomDetailsAsync(string roomId);
        Task<List<Message>> GetMessagesAsync(string roomId);
        Task<HttpResponseMessage> GetAsync(string url);
    }

    public class WebExHttpClient : IWebExHttpClient
    {
        private const string Rooms_List = "/v1/memberships?max=1000";
        private const string Rooms_Details = "/v1/rooms/{0}"; //roomId
        private const string Messages_List = "/v1/messages?roomId={0}&max=1000000000"; //roomId

        private readonly HttpClient _httpClient;


        public WebExHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<RoomDetails> RoomDetailsAsync(string roomId)
        {
            return Get<RoomDetails>(string.Format(Rooms_Details, roomId));
        }

        public async Task<List<Message>> GetMessagesAsync(string roomId)
        {
            var response = await Get<ItemsResponse<Message>>(string.Format(Messages_List, roomId));

            return response.Items;
        }

        public async Task<List<RoomItem>> GetAllRoomsAsync()
        {
            var response = await Get<ItemsResponse<RoomItem>>(Rooms_List);

            return response.Items;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(RetryAfterSeconds(response) * 1000);
                return await GetAsync(url);
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                throw new Exception($"Something went wrong calling url: <{url}>");
            }
            return response;
        }

        private int RetryAfterSeconds(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Retry-After", out var values) && values.Any() && int.TryParse(values.First(), out var seconds))
                return seconds;

            return 10;
        }

        private async Task<T> Get<T>(string url)
        {
            HttpResponseMessage response = await GetAsync(url);

            var contentAsString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(contentAsString);

            return result;
        }
    }
}