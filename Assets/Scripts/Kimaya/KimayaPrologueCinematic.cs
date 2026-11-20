using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    /// <summary>
    /// Cinemática ilustrada del Prólogo de Kimaya (Escenas 1 a 6: Paseo, Pelota Roja,
    /// Tronco Hueco, Bosque del Eco, Captura de Kiara por el Zarzal Sombrío y Aparición de Maya).
    /// Se activa automáticamente al entrar en la escena Prologue_Story antes de llegar al Hub.
    /// </summary>
    public class KimayaPrologueCinematic : MonoBehaviour
    {
        private struct StoryBeat
        {
            public string sceneRes;
            public string chapter;
            public string speaker;
            public Color badgeColor;
            public string text;
            public float autoDuration;
            public KimayaAudio.SFX? cueSfx;

            public StoryBeat(string sceneRes, string chapter, string speaker, Color badgeColor, string text, float autoDuration, KimayaAudio.SFX? cueSfx = null)
            {
                this.sceneRes = sceneRes;
                this.chapter = chapter;
                this.speaker = speaker;
                this.badgeColor = badgeColor;
                this.text = text;
                this.autoDuration = autoDuration;
                this.cueSfx = cueSfx;
            }
        }

        private static readonly StoryBeat[] Beats = new StoryBeat[]
        {
            new StoryBeat("Story/Prologue/Scene_01", "PRÓLOGO · ESCENA 1", "NARRACIÓN", new Color(0.45f, 0.28f, 0.12f),
                "Era una tarde tranquila en el sendero. Kiara, la hermana mayor, caminaba junto a Mía y Falin en su paseo de siempre.", 4.8f, KimayaAudio.SFX.Bark),
            new StoryBeat("Story/Prologue/Scene_02", "PRÓLOGO · ESCENA 2", "FALIN  /  KIARA", new Color(0.18f, 0.42f, 0.72f),
                "De pronto, una pelota roja rebotó hacia lo profundo del bosque.\n— Falin: «¡Yo la alcanzo!»   — Kiara: «¡Falin, espera, no te alejes!»", 5.2f, KimayaAudio.SFX.Jump),
            new StoryBeat("Story/Prologue/Scene_03", "PRÓLOGO · ESCENA 3", "NARRACIÓN", new Color(0.16f, 0.50f, 0.34f),
                "La pelota rodó dentro de un enorme tronco hueco cubierto de musgo y destellos dorados. Sin pensarlo, las tres hermanas cruzaron hacia la luz...", 5.2f, KimayaAudio.SFX.DoorOpen),
            new StoryBeat("Story/Prologue/Scene_04", "PRÓLOGO · ESCENA 4", "MÍA  /  FALIN", new Color(0.74f, 0.22f, 0.18f),
                "Despertaron en El Bosque del Eco, un reino desconocido.\n— Mía: «¡Mira dónde terminamos, Falin! Siempre corres sin pensar.»\n— Falin: «¡Tú dijiste que la pelota estaba por aquí!»", 5.8f, KimayaAudio.SFX.HeavyFall),
            new StoryBeat("Story/Prologue/Scene_04", "PRÓLOGO · ESCENA 4", "KIARA", new Color(0.50f, 0.22f, 0.65f),
                "— Kiara: «Basta, discutir no nos llevará a casa... Quédense cerca. Bajo nuestras patas las raíces tiemblan: algo nos observa.»", 5.0f),
            new StoryBeat("Story/Prologue/Scene_05", "PRÓLOGO · ESCENA 5", "KIARA  /  EL ZARZAL SOMBRÍO", new Color(0.66f, 0.12f, 0.42f),
                "— Kiara: «¡Cuidado! ¡Detrás de mí!»\nUnas raíces espinosas surgieron del suelo. Kiara saltó frente a sus hermanas para protegerlas... y el Zarzal Sombrío la atrapó.", 6.0f, KimayaAudio.SFX.Damage),
            new StoryBeat("Story/Prologue/Scene_05", "PRÓLOGO · ESCENA 5", "FALIN Y MÍA", new Color(0.75f, 0.15f, 0.15f),
                "— Falin y Mía: «¡¡KIARA!!»\nLas espinas la arrastraron hacia las alturas del Sendero de las Raíces. Falin y Mía corrieron tras ella sin dudarlo.", 5.2f, KimayaAudio.SFX.HeavyFall),
            new StoryBeat("Story/Prologue/Scene_06", "PRÓLOGO · ESCENA 6", "MAYA (HADA GUARDIANA)", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «¡Deténganse! Si corren tras ella sin saber adónde van, el Zarzal Sombrío también las atrapará.»", 5.0f, KimayaAudio.SFX.Checkpoint),
            new StoryBeat("Story/Prologue/Scene_06", "PRÓLOGO · ESCENA 6", "MAYA (HADA GUARDIANA)", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «Ella aún está cerca, pero el portal está sellado. Síganme al Círculo de Hadas y hablen conmigo para abrir el camino hacia Kiara.»", 5.6f),
        };

        private static bool _hookRegistered = false;
        private Texture2D[] _loadedTextures;
        private int _beatIndex = 0;
        private float _beatTimer = 0f;
        private float _fadeAlpha = 1f;
        private bool _exiting = false;
        private int _lastTypedChars = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterSceneHook()
        {
            if (!_hookRegistered)
            {
                _hookRegistered = true;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            TryAttachToPrologue(SceneManager.GetActiveScene().name);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachToPrologue(scene.name);
        }

        private static void TryAttachToPrologue(string sceneName)
        {
            if (sceneName != "Prologue_Story") return;
            if (FindAnyObjectByType<KimayaPrologueCinematic>() != null) return;

            var go = new GameObject("KimayaPrologueCinematic_Runtime");
            go.AddComponent<KimayaPrologueCinematic>();
        }

        private void Start()
        {
            Time.timeScale = 1f;

            // Ocultar canvas heredado viejo de la escena Prologue_Story para mostrar nuestra cinemática ilustrada limpia
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if (canvas != null) canvas.gameObject.SetActive(false);
            }

            _loadedTextures = new Texture2D[Beats.Length];
            for (int i = 0; i < Beats.Length; i++)
            {
                _loadedTextures[i] = Resources.Load<Texture2D>(Beats[i].sceneRes);
            }

            StartBeat(0);
        }

        private void StartBeat(int index)
        {
            if (index >= Beats.Length)
            {
                FinishPrologue();
                return;
            }

            _beatIndex = index;
            _beatTimer = 0f;
            _lastTypedChars = 0;
            _fadeAlpha = 1f;

            if (Beats[index].cueSfx.HasValue && KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(Beats[index].cueSfx.Value, 0.85f);
            }
        }

        private void Update()
        {
            if (_exiting) return;

            _beatTimer += Time.deltaTime;
            _fadeAlpha = Mathf.MoveTowards(_fadeAlpha, 0f, Time.deltaTime * 2.8f);

            StoryBeat cur = Beats[_beatIndex];
            float charsPerSec = 42f;
            int visibleChars = Mathf.Min(cur.text.Length, Mathf.FloorToInt(_beatTimer * charsPerSec));
            if (visibleChars > _lastTypedChars && visibleChars % 5 == 0 && visibleChars < cur.text.Length)
            {
                if (KimayaAudio.Instance != null)
                    KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Step, 0.22f);
            }
            _lastTypedChars = visibleChars;

            bool advancePressed = Kimaya.Networking.PhonePadServer.Pressed("A") || Kimaya.Networking.PhonePadServer.Pressed("B");
            bool skipPressed = Kimaya.Networking.PhonePadServer.Pressed("START");

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
                    advancePressed = true;
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    skipPressed = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Evitar avanzar si hace click sobre los botones inferiores
                Vector2 mp = Mouse.current.position.ReadValue();
                if (mp.y > 76f) advancePressed = true;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                    advancePressed = true;
                if (Input.GetKeyDown(KeyCode.Escape))
                    skipPressed = true;
            }
            catch { }

            if (skipPressed)
            {
                FinishPrologue();
                return;
            }

            if (advancePressed)
            {
                if (visibleChars < cur.text.Length - 2)
                {
                    // Primer toque completa todo el texto del diálogo actual
                    _beatTimer = cur.text.Length / charsPerSec;
                }
                else
                {
                    StartBeat(_beatIndex + 1);
                }
                return;
            }

            // Avance automático fluido para que funcione como cinemática de película
            if (_beatTimer >= cur.autoDuration)
            {
                StartBeat(_beatIndex + 1);
            }
        }

        private void FinishPrologue()
        {
            if (_exiting) return;
            _exiting = true;
            PlayerPrefs.SetInt("Kimaya_SeenPrologue", 1);
            PlayerPrefs.Save();
            KimayaSceneTransition.LoadScene("Hub");
        }

        private void OnGUI()
        {
            if (_exiting || _beatIndex < 0 || _beatIndex >= Beats.Length) return;

            GUI.depth = -10;
            StoryBeat cur = Beats[_beatIndex];
            Texture2D tex = _loadedTextures != null ? _loadedTextures[_beatIndex] : null;

            // Fondo negro cinematográfico
            Color prev = GUI.color;
            GUI.color = new Color(0.04f, 0.03f, 0.07f, 1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Ilustración fija en pantalla completa (sin zoom ni movimiento)
            float dlgH = Mathf.Clamp(Screen.height * 0.24f, 150f, 205f);

            if (tex != null)
            {
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), tex, ScaleMode.ScaleAndCrop);
            }

            // Franja superior oscura semitransparente para que el título se lea perfecto
            GUI.color = new Color(0.04f, 0.03f, 0.08f, 0.62f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 46f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Encabezado superior (Capítulo + Progreso)
            GUIStyle topSt = KimayaUITheme.GetOnGUILabelStyle(14, TextAnchor.MiddleLeft, false);
            GUIStyle progSt = KimayaUITheme.GetOnGUILabelStyle(13, TextAnchor.MiddleRight, false);
            GUI.Label(new Rect(28f, 10f, Screen.width * 0.6f, 28f), $"<color=#F8D458>{cur.chapter}</color>", topSt);
            GUI.Label(new Rect(Screen.width * 0.6f, 10f, Screen.width * 0.4f - 28f, 28f), $"<color=#94A3B8>ESCENA {_beatIndex + 1} / {Beats.Length}</color>", progSt);

            // Caja de Diálogo inferior estilo RPG Pixel-Art
            float boxW = Mathf.Min(1100f, Screen.width - 48f);
            float boxX = (Screen.width - boxW) * 0.5f;
            float boxY = Screen.height - dlgH - 14f;

            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.98f);
            GUI.DrawTexture(new Rect(boxX - 3f, boxY - 3f, boxW + 6f, dlgH + 6f), Texture2D.whiteTexture);
            GUI.color = new Color(0.09f, 0.08f, 0.15f, 0.96f);
            GUI.DrawTexture(new Rect(boxX, boxY, boxW, dlgH), Texture2D.whiteTexture);

            // Placa del personaje / narrador
            float badgeW = 320f;
            GUI.color = cur.badgeColor;
            GUI.DrawTexture(new Rect(boxX + 18f, boxY - 16f, badgeW, 32f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle badgeSt = KimayaUITheme.GetOnGUILabelStyle(14, TextAnchor.MiddleCenter, false);
            GUI.Label(new Rect(boxX + 18f, boxY - 16f, badgeW, 32f), $"<color=#FFFFFF>{cur.speaker}</color>", badgeSt);

            // Texto con efecto máquina de escribir
            int visibleChars = Mathf.Min(cur.text.Length, Mathf.FloorToInt(_beatTimer * 42f));
            string shownText = cur.text.Substring(0, visibleChars);
            GUIStyle bodySt = KimayaUITheme.GetOnGUILabelStyle(21, TextAnchor.UpperLeft, true);
            GUI.Label(new Rect(boxX + 24f, boxY + 26f, boxW - 48f, dlgH - 72f), $"<color=#F8FAFC>{shownText}</color>", bodySt);

            // Barra de progreso de auto-avance
            float autoPct = Mathf.Clamp01(_beatTimer / cur.autoDuration);
            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.65f);
            GUI.DrawTexture(new Rect(boxX + 20f, boxY + dlgH - 46f, (boxW - 40f) * autoPct, 3f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Botones inferiores: Siguiente y Saltar
            GUIStyle btnSt = KimayaUITheme.GetOnGUIButtonStyle(12);
            if (GUI.Button(new Rect(boxX + boxW - 390f, boxY + dlgH - 40f, 195f, 32f), "SIGUIENTE [ESPACIO/A]", btnSt))
            {
                StartBeat(_beatIndex + 1);
            }
            if (GUI.Button(new Rect(boxX + boxW - 185f, boxY + dlgH - 40f, 165f, 32f), "SALTAR [ESC]", btnSt))
            {
                FinishPrologue();
            }

            // Fade suave de entrada de cada ilustración
            if (_fadeAlpha > 0.01f)
            {
                GUI.color = new Color(0f, 0f, 0f, _fadeAlpha * 0.65f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            }
            GUI.color = prev;
        }
    }
}
