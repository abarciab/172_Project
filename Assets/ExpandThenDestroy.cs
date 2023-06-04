using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandThenDestroy : MonoBehaviour
{
    public float speed, lifeTime;

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
    }
}
