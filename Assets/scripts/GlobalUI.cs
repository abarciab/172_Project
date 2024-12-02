using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum UIAction { START_BOSS_FIGHT, END_BOSS_FIGHT, SET_BOSS_HEALTH, DISPLAY_QUEST_TEXT, DISPLAY_QUEST_TEXT_LONG, END_QUEST, DISPLAY_GOATS, DISPLAY_PLAYER_HP, UPDATE_HP_OVERLAY, UPDATE_GOOP_OVERLAY,
    DISPLAY_IMAGE, UPDATE_CHARGE, BLACK_OUT, START_CONVERSATION, DISPLAY_LINE, END_CONVERSATION, HIDE_PROMPT, DISPLAY_PROMPT, SUNBLAST_READY, DISPLAY_SUNBLAST_COOLDOWN, CATCH_SPEAR, THROW_SPEAR, RECALL_READY, 
    START_COMBAT, END_COMBAT}


public class GlobalUI : MonoBehaviour
{
    public static GlobalUI i;
    private void Awake() { i = this; }

    [Header("Sub scripts")]
    [SerializeField] private DialogueController _dialogue;
    [SerializeField] private HUDUIController _hud;
    [SerializeField] private PauseMenuController _pauseMenu;

    [HideInInspector] public UnityEvent<UIAction, object> OnUpdateUI;

    public void Do(UIAction type)
    {
        OnUpdateUI?.Invoke(type, null);
    }

    public void Do<T>(UIAction type, T parameter = default)
    {
        CheckParameter(parameter);
        OnUpdateUI?.Invoke(type, parameter);
    }

    private void CheckParameter<T>(T parameter)
    {
        if (parameter == null || parameter is T) return;
        Debug.LogError("Incorrect parameter passed. expected type: " + typeof(T) + ", got: " + parameter.GetType());
    }

    //
    //
    //

    public GameObject tutorialSkip;
    [SerializeField] private Fact tutorialDone;

    [SerializeField] GameObject bottomLeft, crossHair, compass;
    [SerializeField] private Fade _fade;
    bool hidingBLAnim, hidingCompassAnim, wasFighting;


    [Header("Game over")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] float gameOverSoundTime;

    [HideInInspector] public bool Busy;

    private UISound sound;
    private bool _loadingSave;
    private bool _gameOver;
    private bool _hidingBL;

    public bool Talking => _dialogue.Talking;
    public void SetHideHUD(bool state) => _hidingBL = state;
    public void Save() { }
    public void Load() => GameManager.i.RestartScene();
    public void SetFullscreenMode(bool fullScreen) => Screen.fullScreen = fullScreen;
    public void FadeToCredits(float delay) => StartCoroutine(_FadeToCredits(delay));
    public void Pause() => _pauseMenu.gameObject.SetActive(true);

    private void Start()
    {
        _fade.Disapear();
        sound = GetComponent<UISound>();

        Player.i.OnHealthChange.AddListener(OnPlayerHealthChange);
        Player.i.OnStartCombat.AddListener(() => Do(UIAction.START_COMBAT));
        Player.i.OnEndCombat.AddListener(OnEndCombat);
    }

    private void OnEndCombat()
    {
        sound.EndCombat();
        Do(UIAction.END_COMBAT);
    }

    private void OnPlayerHealthChange(float percent)
    {
        Do(UIAction.DISPLAY_PLAYER_HP, percent);
        sound.Heartbeat(1-percent);
    }

    private void Update()
    {
        var fight = Player.i.GetComponent<PFighting>();

        var showBL = Player.i.InCombat || !Player.i.FullHealth() || fight.GetSunblastCooldown() > 0;
        if (Talking || _hidingBL) showBL = false;

        if (bottomLeft.activeInHierarchy && !showBL && !hidingBLAnim) {
            bottomLeft.GetComponent<UIButtonEventCoord>().SetTrigger("Exit");
            hidingBLAnim = true;
        }
        if (!bottomLeft.activeInHierarchy && showBL) {
            bottomLeft.SetActive(true);
            hidingBLAnim = false;
        }

        crossHair.SetActive(!Talking);
    }

    public void GameOver() {
        gameOverScreen.SetActive(true);
        if (_loadingSave) return;
        _loadingSave = true;
        Time.timeScale = 1;
        sound.SnakeHiss();
        StartCoroutine(GameOverTimer());
    }

    IEnumerator GameOverTimer() {
        _fade.Appear();
        yield return new WaitForSeconds(gameOverSoundTime);
        GameManager.i.RestartScene();
    }

    public IEnumerator _FadeToCredits(float delay)
    {
        if (_gameOver) yield break;
        _gameOver = true;

        _fade.GetComponent<Fade>().Appear();
        yield return new WaitForSeconds(delay + 0.5f);
        SceneManager.LoadScene(3);
    }


    public void Resume()
    {
        if (_loadingSave) return; 
        _pauseMenu.gameObject.SetActive(false);
        if (GameManager.i.paused) GameManager.i.Unpause();
    }

    public void ResetGame()
    {
        if (_loadingSave) return;
        _loadingSave = true;
        Time.timeScale = 1;
        StartCoroutine(transition());
    }

    public void Exit()
    {
        Save();
        SceneManager.LoadScene(0);
    }

    private IEnumerator transition()
    {
        _fade.GetComponent<Fade>().Appear();
        yield return new WaitForSeconds(1.5f);
        GameManager.i.RestartScene();
    }
}
