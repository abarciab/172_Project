using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public enum SoundType { sfx, music, ambient}

[CreateAssetMenu(fileName = "New Sound", menuName = "Sound")]
public class Sound : ScriptableObject
{
    [System.Serializable]
    public class ClipData {

        [HideInInspector] public string Name;
        public AudioClip Clip;
        public bool Looping;

        public ClipData() { }

        public ClipData(ClipData toCopy, float volume = -1, float pitch = -1)
        {
            Clip = toCopy.Clip;
            Looping = toCopy.Looping;
        }
    }

    public SoundType Type;
    public bool Unpauseable;

    [SerializeField] private List<ClipData> _clips = new List<ClipData>();

    [Range(0, 1), DefaultValue(1.0f), SerializeField] private float _volume = 1;
    [Range(0, 2), DefaultValue(1.0f), SerializeField] private float _pitch = 1;
    [SerializeField] private bool _randomizePitch;
    [SerializeField, ConditionalField(nameof(_randomizePitch)), Range(0, 1)] private float _pitchRandomizeAmount;

    [HideInInspector] public AudioSource AudioSource;
    [HideInInspector] public bool Instantialized;

    private Vector3 _sourcePos;
    private bool _setPos;
    private float _actualVolume;

    public float percentVolume { get { return AudioSource ? AudioSource.volume / _actualVolume : 0; } }

    private void OnValidate()
    {
        foreach (var clip in _clips) if (clip.Clip) clip.Name = clip.Clip.name;
    }

    public void UpdateLocalPosition(Vector3 pos)
    {
        _sourcePos = pos;
        _setPos = true;
    }

    public void SetPercentVolume(float percent, float smoothness = 1)
    {
        if (!AudioSource) return;
        AudioSource.volume = Mathf.Lerp(AudioSource.volume, _actualVolume * percent, smoothness);
    }

    public void Stop()
    {
        if (!Instantialized || !AudioSource) return;
        AudioSource.Stop();
    }

    private void Awake()
    {
        if (Application.isPlaying) Instantialized = true;
    }

    public void Delete()
    {
        Destroy(AudioSource);
    }

    public void PlaySilent(Transform caller = null, bool restart = true)
    {
        if (!Instantialized) {
            Debug.LogError("PlaySilent() was called on an uninstatizlized Sound");
            return;
        }

        if (!AudioSource) FirstTimePlay(caller, restart);
        Play(true, true);
    }

    public void SetUp(Transform caller = null, bool restart = true)
    {
        if (!Instantialized) {
            Debug.LogError("SetUp() was called on an uninstatizlized Sound");
            return;
        }
        if (_clips.Count == 0) return;

        if (AudioSource == null) FirstTimePlay(caller, restart);
    }

    public void PlayLine(Transform speaker, int index)
    {
        if (!Instantialized) {
            Debug.LogError("PlaySilent() was called on an uninstatizlized Sound");
            return;
        }

        if (!AudioSource) SetUp(speaker);

        AudioSource.Stop();
        Play(true, index:index);
    }

    public float GetClipLength()
    {
        if (_clips.Count == 0 || _clips[0].Clip == null) return 0;
        return _clips[0].Clip.length;
    }

    public void Play(Transform caller = null, bool restart = true)
    {
        if (!Instantialized) {
            Debug.LogError("Play() was called on an uninstatizlized Sound");
            return;
        }
        if (_clips.Count == 0) return;
       
        if (AudioSource == null) FirstTimePlay(caller, restart);
        else Play(restart);
       
    }

    private void Play(bool restart, bool silent = false, int index = 0)
    {
        if (AudioSource.isPlaying && !restart) return;


        if (_setPos) AudioSource.transform.localPosition = _sourcePos;
        _setPos = false;

        if (_randomizePitch) _pitch += Random.Range(-_pitchRandomizeAmount, _pitchRandomizeAmount);

        var clip = GetClip();
        _actualVolume = _volume;
        AudioSource.volume = silent ? 0 : _actualVolume;
        AudioSource.pitch = _pitch;
        AudioSource.loop = clip.Looping;
        AudioSource.clip = clip.Clip;
        AudioSource.Play();
    }

    private ClipData GetClip()
    {
        return _clips[Random.Range(0, _clips.Count)];
    }

    private void FirstTimePlay(Transform caller, bool restart)
    {
        var Aman = AudioManager.i;
        if (!Aman) return;
        Aman.PlaySound(this, caller, restart);
    }

    private void ConfigureSource(ClipData clip, AudioSource source)
    {
        source.clip = clip.Clip;
        source.volume = _volume;
        source.pitch = _pitch;
        source.loop = clip.Looping;
    }
}
