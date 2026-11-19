using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Generador ligero de partículas Pixel-Art 2D para saltos, aterrizajes, rebotes en muro
    /// y recolección de Scooby Galletas.
    /// </summary>
    public class KimayaPixelVFX : MonoBehaviour
    {
        public static bool ParticlesEnabled = true;
        private static KimayaPixelVFX _instance;
        private static Sprite _pixelSpr;

        private struct PixelParticle
        {
            public Transform tr;
            public SpriteRenderer sr;
            public Vector2 vel;
            public float gravity;
            public float life;
            public float maxLife;
            public float startScale;
            public Color baseColor;
        }

        private readonly List<PixelParticle> _active = new List<PixelParticle>();

        private static KimayaPixelVFX EnsureInstance()
        {
            if (_instance == null)
            {
                var go = new GameObject("KimayaPixelVFX_Manager");
                _instance = go.AddComponent<KimayaPixelVFX>();
            }
            if (_pixelSpr == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                var cols = new Color32[16];
                for (int i = 0; i < 16; i++) cols[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(cols);
                tex.Apply();
                _pixelSpr = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 36f);
            }
            return _instance;
        }

        public static void SpawnJumpDust(Vector3 feetPos)
        {
            var inst = EnsureInstance();
            Color[] palette = new Color[]
            {
                new Color(0.62f, 0.94f, 0.48f, 0.95f),
                new Color(0.88f, 0.96f, 0.76f, 0.95f),
                new Color(0.76f, 0.58f, 0.38f, 0.90f)
            };
            for (int i = 0; i < 6; i++)
            {
                float vx = Random.Range(-1.8f, 1.8f);
                float vy = Random.Range(0.7f, 2.3f);
                Color c = palette[i % palette.Length];
                inst.Emit(feetPos + new Vector3(Random.Range(-0.12f, 0.12f), 0.04f, 0f), new Vector2(vx, vy), 3.2f, Random.Range(0.28f, 0.44f), Random.Range(0.65f, 1.05f), c);
            }
        }

        public static void SpawnLandDust(Vector3 feetPos)
        {
            var inst = EnsureInstance();
            Color[] palette = new Color[]
            {
                new Color(0.86f, 0.94f, 0.74f, 0.92f),
                new Color(0.58f, 0.88f, 0.46f, 0.92f),
                new Color(0.72f, 0.55f, 0.36f, 0.88f)
            };
            for (int i = 0; i < 6; i++)
            {
                float dir = (i % 2 == 0) ? -1f : 1f;
                float vx = dir * Random.Range(0.6f, 2.1f);
                float vy = Random.Range(0.35f, 1.15f);
                Color c = palette[i % palette.Length];
                inst.Emit(feetPos + new Vector3(dir * 0.08f, 0.03f, 0f), new Vector2(vx, vy), 2.5f, Random.Range(0.24f, 0.38f), Random.Range(0.60f, 0.95f), c);
            }
        }

        public static void SpawnWallSparks(Vector3 hitPos, float normalX)
        {
            var inst = EnsureInstance();
            for (int i = 0; i < 5; i++)
            {
                float vx = normalX * Random.Range(0.8f, 2.4f);
                float vy = Random.Range(-0.8f, 1.8f);
                Color c = (i % 2 == 0) ? new Color(1f, 0.86f, 0.42f, 0.95f) : new Color(0.85f, 0.68f, 0.48f, 0.92f);
                inst.Emit(hitPos, new Vector2(vx, vy), 4.0f, Random.Range(0.22f, 0.35f), Random.Range(0.55f, 0.90f), c);
            }
        }

        public static void SpawnCookieSparkles(Vector3 pos)
        {
            var inst = EnsureInstance();
            Color[] palette = new Color[]
            {
                new Color(1.00f, 0.88f, 0.28f, 1f),
                new Color(0.45f, 1.00f, 0.68f, 1f),
                new Color(1.00f, 0.98f, 0.82f, 1f)
            };
            for (int i = 0; i < 12; i++)
            {
                float ang = (i / 12f) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
                float spd = Random.Range(1.2f, 3.2f);
                Vector2 vel = new Vector2(Mathf.Cos(ang) * spd, Mathf.Sin(ang) * spd + 0.8f);
                inst.Emit(pos, vel, 2.8f, Random.Range(0.38f, 0.62f), Random.Range(0.75f, 1.25f), palette[i % palette.Length]);
            }
        }

        private void Emit(Vector3 pos, Vector2 vel, float gravity, float maxLife, float startScale, Color col)
        {
            if (!ParticlesEnabled) return;
            var go = new GameObject("PxParticle");
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(startScale, startScale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _pixelSpr;
            sr.color = col;
            sr.sortingOrder = 620;

            _active.Add(new PixelParticle
            {
                tr = go.transform,
                sr = sr,
                vel = vel,
                gravity = gravity,
                life = 0f,
                maxLife = maxLife,
                startScale = startScale,
                baseColor = col
            });
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var p = _active[i];
                if (p.tr == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                p.life += dt;
                if (p.life >= p.maxLife)
                {
                    Destroy(p.tr.gameObject);
                    _active.RemoveAt(i);
                    continue;
                }

                p.vel.y -= p.gravity * dt;
                p.tr.position += new Vector3(p.vel.x * dt, p.vel.y * dt, 0f);

                float t = Mathf.Clamp01(p.life / p.maxLife);
                float sc = Mathf.Lerp(p.startScale, p.startScale * 0.25f, t);
                p.tr.localScale = new Vector3(sc, sc, 1f);

                Color c = p.baseColor;
                c.a = p.baseColor.a * (1f - t * t);
                p.sr.color = c;

                _active[i] = p;
            }
        }
    }
}
