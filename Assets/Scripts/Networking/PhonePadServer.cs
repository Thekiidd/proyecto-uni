using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Kimaya.Networking
{
    /// <summary>
    /// Recibe el mando de la PWA en ws://ip:8080/mando.
    /// La app manda {"b":"A","s":1} al presionar y {"b":"A","s":0} al soltar.
    /// </summary>
    public class PhonePadServer : MonoBehaviour
    {
        public const int Port = 8080;

        static readonly object Gate = new object();
        static readonly HashSet<string> HeldButtons = new HashSet<string>();
        static readonly HashSet<string> PreviousButtons = new HashSet<string>();
        static readonly List<NetworkStream> Clients = new List<NetworkStream>();

        static PhonePadServer instance;
        public static string PlayerName { get; private set; } = "";
        TcpListener listener;
        Thread acceptThread;
        volatile bool running;
        string localIp = "127.0.0.1";
        int clientCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("PhonePadServer");
            go.AddComponent<PhonePadServer>();
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            localIp = FindLocalIp();
            StartServer();
        }

        void LateUpdate()
        {
            lock (Gate)
            {
                PreviousButtons.Clear();
                foreach (var button in HeldButtons)
                    PreviousButtons.Add(button);
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            StopServer();
        }

        public static bool Held(string button)
        {
            lock (Gate) return HeldButtons.Contains(button);
        }

        public static bool Pressed(string button)
        {
            lock (Gate) return HeldButtons.Contains(button) && !PreviousButtons.Contains(button);
        }

        public static void Send(string json)
        {
            var bytes = EncodeFrame(Encoding.UTF8.GetBytes(json));
            lock (Gate)
            {
                for (int i = Clients.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        Clients[i].Write(bytes, 0, bytes.Length);
                    }
                    catch
                    {
                        Clients.RemoveAt(i);
                    }
                }
            }
        }

        void StartServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, Port);
                listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listener.Start();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No se pudo abrir el mando en el puerto " + Port + ": " + ex.Message);
                return;
            }

            running = true;
            acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "PhonePad" };
            acceptThread.Start();
            Debug.Log("Mando listo en ws://" + localIp + ":" + Port + "/mando");
        }

        void StopServer()
        {
            running = false;
            try { listener?.Stop(); } catch { }
            lock (Gate)
            {
                HeldButtons.Clear();
                foreach (var stream in Clients)
                {
                    try { stream.Close(); } catch { }
                }
                Clients.Clear();
            }
        }

        void AcceptLoop()
        {
            while (running)
            {
                TcpClient client;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch
                {
                    if (!running) break;
                    continue;
                }

                var thread = new Thread(() => HandleClient(client)) { IsBackground = true };
                thread.Start();
            }
        }

        void HandleClient(TcpClient client)
        {
            client.NoDelay = true;
            NetworkStream stream = client.GetStream();
            try
            {
                if (!Handshake(stream)) return;

                lock (Gate) Clients.Add(stream);
                Interlocked.Increment(ref clientCount);
                Debug.Log("Celular conectado al mando");

                var header = new byte[2];
                while (running && ReadExact(stream, header, 2))
                {
                    int opcode = header[0] & 0x0F;
                    int length = header[1] & 0x7F;
                    bool masked = (header[1] & 0x80) != 0;

                    if (length == 126)
                    {
                        var ext = new byte[2];
                        if (!ReadExact(stream, ext, 2)) break;
                        length = (ext[0] << 8) | ext[1];
                    }
                    else if (length == 127)
                    {
                        var ext = new byte[8];
                        if (!ReadExact(stream, ext, 8)) break;
                        length = 0;
                        for (int i = 0; i < 8; i++) length = (length << 8) | ext[i];
                    }

                    if (length < 0 || length > 65536) break;

                    byte[] mask = null;
                    if (masked)
                    {
                        mask = new byte[4];
                        if (!ReadExact(stream, mask, 4)) break;
                    }

                    var payload = new byte[length];
                    if (length > 0 && !ReadExact(stream, payload, length)) break;
                    if (masked)
                    {
                        for (int i = 0; i < payload.Length; i++)
                            payload[i] ^= mask[i % 4];
                    }

                    if (opcode == 8) break;
                    if (opcode == 9)
                    {
                        var pong = EncodeFrame(payload, 0xA);
                        stream.Write(pong, 0, pong.Length);
                        continue;
                    }
                    if (opcode == 1)
                        ApplyMessage(Encoding.UTF8.GetString(payload));
                }
            }
            catch (Exception ex)
            {
                if (running) Debug.Log("El celular cerró el mando: " + ex.Message);
            }
            finally
            {
                lock (Gate) Clients.Remove(stream);
                if (Interlocked.Decrement(ref clientCount) <= 0)
                {
                    lock (Gate) HeldButtons.Clear();
                    PlayerName = "";
                }
                try { client.Close(); } catch { }
            }
        }

        static void ApplyMessage(string json)
        {
            if (json.IndexOf("\"name\"", StringComparison.Ordinal) >= 0)
            {
                SetPlayerName(ReadJsonString(json, "name"));
                return;
            }

            string button = ReadJsonString(json, "b");
            if (string.IsNullOrEmpty(button)) return;
            int state = ReadJsonInt(json, "s");
            lock (Gate)
            {
                if (state != 0) HeldButtons.Add(button);
                else HeldButtons.Remove(button);
            }
        }

        static bool Handshake(NetworkStream stream)
        {
            var buffer = new byte[1024];
            int got = 0;
            while (got < buffer.Length)
            {
                int n = stream.Read(buffer, got, buffer.Length - got);
                if (n <= 0) return false;
                got += n;
                var text = Encoding.ASCII.GetString(buffer, 0, got);
                if (text.Contains("\r\n\r\n"))
                {
                    if (text.IndexOf("GET /mando", StringComparison.Ordinal) < 0) return false;
                    string key = Header(text, "Sec-WebSocket-Key");
                    if (string.IsNullOrEmpty(key)) return false;
                    string accept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                    string response =
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Sec-WebSocket-Accept: " + accept + "\r\n\r\n";
                    var bytes = Encoding.ASCII.GetBytes(response);
                    stream.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            return false;
        }

        static string Header(string request, string name)
        {
            int line = request.IndexOf(name + ":", StringComparison.OrdinalIgnoreCase);
            if (line < 0) return "";
            int start = request.IndexOf(':', line) + 1;
            int end = request.IndexOf("\r\n", start, StringComparison.Ordinal);
            if (end < 0) return "";
            return request.Substring(start, end - start).Trim();
        }

        public static void SetPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                PlayerName = "";
                return;
            }

            var clean = new StringBuilder();
            bool previousSpace = false;
            foreach (char c in name.Trim())
            {
                if (c == ' ' || c == '-')
                {
                    if (clean.Length == 0 || previousSpace) continue;
                    clean.Append(c);
                    previousSpace = true;
                    continue;
                }
                if (!char.IsLetterOrDigit(c)) continue;
                clean.Append(c);
                previousSpace = false;
                if (clean.Length >= 24) break;
            }
            PlayerName = clean.ToString().Trim();
        }

        static string ReadJsonString(string json, string key)
        {
            int found = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (found < 0) return "";
            int colon = json.IndexOf(':', found);
            int quote = json.IndexOf('"', colon + 1);
            int end = json.IndexOf('"', quote + 1);
            if (colon < 0 || quote < 0 || end < 0) return "";
            return json.Substring(quote + 1, end - quote - 1);
        }

        static int ReadJsonInt(string json, string key)
        {
            int found = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (found < 0) return 0;
            int colon = json.IndexOf(':', found);
            if (colon < 0) return 0;
            int value = 0;
            bool any = false;
            for (int i = colon + 1; i < json.Length; i++)
            {
                char c = json[i];
                if (c >= '0' && c <= '9')
                {
                    any = true;
                    value = value * 10 + (c - '0');
                }
                else if (any) break;
            }
            return value;
        }

        static bool ReadExact(NetworkStream stream, byte[] buffer, int count)
        {
            int got = 0;
            while (got < count)
            {
                int n = stream.Read(buffer, got, count - got);
                if (n <= 0) return false;
                got += n;
            }
            return true;
        }

        static byte[] EncodeFrame(byte[] payload, int opcode = 1)
        {
            int length = payload.Length;
            int header = length < 126 ? 2 : 4;
            var frame = new byte[header + length];
            frame[0] = (byte)(0x80 | opcode);
            if (length < 126)
            {
                frame[1] = (byte)length;
            }
            else
            {
                frame[1] = 126;
                frame[2] = (byte)(length >> 8);
                frame[3] = (byte)length;
            }
            Buffer.BlockCopy(payload, 0, frame, header, length);
            return frame;
        }

        static string FindLocalIp()
        {
            try
            {
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (adapter.OperationalStatus != OperationalStatus.Up) continue;
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                    foreach (var address in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        string ip = address.Address.ToString();
                        if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                            return ip;
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.Max(14, Screen.height / 42),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            string text = clientCount > 0
                ? "Control conectado"
                : "En el celular: " + localIp;
            float width = Mathf.Min(520f, Screen.width - 16f);
            GUI.depth = -20;
            GUI.Box(new Rect((Screen.width - width) * 0.5f, 8f, width, 34f), text, style);
        }
    }
}
