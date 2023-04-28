using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] List<Fact> removeFactOnDeath = new List<Fact>(), addFactOnDeath = new List<Fact>();


    private void Start()
    {
        health = maxHealth;
        if (blood != null) blood.SetActive(false);
    }

    public override void Hit(HitData hit)
    {
        base.Hit(hit);

        health -= hit.damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0) Die();
        AudioManager.instance.PlaySound(1, gameObject);

        GetComponent<EnemyMovement>()?.KnockBack(hit.source, hit.KB * KBresist);

        StopAllCoroutines();
        if (blood != null) StartCoroutine(Bleed());
    }

    IEnumerator Bleed()
    {
        blood.SetActive(true);
        yield return new WaitForSeconds(bloodTime);
        blood.SetActive(false);
    }

    void Die()
    {
        if (removeFactOnDeath.Count > 0) foreach (var f in removeFactOnDeath) FactManager.i.RemoveFact(f);
        if (addFactOnDeath.Count > 0) foreach (var f in addFactOnDeath) FactManager.i.AddFact(f);

        StopAllCoroutines();
        blood.SetActive(false);
        if (destroy) Destroy(gameObject);
        if (hpBar) hpBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (hpBar) hpBar.value = (float) health / maxHealth;
    }
}
