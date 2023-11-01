using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] float waitTime = 2;

    private void Start()
    {
        GameManager.i.AddCheckPoint(transform, ID);
    }

    private void Update()
    {
        waitTime -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (waitTime > 0) return;

        var player = other.GetComponent<Player>();
        if (player) PlayerPrefs.SetInt("autoCheckpoint", ID);
        if (player && FactManager.i.autoSave) PlayerPrefs.SetInt("checkpoint", ID);
    }
}
