using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] Sound Music1, Music2;

    private void Start()
    {
        Music1 = Instantiate(Music1);
        Music2 = Instantiate(Music2);

        Music1.Play();
        Music2.PlaySilent();
    }

    private void Update()
    {
        if (Player.i.InCombat()) {
            Player.i.EnterCombat();
            Music2.PercentVolume(1, 0.05f);
            Music1.PercentVolume(0, 0.05f);
        }
        else {
            Music2.PercentVolume(0, 0.05f);
            Music1.PercentVolume(1, 0.05f);
        }
    }
}
