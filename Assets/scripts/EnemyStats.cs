using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : HitReciever
{
    public int health;
    public int maxHealth;
    [SerializeField] Slider hpBar;
    public bool destroy;
    [SerializeField] float KBresist;
    [SerializeField] GameObject blood;
    [SerializeField] float bloodTime = 1;
    [SerializeField] int healSpeed;
    [SerializeField] float stunTime;
    [HideInInspector] public float stunTimeLeft;
    public bool boss;
    bool invincible;
    [SerializeField] string bossName;

    [SerializeField] List<Fact> removeFactOnDeath = new List<Fact>(), addFactOnDeath = new List<Fact>();

    [SerializeField] bool inGroup;
    [SerializeField] int groupID;
    [SerializeField] float HPBarDisplayRange = 30;

    [Header("Sounds")]
    [SerializeField] Sound deathSound;

    [Header("Materials")]
    [SerializeField] float hitMatTime = 0.1f;
    [SerializeField] List<SkinnedMeshRenderer> body = new List<SkinnedMeshRenderer>();
    [SerializeField] Material normalMat, hitMat, critMat, stunnedMat;

    Coroutine currentBleed;

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
    }

    public override void Hit(HitData hit)
    {
        if (invincible) return;

        base.Hit(hit);

        if (stunTimeLeft > 0) hit.damage *= 2;
        if (hit.stun) stunTimeLeft = stunTime;

        health -= hit.damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0) Die();

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

        if (deathSound) deathSound.Play(transform);

        if (boss) GlobalUI.i.EndBossFight();
    }

    private void Update()
    {
        

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
