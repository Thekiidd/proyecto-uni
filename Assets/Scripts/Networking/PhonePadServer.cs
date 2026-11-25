using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya.Networking
{
    /// <summary>
    /// Recibe el mando de la PWA en ws://ip:8080/mando.
    /// Soporta Joystick Analógico 360° ({"jx":0.5,"jy":-0.8}) y botones ({"b":"A","s":1} / {"b":"A","s":0}).
    /// </summary>
    public class PhonePadServer : MonoBehaviour
    {
        public const int Port = 8080;

        static readonly object Gate = new object();
        static readonly HashSet<string> HeldButtons = new HashSet<string>();
        static readonly HashSet<string> PendingPresses = new HashSet<string>();
        static readonly HashSet<string> FramePresses = new HashSet<string>();
        static readonly List<NetworkStream> Clients = new List<NetworkStream>();
        static Vector2 analogJoystick = Vector2.zero;

        static PhonePadServer instance;
        public static string PlayerName { get; private set; } = "";
        public static bool IsConnected => instance != null && instance.clientCount > 0;
        TcpListener listener;
        Thread acceptThread;
        PadWebHost webHost;
        volatile bool running;
        string localIp = "127.0.0.1";
        string controlUrl = "";
        Texture2D qrTexture;
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
            controlUrl = "http://" + localIp + ":" + PadWebHost.Port + "/?ip=" + localIp + "&port=" + Port;
            StartServer();
            StartWeb();
            BuildQrTexture();
        }

        void Update()
        {
            lock (Gate)
            {
                FramePresses.Clear();
                foreach (var b in PendingPresses)
                    FramePresses.Add(b);
                PendingPresses.Clear();
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
            lock (Gate) return FramePresses.Contains(button);
        }

        public static Vector2 GetJoystick()
        {
            lock (Gate) return analogJoystick;
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

        void StartWeb()
        {
            string root = Path.Combine(Application.streamingAssetsPath, "KimayaPwa");
            webHost = new PadWebHost(root);
            if (webHost.Start())
                Debug.Log("Control listo en " + controlUrl);
            else
                Debug.LogWarning("No se pudo servir el control desde " + root);
        }

        void BuildQrTexture()
        {
            try
            {
                KimayaQr qr = KimayaQr.Encode(controlUrl);
                const int scale = 8;
                const int quiet = 4;
                int modules = qr.Size + quiet * 2;
                int dim = modules * scale;
                var tex = new Texture2D(dim, dim, TextureFormat.RGB24, false);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                var pixels = new Color32[dim * dim];
                var black = new Color32(0, 0, 0, 255);
                var white = new Color32(255, 255, 255, 255);
                for (int py = 0; py < dim; py++)
                {
                    int qrY = py / scale - quiet;
                    int flipped = dim - 1 - py;
                    for (int px = 0; px < dim; px++)
                    {
                        int qrX = px / scale - quiet;
                        pixels[flipped * dim + px] = qr.Dark(qrX, qrY) ? black : white;
                    }
                }
                tex.SetPixels32(pixels);
                tex.Apply(false, true);
                qrTexture = tex;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No se pudo armar el QR: " + ex.Message);
            }
        }

        void StopServer()
        {
            running = false;
            try { listener?.Stop(); } catch { }
            if (webHost != null) webHost.Stop();
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
                    lock (Gate)
                    {
                        HeldButtons.Clear();
                        PendingPresses.Clear();
                        FramePresses.Clear();
                        analogJoystick = Vector2.zero;
                    }
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

            if (json.IndexOf("\"jx\"", StringComparison.Ordinal) >= 0)
            {
                float jx = ReadJsonFloat(json, "jx");
                float jy = ReadJsonFloat(json, "jy");
                lock (Gate)
                {
                    analogJoystick = Vector2.ClampMagnitude(new Vector2(jx, jy), 1f);
                    if (jx > 0.22f) HeldButtons.Add("RIGHT"); else HeldButtons.Remove("RIGHT");
                    if (jx < -0.22f) HeldButtons.Add("LEFT"); else HeldButtons.Remove("LEFT");
                    if (jy > 0.22f) HeldButtons.Add("UP"); else HeldButtons.Remove("UP");
                    if (jy < -0.22f) HeldButtons.Add("DOWN"); else HeldButtons.Remove("DOWN");
                }
                return;
            }

            string button = ReadJsonString(json, "b");
            if (string.IsNullOrEmpty(button)) return;
            int state = ReadJsonInt(json, "s");
            lock (Gate)
            {
                if (state != 0)
                {
                    HeldButtons.Add(button);
                    PendingPresses.Add(button);
                }
                else
                {
                    HeldButtons.Remove(button);
                }
            }
        }

        static float ReadJsonFloat(string json, string key)
        {
            int found = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (found < 0) return 0f;
            int colon = json.IndexOf(':', found);
            if (colon < 0) return 0f;
            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '"')) start++;
            int end = start;
            while (end < json.Length && ((json[end] >= '0' && json[end] <= '9') || json[end] == '-' || json[end] == '.' || json[end] == '+')) end++;
            if (end <= start) return 0f;
            if (float.TryParse(json.Substring(start, end - start), NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                return val;
            return 0f;
        }

        static bool Handshake(NetworkStream stream)
        {
            // Safari manda cabeceras largas (idioma, origen, extensiones). 1 KB se quedaba justo.
            var buffer = new byte[8192];
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

        // Elige la IP que el celular sí puede alcanzar. Antes se tomaba la primera dirección privada,
        // y con WSL, Hyper-V, Docker o una VPN eso daba una IP interna de la PC y el celular no conectaba.
        // Se descartan los adaptadores virtuales y se prefiere el que tiene puerta de enlace (la red real),
        // y entre esos el Wi-Fi.
        static string FindLocalIp()
        {
            string ip = PickLocalIp(false);
            if (ip == null) ip = PickLocalIp(true);
            return ip ?? "127.0.0.1";
        }

        static string PickLocalIp(bool allowVirtual)
        {
            string best = null;
            int bestScore = -1;
            var found = new List<string>();
            try
            {
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (adapter.OperationalStatus != OperationalStatus.Up) continue;
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        adapter.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                    if (!allowVirtual && IsVirtualAdapter(adapter)) continue;

                    var props = adapter.GetIPProperties();
                    bool hasGateway = false;
                    foreach (var gateway in props.GatewayAddresses)
                    {
                        if (gateway.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !gateway.Address.Equals(IPAddress.Any))
                            hasGateway = true;
                    }

                    foreach (var address in props.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        string candidate = address.Address.ToString();
                        if (!IsPrivateIp(candidate)) continue;

                        found.Add(adapter.Name + "=" + candidate);
                        int score = (hasGateway ? 100 : 0)
                            + (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ? 10 : 0);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = candidate;
                        }
                    }
                }
            }
            catch { }

            if (found.Count > 0)
                Debug.Log("Redes encontradas para el mando: " + string.Join(" | ", found) + " -> se usa " + best);
            return best;
        }

        static bool IsPrivateIp(string ip)
        {
            if (ip.StartsWith("192.168.") || ip.StartsWith("10.")) return true;
            if (!ip.StartsWith("172.")) return false;
            string[] parts = ip.Split('.');
            int second;
            return parts.Length > 1 && int.TryParse(parts[1], out second) && second >= 16 && second <= 31;
        }

        static bool IsVirtualAdapter(NetworkInterface adapter)
        {
            string text = (adapter.Name + " " + adapter.Description).ToLowerInvariant();
            string[] marks = { "vethernet", "wsl", "hyper-v", "virtualbox", "vmware", "docker", "virtual", "vpn", "tap-", "tailscale", "zerotier", "bluetooth" };
            foreach (string mark in marks)
            {
                if (text.Contains(mark)) return true;
            }
            return false;
        }

        void OnGUI()
        {
            // SOLO mostrar el panel del QR en la escena MainMenu (al pasar al Hub o al nivel ya no sale)
            if (SceneManager.GetActiveScene().name != "MainMenu")
                return;

            // Si está abierto Ajustes, Cómo Jugar o Selección de Personaje en el MainMenu, ocultar la tarjeta del QR
            var panelMain = GameObject.Find("PanelMain");
            if (panelMain != null && !panelMain.activeInHierarchy)
                return;

            GUI.depth = -20;

            float cardW = Mathf.Clamp(Screen.width * 0.34f, 320f, 440f);
            float cardH = Mathf.Clamp(Screen.height * 0.56f, 340f, 460f);
            float cardX = Screen.width * 0.71f - cardW * 0.5f;
            float cardY = Screen.height * 0.54f - cardH * 0.5f;

            Color prevCol = GUI.color;

            // Marco exterior dorado e interior obsidiana pixel-art
            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.96f);
            GUI.DrawTexture(new Rect(cardX - 3f, cardY - 3f, cardW + 6f, cardH + 6f), Texture2D.whiteTexture);
            GUI.color = new Color(0.09f, 0.08f, 0.14f, 0.95f);
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), Texture2D.whiteTexture);
            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.35f);
            GUI.DrawTexture(new Rect(cardX + 16f, cardY + 46f, cardW - 32f, 2f), Texture2D.whiteTexture);
            GUI.color = prevCol;

            GUIStyle hdrStyle = Kimaya.KimayaUITheme.GetOnGUILabelStyle(16, TextAnchor.MiddleCenter, false);
            GUIStyle bodyStyle = Kimaya.KimayaUITheme.GetOnGUILabelStyle(13, TextAnchor.MiddleCenter, true);
            GUIStyle smallUrlStyle = Kimaya.KimayaUITheme.GetOnGUILabelStyle(11, TextAnchor.MiddleCenter, true);

            GUI.Label(new Rect(cardX + 10f, cardY + 10f, cardW - 20f, 30f), "<color=#F8D458>MANDO MÓVIL (WI-FI)</color>", hdrStyle);

            bool connected = clientCount > 0;
            string statusBadge = connected
                ? ("<color=#48F088>● CELULAR CONECTADO" + (!string.IsNullOrEmpty(PlayerName) ? $" ({PlayerName})" : "") + "</color>")
                : "<color=#FFD166>● ESCANEA EL QR CON TU CELULAR</color>";
            GUI.Label(new Rect(cardX + 12f, cardY + 52f, cardW - 24f, 24f), statusBadge, bodyStyle);

            if (qrTexture != null)
            {
                float qrSize = Mathf.Clamp(cardH - 200f, 130f, 200f);
                float qrX = cardX + (cardW - qrSize) * 0.5f;
                float qrY = cardY + 82f;

                GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.9f);
                GUI.DrawTexture(new Rect(qrX - 4f, qrY - 4f, qrSize + 8f, qrSize + 8f), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(qrX, qrY, qrSize, qrSize), qrTexture, ScaleMode.ScaleToFit, false);
                GUI.color = prevCol;

                float infoY = qrY + qrSize + 10f;
                string steps = connected
                    ? "<color=#E2E8F0>Joystick 360°: Mover perrito\nBotón A: Saltar / Confirmar · Botón B: Acción / Ladrar\nBotón X: Cambiar Falin/Mía · START: Pausa</color>"
                    : "<color=#E2E8F0>1. Conecta tu celular al mismo Wi-Fi\n2. Escanea el QR para abrir el Joystick\n3. Usa Joystick + Botón A para saltar</color>";
                GUI.Label(new Rect(cardX + 14f, infoY, cardW - 28f, 58f), steps, bodyStyle);

                GUI.Label(new Rect(cardX + 10f, cardY + cardH - 34f, cardW - 20f, 28f), $"<color=#94A3B8>{controlUrl}</color>", smallUrlStyle);
            }
        }
    }
}
