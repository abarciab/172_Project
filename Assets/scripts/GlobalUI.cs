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
    [SerializeField] GameObject title;
    [SerializeField] Image dmgIndicator, dmgFlash;
    [SerializeField] bool showHPbar;

    [Header("Dialogue")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI mainText;

    public void DisplayLine(string speaker, string line)
    {
        showHPbar = false;
        commandPrompt.gameObject.SetActive(false);

        nameText.gameObject.SetActive(true);
        nameText.text = speaker;
        mainText.gameObject.SetActive(true);
        mainText.text = line;
    }

    public void EndConversation()
    {
        print("END");
        StopAllCoroutines();
        showHPbar = true;
        commandPrompt.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
        mainText.gameObject.SetActive(false);
    }

    public void DisplayPrompt(string prompt)
    {
        if (mainText.gameObject.activeInHierarchy) return;

        StopAllCoroutines();
        commandPrompt.text = prompt;
        StartCoroutine(FadeText(commandPrompt, 1));
    }

    public void HidePrompt(string prompt)
    {
        if (string.Equals(prompt, commandPrompt.text)) HidePrompt();
    }

    public void HidePrompt()
    {
        if (commandPrompt.color.a == 0 || !commandPrompt.gameObject.activeInHierarchy) return;

        StopAllCoroutines();
        StartCoroutine(FadeText(commandPrompt, 0));
    }

    public void UpdateDmgIndicator(float health)
    {
        

        var col = dmgIndicator.color;
        col.a = (1-health) * 0.8f;
        if (col.a > dmgIndicator.color.a) StartCoroutine(FlashRed());
        dmgIndicator.color = col;
    }

    IEnumerator FlashRed()
    {
        dmgFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(redFlashTime);
        dmgFlash.gameObject.SetActive(false);
    }

    private void Update()
    {
        HpBar.gameObject.SetActive(!title.activeInHierarchy && showHPbar);
        if (mainText.gameObject.activeInHierarchy) commandPrompt.gameObject.SetActive(false);
    }

    private void Start()
    {
        commandPrompt.color = subtitle.color = new Color(1, 1, 1, 0);

        mainText.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
    }

    
    IEnumerator FadeText(TextMeshProUGUI text, float targetAlpha)
    {
        text.gameObject.SetActive(true);
        var targetcolor = text.color;
        targetcolor.a = targetAlpha;
        while (Mathf.Abs(text.color.a - targetAlpha) > 0.01f) {
            text.color = Color.Lerp(text.color, targetcolor, 0.1f);
            yield return new WaitForEndOfFrame();
        }
        text.color = targetcolor;
        text.gameObject.SetActive(text.color.a != 0);
    }
}
