using System.Collections.Generic;
using UnityEngine;
namespace ArenaTatica
{
    // O anúncio global sobrevive ao descarte de animações atrasadas da apresentação.
    [RequireComponent(typeof(AudioSource))]
    public sealed class ArenaGlobalVoiceQueue : MonoBehaviour
    {
        readonly Queue<AudioClip> pending=new Queue<AudioClip>(); AudioSource source;
        public int Pending=>pending.Count;
        void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=1;}
        public void Enqueue(AudioClip clip){if(clip&&ArenaSom.Recorded) pending.Enqueue(clip);}
        public void Clear(){pending.Clear();if(source)source.Stop();}
        void Update(){if(!ArenaSom.Recorded){Clear();return;}if(source&&!source.isPlaying&&pending.Count>0){source.clip=pending.Dequeue();source.Play();}}
    }
}
