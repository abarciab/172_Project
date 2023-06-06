using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeRoarsSource : MonoBehaviour
{
    [SerializeField] Sound roar1;//, roar2, roar3;
    void Start()
    {
        roar1 = Instantiate(roar1);
    }

    public void PlayRoar()
    {
        roar1.Play();
        CameraShake.i.Shake(0.02f, 0.2f);
    }


}
