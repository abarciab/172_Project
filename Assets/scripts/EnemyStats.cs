using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : HitReciever
{
    [Header("Health")]
    public bool invincible;
    public int health;
    public int maxHealth;
    [SerializeField] int healSpeed;
    [SerializeField] Slider hpBar;
    [SerializeField] float HPBarDisplayRange = 30;

    [Header("Stun")]
    [HideInInspector] public float stunTimeLeft;
    [SerializeField] float stunTime;

    [Header("Blood Effect")]
    [SerializeField] GameObject blood;
    [SerializeField] float bloodTime = 1;

    [Header("Group")]
    public bool inGroup;
    [SerializeField] int groupID;

    [Header("On Death")]
    public bool destroy;
    [SerializeField] GameObject dropWhenDie;
    [SerializeField] List<Fact> removeFactOnDeath = new List<Fact>(), addFactOnDeath = new List<Fact>();

    [Header("Boss")]
    public bool boss;
    [SerializeField] string bossName;
    [SerializeField] bool camShakeOnHurt;
    [SerializeField] Vector2 camShakeParams;

    [Header("Sounds")]
    [SerializeField] Sound deathSound;
    [SerializeField] Sound spawnSound, hurtSound, stunnedHurtSound, getStunnedSound;
    [SerializeField] bool playDeathGlobal, playHurtGlobal;

    [Header("Materials")]
    [SerializeField] float hitMatTime = 0.1f;
    [SerializeField] List<SkinnedMeshRenderer> body = new List<SkinnedMeshRenderer>();
    public Material normalMat, hitMat, critMat, stunnedMat;

    [Header("Misc")]
    [SerializeField] float KBresist;

    [Header("Debug")]
    [SerializeField] bool printHits;

    Coroutine currentBleed;

    public void HideBody()
    {
        foreach (var b in body) b.enabled = false;
    }
    public void SetInvincible()
    {
        invincible = true;
    }

    public void SetVincible()
    {
        invincible = false;
    }

    public void Heal(float percentGoal)
    {
        StartCoroutine(_Heal(percentGoal));
    }
    IEnumerator _Heal(float percentGoal)
    {
        float amountToHeal = (maxHealth * percentGoal) - health;
        while (amountToHeal > 0) {
            health += healSpeed;
            amountToHeal -= healSpeed;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public bool dead()
    {
        return health <= 0;
    }

    private void Start()
    {
        if (inGroup) GameManager.i.AddToGroup(gameObject, groupID);
        health = maxHealth;
        if (blood != null) blood.SetActive(false);

        if (deathSound) deathSound = Instantiate(deathSound);
        if (hurtSound) hurtSound = Instantiate(hurtSound);
        if (stunnedHurtSound) stunnedHurtSound = Instantiate(stunnedHurtSound);
        if (getStunnedSound) getStunnedSound = Instantiate(getStunnedSound);
        if (spawnSound) {
            spawnSound = Instantiate(spawnSound);
            StartCoroutine(PlaySpawnSound());
        }
    }

    IEnumerator PlaySpawnSound()
    {
        yield return new WaitForSeconds(0.2f);

        if (gameObject.activeInHierarchy) spawnSound.Play();
    }

    public override void Hit(HitData hit)
    {
        Hit(hit, false);
    }

    public override void Hit(HitData hit, bool willHit = false)
    {
        if (printHits) print("hit: " + hit.damage + ", willHit: " + willHit);

        if (invincible && !willHit) return;
        bool newlyStunned = stunTimeLeft <= 0 && hit.stun;
        base.Hit(hit);

        if (newlyStunned && getStunnedSound) getStunnedSound.Play(transform);
        if (hurtSound && (!stunnedHurtSound || stunTimeLeft <= 0 || newlyStunned)) {
            if (playHurtGlobal) hurtSound.Play();
            else hurtSound.Play();
        }
        if (stunTimeLeft > 0) {
            hit.damage *= 2;
            if (stunnedHurtSound && !newlyStunned) stunnedHurtSound.Play(transform);
        }
        if (hit.stun) stunTimeLeft = stunTime;

        health -= hit.damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0) Die();
        if (camShakeOnHurt) FindObjectOfType<CameraShake>().Shake(camShakeParams.x, camShakeParams.y);

        GetComponent<EnemyMovement>()?.KnockBack(hit.source, hit.KB * KBresist);

        if (currentBleed != null) StopCoroutine(currentBleed);
        if (blood != null) currentBleed = StartCoroutine(Bleed());
        if (body != null) StartCoroutine(ChangeMat(hit.crit ? critMat : hitMat));
    }

    IEnumerator ChangeMat(Material mat)
    {
        foreach (var b in body) b.material = mat;
        yield return new WaitForSeconds(hitMatTime);
        foreach (var b in body) b.material = normalMat;
    }

    IEnumerator Bleed()
    {
        blood.SetActive(true);
        yield return new WaitForSeconds(bloodTime);
        blood.SetActive(false);
    }

    void Die()
    {
        if (hpBar) hpBar.gameObject.SetActive(false);
        HPBarDisplayRange = 0;
        if (inGroup) GameManager.i.removeFromGroup(gameObject, groupID);

        if (removeFactOnDeath.Count > 0) foreach (var f in removeFactOnDeath) FactManager.i.RemoveFact(f);
        if (addFactOnDeath.Count > 0) foreach (var f in addFactOnDeath) FactManager.i.AddFact(f);

        StopAllCoroutines();
        blood.SetActive(false);
        if (destroy) Destroy(gameObject);
        if (hpBar) hpBar.gameObject.SetActive(false);

        if (deathSound) if (playDeathGlobal) deathSound.Play(transform); else deathSound.Play();

        if (boss) GlobalUI.i.EndBossFight();

        if (dropWhenDie != null) Instantiate(dropWhenDie, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        if (Player.i.dead) gameObject.SetActive(false);

        stunTimeLeft -= Time.deltaTime;
        foreach (var m in body) m.material = stunTimeLeft > 0 ? stunnedMat : normalMat;

        if (boss) {
            GlobalUI.i.StartBossFight(bossName, gameObject);
            GlobalUI.i.bossSlider.value = (float) health / maxHealth;
            return;
        }

        if (!hpBar) return;

        hpBar.value = (float) health / maxHealth;
        float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Player.i.transform.position.x, Player.i.transform.position.z));
        if (dist > HPBarDisplayRange) hpBar.gameObject.SetActive(false);
        else {
            hpBar.gameObject.SetActive(true);
            HPBarDisplayRange = Mathf.Infinity;
        }
        if (invincible) hpBar.gameObject.SetActive(false);
    }
}
