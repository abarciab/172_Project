using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioSource music1Source, music2Source, ambientSource;
    [SerializeField] float agroDist = 10;

    float music1Vol, music2Vol;

    bool music2;

    private void Start()
    {
        AudioManager.instance.PlaySound(6, music1Source);
        music1Vol = music1Source.volume;
        AudioManager.instance.PlaySound(7, ambientSource);
        AudioManager.instance.PlaySound(10, music2Source);
        music2Source.volume = 0;
    }

    private void Update()
    {
        if (Player.i.InCombat() && !music2) {
            Player.i.EnterCombat();
            music2Vol = AudioManager.instance.GetVol(10);
            music2Source.volume = 0;
            music2 = true;
        }
        if (!Player.i.InCombat() && music2) {
            music1Vol = music2Vol = AudioManager.instance.GetVol(6);
            music1Source.volume = 0;
            music2 = false;
        }

        if (music2) {
            music2Source.volume = Mathf.Lerp(music2Source.volume, music2Vol, 0.025f);
            music1Source.volume = Mathf.Lerp(music1Source.volume, 0, 0.05f);
        }
        else {
            music2Source.volume = Mathf.Lerp(music2Source.volume, 0, 0.025f);
            music1Source.volume = Mathf.Lerp(music1Source.volume, music1Vol, 0.05f);
        }
    }

    float ClosestDist(List<EnemyMovement> enemies)
    {
        float closest = Mathf.Infinity;
        foreach (var e in enemies) {
            float dist = Vector3.Distance(Player.i.transform.position, e.transform.position);
            if (dist < closest) {
                closest = dist;
            }
        }
        return closest;
    }
}
