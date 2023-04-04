using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalUI : MonoBehaviour
{
    public static GlobalUI i;
    private void Awake() { i = this; }

    [SerializeField] TextMeshProUGUI commandPrompt, subtitle;
    public Slider HpBar;
    [SerializeField] float redFlashTime = 0.1f;
    [SerializeField] GameObject red;

    public void FlashRed()
    {
        StartCoroutine(_FlashRed());
    }

    IEnumerator _FlashRed()
    {
        red.SetActive(true);
        yield return new WaitForSeconds(redFlashTime);
        red.SetActive(false);
    }

    private void Start()
    {
        commandPrompt.color = subtitle.color = new Color(1, 1, 1, 0);
    }

    public void DisplayPrompt(string prompt)
    {
        if (commandPrompt.text == prompt) return;

        commandPrompt.text = prompt;
        StartCoroutine(FadeText(commandPrompt, 1));
    }

    public void HidePrompt(string prompt = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeText(commandPrompt, 0));
    }

    IEnumerator FadeText(TextMeshProUGUI text, float targetAlpha)
    {
        text.gameObject.SetActive(true);
        var targetcolor = text.color;
        targetcolor.a = targetAlpha;
        while (Mathf.Abs(text.color.a - targetAlpha) > 0.01f) {
            text.color = Color.Lerp(text.color, targetcolor, 0.025f);
            yield return new WaitForEndOfFrame();
        }
        text.color = targetcolor;
        text.gameObject.SetActive(text.color.a != 0);
    }
}
