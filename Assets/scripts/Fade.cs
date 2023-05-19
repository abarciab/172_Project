using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] float transitionTime = 1.3f, threshold = 0.005f, delayTime = 1.5f;
    [SerializeField] Image img;
    [SerializeField] AnimationCurve disapearCurve, appearCurve;
    float targetA, delayTimeLeft, transitionTimer;
    AnimationCurve currentCurve;

    private void Start()
    {
        img.color = new Color(0, 0, 0);
    }

    private void Update()
    {
        if (transitionTimer <= 0) return;
        delayTimeLeft -= Time.deltaTime;
        if (delayTimeLeft > 0) return;

        transitionTimer -= Time.deltaTime;
        float progress = 1 - (transitionTimer / transitionTime);
        float a = currentCurve.Evaluate(progress);

        SetColor(a);
        if (Mathf.Abs(1 - progress) < threshold) {
            SetColor(targetA);
            gameObject.SetActive(targetA > 0);
        }
    }

    void SetColor(float alpha)
    {
        var col = img.color;
        col.a = alpha;
        img.color = col;
    }

    public void Appear()
    {
        currentCurve = appearCurve;
        StartTransition(0, 1);
    }

    public void Disapear()
    {
        delayTimeLeft = delayTime;
        currentCurve = disapearCurve;
        StartTransition(1, 0);
    }

    void StartTransition(float start, float target)
    {
        SetColor(start);
        gameObject.SetActive(true);
        targetA = target;
        transitionTimer = transitionTime;
    }
}
