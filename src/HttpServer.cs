﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    internal class HttpServer : TcpServer
    {
        private string[] SupportedMethods =
        {
            "GET",
            "POST",
        };
        private Encoding DefaultEncoding = Encoding.ASCII;
        private string ServerHttpVersion = "HTTP/1.1";
        public string? Dir { get; }

        public HttpServer(IPAddress Ip, ushort PortNumber, ILogger Logger, string? Dir) 
            : base(Ip, PortNumber, Logger)
        {
            Logger.LogInformation($"Server base dir: {Dir}");
            this.Dir = Dir;
        }

        protected override async Task ProcessRequestAsync(Socket socket)
        {
            try
            {
                var Bytes = new byte[MaxRecvBytes];
                int ReceivedBytesCount = await socket.ReceiveAsync(Bytes, SocketFlags.None);
                Bytes = Bytes[0..ReceivedBytesCount];
                Logger.LogInformation($"{nameof(ReceivedBytesCount)}: {ReceivedBytesCount}");
                Logger.LogInformation($"------- Thread {Thread.CurrentThread.Name} {Thread.CurrentThread.ManagedThreadId} processing request");
                string RequestString = DefaultEncoding.GetString(Bytes);
                Logger.LogInformation($"{nameof(RequestString)}: {RequestString}");

                var ParsedRequest = new HttpRequest(RequestString);

                if (string.IsNullOrWhiteSpace(ParsedRequest.RequestUri))                
                    throw new ArgumentNullException("Request URI cannot be null or white space");        

                if (!SupportedMethods.Contains(ParsedRequest.Method))
                {
                    await socket.SendAsync(DefaultEncoding.GetBytes(HttpResponse.NotImplemented(ServerHttpVersion).ToString()), SocketFlags.None);
                    return;
                }

                switch (ParsedRequest.RequestUri.ToLowerInvariant())
                {

                    case Routes.Base:
                        await socket.SendAsync(DefaultEncoding.GetBytes(HttpResponse.OK(ServerHttpVersion).ToString()), SocketFlags.None);
                        break;

                    case var uri when uri.StartsWith(Routes.Echo):
                        await Echo(ParsedRequest);
                        break;

                    case var uri when uri.StartsWith(Routes.UserAgent):
                        await UserAgent(ParsedRequest);
                        break;

                    case var uri when uri.StartsWith(Routes.Files):
                        await Files(ParsedRequest);
                        break;
                    default:
                        await NotFound();
                        break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally 
            {
                socket.Close(); 
            }

            async Task Echo(HttpRequest ParsedRequest)
            {
                Logger.LogInformation($"Handling {MethodBase.GetCurrentMethod()?.Name}");
                var Response = ParsedRequest.RequestUri?[$"{Routes.Echo}/".Length..];
                var httpResponse = HttpResponse.OK(ServerHttpVersion,
                    new Dictionary<string, string>
                    {
                    { HttpHeaderConstants.ContentType, HttpHeaderConstants.TextPlain },
                    { HttpHeaderConstants.ContentLength, Response?.Length.ToString() ?? "0" }
                    }, Response);
                var ResponseAsString = httpResponse.ToString();
                await socket.SendAsync(DefaultEncoding.GetBytes(ResponseAsString), SocketFlags.None);
            }

            async Task UserAgent(HttpRequest ParsedRequest)
            {
                Logger.LogInformation($"Handling {MethodBase.GetCurrentMethod()?.Name}");
                var UserAgent = ParsedRequest.Headers[HttpHeaderConstants.UserAgent]?.Trim();
                var Response = HttpResponse.OK(ServerHttpVersion, new Dictionary<string, string>()
                    {
                        { HttpHeaderConstants.ContentType, HttpHeaderConstants.TextPlain },
                        { HttpHeaderConstants.ContentLength, UserAgent?.Length.ToString() ?? "0" }
                    }, UserAgent);

                await socket.SendAsync(DefaultEncoding.GetBytes(Response.ToString()), SocketFlags.None);
            }
            async Task Files(HttpRequest ParsedRequest)
            {
                if (Dir is null)
                {
                    Logger.LogError($"Dir is null, can't handle files!");
                    await socket.SendAsync(DefaultEncoding.GetBytes(HttpResponse.BadRequest(ServerHttpVersion).ToString()), SocketFlags.None);
                }
                Logger.LogInformation($"Handling {MethodBase.GetCurrentMethod()?.Name}");
                var SplittedUri = ParsedRequest.RequestUri?.Split('/');
                if (SplittedUri is null || SplittedUri.Length < 3)
                {
                    Logger.LogError("Expected a filename, Send it");
                    await NotFound();
                }
                var TargetFile = Path.Combine(Dir, SplittedUri[2]);
                if (ParsedRequest.IsGet)
                {
                    if (File.Exists(TargetFile))
                    {
                        var FileContents = await File.ReadAllTextAsync(TargetFile);
                        var FileResponse = HttpResponse.OK(ServerHttpVersion, new Dictionary<string, string>()
                    {
                        { HttpHeaderConstants.ContentType, HttpHeaderConstants.OctetStream },
                        { HttpHeaderConstants.ContentLength, FileContents.Length.ToString() }
                    }, FileContents);
                        await SendAsync(FileResponse);
                    }
                    else
                    {
                        await NotFound();
                    }
                }
                else if (ParsedRequest.IsPost)
                {
                    await File.WriteAllTextAsync(TargetFile, ParsedRequest.Body);
                    await SendAsync(HttpResponse.Created(ServerHttpVersion));
                }
                else
                {
                    throw new Exception("WTF!");
                }

            }
            async Task SendAsync(HttpResponse Response)
            {
                await socket.SendAsync(DefaultEncoding.GetBytes(Response.ToString()), SocketFlags.None);
            }
            async Task NotFound()
            {
                Logger.LogInformation($"Handling {MethodBase.GetCurrentMethod()?.Name}");
                await socket.SendAsync(DefaultEncoding.GetBytes(HttpResponse.NotFound(ServerHttpVersion).ToString()), SocketFlags.None);
            }
        }
    }
}
