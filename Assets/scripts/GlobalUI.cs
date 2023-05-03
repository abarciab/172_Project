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
    public Slider HpBar, throwCharge;
    [SerializeField] float redFlashTime = 0.1f;
    [SerializeField] GameObject title;
    [SerializeField] Image dmgIndicator, dmgFlash;
    [SerializeField] bool showHPbar;
    

    [Header("Quest")]
    [SerializeField] TextMeshProUGUI currentQuest;
    [SerializeField] Image questBacking; 
    [SerializeField] Color newQuestColor;
    [SerializeField] float newQuestSmoothness;
    float newQuestColorCooldown;

    [Header("Dialogue")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI mainText;

    [Header("Shockwave")]
    [SerializeField] GameObject swCountdown;
    [SerializeField] TextMeshProUGUI swCountDownText;

    [Header("newItem")]
    [SerializeField] GameObject newItem;
    [SerializeField] Fact spearFact, tutorialDone;
    [SerializeField] float newItemDisplayTime = 1;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;

    public string GetCurrentText()
    {
        return currentQuest.text;
    }

    public void UpdateQuestText(string text)
    {
        currentQuest.text = text;
        questBacking.color = newQuestColor;
        newQuestColorCooldown = 1f;
    }

    public void Exit()
    {
        Save();
        Application.Quit();
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        if (GameManager.i.paused) GameManager.i.Unpause();
    }

    public void Save()
    {
        FactManager.i.GetComponent<SaveManager>().SaveGame();
    }

    public void Load()
    {
        FactManager.i.GetComponent<SaveManager>().LoadGame();
        GameManager.i.RestartScene();
    }

    public void ResetGame()
    {
        FactManager.i.GetComponent<SaveManager>().ResetGame();
        GameManager.i.RestartScene();
    }

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
        throwCharge.gameObject.SetActive(Player.i.GetComponent<PFighting>().HasSpear());
        swCountdown.transform.parent.gameObject.SetActive(Player.i.GetComponent<PFighting>().enabled);

        currentQuest.gameObject.SetActive(!string.IsNullOrEmpty(currentQuest.text));
        newQuestColorCooldown -= Time.deltaTime;
        if (newQuestColorCooldown <= 0)  questBacking.color = Color.Lerp(questBacking.color, Color.black, newQuestSmoothness);

        float cooldown = Player.i.GetComponent<PFighting>().GetSWcooldown();
        swCountdown.SetActive(cooldown > 0);
        swCountDownText.text = Mathf.CeilToInt(cooldown).ToString();

        HpBar.gameObject.SetActive(!title.activeInHierarchy && showHPbar && Player.i.InCombat());
        if (mainText.gameObject.activeInHierarchy) commandPrompt.gameObject.SetActive(false);

        if (FactManager.i.IsPresent(tutorialDone)) {
            Destroy(newItem);
            newItem = null;
            Player.i.UnfreezePlayer();
            if (string.IsNullOrEmpty(mainText.text))mainText.gameObject.SetActive(false);
        }
        if (FactManager.i.IsPresent(spearFact) && newItem != null) {
            Player.i.FreezePlayer();
            newItem.SetActive(true);
        }
        if (newItem && newItem.activeInHierarchy) {
            newItemDisplayTime -= Time.deltaTime;
            if (newItemDisplayTime > 0) return;

            mainText.text = "combat info and controls in the ESC menu";
            mainText.gameObject.SetActive(true);
            showHPbar = false;

            if (!Input.GetKeyDown(KeyCode.F)) return;
            Destroy(newItem);
            newItem = null;
            mainText.text = "";
            Player.i.UnfreezePlayer();
            showHPbar = true;
            mainText.gameObject.SetActive(false);
        }
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
