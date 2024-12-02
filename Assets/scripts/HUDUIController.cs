using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using MyBox;
using System;
#nullable enable

public class HUDUIController : MonoBehaviour
{
    [Header("Sub scripts")]
    [SerializeField] private BossBarController _bossBar;
    [SerializeField] private QuestController _quest;
    [SerializeField] private AbilityPanelController _abilities;
    [SerializeField] private GoatPopup _goat;
    [SerializeField] private ChargeMeter _charge;
    [SerializeField] private MarkerTracker _compass;

    [Header("misc")]
    [SerializeField] private GameObject _crossHair;

    private void Start()
    {
        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (_bossBar) {
            if (type == UIAction.START_BOSS_FIGHT && parameter is GameObject bossObj) _bossBar.StartBossFight(bossObj);
            else if (type == UIAction.END_BOSS_FIGHT) _bossBar.EndBossFight();
            else if (type == UIAction.SET_BOSS_HEALTH && parameter is float bossHp) _bossBar.SetSliderValue(bossHp);
        }
        if (_quest) {
            string? questString = parameter as string;
            if (type == UIAction.DISPLAY_QUEST_TEXT) _quest.UpdateQuestText(questString);
            else if (type == UIAction.DISPLAY_QUEST_TEXT_LONG) _quest.UpdateQuestText(questString);
            else if (type == UIAction.END_QUEST) _quest.EndQuest();
        }
        if (_goat) {
            if (type == UIAction.DISPLAY_GOATS && parameter is int numGoats) _goat.Show(numGoats);
        }
        if (_abilities) {
            if (type == UIAction.DISPLAY_PLAYER_HP && parameter is float playerHP) _abilities.DisplayPlayerHP(playerHP);
        }
        if (_charge) {
            if (type == UIAction.UPDATE_CHARGE && parameter is float chargePercent) _charge.DisplaySpearCharge(chargePercent); 
        }
        if (_compass) {
            if (type == UIAction.START_COMBAT) _compass.Hide();
            if (type == UIAction.END_COMBAT) _compass.Show();
        }
        if (_crossHair) {
            if (type == UIAction.START_CONVERSATION) _crossHair.SetActive(true);
            if (type == UIAction.END_CONVERSATION) _crossHair.SetActive(false);
        }
    }

    [Space(20)]


    [Header("Game over")]
    [SerializeField] private GameObject _gameOverScreen;
    [SerializeField] private float _gameOverSoundTime;


    [Header("Misc")]
    public GameObject TutorialSkip;


    [SerializeField] private GameObject _title;

    [SerializeField] private GameObject _bottomLeft;

    [SerializeField] private Fade _fade;
}
