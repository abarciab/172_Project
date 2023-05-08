using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : MonoBehaviour
{
    [SerializeField] Sound getItem;

    [Header("Dialogue")]
    [SerializeField] Sound oldLadySound;

    private void Start()
    {
        getItem = Instantiate(getItem);
    }

    public void Play_GetItem()
    {
        getItem.Play(transform);
    }
}
