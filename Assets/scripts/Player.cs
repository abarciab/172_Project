using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static Player i;

    [HideInInspector] public PSound Sounds;
    [SerializeField] private AnimationCurve _heartbeatCurve;

    //
    //
    //


    [Header("Prompts")]
    [SerializeField] string startTalkingPrompt = "Press E to talk";

    [Header("Healing")]
    [SerializeField] float healWaitTime;
    [SerializeField] int healRate;
    private float healCooldown;

    [Header("Goop")]
    [SerializeField] int goopDmg = 1;
    [SerializeField] float goopTick;
    private float goopTickCooldown;

    [Header("Misc")]
    public bool poweredUp;
    public bool canRun;
    public bool canRoll;
    public bool dead;

    [SerializeField] float dialogueDisplaySpeed;
    [SerializeField] float lockOnDist = 15;
    [SerializeField] float speakerEndDist;
    [SerializeField] private int maxHealth;
    [SerializeField] GameObject powerBall;
    [SerializeField] List<BaseEnemy> enemies = new List<BaseEnemy>();
    [SerializeField] List<BaseEnemy> currentMeleeEnemies = new List<BaseEnemy>();
    [SerializeField] float meleeRatio = 0.5f, tokenCheckTime = 5;

    [HideInInspector] public float GoopTime { get; private set; }
    [HideInInspector] public UnityEvent<float> OnHealthChange;
    [HideInInspector] public UnityEvent OnStartCombat;
    [HideInInspector] public UnityEvent OnEndCombat;
    [HideInInspector] public UnityEvent OnDie;
    [HideInInspector] public PAnimator animator;
    [HideInInspector] public Vector3 speed3D;
    [HideInInspector] public float forwardSpeed;

    private int health;
    private float meleeCheckCooldown;
    private bool fightingEnabled;
    private string currentLine;
    private string partialLine;
    private int partialLineIndex;
    private Coroutine displayCurrentLine;
    private Speaker interestedSpeaker;
    private Gate interestedInteractable;

    public bool InCombat => enemies.Count > 0;
    private float _healthPercent => (float)maxHealth / health;
    public void SetGoopTime(float time) => GoopTime = Mathf.Max(GoopTime, time);
    public bool FullHealth() => health == maxHealth;

    private void Awake()
    {
        i = this;
        animator = GetComponent<PAnimator>();
    }

    private void Start()
    {
        Sounds = GetComponentInChildren<PSound>();
        health = maxHealth;
        powerBall.SetActive(false);
        Sounds.Get(PSoundKey.HEARTBEAT).PlaySilent();
    }

    private void Update()
    {
        UpdateMeleeEnemies();

        if (Input.GetKeyDown(KeyCode.K)) GameManager.i.RestartScene();

        if (healCooldown <= 0 && health < maxHealth) {
            StartCoroutine(Heal());
            Sounds.Get(PSoundKey.HEALTH_REGEN).Play();
        }
        healCooldown -= Time.deltaTime;

        if (interestedSpeaker == null) GlobalUI.i.Do(UIAction.HIDE_PROMPT, startTalkingPrompt);
        if (interestedSpeaker != null && Vector3.Distance(transform.position, interestedSpeaker.transform.position) > speakerEndDist) {
            interestedSpeaker = null;
            GlobalUI.i.Do(UIAction.HIDE_PROMPT);
        }

        if (GoopTime > 0) {
            GoopTime -= Time.deltaTime;
            goopTickCooldown -= Time.deltaTime;
            if (goopTickCooldown <= 0) {
                ChangeHealth(-goopDmg);
                goopTickCooldown = goopTick;
            }
        }

        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i] == null) enemies.RemoveAt(i);
        }
        if (enemies.Count == 0) {
            if (GetComponent<PMovement>().running) GetComponent<PFighting>().PutAwaySpear();
        }
    }

    public void PowerUp()
    {
        poweredUp = true;
        powerBall.SetActive(true);
    }

    public bool CheckMelee(BaseEnemy enemy, int priority)
    {
        return currentMeleeEnemies.Contains(enemy);
    }

    public void Notify(BaseEnemy enemy, bool agro)
    {
        bool wasFighting = InCombat;
        if (agro) {
            if (!enemies.Contains(enemy)) enemies.Add(enemy);
            if (!wasFighting) OnStartCombat.Invoke();
        }
        else {
            currentMeleeEnemies.Remove(enemy);
            enemies.Remove(enemy);
            if (!InCombat) {
                OnEndCombat.Invoke();
                Sounds.Get(PSoundKey.END_COMBAT).Play();
            }
        }
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
        int deaths = PlayerPrefs.GetInt("deaths", 0);
        PlayerPrefs.SetInt("deaths", deaths + 1);

        GetComponent<PMovement>().enabled = true;
        GetComponent<PFighting>().enabled = fightingEnabled;
    }

    public void RemoveInteractable(Gate interactable)
    {
        if (interestedInteractable == interactable) {
            interestedInteractable = null;
            GlobalUI.i.Do(UIAction.HIDE_PROMPT, interactable.prompt);
        }
    }

    public void AddInteractable(Gate interactable)
    {
        interestedInteractable = interactable;
        GlobalUI.i.Do(UIAction.DISPLAY_PROMPT, interactable.prompt);
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
        GlobalUI.i.Do(UIAction.HIDE_PROMPT);
    }

    public void SpeakerShowInterest(Speaker speaker)
    {
        interestedSpeaker = speaker;
        GlobalUI.i.Do(UIAction.DISPLAY_PROMPT, startTalkingPrompt);
    }

    void StartConversation()
    {
        if (interestedSpeaker == null) return;

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

            GlobalUI.i.Do(UIAction.START_CONVERSATION, interestedSpeaker.characterName);
            GlobalUI.i.Do(UIAction.DISPLAY_LINE, currentLine);

            currentLine = null;
            displayCurrentLine = null;
        }
    }

    private IEnumerator _DisplayCurrentLine()
    {
        partialLine = "";
        partialLineIndex = 0;
        while (currentLine != null) {
            partialLine += currentLine[partialLineIndex];
            partialLineIndex += 1;
            if (partialLineIndex >= currentLine.Length) currentLine = null;

            GlobalUI.i.Do(UIAction.DISPLAY_LINE, partialLine);
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
        GlobalUI.i.Do(UIAction.END_CONVERSATION);
    }

    public void EnterCombat()
    {
        GetComponent<PFighting>().DrawSpear();
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
        //return inputList.OrderBy(x => x.)

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



    public void ChangeHealth(int delta) {
        health = Mathf.Min(health + delta, maxHealth);
        if (health <= 0) Die();

        OnHealthChange.Invoke(_healthPercent);
        if (delta < 0) DoDamageEffects();

        Sounds.Get(PSoundKey.HEARTBEAT).SetPercentVolume(_heartbeatCurve.Evaluate(_healthPercent));
    }

    private void DoDamageEffects()
    {
        healCooldown = healWaitTime;
        StopAllCoroutines();

        if (health > 0) Sounds.Get(PSoundKey.HURT).Play();
        CameraShake.i.Shake();
    }

    void Die() {
        if (dead) return;
        dead = true;

        AchievementController.i.Unlock("FIRST_DEATH");
        AudioManager.i.PauseNonMusic();
        CameraState.i.GetComponent<MusicPlayer>().FadeOutCurrent(0.5f);
        Sounds.Get(PSoundKey.DEATH).Play();
        OnDie.Invoke();
        
    }


    private void OnCollisionEnter(Collision collision)
    {
        /*if (collision.gameObject.tag == "StoneSurface")
            pSound.SetSoftSurface(false);
        else
            pSound.SetSoftSurface(true);*/
    }

}
