using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Maneja todo el audio del juego Kimaya.
    /// Singleton que persiste entre escenas.
    ///
    /// Uso:
    ///   KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Jump);
    ///   KimayaAudio.Instance.PlayMusic(KimayaAudio.Music.Forest);
    /// </summary>
    public class KimayaAudio : MonoBehaviour
    {
        public static KimayaAudio Instance { get; private set; }

        public enum SFX
        {
            Jump, PowerJump, Land, Switch, DoorOpen, Checkpoint,
            Smell, Damage, Victory, Step
        }

        public enum Music { Menu, Forest, Ending }

        [Header("Music Sources")]
        public AudioSource musicSource;
        public float musicVolume = 0.4f;

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

        private Music _currentMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop   = true;
                musicSource.volume = musicVolume;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.volume = sfxVolume;
            }
        }

        // ── Música ────────────────────────────────────────────────────────
        public void PlayMusic(Music music, bool fade = true)
        {
            if (_currentMusic == music && musicSource.isPlaying) return;
            _currentMusic = music;

            AudioClip clip = music switch {
                Music.Menu    => menuMusic,
                Music.Forest  => forestMusic,
                Music.Ending  => endingMusic,
                _             => null
            };
            if (clip == null) return;

            if (fade) StartCoroutine(FadeMusic(clip));
            else { musicSource.clip = clip; musicSource.Play(); }
        }

        private IEnumerator FadeMusic(AudioClip newClip)
        {
            // Fade out
            float t = 0f;
            float startVol = musicSource.volume;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / 0.5f);
                yield return null;
            }
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.Play();
            // Fade in
            t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t / 0.8f);
                yield return null;
            }
            musicSource.volume = musicVolume;
        }

        public void StopMusic() => musicSource.Stop();

        // ── SFX ───────────────────────────────────────────────────────────
        public void PlaySFX(SFX sfx)
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
                _               => null
            };
            if (clip != null && sfxSource != null)
                sfxSource.PlayOneShot(clip, sfxVolume);
        }

        // ── Generador de sonidos procedurales (sin AudioClips) ────────────
        /// <summary>
        /// Genera un tono simple con AudioClip procedural.
        /// Úsalo si no tienes archivos de audio.
        /// </summary>
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

        /// <summary>
        /// Genera los sonidos básicos de forma procedural si no hay clips asignados.
        /// Llama esto en Start si los AudioClips están vacíos.
        /// </summary>
        public void GenerateProceduralSounds()
        {
            if (jumpSFX       == null) jumpSFX       = GenerateTone(440f,  0.15f, 0.3f);
            if (powerJumpSFX  == null) powerJumpSFX  = GenerateTone(660f,  0.2f,  0.4f);
            if (landSFX       == null) landSFX       = GenerateTone(120f,  0.08f, 0.5f);
            if (switchSFX     == null) switchSFX     = GenerateTone(880f,  0.1f,  0.3f);
            if (doorOpenSFX   == null) doorOpenSFX   = GenerateTone(330f,  0.5f,  0.3f, false);
            if (checkpointSFX == null) checkpointSFX = GenerateTone(550f,  0.4f,  0.4f);
            if (smellSFX      == null) smellSFX      = GenerateTone(700f,  0.2f,  0.25f);
            if (damageSFX     == null) damageSFX     = GenerateTone(200f,  0.3f,  0.5f);
            if (victorySFX    == null) victorySFX    = GenerateTone(660f,  1.0f,  0.4f);
            if (stepSFX       == null) stepSFX       = GenerateTone(80f,   0.05f, 0.2f);
        }

        private void Start() => GenerateProceduralSounds();

        // ── Volumen ────────────────────────────────────────────────────────
        public void SetMusicVolume(float v) { musicVolume = v; musicSource.volume = v; }
        public void SetSFXVolume(float v)   { sfxVolume   = v; sfxSource.volume   = v; }
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
