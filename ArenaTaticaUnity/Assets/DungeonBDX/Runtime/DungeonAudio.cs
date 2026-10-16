using DungeonBDX.Skills;
using UnityEngine;

namespace DungeonBDX
{
    public sealed class DungeonAudio : MonoBehaviour
    {
        AudioSource music, effects;
        public bool Muted { get; private set; }
        public void Begin(string scenario)
        {
            music = gameObject.AddComponent<AudioSource>(); music.loop = true; music.volume = .18f;
            music.clip = Resources.Load<AudioClip>("DungeonBDX/Audio/Music/"+scenario);
            effects = gameObject.AddComponent<AudioSource>(); effects.volume = .35f;
            if (Application.isPlaying && music.clip) music.Play();
        }
        public void Play(SkillUseResult result)
        {
            if (!Application.isPlaying || !result.Success || Muted) return;
            var clip = Resources.Load<AudioClip>(SoundResource(result.SkillId));
            if (clip) effects.PlayOneShot(clip);
        }
        public void ToggleMute()
        { Muted = !Muted; music.mute = effects.mute = Muted; }
        public static string SoundResource(string skillId) => "DungeonBDX/Audio/Sfx/"+skillId.Substring("dbdx.".Length).Replace('.','-');
    }
}
