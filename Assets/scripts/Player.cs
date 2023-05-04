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
   
    EnemyStats closestEnemy;
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

    [SerializeField] List<BaseEnemy> enemies = new List<BaseEnemy>();
    [SerializeField] List<BaseEnemy> currentMeleeEnemies = new List<BaseEnemy>();
    [SerializeField] float meleeRatio = 0.5f, tokenCheckTime = 5;
    float meleeCheckCooldown;

    //closer enemies should melee
    //higher priority melee enemies should melee
    //don't have too many ranged or melee enemies

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

    public void FreezePlayer()
    {
        GetComponent<PMovement>().enabled = false;
        GetComponent<PFighting>().enabled = false;
        FindObjectOfType<CameraController>().enabled = false;
    }

    public void UnfreezePlayer()
    {
        GetComponent<PMovement>().enabled = true;
        GetComponent<PFighting>().enabled = true;
        FindObjectOfType<CameraController>().enabled = true;
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
        UpdateMeleeEnemies();

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

    void UpdateMeleeEnemies()
    {
        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i] == null) enemies.RemoveAt(i);
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

        while (outputList.Count < inputList.Count) {
            int currentPriority = 0;
            for (int i = 0; i < inputList.Count; i++) {
                var enemy = inputList[i];
                if (outputList.Contains(enemy)) continue;
                if (enemy.MeleePriority() == currentPriority) {
                    int index = 0;
                    float dist = enemy.Dist();


                    while (enemies.Count > index + 1 && enemies[index].Dist() > dist && enemies[index + 1].MeleePriority() == enemy.MeleePriority()) index += 1;
                    outputList.Insert(index, enemy);
                }
            }
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
