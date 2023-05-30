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
    
    [SerializeField] float redFlashTime = 0.1f;
    [SerializeField] GameObject title, bottomLeft, crossHair, compass;
    public GameObject tutorialSkip;
    [SerializeField] Image dmgIndicator, dmgFlash, goopOverlay;
    public Image fade;

    [Header("boss bar")]
    [SerializeField] GameObject bossBar;
    public Slider bossSlider;
    [SerializeField] TextMeshProUGUI bossName;
    GameObject activeBoss;

    [Header("spear charge")]
    public Slider throwCharge;
    [SerializeField] GameObject chargeParent, centralMarkerShine;
    [SerializeField] Image centralMarker;
    [SerializeField] Color centralMarkerDefaulColor, centralMarkerFullColor;

    [Header("Quest")]
    [SerializeField] TextMeshProUGUI currentQuest;
    [SerializeField] Image questBacking; 
    [SerializeField] Color newQuestColor;
    [SerializeField] float newQuestSmoothness;
    float newQuestColorCooldown;

    [Header("Dialogue")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI mainText;
    [HideInInspector] public bool talking;

    [Header("Abilities")]
    [SerializeField] GameObject swCountdown;
    [SerializeField] TextMeshProUGUI swCountDownText;
    [SerializeField] GameObject swAbilityFlash, lmbRecallFlash, rmbRecallFlash;
    [SerializeField] float abilityFlashTime = 0.1f;
    [SerializeField] Image LMBability, RMBability;
    [SerializeField] GameObject LMBdisable, RMBdisable;
    [SerializeField] Sprite throwSprite, stabSprite, recallSprite;

    [Header("HP bar")]
    public Slider HpBar;
    [SerializeField] bool showHPbar;

    [Header("newItem")]
    [SerializeField] GameObject newItem;
    [SerializeField] Fact spearFact, tutorialDone;
    [SerializeField] float newItemDisplayTime = 1;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsSection, savesSection, controlsSection;

    [Header("Volume Sliders")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider sfxSlider, musicSlider;

    UISound sound;
    bool loadingSave = false;

    public void LoadSave(int save)
    {
        if (loadingSave) return;
        loadingSave = true;
        Time.timeScale = 1;
        StartCoroutine(transition(save));
    }

    public void StartBossFight(string name, GameObject boss)
    {
        bossBar.SetActive(true);
        bossName.text = name;
        activeBoss = boss;
    }

    public void EndBossFight()
    {
        bossBar.SetActive(false);
    }

    public void RestartScene()
    {
        if (loadingSave) return;
        loadingSave = true;
        Time.timeScale = 1;
        StartCoroutine(transition(-1, false, 2));   
    }

    IEnumerator transition(int save = -1, bool reset = true, float time = 1.5f) {
        fade.GetComponent<Fade>().Appear();
        yield return new WaitForSeconds(time);

        if (save != -1) FactManager.i.LoadSaveState(save);
        else {
            if (reset)FactManager.i.GetComponent<SaveManager>().ResetGame();
            GameManager.i.RestartScene();
        }
    }

    public void ToggleSaves()
    {
        HideMenuSections();
        savesSection.SetActive(!savesSection.activeInHierarchy);
    }

    public void ToggleControls()
    {
        HideMenuSections();
        controlsSection.SetActive(!controlsSection.activeInHierarchy);
    }

    public void ToggleSettings()
    {
        HideMenuSections();
        settingsSection.SetActive(!settingsSection.activeInHierarchy);
    }

    public void HideMenuSections()
    {
        settingsSection.SetActive(false);
        controlsSection.SetActive(false);
        savesSection.SetActive(false);
    }

    public string GetCurrentText()
    {
        return currentQuest.text;
    }

    public void UpdateQuestText(string text, bool playLong)
    {
        currentQuest.text = text;
        questBacking.color = newQuestColor;
        newQuestColorCooldown = 1f;
        if (!string.IsNullOrEmpty(text))sound.NewQuest(playLong);
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
        if (loadingSave) return;
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
        if (loadingSave) return;
        loadingSave = true;
        Time.timeScale = 1;
        StartCoroutine(transition());
    }

    public void DisplayLine(string speaker, string line)
    {
        if (!string.IsNullOrEmpty(speaker))talking = true;
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
        sound.TurnPage();
        talking = false;
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
        DisplaySpearCharge();

        DisplayAbilities();

        DisplayOverlay();

        VolumeSliders();

        sound.Heartbeat(1-HpBar.value);

        bool fighting = Player.i.InCombat();
        compass.SetActive(!fighting);
        currentQuest.gameObject.SetActive(!string.IsNullOrEmpty(currentQuest.text) && !talking && !fighting);
        if (activeBoss == null || !activeBoss.activeInHierarchy) {
            EndBossFight();
            activeBoss = null;
        }

        newQuestColorCooldown -= Time.deltaTime;
        if (newQuestColorCooldown <= 0)  questBacking.color = Color.Lerp(questBacking.color, Color.black, newQuestSmoothness);

        
        if (mainText.gameObject.activeInHierarchy) commandPrompt.gameObject.SetActive(false);

        if (FactManager.i.IsPresent(tutorialDone)) {
            Destroy(newItem);
            newItem = null;
            Player.i.UnfreezePlayer();
            if (string.IsNullOrEmpty(mainText.text))mainText.gameObject.SetActive(false);
        }
        if (FactManager.i.IsPresent(spearFact) && newItem != null && !newItem.activeInHierarchy) {
            Player.i.FreezePlayer();
            sound.GetItem();
            newItem.SetActive(true);
        }
        if (newItem && newItem.activeInHierarchy) {
            newItemDisplayTime -= Time.deltaTime;
            if (newItemDisplayTime > 0) return;

            mainText.text = " ";
            mainText.gameObject.SetActive(true);
            showHPbar = false;
            

            if (!Input.GetKeyDown(KeyCode.F)) return;
            sound.TurnPage();
            Destroy(newItem);
            newItem = null;
            mainText.text = "";
            Player.i.UnfreezePlayer();
            showHPbar = true;
            mainText.gameObject.SetActive(false);
        }
    }

    void VolumeSliders()
    {
        AudioManager.instance.SetMasterVolume(masterSlider.value);
        //AudioManager.instance.SetSfxVolume(sfxSlider.value);
        //AudioManager.instance.SetMusicVolume(musicSlider.value);
    }

    void DisplayOverlay()
    {
        int targetAlpha = Player.i.goopTime > 0 ? 1 : 0;
        Color targetCol = Color.black;
        targetCol.a = targetAlpha;
        goopOverlay.color = Color.Lerp(goopOverlay.color, targetCol, 0.05f);
        goopOverlay.gameObject.SetActive(goopOverlay.color.a > 0.01f);
    }

    void DisplayAbilities()
    {
        var fight = Player.i.GetComponent<PFighting>();
        
        bottomLeft.SetActive((!title.activeInHierarchy && showHPbar && Player.i.InCombat()) || !Player.i.FullHealth() || fight.GetSWcooldown() > 0 || !FactManager.i.IsPresent(tutorialDone));
        if (talking) bottomLeft.SetActive(false);
        crossHair.SetActive(!talking);
        
        float cooldown = fight.GetSWcooldown();
        if (cooldown <= 0 && swCountdown.activeInHierarchy) StartCoroutine(FlashAbility(swAbilityFlash, abilityFlashTime));
        swCountdown.SetActive(cooldown > 0);
        swCountDownText.text = Mathf.CeilToInt(cooldown).ToString();


        bool hasSpear = fight.HasSpear();

        if (!hasSpear) {
            LMBability.sprite = RMBability.sprite = recallSprite;
            bool recallReady = fight.RecallReady;
            if (recallReady && LMBdisable.activeInHierarchy) {
                StartCoroutine(FlashAbility(lmbRecallFlash, abilityFlashTime));
                StartCoroutine(FlashAbility(rmbRecallFlash, abilityFlashTime));
            }
            LMBdisable.SetActive(!recallReady);
            RMBdisable.SetActive(!recallReady);
        }
        else {
            LMBability.sprite = throwSprite;
            RMBability.sprite = stabSprite;
            LMBdisable.SetActive(fight.stabbing || fight.chargingSpear());
            RMBdisable.SetActive(fight.stabbing || fight.chargingSpear());
        }
    }

    IEnumerator FlashAbility (GameObject flash, float time)
    {
        flash.SetActive(true);
        yield return new WaitForSeconds(time);
        flash.SetActive(false);
    }

    void DisplaySpearCharge()
    {
        chargeParent.SetActive(Player.i.GetComponent<PFighting>().chargingSpear());
        swCountdown.transform.parent.gameObject.SetActive(Player.i.GetComponent<PFighting>().enabled);

        float charge = throwCharge.value;
        centralMarker.color = Color.Lerp(centralMarkerDefaulColor, centralMarkerFullColor, charge);

        bool perfect = charge > 0.85  && charge < 1;
        if (perfect && !centralMarkerShine.activeInHierarchy) sound.FullCharge();
        centralMarkerShine.SetActive(perfect);
    }


    private void Start()
    {
        fade.GetComponent<Fade>().Disapear();

        commandPrompt.color = subtitle.color = new Color(1, 1, 1, 0);

        mainText.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);

        sound = GetComponent<UISound>();

        goopOverlay.color = new Color(1, 1, 1, 0);
        goopOverlay.gameObject.SetActive(false);

        EndBossFight();
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
