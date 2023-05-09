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

    [SerializeField] List<Fact> removeFactOnDeath = new List<Fact>(), addFactOnDeath = new List<Fact>();

    [SerializeField] bool inGroup;
    [SerializeField] int groupID;
    [SerializeField] float HPBarDisplayRange = 30;

    [Header("Sounds")]
    [SerializeField] Sound deathSound;

    Coroutine currentBleed;

    public void Heal(float percentGoal)
    {
        StartCoroutine(_Heal(percentGoal));
    }
    IEnumerator _Heal(float percentGoal)
    {
        float amountToHeal = (maxHealth * percentGoal) - health;
        while (amountToHeal > 0) {
            print("amountToheal: " + amountToHeal);
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
        base.Hit(hit);

        health -= hit.damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0) Die();
        //AudioManager.instance.PlaySound(1, gameObject);

        GetComponent<EnemyMovement>()?.KnockBack(hit.source, hit.KB * KBresist);

        if (currentBleed != null) StopCoroutine(currentBleed);
        if (blood != null) currentBleed = StartCoroutine(Bleed());
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
    }

    private void Update()
    {
        if (!hpBar) return;
        
        hpBar.value = (float) health / maxHealth;
        float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Player.i.transform.position.x, Player.i.transform.position.z));
        if (dist > HPBarDisplayRange) hpBar.gameObject.SetActive(false);
        else {
            hpBar.gameObject.SetActive(true);
            HPBarDisplayRange = Mathf.Infinity;
        }
    }
}
