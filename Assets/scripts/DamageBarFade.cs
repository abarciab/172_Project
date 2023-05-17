using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageBarFade : MonoBehaviour
{
    [SerializeField] Slider controlSlider, whiteSlider;
    [SerializeField] float delay, lerpSmoothness;
    float lerpStartTime;
    bool lerping;

    private void Start()
    {
        whiteSlider.value = controlSlider.value;
    }
    private void Update()
    {
        if (lerping) {
            lerpStartTime -= Time.deltaTime;
            if (lerpStartTime > 0) return;

            whiteSlider.value = Mathf.Lerp(whiteSlider.value, controlSlider.value, lerpSmoothness);
            if (Mathf.Abs(whiteSlider.value - controlSlider.value) <= 0.01f) {
                whiteSlider.value = controlSlider.value;
                lerping = false;
            }
        }
        else {
            if (controlSlider.value < whiteSlider.value) {
                lerping = true;
                lerpStartTime = delay;
            }
        }
    }

}
