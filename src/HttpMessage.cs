﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    internal abstract class HttpMessage
    {
        protected string CRLF = "\r\n";
        public string RawMessageString { get; }
        protected string StartLine { get; set; }
        public string? Body { get; set; }

        public Dictionary<string, string> Headers { get; protected set; } = new Dictionary<string, string>();
        public HttpMessage(string RawMessageString)
        {
            if (string.IsNullOrWhiteSpace(RawMessageString))
            {
                throw new ArgumentException($"{nameof(RawMessageString)} no puede ser NULL ni un espacio en blanco.", 
                    nameof(RawMessageString));
            }

            this.RawMessageString = RawMessageString;

            var MessageLines = RawMessageString.Split(CRLF);
            Debug.Assert(MessageLines.Length > 0, "HTTP message should have at least a Start Line");

            StartLine = MessageLines[0];
            ProcessStartLine(StartLine);
            ProcessHeaders(MessageLines.Skip(1));
            ProcessBody(MessageLines.Skip(1).Skip(Headers.Count));


        }
        private void ProcessBody(IEnumerable<string> RawBody)
        {
            foreach (var L in RawBody)
            {
                Body += L;
            }
        }
        public HttpMessage()
        {
            //comment is this
        }
        protected abstract void ProcessStartLine(string startLine);
        protected void ProcessHeaders(IEnumerable<string> RawHeaders)
        {
            foreach (var RH in RawHeaders)
            {
                if(RH == "")
                {
                    break;
                }
                if (RH.Contains(":"))
                {
                    var SplittedLine = RH.Split(':');
              
                    Headers.Add(SplittedLine[0], string.Join(':', SplittedLine[1..]));
                }
            }
        }
        public override string ToString() =>
                $"{StartLine}{CRLF}" +
                $"{string.Join(CRLF, Headers.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}" +
                $"{CRLF}{CRLF}" +
                $"{Body}";
        
    }
}
