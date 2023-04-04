using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : HitReciever
{
    public int health;
    public int maxHealth;
    [SerializeField] Slider hpBar;

    private void Start()
    {
        OnHit.AddListener(_Hit);
        health = maxHealth;
    }

    void _Hit()
    {
        int damage = _damage;
        health -= damage;
        if (health <= 0) Destroy(gameObject);
        AudioManager.instance.PlaySound(1, gameObject);
    }

    private void Update()
    {
        if (hpBar) hpBar.value = (float) health / maxHealth;
    }
}
