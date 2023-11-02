using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : MonoBehaviour
{
    [SerializeField] Sound getItem, turnPage, newQuest, fullChargeSound, newQuestLong, heartBeat, endConvo, endConvo2, endCombat, snakeHiss;
    [SerializeField] AnimationCurve heartbeatCurve;

    [Header("Buttons")]
    [SerializeField] Sound buttonClick;
    [SerializeField] Sound menuClose;

    private void Start()
    {
        getItem = Instantiate(getItem);
        turnPage = Instantiate(turnPage);
        newQuest = Instantiate(newQuest);
        newQuestLong = Instantiate(newQuestLong);
        fullChargeSound = Instantiate(fullChargeSound);
        buttonClick = Instantiate(buttonClick);
        menuClose = Instantiate(menuClose);
        heartBeat = Instantiate(heartBeat);
        endConvo = Instantiate(endConvo);
        endConvo2 = Instantiate(endConvo2);
        endCombat = Instantiate(endCombat);
        snakeHiss = Instantiate(snakeHiss);

        heartBeat.PlaySilent();
    }

    public void EndCombat()
    {
        endCombat.Play();
    }

    public void Heartbeat(float healthPercent)
    {
        heartBeat.PercentVolume(heartbeatCurve.Evaluate(healthPercent));
    }

    public void SnakeHiss() {
        snakeHiss.Play();
    }

    public void ClickButton()
    {
        buttonClick.Play();
    }

    public void MenuClose()
    {
        menuClose.Play();
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

    public void NewQuest(bool playLong)
    {
        if (playLong)newQuestLong.Play(restart:false);
        else newQuest.Play(restart:false);
    }

    public void EndConversation()
    {
        endConvo.Play();
        endConvo2.Play();
    }

}
