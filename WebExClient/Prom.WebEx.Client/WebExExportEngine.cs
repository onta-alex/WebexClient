using System.Text;
using Prom.WebEx.Client.Extensions;
using Prom.WebEx.Client.Models;

namespace Prom.WebEx.Client
{
    public class WebExExportEngineOptions
    {
        public string ExportPath { get; set; }
        public bool ExportObsoleteRooms { get; set; }
        public bool ExportRoomAttachment { get; set; }
        public bool ExportDirectChats { get; set; }
        public bool ExportGroupChats { get; set; }
    }
    public class WebExExportEngine
    {
        private readonly IWebExHttpClient _webexClient;
        private readonly WebExExportEngineOptions _options;

        public WebExExportEngine(IWebExHttpClient webexClient, WebExExportEngineOptions options)
        {
            _webexClient = webexClient;
            _options = options;
        }

        public async Task Export()
        {
            var allRooms = await _webexClient.GetAllRoomsAsync();

            Console.WriteLine("Found {0} rooms", allRooms.Count);

            if (_options.ExportDirectChats)
                await ExportRooms(allRooms, "direct");
            if (_options.ExportGroupChats)
                await ExportRooms(allRooms, "group");
        }

        private async Task ExportRooms(List<RoomItem> allRooms, string roomType)
        {
            var rooms = allRooms.Where(r => r.RoomType == roomType).ToList();

            var roomTypePath = Path.Combine(_options.ExportPath, roomType);

            CreateDirectoryIfNotExists(roomTypePath);

            Console.WriteLine("Exporting {0} {1} rooms to: <{2}>", rooms.Count, roomType, roomTypePath);

            for (int i = 0; i < rooms.Count; i++)
            {
                RoomItem r = rooms[i];
                await ExportRoom(r, roomTypePath, i);
            }
        }

        private async Task ExportRoom(RoomItem room, string path, int roomIndex)
        {
            var roomDetails = await _webexClient.RoomDetailsAsync(room.RoomId);

            if (_options.ExportObsoleteRooms && IsObsoleteRoom(roomDetails)) return;

            var roomPath = GetRoomPath(path, roomDetails);

            Console.Write($"{DateTime.Now:T}: ({roomIndex + 1})Exporting room {roomDetails.Title}: ");

            try
            {
                await ExportRoomMessages(roomPath, room.RoomId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong exporting room: {0}", roomDetails.Title);
                Console.WriteLine(ex);
            }
        }

        private async Task ExportRoomMessages(string roomPath, string roomId)
        {
            var messages = await _webexClient.GetMessagesAsync(roomId);

            if (messages.Count == 0)
            {
                Console.WriteLine($"...Skiping room because it contains no messages.");
                return;
            }

            CreateDirectoryIfNotExists(roomPath);

            messages.Reverse();

            await WriteMessages(roomPath, messages);

            Console.WriteLine($"...Exported {messages.Count} messages");
        }

        private async Task WriteMessages(string roomPath, List<Message> messages)
        {
            using (var progress = new ProgressBar())
            {
                var messagesPath = Path.Combine(roomPath, "chat.txt");
                var messagesAsString = MessagesToString(messages, roomPath, progress);
                await File.WriteAllTextAsync(messagesPath, await messagesAsString);
            }
        }

        private static string GetRoomPath(string path, RoomDetails roomDetails)
        {
            if (IsObsoleteRoom(roomDetails))
                return Path.Combine(path, "Obsolete_User").NextAvailableDirectory();

            return Path.Combine(path, roomDetails.Title.Replace(" ", "_").Replace("/", "-"));
        }

        private static bool IsObsoleteRoom(RoomDetails roomDetails)
        {
            return roomDetails.Title == "Empty Title";
        }

        private async Task<string> MessagesToString(List<Message> messages, string roomPath, ProgressBar progress)
        {
            var contentPath = Path.Combine(roomPath, "content");
            var sb = new StringBuilder();

            Message lastMessage = null;
            var index = 0;

            while (index < messages.Count)
            {
                var message = messages[index++];
                if (!ShouldMerge(lastMessage, message))
                {
                    sb.AppendLine($">>{NameFromEmail(message.PersonEmail)} {message.Created:g}");
                }

                await WriteMessage(sb, message, contentPath);

                progress.Report((double)index / messages.Count);

                lastMessage = message;
            }

            return sb.ToString();
        }

        private async Task WriteMessage(StringBuilder sb, Message message, string contentPath)
        {
            if (string.IsNullOrEmpty(message.Text) && _options.ExportRoomAttachment)
            {
                await WriteContentMessage(sb, message, contentPath);
            }
            else
            {
                WriteTextMessage(sb, message);
            }
        }

        private async Task WriteContentMessage(StringBuilder sb, Message message, string contentPath)
        {
            if (message.Files == null) return;

            for (int i = 0; i < message.Files.Length; i++)
            {
                var contentUrl = message.Files[i];
                await WriteContentMessage(sb, contentPath, contentUrl);
            }
        }

        private async Task WriteContentMessage(StringBuilder sb, string contentPath, string url)
        {
            var response = await _webexClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Something went wrong getting content from {0}", url);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                Console.WriteLine(response.Headers.ToString());
                Console.WriteLine(response.StatusCode.ToString());
                return;
            }

            if (!response.Content.Headers.TryGetValues("Content-Disposition", out var values))
            {
                Console.WriteLine("Cannot find Content-Disposition header");
                return;
            }

            CreateDirectoryIfNotExists(contentPath);
            string fileName = GetFileName(values);

            var filePath = Path.Combine(contentPath, fileName).NextAvailableFilename();
            using var fs = new FileStream(filePath, FileMode.CreateNew);
            await response.Content.CopyToAsync(fs);

            sb.AppendLine($"attachment [./content/{fileName}]");
        }

        private static string GetFileName(IEnumerable<string> values)
        {
            if (!values.Any())
            {
                Console.WriteLine("Cannot find any Content-Disposition header");
                return null;
            }

            var segments = values.First().Split('=');

            if (segments.Length < 2)
            {
                Console.WriteLine($"Cannot extract file name from {values.First()}");
                return null;
            }

            return segments[1].Trim('"');
        }

        private static void WriteTextMessage(StringBuilder sb, Message message)
        {
            sb.AppendLine(message.Text);
        }

        private static bool ShouldMerge(Message lastMessage, Message message)
        {
            return lastMessage != null &&
                   lastMessage.PersonEmail == message.PersonEmail &&
                   !OffsetIsOver1Min(lastMessage, message);
        }

        private static bool OffsetIsOver1Min(Message lastMessage, Message message)
        {
            return (message.Created - lastMessage.Created) > new TimeSpan(0, 1, 0);
        }

        private static string NameFromEmail(string emailAddress) => emailAddress.Split('@')[0];

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}