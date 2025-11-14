using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerState { MOVING, SPEAKING}

[RequireComponent(typeof(PMovement), typeof(PDialogue), typeof(PAnimator))]
public class Player : MonoBehaviour
{
    public static Player i;

    [SerializeField] private AnimationCurve _heartbeatCurve;

    [HideInInspector] public UnityEvent<float> OnHealthChange;
    [HideInInspector] public UnityEvent OnStartCombat;
    [HideInInspector] public UnityEvent OnEndCombat;
    [HideInInspector] public UnityEvent OnDie;
    [HideInInspector] public PSound Sounds;

    private PMovement _move;
    private PDialogue _dialogue;

    [HideInInspector] public PAnimator Anim { get; private set; }
    [HideInInspector] public Rigidbody RB { get; private set; }

    //
    //
    //

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

    [SerializeField] private int maxHealth;
    [SerializeField] GameObject powerBall;
    [SerializeField] List<BaseEnemy> enemies = new List<BaseEnemy>();
    [SerializeField] List<BaseEnemy> currentMeleeEnemies = new List<BaseEnemy>();
    [SerializeField] float meleeRatio = 0.5f;
    [SerializeField] float tokenCheckTime = 5;

    [HideInInspector] public float GoopTime { get; private set; }



    private int health;
    private float meleeCheckCooldown;
    private bool fightingEnabled;
    private PlayerState _currentState = PlayerState.MOVING;

    public bool InCombat => enemies.Count > 0;
    private float _healthPercent => (float)maxHealth / health;
    public void SetGoopTime(float time) => GoopTime = Mathf.Max(GoopTime, time);
    public bool FullHealth() => health == maxHealth;
    public bool IsRunning => _move.IsRunning;
    public void SetSpearDamage(int damage) => GetComponent<PFighting>().SetSpearDmg(damage);
    public void ShowInterest(Speaker speaker) => _dialogue.ShowInterest(speaker);
    public void StopInterest(Speaker speaker) => _dialogue.StopInterest(speaker);
    public float ForwardSpeed => _move.ForwardSpeed;

    private void Awake() {
        i = this;
        Anim = GetComponent<PAnimator>();
    }

    private void Start()
    {
        _move = GetComponent<PMovement>();
        _dialogue = GetComponent<PDialogue>();
        Sounds = GetComponentInChildren<PSound>();

        RB = GetComponent<Rigidbody>();

        health = maxHealth;
        powerBall.SetActive(false);
        SwitchState(PlayerState.MOVING);
    }

    private void Update()
    {
        UpdateMeleeEnemies();

        if (healCooldown <= 0 && health < maxHealth) {
            StartCoroutine(Heal());
            Sounds.Get(PSoundKey.HEALTH_REGEN).Play();
        }
        healCooldown -= Time.deltaTime;

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
            if (GetComponent<PMovement>().IsRunning) GetComponent<PFighting>().PutAwaySpear();
        }
    }

    public void SwitchState(PlayerState newState)
    {
        _move.enabled = newState == PlayerState.MOVING;
        _dialogue.enabled = newState == PlayerState.SPEAKING;

        if (newState == PlayerState.SPEAKING) {
            _dialogue.StartConversation();
        }
    }

    public void Interact()
    {
        if (_dialogue.IsTalking) _dialogue.AdvanceConversation();
        else if (_dialogue.HasSpeaker) SwitchState(PlayerState.SPEAKING); 
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
        var sortedList = enemies.OrderBy(x => x.MeleePriority()).ToList();
        for (int i = 0; i < sortedList.Count; i++) {
            if ((float) currentMeleeEnemies.Count / enemies.Count >= meleeRatio) break;
            currentMeleeEnemies.Add(sortedList[i]);
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
        Sounds.Get(PSoundKey.DEATH).Play();
        OnDie.Invoke();
        
    }
}
