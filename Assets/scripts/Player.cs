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
    [SerializeField] int Health;

    [Header("Prompts")]
    [SerializeField] string startTalkingPrompt = "Press E to talk";

    public enum TESTENUM { hidden, option1, option2}
    
    public TESTENUM showEnum;

    public bool SHOW;
    [ConditionalHide(nameof(SHOW))]
    public float field1;

    Speaker interestedSpeaker;

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

    public void StartConversation()
    {
        if (interestedSpeaker == null) return;

        GetComponent<PMovement>().StopMovement();

        var nextLine = ConversationHolder.i.GetNextLine(interestedSpeaker.speaker);
        if (string.Equals("END", nextLine)) EndConversation();
        else GlobalUI.i.DisplayLine(interestedSpeaker.speaker.ToString(), nextLine);
    }

    public void EndConversation()
    {
        GetComponent<PMovement>().ResumeMovement();
        interestedSpeaker = null;
        GlobalUI.i.EndConversation();
    }

    private void Update() {
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
        Health = maxHealth;
    }

    public void ChangeHealth(int delta) {
        Health = Mathf.Min(Health + delta, maxHealth);
        if (Health <= 0) Die();
        GlobalUI.i.HpBar.value = (float)Health / maxHealth;

        if (delta > 0) return;
        
        GlobalUI.i.FlashRed();
        AudioManager.instance.PlaySound(2, transform.GetChild(0).gameObject);
        GetComponent<PFighting>().Inturrupt(1);
    }

    void Die() {
        Destroy(gameObject);
    }

    private void Awake()
    {
        i = this;
        animator = GetComponent<PAnimator>();
    }

}
