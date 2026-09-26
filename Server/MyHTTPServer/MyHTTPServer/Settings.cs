using System;
using System.Collections.Generic;
using System.Text;

namespace MyHTTPServer
{
    internal class Settings
    {
        public string Port { get; set; } = "9999";
        public string Host { get; set; } = "127.0.0.1";
        public string Path { get; set; } = "connection";
    }
}
