using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player i;
    [HideInInspector] public PAnimator animator;
    [HideInInspector] public Vector3 speed3D;
    [HideInInspector] public float forwardSpeed;
   
    EnemyMovement closestEnemy;
    [SerializeField] float lockOnDist = 15, speakerEndDist;

    [SerializeField] int maxHealth;
    [SerializeField] int health;

    [Header("Prompts")]
    [SerializeField] string startTalkingPrompt = "Press E to talk";

    [Header("sfx")]
    [SerializeField] AudioSource hurtSource;

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

    [HideInInspector] public List<EnemyMovement> enemies = new List<EnemyMovement>();
    List<EnemyMovement> currentMeleeEnemies = new List<EnemyMovement>();
    public int meleeTokens = 3;

    public void RemoveInteractable(Gate interactable)
    {
        if (interestedInteractable == interactable) {
            interestedInteractable = null;
            GlobalUI.i.HidePrompt(interactable.prompt);
        }
    }

    public bool TryToMelee(EnemyMovement move)
    {
        if (currentMeleeEnemies.Contains(move)) return true;
        if (meleeTokens == 0) return false;

        currentMeleeEnemies.Add(move);
        meleeTokens -= 1;
        return true;
    }

    public void EndMelee(EnemyMovement move)
    {
        if (!currentMeleeEnemies.Contains(move)) return;

        currentMeleeEnemies.Remove(move);
        meleeTokens += 1;
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

        GetComponent<PMovement>().StopMovement();

        var nextLine = interestedSpeaker.GetNextLine();
        if (string.Equals("END", nextLine)) EndConversation();
        else GlobalUI.i.DisplayLine(interestedSpeaker.characterName, nextLine);
    }

    public void EndConversation()
    {
        interestedSpeaker.EndConversation();
        GetComponent<PMovement>().ResumeMovement();
        interestedSpeaker = null;
        GlobalUI.i.EndConversation();
    }

    public void EnterCombat()
    {
        GetComponent<PFighting>().DrawSpear();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) GameManager.i.RestartScene();
        if (Input.GetKeyDown(KeyCode.L)) GameManager.i.GetComponent<SaveManager>().SaveGame();
        if (Input.GetKeyDown(KeyCode.O)) GameManager.i.GetComponent<SaveManager>().ResetGame();

        if (healCooldown <= 0 && health < maxHealth) StartCoroutine(Heal());
        healCooldown -= Time.deltaTime;

        if (interestedSpeaker == null) GlobalUI.i.HidePrompt(startTalkingPrompt);
        if (interestedSpeaker != null && Vector3.Distance(transform.position, interestedSpeaker.transform.position) > speakerEndDist) {
            interestedSpeaker = null;
            GlobalUI.i.HidePrompt();
        }

        if (goopTime > 0) {
            goopTime -= Time.deltaTime;
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
            CameraState.i.StopLockOn();
            if (GetComponent<PMovement>().running) GetComponent<PFighting>().PutAwaySpear();
            return;
        }
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

    public void ToggleLockOn()
    {
        if (CameraState.i.GetLockedEnemy() != null) CameraState.i.StopLockOn();
        else LockOn();
    }

    void LockOn()
    {
        if (enemies.Count == 0) return;

        if (closestEnemy == null && enemies[0] != null) closestEnemy = enemies[0];

        foreach (var e in enemies) {
            if (Vector3.Distance(transform.position, e.transform.position) < Vector3.Distance(transform.position, closestEnemy.transform.position)) closestEnemy = e;
        }
        if (Vector3.Distance(transform.position, closestEnemy.transform.position) > lockOnDist) {
            CameraState.i.StopLockOn();
            return;
        }

        CameraState.i.LockOnEnemy(closestEnemy.gameObject, closestEnemy.centerOffset);
        //GetComponent<PFighting>().DrawWeapon();
    }

    private void Start() {
        health = maxHealth;
    }

    public void ChangeHealth(int delta) {
        health = Mathf.Min(health + delta, maxHealth);
        if (health <= 0) Die();
        GlobalUI.i.HpBar.value = (float)health / maxHealth;
        GlobalUI.i.UpdateDmgIndicator((float)health / maxHealth);

        if (delta > 0) return;

        healCooldown = healWaitTime;
        StopAllCoroutines();
        
        AudioManager.instance.PlaySound(2, hurtSource);
        //GetComponent<PFighting>().Inturrupt(1);
    }

    void Die() {
        GameManager.i.RestartScene();
    }

    private void Awake()
    {
        i = this;
        animator = GetComponent<PAnimator>();
    }

}
