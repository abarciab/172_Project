using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AudioManager.Sound;

public class AudioManager : MonoBehaviour {
    [System.Serializable]
    public class Sound {
        [System.Serializable]
        public class Clip
        {
            [HideInInspector] public string name;
            public AudioClip clip;
            [Range(0, 2), Tooltip("if set to 0, this clip will use parent volume instead of local volume")]
            public float localVolume = 1;
            [Range(0, 2), Tooltip("if set to 0, this clip will use parent pitch instead of local pitch")]
            public float localPitch = 1;
            public bool looping;

            public Clip(Clip toCopy, float volume = -1, float pitch = -1) {
                clip = toCopy.clip;
                localVolume = volume == -1 ? toCopy.localVolume : volume;
                localPitch = pitch == -1 ? toCopy.localPitch: pitch;
                looping = toCopy.looping;
            }
        }

        [HideInInspector] public string name;
        [HideInInspector] public int ID;
        [SerializeField] string displayName;
        [SerializeField] List<Clip> clips = new List<Clip>();
        [Range(0, 2)]
        public float parentVolume;
        [Range(0, 2)]
        public float parentPitch;

        public void OnValidate(int i)
        {
            ID = i;
            for (i = 0; i < clips.Count; i++) {
                if (clips[i].clip == null) continue;
                clips[i].name = i + ": " + clips[i].clip.name;
            }

            if (!string.IsNullOrEmpty(displayName)) {
                name = ID + ": " + displayName;
                return;
            }
            if (clips.Count <= 0 || clips[0].clip == null) {
                name = ID + ": NULL";
                return;
            }
            name = ID + ": " + clips[0].clip.name;
        }

        public Clip getClip(int _ID)
        {
            if (clips.Count == 0 || _ID != ID) return null;
            Clip clip = clips[Random.Range(0, clips.Count)];
            return new Clip(clip, clip.localVolume == 0 ? parentVolume : -1, clip.localPitch == 0 ? parentPitch : -1);
        }
    }

    public static AudioManager instance;
    [SerializeField] List<Sound> sounds = new List<Sound>();
    [Range(0, 2)]
    public float masterVolume = 1;

    private void OnValidate()
    {
        for (int i = 0; i < sounds.Count; i++) {
            sounds[i].OnValidate(i);   
        }
    }

    private void Awake()
    {
        instance = this;
    }

    //--1/3
    public void PlaySound(int ID)
    {
        AudioSource source = GetFreeAudioSource(gameObject);
        PlaySound(ID, source);
    }
    //--2/3
    public void PlaySound(int ID, GameObject obj)
    {
        AudioSource source = null;
        if (!obj.TryGetComponent(out source)) source = obj.AddComponent<AudioSource>();
        PlaySound(ID, source);
    }
    //--3/3
    public void PlaySound(int ID, AudioSource source)
    {
        Clip clip = getClip(ID);
        ConfigureSource(clip, source);
        source.Play();
    }

    Clip getClip(int ID)
    {
        Clip clip = null; 
        for (int i = 0; i < sounds.Count; i++) {
            clip = sounds[i].getClip(ID);
            if (clip != null) break;
        }
        return clip;
    }

    AudioSource GetFreeAudioSource(GameObject obj)
    {
        List<AudioSource> sources = obj.GetComponents<AudioSource>().ToList();

        for (int i = 0; i < sources.Count; i++) {
            if (!sources[i].isPlaying) return sources[i];
        }
        return obj.AddComponent<AudioSource>();
    }

    void ConfigureSource(Clip clip, AudioSource source)
    {
        source.clip = clip.clip;
        source.volume = clip.localVolume * masterVolume;
        source.pitch = clip.localPitch;
        source.loop = clip.looping;
    }

}
