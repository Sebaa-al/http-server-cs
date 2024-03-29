using codecrafters_http_server.src;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
//Console.WriteLine(">>>");
using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger Logger = factory.CreateLogger("program");
var Server = new HttpServer(IPAddress.Any, 4221, Logger);
Server.Start();

//Uncomment this block to pass the first stage

//TcpListener Server = new TcpListener(IPAddress.Any, 4221);
//Server.Start();
//using (Socket socket = Server.AcceptSocket())

string ExtractHttpPath(string? RequestString)
{
    if (string.IsNullOrEmpty(RequestString))
    {
        throw new ArgumentException($"'{nameof(RequestString)}' cannot be null or whitespace.", nameof(RequestString));
    }
    var RequestLines = RequestString.Split('\n');
    Debug.Assert(RequestLines.Length > 0, "HTTP request should have at least 1 line");

    var SplittedFirstLine = RequestLines[0].Split(' ');
    Debug.Assert(SplittedFirstLine.Length == 3, "Start line should have three space-separated values!");

    var Path = SplittedFirstLine[1];
    Debug.Assert(!string.IsNullOrWhiteSpace(Path), "the path shouldn't be empty");
    return Path;

}