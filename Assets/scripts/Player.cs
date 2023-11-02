using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player i;
    [HideInInspector] public PAnimator animator;
    [HideInInspector] public Vector3 speed3D;
    [HideInInspector] public float forwardSpeed;
    [SerializeField] Fact tutorialComplete, hasSpear;
    [SerializeField] float dialogueDisplaySpeed;
   
    EnemyStats closestEnemy;
    [SerializeField] float lockOnDist = 15, speakerEndDist;

    [SerializeField] int maxHealth;
    [SerializeField] int health;

    [Header("Prompts")]
    [SerializeField] string startTalkingPrompt = "Press E to talk";

    [Header("sounds")]
    [SerializeField] Sound hurtSound;
    [SerializeField] Sound deathSound;
    [SerializeField] Sound healthRegenSound;
    private PSound pSound;

    Speaker interestedSpeaker;
    Gate interestedInteractable;

    [Header("Healing")]
    [SerializeField] float healWaitTime;
    [SerializeField] int healRate;
    float healCooldown;

    [Header("Goop")]
    [SerializeField] int goopDmg = 1;
    [SerializeField] float goopTick;
    float goopTickCooldown;
    [HideInInspector] public float goopTime;
    [SerializeField] float goopYLevel;

    [SerializeField] List<BaseEnemy> enemies = new List<BaseEnemy>();
    [SerializeField] List<BaseEnemy> currentMeleeEnemies = new List<BaseEnemy>();
    [SerializeField] float meleeRatio = 0.5f, tokenCheckTime = 5;
    float meleeCheckCooldown;

    //dialogue
    string currentLine, partialLine;
    int partialLineIndex;
    Coroutine displayCurrentLine;

    bool fightingEnabled;

    [Header("PowerUp")]
    [SerializeField] GameObject powerBall;
    [HideInInspector] public bool poweredUp;

    [HideInInspector] public bool canRun, canRoll, dead;

    public void PowerUp()
    {
        poweredUp = true;
        powerBall.SetActive(true);
    }
    public bool FullHealth()
    {
        return health == maxHealth;
    }

    public bool CheckMelee(BaseEnemy enemy, int priority)
    {
        return currentMeleeEnemies.Contains(enemy);
    }

    public void Notify(BaseEnemy enemy, bool agro)
    {
        if (agro) {
            if (enemies.Contains(enemy)) return;
            enemies.Add(enemy);
            return;
        }

        
        currentMeleeEnemies.Remove(enemy);
        enemies.Remove(enemy);
    }

    public bool InCombat()
    {
        return enemies.Count > 0;
    }

    public void SetSpearDamage(int damage)
    {
        GetComponent<PFighting>().SetSpearDmg(damage);
    }

    public void SetSpearLayer(int layer)
    {
        GetComponent<PFighting>().SetSpearLayer(layer);
    }

    public void FreezePlayer()
    {
        fightingEnabled = GetComponent<PFighting>().enabled;
        GetComponent<PMovement>().enabled = false;
        GetComponent<PFighting>().enabled = false;
    }

    public void UnfreezePlayer()
    {
        var fMan = FactManager.i;
        GetComponent<PMovement>().enabled = true;
        GetComponent<PFighting>().enabled = fightingEnabled || fMan.IsPresent(tutorialComplete) || fMan.IsPresent(hasSpear);
    }

    public void RemoveInteractable(Gate interactable)
    {
        if (interestedInteractable == interactable) {
            interestedInteractable = null;
            GlobalUI.i.HidePrompt(interactable.prompt);
        }
    }

    public void AddInteractable(Gate interactable)
    {
        interestedInteractable = interactable;
        GlobalUI.i.DisplayPrompt(interactable.prompt);
    }

    public void ActivateInteractable()
    {
        if (interestedSpeaker) StartConversation();
        else if (interestedInteractable) interestedInteractable.Interact();
    }

    public void SpeakerStopInterest(Speaker speaker)
    {
        if (interestedSpeaker != speaker) return;
        interestedSpeaker = null;
        GlobalUI.i.HidePrompt();
    }

    public void SpeakerShowInterest(Speaker speaker)
    {
        interestedSpeaker = speaker;
        GlobalUI.i.DisplayPrompt(startTalkingPrompt);
    }

    void StartConversation()
    {
        if (interestedSpeaker == null) return;

        if (!GlobalUI.i.talking) pSound.StartConversation();

        GetComponent<PMovement>().StopMovement();

        if (string.IsNullOrEmpty(currentLine)) currentLine = interestedSpeaker.GetNextLine();

        if (string.Equals("END", currentLine)) EndConversation();
        else {

            SwitchToDialogueCam();
            interestedSpeaker.talking = true;

            if (displayCurrentLine == null) { 
                displayCurrentLine = StartCoroutine(_DisplayCurrentLine()); 
                return; 
            }

            StopCoroutine(displayCurrentLine);
            GlobalUI.i.DisplayLine(interestedSpeaker.characterName, currentLine);
            currentLine = null;
            displayCurrentLine = null;
        }
    }

    IEnumerator _DisplayCurrentLine()
    {
        partialLine = "";
        partialLineIndex = 0;
        while (currentLine != null) {
            partialLine += currentLine[partialLineIndex];
            partialLineIndex += 1;
            if (partialLineIndex >= currentLine.Length) currentLine = null;

            GlobalUI.i.DisplayLine(interestedSpeaker.characterName, partialLine);
            yield return new WaitForSeconds(dialogueDisplaySpeed);
        }
        displayCurrentLine = null;
    }


    void SwitchToDialogueCam()
    {
        if (CameraState.i.current.displayName == CameraState.StateName.dialogue) return;

        CameraState.i.SwitchToState(CameraState.StateName.dialogue);
        CameraState.i.SetDialogueTarget(interestedSpeaker.transform, interestedSpeaker.cameraOffset);

        Vector3 speakerPosition = interestedSpeaker.GetStandPosition();
        speakerPosition.y = transform.position.y;
        transform.position = speakerPosition;
        transform.LookAt(interestedSpeaker.transform.position);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    public void EndConversation()
    {
        CameraState.i.SwitchToState(CameraState.StateName.MouseFollow);
        interestedSpeaker.EndConversation();
        GetComponent<PMovement>().ResumeMovement();
        interestedSpeaker = null;
        displayCurrentLine = null;
        currentLine = null;
        GlobalUI.i.EndConversation();
    }

    public void EnterCombat()
    {
        GetComponent<PFighting>().DrawSpear();
    }

    private void Update() {
        UpdateMeleeEnemies();

        if (Input.GetKeyDown(KeyCode.K)) GameManager.i.RestartScene();
        if (Input.GetKeyDown(KeyCode.L)) GameManager.i.GetComponent<SaveManager>().SaveGame();
        if (Input.GetKeyDown(KeyCode.O)) GameManager.i.GetComponent<SaveManager>().ResetGame();

        if (healCooldown <= 0 && health < maxHealth){
            StartCoroutine(Heal());
            healthRegenSound.Play();
        }
        healCooldown -= Time.deltaTime;

        if (interestedSpeaker == null) GlobalUI.i.HidePrompt(startTalkingPrompt);
        if (interestedSpeaker != null && Vector3.Distance(transform.position, interestedSpeaker.transform.position) > speakerEndDist) {
            interestedSpeaker = null;
            GlobalUI.i.HidePrompt();
        }

        bool underwater = transform.position.y <= goopYLevel;

        if (underwater) goopTime = 1;
        if (goopTime > 0) {
            goopTime -= Time.deltaTime;
            goopTickCooldown -= Time.deltaTime;
            if (goopTickCooldown <= 0) {
                ChangeHealth(underwater ? -goopDmg * 5 : -goopDmg);
                goopTickCooldown = goopTick;
            }
        }

        if (FactManager.i.IsPresent(tutorialComplete)) GetComponent<PFighting>().enabled = true;

        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i] == null) enemies.RemoveAt(i);
        }
        if (enemies.Count == 0) {
            CameraState.i.StopLockOn();
            if (GetComponent<PMovement>().running) GetComponent<PFighting>().PutAwaySpear();
            return;
        }
    }

    void UpdateMeleeEnemies()
    {
        for (int i = 0; i < enemies.Count; i++) {
            if (i >= enemies.Count) continue;
            if (enemies[i] == null || enemies[i].gameObject == null) enemies.RemoveAt(i);
        }
        for (int i = 0; i < currentMeleeEnemies.Count; i++) {
            if (i >= currentMeleeEnemies.Count) continue;
            if (currentMeleeEnemies[i] == null || currentMeleeEnemies[i].gameObject == null) currentMeleeEnemies.RemoveAt(i);
        }

        meleeCheckCooldown -= Time.deltaTime;
        if (meleeCheckCooldown > 0) return;
        meleeCheckCooldown = tokenCheckTime;

        currentMeleeEnemies.Clear();
        var sortedList = SortList(enemies);
        for (int i = 0; i < sortedList.Count; i++) {
            if ((float) currentMeleeEnemies.Count / enemies.Count >= meleeRatio) break;
            currentMeleeEnemies.Add(sortedList[i]);
        }
    }

    List<BaseEnemy> SortList(List<BaseEnemy> inputList)
    {
        var outputList = new List<BaseEnemy>();

        int currentPriority = 0;
        while (outputList.Count < inputList.Count) {
            for (int i = 0; i < inputList.Count; i++) {
                var enemy = inputList[i];
                if (outputList.Contains(enemy)) continue;
                if (enemy.MeleePriority() == currentPriority) {
                    int index = 0;
                    float dist = enemy.Dist();

                    while (enemies.Count > index + 1 && enemies[index].Dist() > dist && enemies[index + 1].MeleePriority() == enemy.MeleePriority()) index += 1;
                    if (outputList.Count == 0) {
                        outputList.Add(enemy);
                    }
                    else {
                        if (index > outputList.Count) index = outputList.Count - 1;
                        outputList.Insert(index, enemy);
                    }
                }
            }
            currentPriority += 1;
        }
        return outputList;
    }

    IEnumerator Heal()
    {
        healCooldown = Mathf.Infinity;
        while (health < maxHealth) {
            ChangeHealth(healRate);
            yield return new WaitForSeconds(0.5f);
        }
        healCooldown = healWaitTime;
    }


    private void Start() {
        health = maxHealth;
        hurtSound = Instantiate(hurtSound);
        deathSound = Instantiate(deathSound);
        healthRegenSound = Instantiate(healthRegenSound);
        pSound = GameObject.FindGameObjectWithTag("Bonnie").GetComponent<PSound>();
        powerBall.SetActive(false);
    }

    public void ChangeHealth(int delta) {
        health = Mathf.Min(health + delta, maxHealth);
        if (health <= 0) Die();
        GlobalUI.i.HpBar.value = (float)health / maxHealth;
        GlobalUI.i.UpdateDmgIndicator((float)health / maxHealth);

        if (delta > 0) return;

        healCooldown = healWaitTime;
        StopAllCoroutines();

        if (health > 0)hurtSound.Play();
        CameraShake.i.Shake();
    }

    void Die() {
        if (dead) return;

        AudioManager.instance.PauseNonMusic();
        CameraState.i.GetComponent<MusicPlayer>().FadeOut();
        deathSound.Play();
        GlobalUI.i.GameOver();
        dead = true;
        
    }

    private void Awake()
    {
        i = this;
        animator = GetComponent<PAnimator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "StoneSurface")
            pSound.SetSoftSurface(false);
        else
            pSound.SetSoftSurface(true);
    }

}
