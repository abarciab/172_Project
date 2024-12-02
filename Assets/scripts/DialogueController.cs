using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable enable
public class DialogueController : MonoBehaviour
{
    [Header("textBoxes")]
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _promptText;

    [Header("Sounds")]
    [SerializeField] private Sound _turnPageSound;
    [SerializeField] private Sound _endConvoSound;
    public bool Talking { get; private set; }

    private string _currentSpeaker;

    private void Start()
    {
        _mainText.gameObject.SetActive(false);
        _nameText.gameObject.SetActive(false);

        _turnPageSound = Instantiate(_turnPageSound);
        _endConvoSound = Instantiate(_endConvoSound);

        _promptText.text = "";

        GlobalUI.i.OnUpdateUI.AddListener(OnUpdateUI);
    }

    private void OnUpdateUI(UIAction type, object parameter)
    {
        if (type == UIAction.START_CONVERSATION && parameter is string speakerName) StartConversation(speakerName);
        else if (type == UIAction.DISPLAY_LINE && parameter is string line) DisplayLine(line);
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
        _nameText.gameObject.SetActive(true);
        _nameText.text = _currentSpeaker;
    }

    private void DisplayLine(string line)
    {
        _mainText.gameObject.SetActive(true);
        _mainText.text = line;
        _turnPageSound.Play();
    }

    private void EndConversation()
    {
        _nameText.gameObject.SetActive(false);
        _mainText.gameObject.SetActive(false);
        Talking = false;
        _endConvoSound.Play();
    }
}
