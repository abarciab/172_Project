using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollectibleGoat : HitReciever
{
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpResetTime;
    private float jumpCooldown;
    private bool jumping;
    [SerializeField] private int ID;
    [SerializeField] private bool resetSaveData;

    private void Start()
    {
        if (PlayerPrefs.GetInt("goat" + ID, -1) != -1) {
            GameManager.i.FoundGoat(false);
            jumping = true;
        }
    }

    public override void Hit(HitData hit)
    {
        if (jumping) return;
        
        base.Hit(hit);
        jumping = true;
        GameManager.i.FoundGoat();
        PlayerPrefs.SetInt("goat" + ID, 1);
    }

    private void Update()
    {
        if (resetSaveData) {
            GameManager.i.ResetGoatData();
            resetSaveData = false;
            PlayerPrefs.SetInt("goat" + ID, -1);
        }

        jumpCooldown -= Time.deltaTime;
        if (!jumping || jumpCooldown > 0) return;   

        GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce);
        jumpCooldown = jumpResetTime;
    }
}
