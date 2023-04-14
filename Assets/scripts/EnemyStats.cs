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

    private void Start()
    {
        OnHit.AddListener(_Hit);
        health = maxHealth;
    }

    void _Hit()
    {
        int damage = _damage;
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0 && destroy) Destroy(gameObject);
        AudioManager.instance.PlaySound(1, gameObject);

        GetComponent<EnemyMovement>()?.KnockBack(source, KB * KBresist);
    }

    private void Update()
    {
        if (hpBar) hpBar.value = (float) health / maxHealth;
    }
}
