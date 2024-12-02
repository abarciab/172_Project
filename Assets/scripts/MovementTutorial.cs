using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovementTutorial : MonoBehaviour
{
    [SerializeField] GameObject mouseParent, compassParent, questParent, wasdParent, runParent, rollParent;
    
    [SerializeField] float mouseMoveThreshold = 10, runTimeThreshold, wasdDirectionTime = 0.5f, numRolls = 4;
    [SerializeField] Slider progressSlider;

    [Header("Sounds")]
    [SerializeField] Sound advanceSound;
    [SerializeField] Sound completeSound;

    [Header("Parameter")]
    [SerializeField] private float _displayTime = 1.8f;

    private GlobalUI _ui;
    private CameraController _cam;
    private float _mouseMoveTime;
    private float _wTime;
    private float _aTime;
    private float _sTime;
    private float _dTime;
    private float _runTime;

    int currentStage;
    float progressTarget;

    private void Start()
    {
        advanceSound = Instantiate(advanceSound);
        completeSound = Instantiate(completeSound);
    }

    public async void Activate()
    {
        GlobalUI.i.Busy = true;

        gameObject.SetActive(true);

        _ui = GlobalUI.i;
        _cam = FindObjectOfType<CameraController>(true);

        Player.i.FreezePlayer();
        HideAllUI();

        currentStage = 0;

        var toShow = new List<GameObject> { progressSlider.gameObject, mouseParent };
        await Task.Delay((int)(_displayTime * 1000));
        foreach (var o in toShow) o.SetActive(true);
        AdvanceToNextStage();
    }

    void HideAllUI()
    {
        //_ui.SetHideCompass(true);
        //_ui.SetHideQuest(true);
        _ui.SetHideHUD(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) EndTutorial(); 

        if (1 - progressSlider.value < 0.01f) AdvanceToNextStage();

        if (currentStage == 1) CheckForMouseMove();
        if (currentStage == 4) CheckForWASD();
        if (currentStage == 5) CheckForRun();
        if (currentStage == 6) CheckForRoll();
    }

    void CheckForWASD()
    {
        if (Input.GetKey(KeyCode.W) && _wTime < wasdDirectionTime) _wTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.A) && _aTime < wasdDirectionTime) _aTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.S) && _sTime < wasdDirectionTime) _sTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.D) && _dTime < wasdDirectionTime) _dTime += Time.deltaTime;

        progressSlider.value = (_wTime + _aTime + _sTime + _dTime) / (4 * wasdDirectionTime);
    }

    void CheckForRun()
    {
        bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        if (moving && Input.GetKey(KeyCode.LeftShift)) _runTime += Time.deltaTime;

        progressSlider.value = _runTime / runTimeThreshold;
    }

    void CheckForRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space)) progressTarget += 1 / numRolls;
        progressSlider.value = Mathf.Lerp(progressSlider.value, progressTarget, 0.2f);
    }

    public void AdvanceToNextStage()
    {
        DisableAll();
        progressSlider.value = 0;

        if (currentStage < 6) advanceSound.Play();

        switch (currentStage) {
            case 0: StartMouseStage();
                break;
            case 1: StartCompassStage();
                break;
            case 2: StartQuestStage();
                break;
            case 3: StartWASDStage();
                break;
            case 4: StartRunStage();
                break;
            case 5: StartRollStage();
                break;
            case 6: EndTutorial();
                break;
        }

        currentStage += 1;
    }

    private async void EndTutorial()
    {
        //_ui.SetHideCompass(false);
        //_ui.SetHideQuest(false);
        _ui.SetHideHUD(false);

        gameObject.SetActive(false);
        completeSound.Play();

        Player.i.UnfreezePlayer();
        Player.i.canRoll = Player.i.canRun = true;

        await Task.Yield();
        GlobalUI.i.Busy = false;
    }

    void DisableAll()
    {
        mouseParent.SetActive(false);
        compassParent.SetActive(false);
        questParent.SetActive(false);
        wasdParent.SetActive(false);
        runParent.SetActive(false);
        rollParent.SetActive(false);
    }

    void StartWASDStage()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Player.i.UnfreezePlayer();
        Player.i.canRun = false;
        Player.i.canRoll = false;

        progressSlider.gameObject.SetActive(true);
        wasdParent.SetActive(true);
    }

    void StartRunStage()
    {
        Player.i.canRun = true;
        runParent.SetActive(true);
    }

    void StartRollStage()
    {
        Player.i.canRoll = true;
        rollParent.SetActive(true);
    }

    void StartCompassStage()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        //_ui.SetHideCompass(false);
        progressSlider.gameObject.SetActive(false);
        compassParent.SetActive(true);
    }

    void StartQuestStage()
    {
        //_ui.SetHideQuest(false);
        questParent.SetActive(true);
    }

    void StartMouseStage()
    {
        mouseParent.SetActive(true);
        _mouseMoveTime = 0;
    }

    void CheckForMouseMove()
    {
        _mouseMoveTime += _cam.lastMouseMoveDist;
        progressSlider.value = _mouseMoveTime / mouseMoveThreshold;
    }
}
