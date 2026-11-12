// Assets/Scripts/Audio/AudioManager.cs
// Namespace: KANYE.Audio
// Autonomous procedural chiptune audio synthesizer and audio manager.

using System.Collections.Generic;
using UnityEngine;

namespace KANYE.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private const string VolumePrefKey = "KANYE.MasterVolume";
        public static AudioManager Instance { get; private set; }

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private readonly List<AudioSource> sfxPool = new List<AudioSource>(8);

        // Procedural Audio Clips
        private AudioClip shootClip;
        private AudioClip exShootClip;
        private AudioClip superBeamClip;
        private AudioClip explosionSmallClip;
        private AudioClip explosionLargeClip;
        private AudioClip hitClip;
        private AudioClip parryClip;
        private AudioClip bossWarningClip;
        private AudioClip victoryClip;
        private AudioClip defeatClip;
        private AudioClip clickClip;
        private AudioClip radioBeepClip;
        private AudioClip radioOpenClip;
        private AudioClip powerupClip;
        private AudioClip arcadeMusicLoop;

        public static float MasterVolume { get; private set; } = 0.75f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                var go = new GameObject("[AudioManager]");
                Instance = go.AddComponent<AudioManager>();
                if (Application.isPlaying)
                    DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);

            MasterVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0.75f);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = MasterVolume * 0.45f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = MasterVolume;

            for (var i = 0; i < 6; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.volume = MasterVolume;
                sfxPool.Add(src);
            }

            SynthesizeClips();
            StartMusic();
        }

        public static void SetVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(VolumePrefKey, MasterVolume);
            PlayerPrefs.Save();

            if (Instance != null)
            {
                if (Instance.musicSource != null)
                    Instance.musicSource.volume = MasterVolume * 0.45f;
                if (Instance.sfxSource != null)
                    Instance.sfxSource.volume = MasterVolume;
                foreach (var src in Instance.sfxPool)
                {
                    if (src != null)
                        src.volume = MasterVolume;
                }
            }
        }

        private void StartMusic()
        {
            if (arcadeMusicLoop != null && musicSource != null)
            {
                musicSource.clip = arcadeMusicLoop;
                musicSource.Play();
            }
        }

        private void PlayOneShot(AudioClip clip, float volumeMultiplier = 1f, float pitchVariation = 0f)
        {
            if (clip == null || MasterVolume <= 0.001f)
                return;

            AudioSource target = sfxSource;
            for (var i = 0; i < sfxPool.Count; i++)
            {
                if (!sfxPool[i].isPlaying)
                {
                    target = sfxPool[i];
                    break;
                }
            }

            target.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            target.PlayOneShot(clip, MasterVolume * volumeMultiplier);
        }

        public static void PlayShoot()
        {
            Instance?.PlayOneShot(Instance.shootClip, 0.75f, 0.08f);
        }

        public static void PlayEXShoot()
        {
            Instance?.PlayOneShot(Instance.exShootClip, 0.95f, 0.04f);
        }

        public static void PlaySuperBeam()
        {
            Instance?.PlayOneShot(Instance.superBeamClip, 1.0f);
        }

        public static void PlayExplosion(bool large = false)
        {
            if (Instance == null) return;
            var clip = large ? Instance.explosionLargeClip : Instance.explosionSmallClip;
            Instance.PlayOneShot(clip, large ? 1.0f : 0.8f, 0.1f);
        }

        public static void PlayHit()
        {
            Instance?.PlayOneShot(Instance.hitClip, 0.6f, 0.12f);
        }

        public static void PlayParry()
        {
            Instance?.PlayOneShot(Instance.parryClip, 1.0f);
        }

        public static void PlayBossWarning()
        {
            Instance?.PlayOneShot(Instance.bossWarningClip, 0.9f);
        }

        public static void PlayVictory()
        {
            Instance?.PlayOneShot(Instance.victoryClip, 1.0f);
        }

        public static void PlayDefeat()
        {
            Instance?.PlayOneShot(Instance.defeatClip, 0.9f);
        }

        public static void PlayClick()
        {
            Instance?.PlayOneShot(Instance.clickClip, 0.7f);
        }

        public static void PlayRadioBeep()
        {
            Instance?.PlayOneShot(Instance.radioBeepClip, 0.45f, 0.18f);
        }

        public static void PlayRadioOpen()
        {
            Instance?.PlayOneShot(Instance.radioOpenClip, 0.85f);
        }

        public static void PlayPowerup()
        {
            Instance?.PlayOneShot(Instance.powerupClip, 0.9f, 0.04f);
        }

        // ==========================================
        // PROCEDURAL AUDIO SYNTHESIZERS (Zero assets)
        // ==========================================

        private void SynthesizeClips()
        {
            const int sampleRate = 44100;

            // 1. Shoot: Downward frequency chirp (850Hz -> 200Hz, 0.09s)
            shootClip = SynthesizeClip("Sfx_Shoot", sampleRate, 0.09f, (t, dur) =>
            {
                var env = 1f - (t / dur);
                var freq = Mathf.Lerp(850f, 200f, (t / dur) * (t / dur));
                var wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                return wave * env * 0.7f;
            });

            // 2. EX Shoot: Heavy rocket punch (340Hz -> 90Hz with square wave, 0.22s)
            exShootClip = SynthesizeClip("Sfx_EXShoot", sampleRate, 0.22f, (t, dur) =>
            {
                var env = Mathf.Pow(1f - (t / dur), 1.5f);
                var freq = Mathf.Lerp(340f, 90f, t / dur);
                var sine = Mathf.Sin(2f * Mathf.PI * freq * t);
                var square = Mathf.Sign(sine);
                var noise = (Random.value * 2f - 1f) * 0.25f;
                return (square * 0.6f + noise) * env * 0.85f;
            });

            // 3. Super Beam: Massive energy laser beam (1.1s)
            superBeamClip = SynthesizeClip("Sfx_SuperBeam", sampleRate, 1.1f, (t, dur) =>
            {
                var env = t < 0.1f ? t / 0.1f : Mathf.Pow(1f - (t / dur), 0.7f);
                var f1 = 110f + Mathf.Sin(t * 40f) * 15f;
                var f2 = 220f + Mathf.Sin(t * 80f) * 30f;
                var wave = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.3f;
                var noise = (Random.value * 2f - 1f) * 0.35f;
                return (wave + noise) * env * 0.9f;
            });

            // 4. Small Explosion: Crunchy white noise with lowpass falloff (0.28s)
            explosionSmallClip = SynthesizeClip("Sfx_ExplosionSmall", sampleRate, 0.28f, (t, dur) =>
            {
                var env = Mathf.Pow(1f - (t / dur), 2.2f);
                var noise = Random.value * 2f - 1f;
                var rumble = Mathf.Sin(2f * Mathf.PI * 85f * t);
                return (noise * 0.7f + rumble * 0.4f) * env * 0.85f;
            });

            // 5. Large Explosion: Deep Broforce bass rumble & shattered debris crunch (0.7s)
            explosionLargeClip = SynthesizeClip("Sfx_ExplosionLarge", sampleRate, 0.7f, (t, dur) =>
            {
                var env = Mathf.Pow(1f - (t / dur), 1.6f);
                var subBass = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 30f, t / dur) * t);
                var noise = Random.value * 2f - 1f;
                return (subBass * 0.65f + noise * 0.55f) * env * 0.95f;
            });

            // 6. Hit: Crisp metallic click (0.04s)
            hitClip = SynthesizeClip("Sfx_Hit", sampleRate, 0.04f, (t, dur) =>
            {
                var env = 1f - (t / dur);
                var wave = Mathf.Sin(2f * Mathf.PI * 1350f * t) * 0.6f + (Random.value * 2f - 1f) * 0.4f;
                return wave * env * 0.65f;
            });

            // 7. Cuphead Parry Bell: Heavenly crystalline chime (C6 = 1046.5Hz + C7 harmonic, 0.55s)
            parryClip = SynthesizeClip("Sfx_ParryBell", sampleRate, 0.55f, (t, dur) =>
            {
                var env = Mathf.Pow(1f - (t / dur), 1.8f);
                var f1 = 1046.5f; // C6
                var f2 = 2093.0f; // C7
                var f3 = 3135.9f; // G7
                var wave = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.55f 
                         + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.35f 
                         + Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.15f;
                return wave * env * 0.95f;
            });

            // 8. Boss Warning: Klaxon siren (0.42s)
            bossWarningClip = SynthesizeClip("Sfx_BossWarning", sampleRate, 0.42f, (t, dur) =>
            {
                var freq = (t < 0.21f) ? 520f : 780f;
                var env = Mathf.Sin((t / dur) * Mathf.PI);
                var wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                return wave * env * 0.7f;
            });

            // 9. Victory Fanfare: Heroic ascending 8-bit fanfare (1.1s)
            victoryClip = SynthesizeClip("Sfx_Victory", sampleRate, 1.1f, (t, dur) =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
                var noteIdx = Mathf.Clamp(Mathf.FloorToInt(t / (dur / 4f)), 0, 3);
                var noteTime = t % (dur / 4f);
                var env = 1f - (noteTime / (dur / 4f)) * 0.5f;
                var freq = notes[noteIdx];
                var wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                return wave * env * 0.6f;
            });

            // 10. Defeat Jingle: Sad descending minor tones (0.9s)
            defeatClip = SynthesizeClip("Sfx_Defeat", sampleRate, 0.9f, (t, dur) =>
            {
                float[] notes = { 440f, 392f, 349.23f, 261.63f }; // A4, G4, F4, C4
                var noteIdx = Mathf.Clamp(Mathf.FloorToInt(t / (dur / 4f)), 0, 3);
                var noteTime = t % (dur / 4f);
                var env = 1f - (noteTime / (dur / 4f));
                var freq = notes[noteIdx];
                var wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                return wave * env * 0.7f;
            });

            // 11. UI Click: Clean 620Hz blip (0.035s)
            clickClip = SynthesizeClip("Sfx_Click", sampleRate, 0.035f, (t, dur) =>
            {
                var env = 1f - (t / dur);
                return Mathf.Sin(2f * Mathf.PI * 620f * t) * env * 0.5f;
            });

            // 12. Arcade Music Loop: 124 BPM driving cyberpunk bassline groove (~7.74s)
            var bpm = 124f;
            var beatDuration = 60f / bpm;
            var totalDuration = beatDuration * 16f; // 4 bars = 16 beats

            arcadeMusicLoop = SynthesizeClip("Music_CyberArcade", sampleRate, totalDuration, (t, dur) =>
            {
                var beat = (t / beatDuration);
                var beatFract = beat - Mathf.Floor(beat);
                var currentBeat = Mathf.FloorToInt(beat) % 16;

                // Bassline notes across the 4 bars (Am - F - C - G progression)
                float[] bassProgression = { 110f, 110f, 110f, 110f, 87.3f, 87.3f, 87.3f, 87.3f, 130.8f, 130.8f, 130.8f, 130.8f, 98f, 98f, 98f, 123.47f };
                var bassFreq = bassProgression[currentBeat];

                // 16th-note sub-pulse
                var sixteenth = (beatFract * 4f) % 1f;
                var bassEnv = Mathf.Pow(1f - sixteenth, 2.2f);
                var bassWave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * bassFreq * t)) * 0.35f;

                // Driving Hi-hat on off-beats
                var hatEnv = (beatFract > 0.45f && beatFract < 0.65f) ? (1f - (beatFract - 0.45f) / 0.2f) : 0f;
                var hatNoise = (Random.value * 2f - 1f) * hatEnv * 0.22f;

                // Arpeggio synth lead
                float[] arpScale = { 440f, 523.25f, 659.25f, 783.99f }; // A, C, E, G
                var arpIdx = Mathf.FloorToInt((t * 8f)) % arpScale.Length;
                var arpEnv = Mathf.Pow(1f - ((t * 8f) % 1f), 1.8f);
                var arpWave = Mathf.Sin(2f * Mathf.PI * arpScale[arpIdx] * t) * arpEnv * 0.16f;

                return Mathf.Clamp((bassWave * bassEnv + hatNoise + arpWave) * 0.7f, -1f, 1f);
            });

            // 13. Radio Speech Beep (0.032s chirpy radio syllable)
            radioBeepClip = SynthesizeClip("Sfx_RadioBeep", sampleRate, 0.032f, (t, dur) =>
            {
                var env = 1f - (t / dur);
                var wave = Mathf.Sin(2f * Mathf.PI * 680f * t);
                return wave * env * 0.45f;
            });

            // 14. Radio Open Chime (0.15s retro hologram connection sound)
            radioOpenClip = SynthesizeClip("Sfx_RadioOpen", sampleRate, 0.15f, (t, dur) =>
            {
                var freq = t < 0.075f ? 480f : 740f;
                var wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                var env = 1f - ((t % 0.075f) / 0.075f);
                return wave * env * 0.55f;
            });

            // 15. Powerup / Nanite Healing Chime (0.28s rising arpeggio C-E-G-C)
            powerupClip = SynthesizeClip("Sfx_Powerup", sampleRate, 0.28f, (t, dur) =>
            {
                var step = Mathf.FloorToInt(t / (dur / 4f));
                float freq = 523.25f; // C5
                if (step == 1) freq = 659.25f; // E5
                else if (step == 2) freq = 783.99f; // G5
                else if (step >= 3) freq = 1046.50f; // C6

                var env = 1f - ((t % (dur / 4f)) / (dur / 4f));
                var wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.3f;
                return wave * env * 0.6f;
            });
        }

        private static AudioClip SynthesizeClip(string name, int sampleRate, float duration, System.Func<float, float, float> generator)
        {
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / sampleRate;
                samples[i] = Mathf.Clamp(generator(t, duration), -1f, 1f);
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
