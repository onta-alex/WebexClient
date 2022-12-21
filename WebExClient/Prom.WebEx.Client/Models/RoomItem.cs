using Newtonsoft.Json;

namespace Prom.WebEx.Client.Models
{
    public class RoomItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("roomId")]
        public string RoomId { get; set; }

        [JsonProperty("roomType")]
        public string RoomType { get; set; }

        [JsonProperty("personId")]
        public string PersonId { get; set; }

        [JsonProperty("personEmail")]
        public string PersonEmail { get; set; }

        [JsonProperty("personDisplayName")]
        public string PersonDisplayName { get; set; }

        [JsonProperty("personOrgId")]
        public string PersonOrgId { get; set; }

        [JsonProperty("isModerator")]
        public bool IsModerator { get; set; }

        [JsonProperty("isMonitor")]
        public bool IsMonitor { get; set; }

        [JsonProperty("isRoomHidden")]
        public bool IsRoomHidden { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }
    }
}
