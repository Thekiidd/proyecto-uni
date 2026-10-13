import urllib.request, urllib.parse, json, time, os

API_URL = "https://api.pixellab.ai/mcp"
AUTH_TOKEN = "f79c0663-7f4f-4284-a8cd-60f614ef736f"

def call_tool(name: str, arguments: dict = None) -> dict:
    """Invoca una herramienta MCP en PixelLab mediante JSON-RPC."""
    if arguments is None:
        arguments = {}
    payload = {
        "jsonrpc": "2.0",
        "id": int(time.time() * 1000),
        "method": "tools/call",
        "params": {
            "name": name,
            "arguments": arguments
        }
    }
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        API_URL,
        data=data,
        headers={
            "Authorization": f"Bearer {AUTH_TOKEN}",
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream"
        }
    )
    with urllib.request.urlopen(req) as resp:
        raw = resp.read().decode("utf-8", errors="ignore")
        for line in raw.splitlines():
            if line.startswith("data: "):
                res = json.loads(line[6:])
                if "error" in res:
                    raise Exception(f"PixelLab Error: {res['error']}")
                return res.get("result", {})
    return {}

def download_file(url: str, output_path: str):
    """Descarga un archivo (spritesheet, tileset, png) en la ruta indicada."""
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    urllib.request.urlretrieve(url, output_path)
    print(f"✅ Descargado: {output_path}")

if __name__ == "__main__":
    print("Probando conexión con PixelLab...")
    res = call_tool("list_characters")
    print("Respuesta de list_characters:")
    print(json.dumps(res, indent=2))
