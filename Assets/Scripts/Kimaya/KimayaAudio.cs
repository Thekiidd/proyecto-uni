using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Maneja todo el audio del juego Kimaya (Música Retro de Menú y Hub/Torre + SFX).
    /// Singleton automático que persiste entre escenas y se sincroniza con Ajustes (KimayaSettingsController).
    /// </summary>
    public class KimayaAudio : MonoBehaviour
    {
        public static KimayaAudio Instance { get; private set; }

        public enum SFX
        {
            Jump, PowerJump, Land, Switch, DoorOpen, Checkpoint,
            Smell, Damage, Victory, Step,
            WallBounce, HeavyFall, FallWhistle, ChargeTick, Bark
        }

        public enum Music { None, Menu, Forest, Ending }

        [Header("Music Sources")]
        public AudioSource musicSource;
        public float musicVolume = 0.8f;

        [Header("SFX Source")]
        public AudioSource sfxSource;
        public float sfxVolume = 0.8f;

        [Header("Music Clips")]
        public AudioClip menuMusic;
        public AudioClip forestMusic;
        public AudioClip endingMusic;

        [Header("SFX Clips")]
        public AudioClip jumpSFX;
        public AudioClip powerJumpSFX;
        public AudioClip landSFX;
        public AudioClip switchSFX;
        public AudioClip doorOpenSFX;
        public AudioClip checkpointSFX;
        public AudioClip smellSFX;
        public AudioClip damageSFX;
        public AudioClip victorySFX;
        public AudioClip stepSFX;
        public AudioClip wallBounceSFX;
        public AudioClip heavyFallSFX;
        public AudioClip fallWhistleSFX;
        public AudioClip chargeTickSFX;
        public AudioClip barkSFX;

        private Music _currentMusic = Music.None;
        private Coroutine _fadeRoutine;
        private static bool _sceneHookRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapAudio()
        {
            EnsureInstance();
            if (!_sceneHookRegistered)
            {
                _sceneHookRegistered = true;
                SceneManager.sceneLoaded += (scene, mode) =>
                {
                    if (Instance != null) Instance.PlayMusicForScene(scene.name);
                };
            }
            if (Instance != null)
            {
                Instance.PlayMusicForScene(SceneManager.GetActiveScene().name);
            }
        }

        public static KimayaAudio EnsureInstance()
        {
            if (Instance == null)
            {
                var existing = FindAnyObjectByType<KimayaAudio>();
                if (existing != null)
                {
                    Instance = existing;
                }
                else
                {
                    var go = new GameObject("KimayaAudio_Global");
                    Instance = go.AddComponent<KimayaAudio>();
                }
            }
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicVolume = PlayerPrefs.GetFloat("Kimaya_MusicVolume", 0.8f);
            sfxVolume   = PlayerPrefs.GetFloat("Kimaya_SFXVolume", 0.8f);
            AudioListener.volume = 1f;

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.spatialBlend = 0f;
                musicSource.volume = musicVolume;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
                sfxSource.volume = sfxVolume;
            }

            LoadMusicClipsFromResources();
            GenerateProceduralSounds();
        }

        private void LoadMusicClipsFromResources()
        {
            if (menuMusic == null)
                menuMusic = Resources.Load<AudioClip>("Music/Title_Screen_Sunrise");
            if (forestMusic == null)
                forestMusic = Resources.Load<AudioClip>("Music/Under_the_Emerald_Boughs");
            if (endingMusic == null)
                endingMusic = forestMusic != null ? forestMusic : menuMusic;
        }

        public void PlayMusicForScene(string sceneName)
        {
            LoadMusicClipsFromResources();
            if (string.IsNullOrEmpty(sceneName)) return;

            if (sceneName.Contains("MainMenu") || sceneName.Contains("Prologue"))
            {
                PlayMusic(Music.Menu, fade: true);
            }
            else
            {
                PlayMusic(Music.Forest, fade: true);
            }
        }

        // ── Música ────────────────────────────────────────────────────────
        public void PlayMusic(Music music, bool fade = true)
        {
            LoadMusicClipsFromResources();
            AudioClip clip = music switch {
                Music.Menu    => menuMusic,
                Music.Forest  => forestMusic,
                Music.Ending  => endingMusic,
                _             => null
            };
            if (clip == null || musicSource == null) return;

            if (_currentMusic == music && musicSource.clip == clip && musicSource.isPlaying)
            {
                musicSource.volume = musicVolume;
                return;
            }
            _currentMusic = music;

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (fade && gameObject.activeInHierarchy && musicSource.isPlaying)
            {
                _fadeRoutine = StartCoroutine(FadeMusic(clip));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        private IEnumerator FadeMusic(AudioClip newClip)
        {
            float t = 0f;
            float startVol = musicSource.volume;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / 0.35f);
                yield return null;
            }
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.loop = true;
            musicSource.Play();

            t = 0f;
            while (t < 0.55f)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t / 0.55f);
                yield return null;
            }
            musicSource.volume = musicVolume;
            _fadeRoutine = null;
        }

        public void StopMusic() => musicSource.Stop();

        // ── SFX ───────────────────────────────────────────────────────────
        public void PlaySFX(SFX sfx, float volumeScale = 1f)
        {
            AudioClip clip = sfx switch {
                SFX.Jump        => jumpSFX,
                SFX.PowerJump   => powerJumpSFX ?? jumpSFX,
                SFX.Land        => landSFX,
                SFX.Switch      => switchSFX,
                SFX.DoorOpen    => doorOpenSFX,
                SFX.Checkpoint  => checkpointSFX,
                SFX.Smell       => smellSFX,
                SFX.Damage      => damageSFX,
                SFX.Victory     => victorySFX,
                SFX.Step        => stepSFX,
                SFX.WallBounce  => wallBounceSFX ?? landSFX,
                SFX.HeavyFall   => heavyFallSFX ?? damageSFX,
                SFX.FallWhistle => fallWhistleSFX,
                SFX.ChargeTick  => chargeTickSFX,
                SFX.Bark        => barkSFX ?? jumpSFX,
                _               => null
            };
            if (clip != null && sfxSource != null)
            {
                sfxSource.pitch = 1f;
                sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
            }
        }

        public void PlayChargeTick(float chargePercent)
        {
            if (sfxSource == null || chargeTickSFX == null || sfxVolume <= 0.001f) return;
            // Generamos un pulso rápido cuya altura sube con el porcentaje de carga
            int sr = 44100;
            float dur = 0.045f;
            int n = Mathf.RoundToInt(sr * dur);
            var data = new float[n];
            float f0 = Mathf.Lerp(280f, 760f, Mathf.Clamp01(chargePercent));
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Sin(Mathf.PI * (t / dur));
                float s = Mathf.Sin(2f * Mathf.PI * f0 * t) * 0.65f +
                          Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f0 * t)) * 0.25f;
                data[i] = s * env * 0.22f;
            }
            chargeTickSFX.SetData(data, 0);
            sfxSource.PlayOneShot(chargeTickSFX, sfxVolume * 0.75f);
        }

        // ── Generador de sonidos retro 16-bit procedurales ────────────────
        public static AudioClip GenerateTone(float frequency, float duration,
            float volume = 0.3f, bool fadeOut = true)
        {
            int sampleRate = 44100;
            int samples    = Mathf.RoundToInt(sampleRate * duration);
            var clip       = AudioClip.Create("ProceduralTone", samples, 1, sampleRate, false);
            var data       = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t    = (float)i / sampleRate;
                float fade = fadeOut ? Mathf.Clamp01(1f - t / duration) : 1f;
                data[i]    = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * fade;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateRetroSweep(string name, float fStart, float fEnd, float duration, float volume, float squareMix = 0.35f, float noiseMix = 0f)
        {
            int sr = 44100;
            int samples = Mathf.RoundToInt(sr * duration);
            var clip = AudioClip.Create(name, samples, 1, sr, false);
            var data = new float[samples];
            float phase = 0f;
            uint rng = 1234567u;

            for (int i = 0; i < samples; i++)
            {
                float u = (float)i / samples;
                float freq = Mathf.Lerp(fStart, fEnd, Mathf.Pow(u, 0.75f));
                phase += 2f * Mathf.PI * freq / sr;

                float sine = Mathf.Sin(phase);
                float sq = Mathf.Sign(sine);
                rng = rng * 1664525u + 1013904223u;
                float noise = ((rng & 0xFFFF) / 32768f) - 1f;

                float attack = Mathf.Clamp01(u / 0.06f);
                float release = Mathf.Pow(1f - u, 1.35f);
                float env = attack * release;

                float wave = sine * (1f - squareMix - noiseMix) + sq * squareMix + noise * noiseMix;
                data[i] = Mathf.Clamp(wave * env * volume, -0.95f, 0.95f);
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateHeavyFallSFX()
        {
            // Impacto grave (Golpe + Splat de caída fuerte estilo Jump King)
            int sr = 44100;
            float duration = 0.48f;
            int samples = Mathf.RoundToInt(sr * duration);
            var clip = AudioClip.Create("HeavyFallSplat", samples, 1, sr, false);
            var data = new float[samples];
            float phase = 0f;
            uint rng = 8912345u;
            float lpNoise = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / samples;

                // Fase 1 (0..0.16s): Golpe seco grave + crujido de tierra/madera
                // Fase 2 (0.12..0.48s): Vibración de aturdimiento descendente
                float subFreq = t < 0.16f ? Mathf.Lerp(155f, 42f, t / 0.16f) : Mathf.Lerp(190f, 85f, (t - 0.16f) / 0.32f) + Mathf.Sin(t * 55f) * 18f;
                phase += 2f * Mathf.PI * subFreq / sr;

                rng = rng * 1664525u + 1013904223u;
                float rawNoise = ((rng & 0xFFFF) / 32768f) - 1f;
                lpNoise = lpNoise * 0.78f + rawNoise * 0.22f;

                float thudEnv = Mathf.Exp(-t * 14f);
                float splatEnv = Mathf.Exp(-t * 5.5f);
                float body = Mathf.Sin(phase) * 0.65f + Mathf.Sign(Mathf.Sin(phase)) * 0.20f;
                float crunch = lpNoise * thudEnv * 0.85f;

                data[i] = Mathf.Clamp((body * splatEnv + crunch) * 0.78f, -0.95f, 0.95f);
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateWallBounceSFX()
        {
            // Rebote contra tronco/muro en el aire: golpe seco de madera + rebote elástico corto
            int sr = 44100;
            float duration = 0.16f;
            int samples = Mathf.RoundToInt(sr * duration);
            var clip = AudioClip.Create("WallBounce", samples, 1, sr, false);
            var data = new float[samples];
            float phase = 0f;
            uint rng = 456789u;
            float lp = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / samples;
                float freq = t < 0.05f ? Mathf.Lerp(240f, 95f, t / 0.05f) : Mathf.Lerp(130f, 210f, (t - 0.05f) / 0.11f);
                phase += 2f * Mathf.PI * freq / sr;

                rng = rng * 1664525u + 1013904223u;
                float n = ((rng & 0xFFFF) / 32768f) - 1f;
                lp = lp * 0.70f + n * 0.30f;

                float env = Mathf.Pow(1f - u, 1.5f);
                float hit = lp * Mathf.Exp(-t * 32f) * 0.75f;
                float tone = (Mathf.Sin(phase) * 0.7f + Mathf.Sign(Mathf.Sin(phase)) * 0.3f) * env;
                data[i] = Mathf.Clamp((tone + hit) * 0.65f, -0.95f, 0.95f);
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateSpikeDamageSFX()
        {
            // Golpe contra picos espinosos + aullido corto retro
            int sr = 44100;
            float duration = 0.36f;
            int samples = Mathf.RoundToInt(sr * duration);
            var clip = AudioClip.Create("SpikeDamage", samples, 1, sr, false);
            var data = new float[samples];
            float phase = 0f;
            uint rng = 9876543u;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float u = (float)i / samples;
                // Primero un pico agudo (yelp 680Hz -> 210Hz) con vibrato rápido
                float freq = Mathf.Lerp(680f, 175f, Mathf.Pow(u, 0.65f)) + Mathf.Sin(t * 75f) * 28f;
                phase += 2f * Mathf.PI * freq / sr;

                rng = rng * 1664525u + 1013904223u;
                float noise = ((rng & 0xFFFF) / 32768f) - 1f;
                float impactNoise = noise * Mathf.Exp(-t * 24f) * 0.6f;

                float env = Mathf.Pow(1f - u, 1.2f);
                float voice = (Mathf.Sin(phase) * 0.55f + Mathf.Sign(Mathf.Sin(phase)) * 0.35f) * env;
                data[i] = Mathf.Clamp((voice + impactNoise) * 0.72f, -0.95f, 0.95f);
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggioSFX(string name, float[] notes, float noteLen, float volume)
        {
            int sr = 44100;
            float duration = notes.Length * noteLen;
            int samples = Mathf.RoundToInt(sr * duration);
            var clip = AudioClip.Create(name, samples, 1, sr, false);
            var data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                int idx = Mathf.Clamp((int)(t / noteLen), 0, notes.Length - 1);
                float localT = (t - idx * noteLen) / noteLen;
                float freq = notes[idx];
                phase += 2f * Mathf.PI * freq / sr;
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(localT));
                float wave = Mathf.Sin(phase) * 0.70f + Mathf.Sign(Mathf.Sin(phase)) * 0.25f;
                data[i] = wave * env * volume;
            }
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Genera todos los efectos de sonido retro (Brinco, Brinco Cargado, Golpe de Muro,
        /// Aterrizaje, Caída Fuerte/Splat, Silbido de Caída, Daño de Picos, Ladrido y Pasos).
        /// </summary>
        public void GenerateProceduralSounds()
        {
            if (jumpSFX        == null) jumpSFX        = CreateRetroSweep("JumpSFX",        235f, 590f, 0.16f, 0.48f, squareMix: 0.32f, noiseMix: 0.04f);
            if (powerJumpSFX   == null) powerJumpSFX   = CreateRetroSweep("PowerJumpSFX",   260f, 890f, 0.24f, 0.58f, squareMix: 0.38f, noiseMix: 0.06f);
            if (landSFX        == null) landSFX        = CreateRetroSweep("LandSFX",        145f,  58f, 0.11f, 0.52f, squareMix: 0.18f, noiseMix: 0.35f);
            if (heavyFallSFX   == null) heavyFallSFX   = CreateHeavyFallSFX();
            if (fallWhistleSFX == null) fallWhistleSFX = CreateRetroSweep("FallWhistleSFX", 760f, 210f, 0.38f, 0.32f, squareMix: 0.12f, noiseMix: 0.08f);
            if (wallBounceSFX  == null) wallBounceSFX  = CreateWallBounceSFX();
            if (chargeTickSFX  == null) chargeTickSFX  = AudioClip.Create("ChargeTickSFX", Mathf.RoundToInt(44100 * 0.045f), 1, 44100, false);
            if (damageSFX      == null) damageSFX      = CreateSpikeDamageSFX();
            if (barkSFX        == null) barkSFX        = CreateRetroSweep("BarkSFX",        420f, 690f, 0.14f, 0.46f, squareMix: 0.28f, noiseMix: 0.12f);
            if (switchSFX      == null) switchSFX      = CreateArpeggioSFX("SwitchSFX",     new float[] { 523.25f, 659.25f }, 0.065f, 0.38f);
            if (doorOpenSFX    == null) doorOpenSFX    = CreateArpeggioSFX("DoorOpenSFX",   new float[] { 392f, 493.88f, 587.33f, 783.99f }, 0.09f, 0.42f);
            if (checkpointSFX  == null) checkpointSFX  = CreateArpeggioSFX("TreatSFX",      new float[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.075f, 0.45f);
            if (smellSFX       == null) smellSFX       = CreateRetroSweep("SmellSFX",       520f, 780f, 0.22f, 0.35f, squareMix: 0.15f, noiseMix: 0.15f);
            if (victorySFX     == null) victorySFX     = CreateArpeggioSFX("VictorySFX",    new float[] { 523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 1046.5f }, 0.13f, 0.50f);
            if (stepSFX        == null) stepSFX        = CreateRetroSweep("StepSFX",        115f,  62f, 0.055f, 0.26f, squareMix: 0.10f, noiseMix: 0.45f);
        }

        private void Start() => GenerateProceduralSounds();

        // ── Volumen ────────────────────────────────────────────────────────
        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        public void SetSFXVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }
    }

    /// <summary>
    /// Añade sonidos al PlayerController2D de forma automática.
    /// Detecta salto, aterrizaje y pasos.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerAudio : MonoBehaviour
    {
        private PlayerController2D _pc;
        private bool _wasGrounded = true;
        private float _stepTimer  = 0f;
        private const float STEP_INTERVAL = 0.32f;

        private void Awake() => _pc = GetComponent<PlayerController2D>();

        private void Update()
        {
            if (KimayaAudio.Instance == null) return;

            // Salto
            bool jumping = !_pc.IsGrounded && _pc.VelocityY > 1f;
            bool justJumped = jumping && _wasGrounded;
            if (justJumped) KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Jump);

            // Aterrizaje
            bool justLanded = _pc.IsGrounded && !_wasGrounded;
            if (justLanded) KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Land);

            // Pasos
            if (_pc.IsGrounded && Mathf.Abs(_pc.VelocityX) > 0.5f)
            {
                _stepTimer += Time.deltaTime;
                if (_stepTimer >= STEP_INTERVAL)
                {
                    KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Step);
                    _stepTimer = 0f;
                }
            }
            else _stepTimer = 0f;

            _wasGrounded = _pc.IsGrounded;
        }
    }
}
