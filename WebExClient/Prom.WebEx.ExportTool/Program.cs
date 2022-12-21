// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Prom.WebEx.Client;

try
{
    var bearer = ReadString("Bearer Value (Log in on https://developer.webex.com/docs/api/v1/messages/list-direct-messages to get it):", null);

    var webexBaseAddress = "https://webexapis.com";

    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(webexBaseAddress)
    };

    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer);

    var options = new WebExExportEngineOptions
    {
        ExportPath = ReadString("Export path:", DefaultExportPath()),
        ExportRoomAttachment = ReadBool("Export chat attachments:"),
        ExportDirectChats = ReadBool("Export direct chats:"),
        ExportGroupChats = ReadBool("Export group chats:"),
        ExportObsoleteRooms = ReadBool("Export obsolete chats (Obsolete users):")
    };
    Console.WriteLine();

    var engine = new WebExExportEngine(new WebExHttpClient(httpClient), options);
    var sw = Stopwatch.StartNew();

    await engine.Export();

    sw.Stop();

    Console.WriteLine("Done in ...{0}", sw.Elapsed);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}



string ReadString(string message, string defaultValue)
{
    Console.WriteLine(message);

    if (!string.IsNullOrEmpty(defaultValue))
        Console.Write($" default ({defaultValue}):");

    var readMessage = Console.ReadLine();

    if (string.IsNullOrEmpty(defaultValue))
    {
        while (string.IsNullOrEmpty(readMessage))
        {
            Console.WriteLine(message);
            readMessage = Console.ReadLine();
        }
    }

    return string.IsNullOrEmpty(readMessage) ? defaultValue : readMessage;
}

bool ReadBool(string message)
{
    var acceptedValues = new HashSet<char> { 'y', 'n' };
    var r = '0';

    do
    {
        Console.WriteLine();
        Console.Write(message + "y(Yes)/n(No):");

        r = Console.ReadKey().KeyChar;
    } while (!acceptedValues.Contains(r));

    return r == 'y';
}

string BoolString(bool v) => v ? "Yes" : "No";


string DefaultExportPath()
{
    return Environment.CurrentDirectory;
}