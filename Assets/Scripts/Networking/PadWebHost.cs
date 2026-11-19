using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Kimaya.Networking
{
    /// <summary>
    /// Sirve la PWA ya compilada por HTTP, para que el QR abra el control en el celular.
    /// Una página https no puede hablar con el juego por ws://.
    /// </summary>
    public sealed class PadWebHost
    {
        public const int Port = 8090;

        static readonly Dictionary<string, string> Types = new Dictionary<string, string>
        {
            { ".html", "text/html; charset=utf-8" },
            { ".js", "text/javascript; charset=utf-8" },
            { ".css", "text/css; charset=utf-8" },
            { ".json", "application/json" },
            { ".png", "image/png" },
            { ".webp", "image/webp" },
            { ".svg", "image/svg+xml" },
            { ".ico", "image/x-icon" },
            { ".woff2", "font/woff2" },
            { ".txt", "text/plain; charset=utf-8" }
        };

        readonly string root;
        TcpListener listener;
        Thread thread;
        volatile bool running;

        public PadWebHost(string root)
        {
            this.root = root;
        }

        public bool Start()
        {
            if (!Directory.Exists(root)) return false;
            try
            {
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listener.Start();
            }
            catch
            {
                return false;
            }

            running = true;
            thread = new Thread(AcceptLoop) { IsBackground = true, Name = "KimayaPwa" };
            thread.Start();
            return true;
        }

        public void Stop()
        {
            running = false;
            try { listener?.Stop(); } catch { }
        }

        void AcceptLoop()
        {
            while (running)
            {
                TcpClient client;
                try { client = listener.AcceptTcpClient(); }
                catch { break; }
                var worker = new Thread(() => Serve(client)) { IsBackground = true };
                worker.Start();
            }
        }

        void Serve(TcpClient client)
        {
            using (client)
            {
                try
                {
                    client.ReceiveTimeout = 4000;
                    var stream = client.GetStream();
                    var buffer = new byte[2048];
                    int got = 0;
                    while (got < buffer.Length)
                    {
                        int n = stream.Read(buffer, got, buffer.Length - got);
                        if (n <= 0) return;
                        got += n;
                        if (Encoding.ASCII.GetString(buffer, 0, got).Contains("\r\n\r\n")) break;
                    }

                    string request = Encoding.ASCII.GetString(buffer, 0, got);
                    string path = "/";
                    int start = request.IndexOf(' ');
                    int end = start < 0 ? -1 : request.IndexOf(' ', start + 1);
                    if (start >= 0 && end > start) path = request.Substring(start + 1, end - start - 1);
                    int query = path.IndexOf('?');
                    if (query >= 0) path = path.Substring(0, query);
                    path = Uri.UnescapeDataString(path);
                    if (path == "/") path = "/index.html";

                    string relative = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    string file = Path.GetFullPath(Path.Combine(root, relative));
                    string rootFull = Path.GetFullPath(root);
                    if (!rootFull.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        rootFull += Path.DirectorySeparatorChar;
                    if (!file.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) || !File.Exists(file))
                    {
                        Write(stream, 404, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("No encontrado"));
                        return;
                    }

                    string type;
                    if (!Types.TryGetValue(Path.GetExtension(file).ToLowerInvariant(), out type))
                        type = "application/octet-stream";
                    Write(stream, 200, type, File.ReadAllBytes(file));
                }
                catch { }
            }
        }

        static void Write(NetworkStream stream, int status, string type, byte[] body)
        {
            string head = "HTTP/1.1 " + status + (status == 200 ? " OK" : " Not Found") + "\r\n" +
                          "Content-Type: " + type + "\r\n" +
                          "Content-Length: " + body.Length + "\r\n" +
                          "Cache-Control: no-cache\r\n" +
                          "Connection: close\r\n\r\n";
            byte[] header = Encoding.ASCII.GetBytes(head);
            stream.Write(header, 0, header.Length);
            stream.Write(body, 0, body.Length);
        }
    }
}
