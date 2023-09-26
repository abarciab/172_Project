using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCoordinator : MonoBehaviour
{
    List<AudioSource> sources = new List<AudioSource>();
    List<AudioSource> pausedSources = new List<AudioSource>();

    public void AddNewSound(Sound sound, bool restart, bool _3D = true)
    {
        var source = gameObject.AddComponent<AudioSource>();
        sources.Add(source);
        source.outputAudioMixerGroup = AudioManager.instance.GetMixer(sound.type);
        source.playOnAwake = false;
        if (_3D) source.spatialBlend = 1;
        sound.audioSource = source;
        sound.Play(transform.parent, restart);
    }

    public void PauseNonMusic() {
        pausedSources.Clear();
        foreach (var s in sources) {
            if (s && s.isPlaying && s.outputAudioMixerGroup != AudioManager.instance.GetMixer(SoundType.music)) {
                s.Pause();
                pausedSources.Add(s);
            }
        }
    }

    public void Pause() {
        pausedSources.Clear();
        foreach (var s in sources) {
            if (s && s.isPlaying) {
                s.Pause();
                pausedSources.Add(s);
            }
        }
    }

    public void Unpause() {
        foreach (var s in pausedSources) s.Play();
    }

}
