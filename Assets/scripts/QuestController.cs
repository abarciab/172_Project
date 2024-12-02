using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private TextMeshProUGUI _currentQuest;
    [SerializeField] private GameObject _newQuestFlash;

    private bool _hidingQuestAnim;
    [SerializeField] private Sound _newQuestSound;
    [SerializeField] private Sound _newQuestLongSound;


    private void OnEnable()
    {
        InitializeSounds();
    }

    private void InitializeSounds()
    {
        if (_newQuestSound.Instantialized) return;
        _newQuestSound = Instantiate(_newQuestSound);
        _newQuestLongSound = Instantiate(_newQuestLongSound);
    }

    public string GetCurrentText()
    {
        return _currentQuest.text;
    }

    public void UpdateQuestTextLong(string text)
    {
        UpdateQuestText(text, true);
    }

    public void UpdateQuestText(string text, bool playLong = false)
    {
        gameObject.SetActive(true);
        _currentQuest.text = text;
        _newQuestFlash.SetActive(true);
        if (!string.IsNullOrEmpty(text)) {
            if (playLong) _newQuestSound.Play();
            else _newQuestLongSound.Play();
        }
    }

    public void EndQuest()
    {
        gameObject.SetActive(false);
    }
}
