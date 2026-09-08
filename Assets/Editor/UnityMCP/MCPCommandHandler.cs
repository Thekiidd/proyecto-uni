// MCPCommandHandler — executes MCP commands using Unity Editor APIs. Runs on the main thread.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityMCP
{
    [InitializeOnLoad]
    public static class MCPCommandHandler
    {
        // Ring buffer of recent console logs.
        struct LogEntry { public string message; public string stack; public string type; }
        static readonly List<LogEntry> _logs = new List<LogEntry>();
        const int MaxLogs = 200;

        static MCPCommandHandler()
        {
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
        }

        static void OnLog(string condition, string stackTrace, LogType type)
        {
            lock (_logs)
            {
                _logs.Add(new LogEntry { message = condition, stack = stackTrace, type = type.ToString() });
                if (_logs.Count > MaxLogs) _logs.RemoveAt(0);
            }
        }

        public static string Handle(string command, Dictionary<string, object> args)
        {
            object result;
            switch (command)
            {
                case "unity_get_project_info": result = GetProjectInfo(); break;
                case "unity_get_scene_hierarchy": result = GetSceneHierarchy(); break;
                case "unity_get_object_components": result = GetObjectComponents(Str(args, "gameObjectName")); break;
                case "unity_modify_transform": result = ModifyTransform(args); break;
                case "unity_create_gameobject": result = CreateGameObject(args); break;
                case "unity_read_script": result = ReadScript(Str(args, "scriptPath")); break;
                case "unity_write_script": result = WriteScript(Str(args, "scriptPath"), Str(args, "content")); break;
                case "unity_get_console_logs": result = GetConsoleLogs(args); break;
                case "unity_enter_play_mode": result = SetPlayMode(true); break;
                case "unity_exit_play_mode": result = SetPlayMode(false); break;
                case "unity_compile_scripts": result = CompileScripts(); break;
                default: result = Err("Unknown command: " + command); break;
            }
            return Json.Serialize(result);
        }

        // ---- Command implementations ----

        static object GetProjectInfo()
        {
            var scene = SceneManager.GetActiveScene();
            return new Dictionary<string, object>
            {
                { "productName", Application.productName },
                { "unityVersion", Application.unityVersion },
                { "activeScene", scene.name },
                { "scenePath", scene.path },
                { "dataPath", Application.dataPath },
                { "isPlaying", Application.isPlaying },
            };
        }

        static object GetSceneHierarchy()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var list = new List<object>();
            foreach (var go in roots) list.Add(NodeOf(go));
            return new Dictionary<string, object> { { "scene", scene.name }, { "objects", list } };
        }

        static object NodeOf(GameObject go)
        {
            var children = new List<object>();
            foreach (Transform c in go.transform) children.Add(NodeOf(c.gameObject));
            return new Dictionary<string, object>
            {
                { "name", go.name },
                { "active", go.activeSelf },
                { "tag", go.tag },
                { "children", children },
            };
        }

        static object GetObjectComponents(string name)
        {
            var go = Find(name);
            if (go == null) return Err("GameObject not found: " + name);
            var comps = new List<object>();
            foreach (var c in go.GetComponents<Component>())
            {
                if (c == null) { comps.Add(new Dictionary<string, object> { { "type", "<missing script>" } }); continue; }
                comps.Add(new Dictionary<string, object> { { "type", c.GetType().Name } });
            }
            var t = go.transform;
            return new Dictionary<string, object>
            {
                { "name", go.name },
                { "position", Vec(t.position) },
                { "rotation", Vec(t.eulerAngles) },
                { "scale", Vec(t.localScale) },
                { "components", comps },
            };
        }

        static object ModifyTransform(Dictionary<string, object> args)
        {
            var go = Find(Str(args, "gameObjectName"));
            if (go == null) return Err("GameObject not found");
            Undo.RecordObject(go.transform, "MCP Modify Transform");
            var t = go.transform;
            if (args.ContainsKey("position")) t.position = ToVec(args["position"], t.position);
            if (args.ContainsKey("rotation")) t.eulerAngles = ToVec(args["rotation"], t.eulerAngles);
            if (args.ContainsKey("scale")) t.localScale = ToVec(args["scale"], t.localScale);
            EditorUtility.SetDirty(go);
            return new Dictionary<string, object>
            {
                { "ok", true }, { "name", go.name },
                { "position", Vec(t.position) }, { "rotation", Vec(t.eulerAngles) }, { "scale", Vec(t.localScale) },
            };
        }

        static object CreateGameObject(Dictionary<string, object> args)
        {
            string name = Str(args, "name");
            string primitive = Str(args, "primitive");
            string parentName = Str(args, "parentName");
            GameObject go;
            if (!string.IsNullOrEmpty(primitive) && primitive != "None" &&
                Enum.TryParse(primitive, out PrimitiveType pt))
                go = GameObject.CreatePrimitive(pt);
            else
                go = new GameObject();
            if (!string.IsNullOrEmpty(name)) go.name = name;
            if (!string.IsNullOrEmpty(parentName))
            {
                var parent = Find(parentName);
                if (parent != null) go.transform.SetParent(parent.transform, false);
            }
            Undo.RegisterCreatedObjectUndo(go, "MCP Create GameObject");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return new Dictionary<string, object> { { "ok", true }, { "name", go.name } };
        }

        static object ReadScript(string scriptPath)
        {
            string full = Path.Combine(Application.dataPath, scriptPath);
            if (!File.Exists(full)) return Err("File not found: Assets/" + scriptPath);
            return new Dictionary<string, object> { { "path", "Assets/" + scriptPath }, { "content", File.ReadAllText(full) } };
        }

        static object WriteScript(string scriptPath, string content)
        {
            if (string.IsNullOrEmpty(scriptPath)) return Err("scriptPath required");
            string full = Path.Combine(Application.dataPath, scriptPath);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, content ?? "");
            AssetDatabase.Refresh();
            return new Dictionary<string, object> { { "ok", true }, { "path", "Assets/" + scriptPath } };
        }

        static object GetConsoleLogs(Dictionary<string, object> args)
        {
            int count = (int)Num(args, "count", 20);
            string filter = Str(args, "filter");
            if (string.IsNullOrEmpty(filter)) filter = "all";
            var outList = new List<object>();
            lock (_logs)
            {
                for (int i = _logs.Count - 1; i >= 0 && outList.Count < count; i--)
                {
                    var e = _logs[i];
                    bool match = filter == "all"
                        || (filter == "error" && (e.type == "Error" || e.type == "Exception" || e.type == "Assert"))
                        || (filter == "warning" && e.type == "Warning")
                        || (filter == "info" && e.type == "Log");
                    if (match) outList.Add(new Dictionary<string, object> { { "type", e.type }, { "message", e.message } });
                }
            }
            return new Dictionary<string, object> { { "count", outList.Count }, { "logs", outList } };
        }

        static object SetPlayMode(bool play)
        {
            EditorApplication.isPlaying = play;
            return new Dictionary<string, object> { { "ok", true }, { "isPlaying", play } };
        }

        static object CompileScripts()
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            return new Dictionary<string, object> { { "ok", true }, { "message", "Compilation requested" } };
        }

        // ---- Helpers ----

        static GameObject Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var direct = GameObject.Find(name);
            if (direct != null) return direct;
            foreach (var go in GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (go.name == name) return go;
            return null;
        }

        static Dictionary<string, object> Vec(Vector3 v)
        {
            return new Dictionary<string, object> { { "x", v.x }, { "y", v.y }, { "z", v.z } };
        }

        static Vector3 ToVec(object o, Vector3 fallback)
        {
            if (!(o is Dictionary<string, object> d)) return fallback;
            return new Vector3(
                d.ContainsKey("x") ? (float)ToDouble(d["x"]) : fallback.x,
                d.ContainsKey("y") ? (float)ToDouble(d["y"]) : fallback.y,
                d.ContainsKey("z") ? (float)ToDouble(d["z"]) : fallback.z);
        }

        static double ToDouble(object o)
        {
            if (o is long l) return l;
            if (o is double dd) return dd;
            if (o is int i) return i;
            double r; double.TryParse(Convert.ToString(o, System.Globalization.CultureInfo.InvariantCulture),
                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out r);
            return r;
        }

        static string Str(Dictionary<string, object> args, string key)
        {
            return args != null && args.ContainsKey(key) ? args[key] as string : null;
        }

        static double Num(Dictionary<string, object> args, string key, double fallback)
        {
            if (args == null || !args.ContainsKey(key)) return fallback;
            return ToDouble(args[key]);
        }

        static Dictionary<string, object> Err(string msg)
        {
            return new Dictionary<string, object> { { "error", msg } };
        }
    }
}
