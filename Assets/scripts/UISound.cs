using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : MonoBehaviour
{
    [SerializeField] Sound getItem, turnPage, newQuest, fullChargeSound;

    [Header("Dialogue")]
    [SerializeField] Sound oldLadySound;

    private void Start()
    {
        getItem = Instantiate(getItem);
        turnPage = Instantiate(turnPage);
        newQuest = Instantiate(newQuest);
        fullChargeSound = Instantiate(fullChargeSound);
    }

    public void FullCharge()
    {
        fullChargeSound.Play();
    }

    public void GetItem()
    {
        getItem.Play();
    }

    public void TurnPage()
    {
        turnPage.Play();
    }

    public void NewQuest()
    {
        newQuest.Play(restart:false);
    }
}
