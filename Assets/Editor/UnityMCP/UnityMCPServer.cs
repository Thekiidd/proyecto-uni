// UnityMCPServer — minimal WebSocket server (port 6400) that lets server.py drive the Unity Editor.
// Menu: Tools > MCP Server > Start / Stop. Commands are marshaled onto the main thread.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityMCP
{
    [InitializeOnLoad]
    public static class UnityMCPServer
    {
        const int Port = 6400;
        static TcpListener _listener;
        static Thread _acceptThread;
        static volatile bool _running;
        static readonly ConcurrentQueue<PendingCommand> _queue = new ConcurrentQueue<PendingCommand>();

        class PendingCommand
        {
            public string Command;
            public Dictionary<string, object> Args;
            public string Response;
            public readonly ManualResetEventSlim Done = new ManualResetEventSlim(false);
        }

        static UnityMCPServer()
        {
            // Auto-start when the editor loads, and pump the main-thread queue.
            EditorApplication.update += PumpQueue;
            EditorApplication.quitting += Stop;
            if (!_running) Start();
        }

        [MenuItem("Tools/MCP Server/Start")]
        public static void Start()
        {
            if (_running) { Debug.Log("[MCP] Already running."); return; }
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, Port);
                _listener.Start();
                _running = true;
                _acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "UnityMCP-Accept" };
                _acceptThread.Start();
                Debug.Log($"[MCP] ✅ Server started on ws://127.0.0.1:{Port}");
            }
            catch (Exception e)
            {
                _running = false;
                Debug.LogError($"[MCP] Failed to start: {e.Message}");
            }
        }

        [MenuItem("Tools/MCP Server/Stop")]
        public static void Stop()
        {
            _running = false;
            try { _listener?.Stop(); } catch { }
            _listener = null;
            Debug.Log("[MCP] Server stopped.");
        }

        static void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    var t = new Thread(() => HandleClient(client)) { IsBackground = true, Name = "UnityMCP-Client" };
                    t.Start();
                }
                catch (SocketException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception e) { if (_running) Debug.LogError($"[MCP] Accept error: {e.Message}"); }
            }
        }

        static void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    if (!Handshake(stream)) return;
                    while (_running && client.Connected)
                    {
                        string message = ReadMessage(stream, out int opcode);
                        if (opcode == 0x8) break;            // close
                        if (opcode == 0x9) { continue; }      // ping (ignored)
                        if (message == null) break;
                        string response = Dispatch(message);
                        SendText(stream, response);
                    }
                }
            }
            catch (Exception e)
            {
                if (_running) Debug.LogWarning($"[MCP] Client error: {e.Message}");
            }
        }

        // ---- WebSocket handshake ----
        static bool Handshake(NetworkStream stream)
        {
            var sb = new StringBuilder();
            int prev = -1, prev2 = -1, prev3 = -1, b;
            // Read HTTP request until \r\n\r\n
            while ((b = stream.ReadByte()) != -1)
            {
                sb.Append((char)b);
                if (prev3 == '\r' && prev2 == '\n' && prev == '\r' && b == '\n') break;
                prev3 = prev2; prev2 = prev; prev = b;
            }
            string request = sb.ToString();
            string key = null;
            foreach (var line in request.Split(new[] { "\r\n" }, StringSplitOptions.None))
            {
                if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
                    key = line.Substring(line.IndexOf(':') + 1).Trim();
            }
            if (key == null) return false;
            string accept = Convert.ToBase64String(
                SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            string resp = "HTTP/1.1 101 Switching Protocols\r\n" +
                          "Upgrade: websocket\r\n" +
                          "Connection: Upgrade\r\n" +
                          "Sec-WebSocket-Accept: " + accept + "\r\n\r\n";
            byte[] respBytes = Encoding.UTF8.GetBytes(resp);
            stream.Write(respBytes, 0, respBytes.Length);
            return true;
        }

        // ---- Frame reading (handles fragmentation + 16/64-bit lengths) ----
        static string ReadMessage(NetworkStream stream, out int opcode)
        {
            opcode = 0;
            var payload = new List<byte>();
            bool fin = false;
            int firstOpcode = 0;
            while (!fin)
            {
                byte[] hdr = ReadExactly(stream, 2);
                if (hdr == null) { opcode = 0x8; return null; }
                fin = (hdr[0] & 0x80) != 0;
                int op = hdr[0] & 0x0F;
                if (firstOpcode == 0 && op != 0) firstOpcode = op;
                bool masked = (hdr[1] & 0x80) != 0;
                long len = hdr[1] & 0x7F;
                if (len == 126)
                {
                    byte[] ext = ReadExactly(stream, 2);
                    len = (ext[0] << 8) | ext[1];
                }
                else if (len == 127)
                {
                    byte[] ext = ReadExactly(stream, 8);
                    len = 0;
                    for (int i = 0; i < 8; i++) len = (len << 8) | ext[i];
                }
                byte[] mask = masked ? ReadExactly(stream, 4) : null;
                byte[] data = len > 0 ? ReadExactly(stream, (int)len) : new byte[0];
                if (data == null) { opcode = 0x8; return null; }
                if (masked) for (int i = 0; i < data.Length; i++) data[i] = (byte)(data[i] ^ mask[i % 4]);
                payload.AddRange(data);
                if (op == 0x8) { opcode = 0x8; return null; }
            }
            opcode = firstOpcode;
            return Encoding.UTF8.GetString(payload.ToArray());
        }

        static byte[] ReadExactly(NetworkStream stream, int count)
        {
            byte[] buffer = new byte[count];
            int read = 0;
            while (read < count)
            {
                int r = stream.Read(buffer, read, count - read);
                if (r <= 0) return null;
                read += r;
            }
            return buffer;
        }

        static void SendText(NetworkStream stream, string text)
        {
            byte[] payload = Encoding.UTF8.GetBytes(text);
            var frame = new List<byte> { 0x81 }; // FIN + text
            if (payload.Length < 126)
            {
                frame.Add((byte)payload.Length);
            }
            else if (payload.Length <= 65535)
            {
                frame.Add(126);
                frame.Add((byte)((payload.Length >> 8) & 0xFF));
                frame.Add((byte)(payload.Length & 0xFF));
            }
            else
            {
                frame.Add(127);
                long l = payload.Length;
                for (int i = 7; i >= 0; i--) frame.Add((byte)((l >> (8 * i)) & 0xFF));
            }
            frame.AddRange(payload);
            byte[] outBytes = frame.ToArray();
            stream.Write(outBytes, 0, outBytes.Length);
        }

        // ---- Dispatch onto the main thread ----
        static string Dispatch(string message)
        {
            string command;
            Dictionary<string, object> args;
            try
            {
                var root = Json.Deserialize(message) as Dictionary<string, object>;
                command = root != null && root.ContainsKey("command") ? root["command"] as string : null;
                args = root != null && root.ContainsKey("args") ? root["args"] as Dictionary<string, object> : new Dictionary<string, object>();
                if (args == null) args = new Dictionary<string, object>();
            }
            catch (Exception e)
            {
                return Json.Serialize(new Dictionary<string, object> { { "error", "BadRequest" }, { "detail", e.Message } });
            }
            if (string.IsNullOrEmpty(command))
                return Json.Serialize(new Dictionary<string, object> { { "error", "NoCommand" } });

            var pc = new PendingCommand { Command = command, Args = args };
            _queue.Enqueue(pc);
            if (!pc.Done.Wait(28000))
                return Json.Serialize(new Dictionary<string, object> { { "error", "Timeout" }, { "detail", "Main thread did not respond" } });
            return pc.Response;
        }

        static void PumpQueue()
        {
            while (_queue.TryDequeue(out var pc))
            {
                try { pc.Response = MCPCommandHandler.Handle(pc.Command, pc.Args); }
                catch (Exception e)
                {
                    pc.Response = Json.Serialize(new Dictionary<string, object>
                    { { "error", e.GetType().Name }, { "detail", e.Message } });
                }
                pc.Done.Set();
            }
        }
    }
}
