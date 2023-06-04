using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraShake : MonoBehaviour
{

    public bool test;
    [SerializeField] float testIntensity, testTime;
    [SerializeField] AnimationCurve shakeCurve;

    private void Update()
    {
        if (test) {
            test = false;
            Shake(testIntensity, testTime);
        }
    }

    public void Shake(float intensity, float time)
    {
        StartCoroutine(_Shake(intensity, time));
    }

    IEnumerator _Shake(float intensity, float time)
    {
        float maxTime = time;
        while (time > 0) {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
            float progress = time / maxTime;
            progress = Mathf.Cos(progress);
            print("progress: " + progress + ", percent: " + Mathf.RoundToInt((time / maxTime) * 100) + "%");
            transform.localPosition = new Vector3(1, 1, 0) * shakeCurve.Evaluate(progress) * intensity;
        }
        transform.localPosition = Vector3.zero;
    }
}
