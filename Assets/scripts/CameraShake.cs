using System.Collections;
using System.ComponentModel;
using UnityEngine;

/* 
 AUTHOR: Aidan B Bacon, see more at aidanbarciabbacon.wordpress.com

 DESCRIPTION: shakes whatever gameobject it's attatched to with a given intensity and time
 
 INSTRUCTRIONS: for best results make the animation curves similar but offset wiggles, going from
 -1 -> +1 on the y axis. 
 use the test values in edit mode to find good defaults for your use case.
 to activate, call "CameraShake.i.Shake()" from anywhere in your script with some optional parameters
*/


[ExecuteAlways]
public class CameraShake : MonoBehaviour
{
    public static CameraShake i;
    private void Awake()
    {
        i = this;
    }

    public bool test;
    [SerializeField] float testIntensity, testTime, testMod;
    [SerializeField] AnimationCurve shakeCurveX, shakeCurveY;

    private void Update()
    {
        if (test) {
            test = false;
            Shake(testIntensity, testTime, testMod);
        }
    }

    public void Shake(float intensity = 0.02f, float time = 0.2f, float mult = 1)
    {
        StartCoroutine(_Shake(intensity, time, mult));
    }

    IEnumerator _Shake(float intensity, float time, float mult = 1)
    {
        float maxTime = time;
        while (time > 0) {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
            float progress = 1 - ((time / maxTime));
            progress *= mult;
            transform.localPosition = new Vector3(shakeCurveX.Evaluate(progress), shakeCurveY.Evaluate(progress), 0) * intensity;
        }
        transform.localPosition = Vector3.zero;
    }
}
