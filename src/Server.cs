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

