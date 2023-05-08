using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    // users assign sound clips, set the volume and pitch for each clip (optional)
    // users can assign multiple audio files for the same call, each with different volume and pitch (or syncronize them)
    // users can play sounds by calling 'play' on a PlayableSound scriptable object.
    //     settings for that sound can be found on that scriptable object

    public static AudioManager instance;

    [SerializeField] GameObject coordinatorPrefab;
    List<SoundCoordinator> soundCoordinators = new List<SoundCoordinator>();
    [SerializeField] AudioMixerGroup sfxMixer, musicMixer, AmbientMixer;

    public AudioMixerGroup GetMixer(SoundType type)
    {
        switch (type) {
            case SoundType.sfx:
                return sfxMixer;
            case SoundType.music:
                return musicMixer;
            case SoundType.ambient:
                return AmbientMixer;
        }
        return null;
    }

    private void Awake()
    {
        instance = this;
    }

    public void PlaySound(Sound sound, Transform caller = null, bool restart = true)
    {
        if (caller == null) caller = transform;
        var coordinator = GetExistingCoordinator(caller);
        coordinator.AddNewSound(sound, restart);
    }
    
    SoundCoordinator GetExistingCoordinator(Transform caller)
    {
        foreach (var coord in soundCoordinators) {
            if (coord.transform.parent == caller) return coord;
        }
        return AddNewCoord(caller);
    }

    SoundCoordinator AddNewCoord(Transform caller)
    {
        var coordObj = Instantiate(coordinatorPrefab, caller);
        var coord = coordObj.GetComponent<SoundCoordinator>();
        soundCoordinators.Add(coord);
        return coord;
    }
}
