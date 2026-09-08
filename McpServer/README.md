# Unity MCP — Puente para Claude Desktop

Permite que **Claude Desktop** controle tu Editor de Unity (crear objetos, mover
transforms, leer/escribir scripts, entrar/salir de Play, leer la consola, etc.).

## Arquitectura

```
Claude Desktop  --(MCP / stdio)-->  server.py  --(WebSocket :6400)-->  Unity (UnityMCPServer.cs)
```

- **Unity (ya listo):** `Assets/Editor/UnityMCP/UnityMCPServer.cs` levanta un
  servidor WebSocket en `ws://127.0.0.1:6400` (arranca solo con el Editor;
  tambien Tools > MCP Server > Start / Stop).
- **server.py (este puente):** traduce el protocolo MCP de Claude Desktop a
  mensajes WebSocket para Unity.

## Instalacion

1. Instala Python 3.10 o superior (marca "Add Python to PATH" en el instalador).

2. Instala las dependencias del puente:

   ```bash
   cd "D:/Gamess/My project (8)/McpServer"
   pip install -r requirements.txt
   ```

3. Configura Claude Desktop. Edita el archivo de configuracion:

   - **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
   - **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`

   Pega el contenido de `claude_desktop_config.example.json` (si el archivo ya
   existe, agrega solo la entrada `"unity"` dentro de `"mcpServers"`).
   Asegurate de que la ruta a `server.py` sea correcta.

4. **Reinicia Claude Desktop** por completo (ciérralo desde la bandeja del
   sistema, no solo la ventana).

## Uso

1. Abre tu proyecto en Unity (el servidor MCP arranca solo).
2. Abre Claude Desktop. Deberias ver las herramientas de Unity disponibles
   (icono de herramientas / enchufe).
3. Pidele cosas, por ejemplo:
   - "Crea un cubo llamado Suelo en Unity."
   - "Muestrame la jerarquia de la escena."
   - "Entra en Play Mode."
   - "Lee los errores de la consola."

## Comandos disponibles

| Herramienta MCP        | Comando Unity                  | Que hace                              |
|------------------------|--------------------------------|---------------------------------------|
| get_project_info       | unity_get_project_info         | Info del proyecto/escena              |
| get_scene_hierarchy    | unity_get_scene_hierarchy      | Jerarquia de la escena                |
| get_object_components  | unity_get_object_components    | Componentes de un GameObject          |
| modify_transform       | unity_modify_transform         | Mover/rotar/escalar un objeto         |
| create_gameobject      | unity_create_gameobject        | Crear GameObject (primitiva opcional) |
| read_script            | unity_read_script              | Leer un script de Assets              |
| write_script           | unity_write_script             | Crear/editar un script en Assets      |
| get_console_logs       | unity_get_console_logs         | Leer la consola                       |
| enter_play_mode        | unity_enter_play_mode          | Entrar en Play                        |
| exit_play_mode         | unity_exit_play_mode           | Salir de Play                         |
| compile_scripts        | unity_compile_scripts          | Recompilar scripts                    |

## Problemas comunes

- **"No se pudo conectar a Unity":** el Editor no esta abierto o el servidor no
  esta corriendo. Abre Unity y revisa Tools > MCP Server > Start. Confirma el
  log `[MCP] Server started on ws://127.0.0.1:6400`.
- **Claude Desktop no muestra las herramientas:** revisa que la ruta de
  `server.py` en el config sea correcta, que `python` este en el PATH
  (prueba `python --version` en una terminal) y reinicia Claude Desktop.
- **En Windows con varias versiones de Python:** usa la ruta completa al
  ejecutable en `"command"` (ej: `"C:/Python311/python.exe"`).
