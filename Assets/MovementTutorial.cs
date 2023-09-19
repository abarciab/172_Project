using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovementTutorial : MonoBehaviour
{
    [SerializeField] GameObject mouseParent, compassParent, questParent, wasdParent, runParent, rollParent;
    
    [SerializeField] float mouseMoveThreshold = 10, runTimeThreshold, wasdDirectionTime = 0.5f, numRolls = 4;
    [SerializeField] Slider progressSlider;

    GlobalUI ui;
    CameraController cam;

    float mouseMoveTime, wTime, aTime, sTime, dTime, runTime;

    int currentStage;

    public void Activate()
    {
        ui = GlobalUI.i;
        cam = FindObjectOfType<CameraController>(true);

        Player.i.FreezePlayer();
        HideAllUI();

        progressSlider.gameObject.SetActive(true);
        currentStage = 0;
        AdvanceToNextStage();
    }

    void HideAllUI()
    {
        ui.SetHideCompass(true);
        ui.SetHideQuest(true);
        ui.SetHideHUD(true);
    }

    private void Update()
    {
        if (1 - progressSlider.value < 0.001f) AdvanceToNextStage();

        if (currentStage == 1) CheckForMouseMove();
        if (currentStage == 4) CheckForWASD();
        if (currentStage == 5) CheckForRun();
        if (currentStage == 6) CheckForRoll();
    }

    void CheckForWASD()
    {
        if (Input.GetKey(KeyCode.W) && wTime < wasdDirectionTime) wTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.A) && aTime < wasdDirectionTime) aTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.S) && sTime < wasdDirectionTime) sTime += Time.deltaTime;
        if (Input.GetKey(KeyCode.D) && dTime < wasdDirectionTime) dTime += Time.deltaTime;

        progressSlider.value = (wTime + aTime + sTime + dTime) / (4 * wasdDirectionTime);
    }

    void CheckForRun()
    {
        bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        if (moving && Input.GetKey(KeyCode.LeftShift)) runTime += Time.deltaTime;

        progressSlider.value = runTime / runTimeThreshold;
    }

    void CheckForRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space)) progressSlider.value += 1 / numRolls;
    }

    public void AdvanceToNextStage()
    {
        DisableAll();
        progressSlider.value = 0;

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

    void EndTutorial()
    {
        ui.SetHideCompass(false);
        ui.SetHideQuest(false);
        ui.SetHideHUD(false);

        gameObject.SetActive(false);
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

        ui.SetHideCompass(false);
        progressSlider.gameObject.SetActive(false);
        compassParent.SetActive(true);
    }

    void StartQuestStage()
    {
        ui.SetHideQuest(false);
        questParent.SetActive(true);
    }

    void StartMouseStage()
    {
        mouseParent.SetActive(true);
        mouseMoveTime = 0;
    }

    void CheckForMouseMove()
    {
        mouseMoveTime += cam.lastMouseMoveDist;
        progressSlider.value = mouseMoveTime / mouseMoveThreshold;
    }
}