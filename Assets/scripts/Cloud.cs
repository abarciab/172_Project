using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class Cloud : MonoBehaviour
{
    public Vector3 speed;
    public float lifeTime;
    Vector3 targetScale;
    bool starting;

    private void Start()
    {
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        starting = true;
    }


    private void Update()
    {
        if (starting) transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 0.025f);
        if (Mathf.Abs((transform.localScale - targetScale).magnitude) <= 0.1f) starting = false;

        transform.position += speed * Time.deltaTime;

        if (starting) return;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.025f);
        if (transform.localScale.magnitude <= 0.01f) {
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }
    }

}
