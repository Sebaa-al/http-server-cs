using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    internal class HttpRequest : HttpMessage
    {
        public string? Method { get; private set; }
        public string? RequestUri { get; private set; }
        public string? HttpVersion { get; private set; }
        public HttpRequest(string RawMessageString) :
            base(RawMessageString)
        {

        }
        protected override void ProcessStartLine(string RequestLine)
        {
            if (string.IsNullOrWhiteSpace(RequestLine))
            {
                throw new ArgumentException($"'{nameof(RequestLine)}' no puede ser NULL ni un espacio en blanco.", 
                    nameof(RequestLine));
            }
            string[] SplittedStartLine = RequestLine.Split(' ');
            Method = SplittedStartLine[0] ?? throw new ArgumentNullException($"{Method} cannot be null");
            RequestUri = SplittedStartLine[1] ?? throw new ArgumentNullException($"{RequestUri} cannot be null");
            HttpVersion = SplittedStartLine[2] ?? throw new ArgumentNullException($"{HttpVersion} cannot be null");
        }
    }
}
