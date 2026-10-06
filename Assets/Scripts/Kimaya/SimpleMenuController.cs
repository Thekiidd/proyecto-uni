using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Controlador del menú simple.
    /// Conecta el botón PLAY y anima las luciérnagas.
    /// </summary>
    public class SimpleMenuController : MonoBehaviour
    {
        private float _t = 0f;

        private void Start()
        {
            Time.timeScale = 1f;

            var btn = GameObject.Find("Btn_Play")?.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnPlay);
        }

        private void OnPlay()
        {
            // Intenta cargar Historia; si no existe va directo al Hub o Level
            string next = SceneExists("Historia") ? "Historia"
                        : SceneExists("Hub")      ? "Hub"
                        : "Level_01_Bosque";
            SceneManager.LoadScene(next);
        }

        private bool SceneExists(string name)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                if (path.Contains(name)) return true;
            }
            return false;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            // Anima luciérnagas con pulso suave
            for (int i = 0; i < 10; i++)
            {
                var go = GameObject.Find($"Fairy_{i}");
                if (go == null) continue;
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr == null) continue;
                float alpha = 0.35f + 0.55f * Mathf.Sin(_t * 1.2f + i * 0.65f);
                float yOff  = Mathf.Sin(_t * 0.8f + i * 0.5f) * 0.05f;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
                go.transform.localPosition = new Vector3(
                    go.transform.localPosition.x,
                    go.transform.localPosition.y + yOff * Time.deltaTime,
                    go.transform.localPosition.z);
            }
        }
    }
}
