using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundCoordinator : MonoBehaviour
{
    List<KeyValuePair<Sound, AudioSource>> sources = new List<KeyValuePair<Sound, AudioSource>>();
    List<AudioSource> pausedSources = new List<AudioSource>();

    public void AddNewSound(Sound sound, bool restart, bool _3D = true)
    {
        var newSource = gameObject.AddComponent<AudioSource>();
        newSource.outputAudioMixerGroup = AudioManager.i.GetMixer(sound.Type);
        newSource.playOnAwake = false;
        if (_3D) newSource.spatialBlend = 1;
        sound.AudioSource = newSource;
        sound.Play(transform.parent, restart);

        var newPair = new KeyValuePair<Sound, AudioSource>(sound, newSource);
        sources.Add(newPair);
    }

    public void Pause()
    {
        sources = sources.Where(x => x.Value != null && x.Key != null).ToList();
        foreach (var s in sources) {
            if (!s.Key.Unpauseable && s.Value.isPlaying) {
                s.Value.Pause();
                pausedSources.Add(s.Value);
            }
        }
    }


    public void PauseNonMusic()
    {
        foreach (var s in sources) {
            if (!s.Key.Unpauseable && s.Value.isPlaying && s.Key.Type != SoundType.music) {
                s.Value.Pause();
                pausedSources.Add(s.Value);
            }
        }
    }


    public void Resume()
    {
        foreach (var s in pausedSources) s.Play();
        pausedSources.Clear();
    }

}
