#!/usr/bin/env python3
"""
server.py â€” Puente MCP entre Claude Desktop y Unity.

Claude Desktop  --(MCP / stdio)-->  server.py  --(WebSocket :6400)-->  Unity (UnityMCPServer.cs)

Requisitos:
    pip install -r requirements.txt
    (instala: mcp, websockets)

El Editor de Unity debe estar abierto y el servidor MCP corriendo
(arranca solo, o desde el menu Tools > MCP Server > Start).
"""

import asyncio
import json
from typing import Any, Optional

import websockets
from mcp.server.fastmcp import FastMCP

UNITY_WS_URL = "ws://127.0.0.1:6400"
REQUEST_TIMEOUT = 30  # segundos

mcp = FastMCP("unity-mcp")


async def send_to_unity(command: str, args: Optional[dict] = None) -> dict:
    """Abre una conexion WebSocket a Unity, envia un comando y devuelve la respuesta JSON."""
    payload = json.dumps({"command": command, "args": args or {}})
    try:
        async with websockets.connect(UNITY_WS_URL, open_timeout=10) as ws:
            await ws.send(payload)
            raw = await asyncio.wait_for(ws.recv(), timeout=REQUEST_TIMEOUT)
            try:
                return json.loads(raw)
            except json.JSONDecodeError:
                return {"raw": raw}
    except ConnectionRefusedError:
        return {
            "error": "No se pudo conectar a Unity en " + UNITY_WS_URL,
            "hint": "Abre el Editor de Unity y verifica Tools > MCP Server > Start.",
        }
    except asyncio.TimeoutError:
        return {"error": "Unity no respondio a tiempo (timeout)."}
    except Exception as e:  # noqa: BLE001
        return {"error": type(e).__name__, "detail": str(e)}


def _fmt(result: dict) -> str:
    return json.dumps(result, indent=2, ensure_ascii=False)


# ----------------------------------------------------------------------------
# Herramientas MCP â€” cada una mapea a un comando del MCPCommandHandler.cs
# ----------------------------------------------------------------------------

@mcp.tool()
async def get_project_info() -> str:
    """Devuelve informacion del proyecto Unity (nombre, version, escena activa, si esta en Play)."""
    return _fmt(await send_to_unity("unity_get_project_info"))


@mcp.tool()
async def get_scene_hierarchy() -> str:
    """Devuelve la jerarquia completa de la escena activa (objetos raiz e hijos)."""
    return _fmt(await send_to_unity("unity_get_scene_hierarchy"))


@mcp.tool()
async def get_object_components(game_object_name: str) -> str:
    """Lista los componentes y el transform de un GameObject por su nombre."""
    return _fmt(await send_to_unity("unity_get_object_components",
                                    {"gameObjectName": game_object_name}))


@mcp.tool()
async def modify_transform(
    game_object_name: str,
    position: Optional[dict] = None,
    rotation: Optional[dict] = None,
    scale: Optional[dict] = None,
) -> str:
    """Modifica el transform de un GameObject. position/rotation/scale son dicts {x,y,z} opcionales."""
    args: dict[str, Any] = {"gameObjectName": game_object_name}
    if position is not None:
        args["position"] = position
    if rotation is not None:
        args["rotation"] = rotation
    if scale is not None:
        args["scale"] = scale
    return _fmt(await send_to_unity("unity_modify_transform", args))


@mcp.tool()
async def create_gameobject(
    name: str,
    primitive: Optional[str] = None,
    parent_name: Optional[str] = None,
) -> str:
    """Crea un GameObject. primitive puede ser Cube, Sphere, Capsule, Cylinder, Plane, Quad o None."""
    args: dict[str, Any] = {"name": name}
    if primitive:
        args["primitive"] = primitive
    if parent_name:
        args["parentName"] = parent_name
    return _fmt(await send_to_unity("unity_create_gameobject", args))


@mcp.tool()
async def read_script(script_path: str) -> str:
    """Lee un script. script_path es relativo a la carpeta Assets (ej: 'Scripts/Player.cs')."""
    return _fmt(await send_to_unity("unity_read_script", {"scriptPath": script_path}))


@mcp.tool()
async def write_script(script_path: str, content: str) -> str:
    """Escribe/crea un script. script_path es relativo a Assets. Refresca el AssetDatabase."""
    return _fmt(await send_to_unity("unity_write_script",
                                    {"scriptPath": script_path, "content": content}))


@mcp.tool()
async def get_console_logs(count: int = 20, filter: str = "all") -> str:
    """Devuelve logs recientes de la consola. filter: all | error | warning | info."""
    return _fmt(await send_to_unity("unity_get_console_logs",
                                    {"count": count, "filter": filter}))


@mcp.tool()
async def enter_play_mode() -> str:
    """Entra en Play Mode en el Editor."""
    return _fmt(await send_to_unity("unity_enter_play_mode"))


@mcp.tool()
async def exit_play_mode() -> str:
    """Sale de Play Mode en el Editor."""
    return _fmt(await send_to_unity("unity_exit_play_mode"))


@mcp.tool()
async def compile_scripts() -> str:
    """Solicita la recompilacion de scripts en Unity."""
    return _fmt(await send_to_unity("unity_compile_scripts"))


if __name__ == "__main__":
    # Transporte stdio: es lo que espera Claude Desktop.
    mcp.run(transport="stdio")
