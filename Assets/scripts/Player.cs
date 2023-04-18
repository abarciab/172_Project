using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player i;
    [HideInInspector] public PAnimator animator;
    [HideInInspector] public Vector3 speed3D;
    [HideInInspector] public float forwardSpeed;
    public List<EnemyMovement> enemies = new List<EnemyMovement>();
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
        print("Show interest: " + speaker.characterName);
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

    private void Update() {
        if (healCooldown <= 0 && health < maxHealth) StartCoroutine(Heal());
        healCooldown -= Time.deltaTime;

        if (interestedSpeaker == null) GlobalUI.i.HidePrompt(startTalkingPrompt);
        if (interestedSpeaker != null && Vector3.Distance(transform.position, interestedSpeaker.transform.position) > speakerEndDist) {
            interestedSpeaker = null;
            GlobalUI.i.HidePrompt();
        }

        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i] == null) enemies.RemoveAt(i);
        }
        if (enemies.Count == 0) {
            CameraState.i.StopLockOn();
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
        GetComponent<PFighting>().DrawWeapon();
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
        GetComponent<PFighting>().Inturrupt(1);
    }

    void Die() {
        health = maxHealth;
    }

    private void Awake()
    {
        i = this;
        animator = GetComponent<PAnimator>();
    }

}
