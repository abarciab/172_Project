using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goopExplodeDestroySelf : MonoBehaviour
{
    public float speed, lifeTime;
    float maxLife;
    //[SerializeField] AnimationCurve speedProfile;

    private void Start()
    {
        maxLife = lifeTime;
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
        float progress = 1 - (lifeTime / maxLife);
        //transform.localScale += Vector3.one * speedProfile.Evaluate(progress) * speed * Time.deltaTime;
    }
}

