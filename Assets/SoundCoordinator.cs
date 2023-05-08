using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCoordinator : MonoBehaviour
{
    public void AddNewSound(Sound sound, bool restart)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = AudioManager.instance.GetMixer(sound.type);
        source.playOnAwake = false;
        sound.audioSource = source;
        sound.Play(transform.parent, restart);
    }

}
