using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUI : MonoBehaviour
{
    [System.Serializable]
    public class DisplayImageData
    {
        [HideInInspector] public string name;
        public Fact triggerFact;
        public Sprite img;
        [SerializeField] Sound playOnDisplay;
        public float waitTime = 1.5f;

        public void PlaySound()
        {
            if (playOnDisplay == null) return;

            if (!playOnDisplay.instantialized) playOnDisplay = Instantiate(playOnDisplay);
            playOnDisplay.Play();
        }
    }

    public static GlobalUI i;
    private void Awake() { i = this; }

    [SerializeField] TextMeshProUGUI commandPrompt, subtitle;
    
    [SerializeField] float redFlashTime = 0.1f;
    [SerializeField] GameObject title, bottomLeft, crossHair, compass, blackOut;
    public GameObject tutorialSkip;
    [SerializeField] Image dmgIndicator, dmgFlash, goopOverlay;
    public Image fade;
    [SerializeField] Fact tutorialDone;
    bool hidingBLAnim, hidingCompassAnim, wasFighting;

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
    [SerializeField] GameObject newQuestFlash;
    bool hidingQuestAnim;

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

    [Header("display images")]
    [SerializeField] List<DisplayImageData> displayImages = new List<DisplayImageData>();
    [SerializeField] Image displayObj;
    bool displayingImage;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsSection, savesSection, controlsSection;
    [SerializeField] Sound openMenuSound;

    [Header("Volume Sliders")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider sfxSlider, musicSlider;

    [Header("Game over")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] float gameOverSoundTime;

    [Header("goat dropdown")]
    [SerializeField] private GameObject goatNotificationParent;
    [SerializeField] private TextMeshProUGUI goatNotificationText;

    UISound sound;
    bool loadingSave = false, gameOver;

    bool hidingQuest, hidingCompass, hidingBL;

    public void SetHideHUD(bool state) { hidingBL = state; }
    public void SetHideCompass(bool state) { hidingCompass = state; }
    public void SetHideQuest(bool state) { hidingQuest = state; }

    public void ShowNewGoat(int numGoatsFound, int numTotalGoats)
    {
        goatNotificationParent.SetActive(true);
        goatNotificationText.text = numGoatsFound + "/" + numTotalGoats;
    }

    public void SetFullscreen() {
        Screen.fullScreen = true;
    }

    public void SetWindowed() {
        Screen.fullScreen = false;
    }

    public void GameOver() {
        gameOverScreen.SetActive(true);
        if (loadingSave) return;
        loadingSave = true;
        Time.timeScale = 1;
        sound.SnakeHiss();
        StartCoroutine(GameOverTimer());
    }

    IEnumerator GameOverTimer() {
        fade.GetComponent<Fade>().Appear();
        yield return new WaitForSeconds(gameOverSoundTime);
        GameManager.i.RestartScene();
    }

    public void FadeToCredits(float delay)
    {
        StartCoroutine(_FadeToCredits(delay));
    }

    public IEnumerator _FadeToCredits(float delay)
    {
        if (gameOver) yield break;
        gameOver = true;

        fade.GetComponent<Fade>().Appear();
        yield return new WaitForSeconds(delay + 0.5f);
        SceneManager.LoadScene(3);
    }

    public void BlackOut(float delay)
    {
        blackOut.SetActive(true);
        StartCoroutine(_BackOut(delay));
    }

    IEnumerator _BackOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        blackOut.SetActive(false);
    }

    public bool DisplayingImage()
    {
        return displayingImage;
    }

    public void Inform(Fact newFact)
    {
        for (int i = 0; i < displayImages.Count; i++) {
            if (displayImages[i].triggerFact != newFact) continue;
            DisplayImage(displayImages[i]);
            displayImages.RemoveAt(0);
        }
    }

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

    void DisplayImage(DisplayImageData data)
    {

        displayingImage = true;
        displayObj.sprite = data.img;
        displayObj.gameObject.SetActive(true);

        Player.i.FreezePlayer();
        data.PlaySound();

        StartCoroutine(WaitForDisplayedItem(data));
    }

    IEnumerator WaitForDisplayedItem(DisplayImageData data)
    {
        yield return new WaitForSeconds(data.waitTime);

        DisplayLine("", "");

        while (!Input.GetKeyDown(KeyCode.F)) yield return null;

        EndDisplayImage(data);
    }

    void EndDisplayImage(DisplayImageData data)
    {
        sound.TurnPage();
        displayObj.gameObject.SetActive(false);
        Player.i.UnfreezePlayer();
        mainText.gameObject.SetActive(false);
        displayingImage = false;
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
        newQuestFlash.SetActive(true);
        if (!string.IsNullOrEmpty(text))sound.NewQuest(playLong);
    }

    public void Exit()
    {
        Save();
        SceneManager.LoadScene(0);
        //Application.Quit();
    }

    public void Pause()
    {
        openMenuSound.Play();
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
        if (!displayingImage) StopAllCoroutines();
        showHPbar = true;
        commandPrompt.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
        mainText.gameObject.SetActive(false);
        sound.TurnPage();
        talking = false;
        sound.EndConversation();
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

    public void SetSliderPositions(float master, float sfx, float music)
    {
        masterSlider.value = master;
        sfxSlider.value = sfx;
        musicSlider.value = music;
    }

    private void Update()
    {
        DisplaySpearCharge();

        DisplayAbilities();

        DisplayOverlays();

        VolumeSliders();

        sound.Heartbeat(1-HpBar.value);

        if (activeBoss == null || !activeBoss.activeInHierarchy) {
            EndBossFight();
            activeBoss = null;
        }

        if (mainText.gameObject.activeInHierarchy) commandPrompt.gameObject.SetActive(false);        
    }

    void VolumeSliders()
    {
        if (!pauseMenu.activeInHierarchy) return;

        AudioManager.instance.SetMasterVolume(masterSlider.value);
        AudioManager.instance.SetSfxVolume(sfxSlider.value);
        AudioManager.instance.SetMusicVolume(musicSlider.value);
    }

    void DisplayOverlays()
    {
        bool fighting = Player.i.InCombat();

        bool showQuest = !string.IsNullOrEmpty(currentQuest.text) && !talking && !fighting && !hidingQuest;
        if (showQuest && !currentQuest.gameObject.activeInHierarchy) {
            currentQuest.gameObject.SetActive(true);
            hidingQuestAnim = false;
        }
        if (!showQuest && currentQuest.gameObject.activeInHierarchy && !hidingQuestAnim) {
            currentQuest.GetComponent<UIEventCoord>().SetTrigger("Exit");
            hidingQuestAnim = true;
        }
        if (chargeParent.activeInHierarchy) currentQuest.gameObject.SetActive(false);

        bool showCompass = !fighting && !talking && !hidingCompass;
        if (fighting) wasFighting = true;
        if (!showCompass && compass.activeInHierarchy && !hidingCompassAnim) {
            compass.GetComponent<UIEventCoord>().SetTrigger("Exit");
            hidingCompassAnim = true;
        }
        if (showCompass && !compass.activeInHierarchy) {
            compass.SetActive(true);
            hidingCompassAnim = false;
            if (wasFighting) sound.EndCombat();
            wasFighting = false;
        }

        int targetAlpha = Player.i.goopTime > 0 ? 1 : 0;
        Color targetCol = Color.black;
        targetCol.a = targetAlpha;
        goopOverlay.color = Color.Lerp(goopOverlay.color, targetCol, 0.05f);
        goopOverlay.gameObject.SetActive(goopOverlay.color.a > 0.01f);
    }

    void DisplayAbilities()
    {
        var fight = Player.i.GetComponent<PFighting>();
        
        var showBL = (!title.activeInHierarchy && showHPbar && Player.i.InCombat()) || !Player.i.FullHealth() || fight.GetSWcooldown() > 0 || !FactManager.i.IsPresent(tutorialDone);
        if (talking || hidingBL) showBL = false;

        if (bottomLeft.activeInHierarchy && !showBL && !hidingBLAnim) {
            bottomLeft.GetComponent<UIEventCoord>().SetTrigger("Exit");
            hidingBLAnim = true;
        }
        if (!bottomLeft.activeInHierarchy && showBL) {
            bottomLeft.SetActive(true);
            hidingBLAnim = false;
        }

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

        openMenuSound = Instantiate(openMenuSound);
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
