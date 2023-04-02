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
    [SerializeField] float lockOnDist = 15;

    [SerializeField] int maxHealth;
    [SerializeField] int Health;

    private void Update() {
        if (enemies.Count == 0) {
            CameraState.i.StopLockOn();
            return;
        }

        if (closestEnemy == null) closestEnemy = enemies[0];
        foreach (var e in enemies) {
            if (Vector3.Distance(transform.position, e.transform.position) < Vector3.Distance(transform.position, closestEnemy.transform.position)) closestEnemy = e;
        }
        if (Vector3.Distance(transform.position, closestEnemy.transform.position) > lockOnDist) {
            CameraState.i.StopLockOn();
            return;
        }

        CameraState.i.LockOnEnemy(closestEnemy.gameObject, closestEnemy.centerOffset);
    }

    private void Start() {
        Health = maxHealth;
    }

    public void ChangeHealth(int delta) {
        Health = Mathf.Min(Health + delta, maxHealth);
        if (Health <= 0) Die();
        GlobalUI.i.HpBar.value = (float)Health / maxHealth;
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
