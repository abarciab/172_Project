using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("textBoxes")]
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _promptText;

    [Header("Sounds")]
    [SerializeField] private Sound _turnPageSound;
    [SerializeField] private Sound _endConvoSound;

    [Header("Misc")]
    [SerializeField] private GameObject _textBoxParent;
    [SerializeField] private float _letterDisplayTime = 0.05f;

    private bool _talking;
    private string _currentSpeaker;
    private string _currentLine;
    private bool _animating;

    private void Start()
    {
        _textBoxParent.SetActive(false);

        _turnPageSound = Instantiate(_turnPageSound);
        _endConvoSound = Instantiate(_endConvoSound);

        _promptText.text = "";

        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (type == UIAction.START_CONVERSATION && parameter is string speakerName) StartConversation(speakerName);
        else if (type == UIAction.ANIMATE_LINE && parameter is string line) AnimateLine(line);
        else if (type == UIAction.FINISH_LINE_ANIMATION) FinishAnimation();
        else if (type == UIAction.END_CONVERSATION) EndConversation();
        else if (type == UIAction.DISPLAY_PROMPT && parameter is string displayPrompt) DisplayPrompt(displayPrompt);

        if (type == UIAction.HIDE_PROMPT && parameter is string hidePrompt) HidePrompt(hidePrompt);
        else if (type == UIAction.HIDE_PROMPT) HidePrompt();
    }

    private void DisplayPrompt(string prompt)
    {
        StopAllCoroutines();
        _promptText.text = prompt;
    }

    private void HidePrompt(string prompt)
    {
        if (string.Equals(prompt, _promptText.text)) HidePrompt();
    }

    private void HidePrompt()
    {
        _promptText.text = "";
    }

    private void StartConversation(string speakerName)
    {
        _currentSpeaker = speakerName;
        _textBoxParent.SetActive(true);
        _nameText.text = _currentSpeaker;
        _mainText.text = "";
    }

    private async void AnimateLine(string line)
    {
        _turnPageSound.Play();
        _mainText.text = "";
        _currentLine = line;

        _animating = true;
        foreach (var letter in line) {
            if (!_animating) return;
            _mainText.text += letter;
            await Task.Delay(Mathf.RoundToInt(_letterDisplayTime * 1000));
        }
        GlobalUI.i.TriggerOnResult(UIResult.COMPLETE_LINE_ANIMATE);
        _animating = false;
    } 

    private void FinishAnimation()
    {
        _animating = false;
        _mainText.text = _currentLine;
    }

    private void EndConversation()
    {
        _textBoxParent.SetActive(false);
        _talking = false;
        _endConvoSound.Play();
    }
}
