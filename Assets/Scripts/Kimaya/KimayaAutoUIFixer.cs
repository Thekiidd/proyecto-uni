using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Kimaya
{
    /// <summary>
    /// Se ejecuta automáticamente al cargar cualquier escena en Play mode.
    /// Garantiza que:
    ///   1. Siempre exista un EventSystem con InputSystemUIInputModule (¡hace que los botones respondan a clics!).
    ///   2. Los botones de MainMenu (Btn_Jugar, Btn_Play, Btn_Salir, etc.) funcionen de inmediato.
    ///   3. Las etiquetas y fondos no bloqueen los raycasts del ratón.
    ///   4. Los botones de Historia, Hub y Ending queden conectados automáticamente.
    /// </summary>
    public static class KimayaAutoUIFixer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnSceneLoaded()
        {
            Time.timeScale = 1f;

            // 1. Asegurar EventSystem en cualquier escena que tenga Canvas
            EnsureEventSystem();

            // 2. Desactivar raycastTarget en textos dentro de botones para que el clic pase al botón
            FixButtonRaycasts();

            // 3. Conectar botones según la escena activa
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene.Contains("MainMenu") || GameObject.Find("Btn_Jugar") != null || GameObject.Find("Btn_Play") != null)
            {
                SetupMainMenuButtons();
            }
            else if (currentScene.Contains("Historia") || GameObject.Find("Btn_Skip") != null)
            {
                SetupHistoriaButtons();
            }
            else if (currentScene.Contains("Ending") || GameObject.Find("Btn_Rejugar") != null)
            {
                SetupEndingButtons();
            }
        }

        private static void EnsureEventSystem()
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var es = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[Kimaya] EventSystem creado automáticamente con InputSystemUIInputModule.");
            }
            else
            {
                // Asegurar que tenga el módulo correcto del nuevo Input System
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    var oldModule = es.GetComponent<StandaloneInputModule>();
                    if (oldModule != null) UnityEngine.Object.Destroy(oldModule);
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                    Debug.Log("[Kimaya] Módulo de EventSystem actualizado a InputSystemUIInputModule.");
                }
            }
        }

        private static void FixButtonRaycasts()
        {
            // Las etiquetas de texto dentro de un botón no deben bloquear el raycast del botón
            foreach (var btn in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (var txt in btn.GetComponentsInChildren<Text>(true))
                    txt.raycastTarget = false;

                // Si hay imágenes hijas decorativas dentro del botón
                foreach (var img in btn.GetComponentsInChildren<Image>(true))
                {
                    if (img.gameObject != btn.gameObject)
                        img.raycastTarget = false;
                    else
                        img.raycastTarget = true; // La imagen del botón sí debe recibir clic
                }
            }

            // Los fondos decorativos nunca deben bloquear clics
            string[] nonClickablePanels = { "Overlay", "Separator", "BG", "Background", "TitleLine" };
            foreach (var panelName in nonClickablePanels)
            {
                var go = GameObject.Find(panelName);
                if (go != null)
                {
                    var img = go.GetComponent<Image>();
                    if (img != null && go.GetComponent<Button>() == null)
                        img.raycastTarget = false;
                }
            }
        }

        private static void SetupMainMenuButtons()
        {
            // Botón JUGAR / PLAY
            var btnPlay = GameObject.Find("Btn_Jugar")?.GetComponent<Button>()
                       ?? GameObject.Find("Btn_Play")?.GetComponent<Button>();

            if (btnPlay != null)
            {
                btnPlay.onClick.RemoveAllListeners();
                btnPlay.onClick.AddListener(() =>
                {
                    Debug.Log("[Kimaya] Botón JUGAR presionado. Cargando siguiente escena...");
                    string next = SceneExists("Historia") ? "Historia"
                                : SceneExists("Hub")      ? "Hub"
                                : "Level_01_Bosque";
                    SceneManager.LoadScene(next);
                });
            }

            // Botón SALIR
            var btnQuit = GameObject.Find("Btn_Salir")?.GetComponent<Button>();
            if (btnQuit != null)
            {
                btnQuit.onClick.RemoveAllListeners();
                btnQuit.onClick.AddListener(() =>
                {
                    Debug.Log("[Kimaya] Botón SALIR presionado.");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }

            // Botón CRÉDITOS
            var btnCred = GameObject.Find("Btn_Creditos")?.GetComponent<Button>();
            if (btnCred != null)
            {
                btnCred.onClick.RemoveAllListeners();
                btnCred.onClick.AddListener(() =>
                {
                    Debug.Log("[Kimaya] Créditos: Cedillo · Baca · García · Batista · Mora · Santoyo (UTCH IDGS102N 2026)");
                });
            }
        }

        private static void SetupHistoriaButtons()
        {
            var skipBtn = GameObject.Find("Btn_Skip")?.GetComponent<Button>();
            if (skipBtn != null)
            {
                skipBtn.onClick.RemoveAllListeners();
                skipBtn.onClick.AddListener(() =>
                {
                    string next = SceneExists("Hub") ? "Hub" : "Level_01_Bosque";
                    SceneManager.LoadScene(next);
                });
            }
        }

        private static void SetupEndingButtons()
        {
            var menuBtn = GameObject.Find("Btn_Menu")?.GetComponent<Button>();
            if (menuBtn != null)
            {
                menuBtn.onClick.RemoveAllListeners();
                menuBtn.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
            }

            var replayBtn = GameObject.Find("Btn_Rejugar")?.GetComponent<Button>();
            if (replayBtn != null)
            {
                replayBtn.onClick.RemoveAllListeners();
                replayBtn.onClick.AddListener(() => SceneManager.LoadScene("Level_01_Bosque"));
            }
        }

        private static bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (path.EndsWith("/" + sceneName + ".unity", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith("\\" + sceneName + ".unity", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
