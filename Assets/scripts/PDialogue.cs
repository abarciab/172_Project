using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PDialogue : MonoBehaviour
{
    [SerializeField] string _startTalkingPrompt = "Press E to talk";
    [SerializeField] float _speakerEndDist;
    [SerializeField] float _dialogueDisplaySpeed = 0.1F;

    private Player _p;
    private Speaker _interestedSpeaker;
    private bool _readyForNextLine;

    public bool HasSpeaker => _interestedSpeaker != null;
    public bool IsTalking { get; private set; } 

    private void Start()
    {
        _p = GetComponent<Player>();
        GlobalUI.i.OnResult.AddListener(OnUIResult);
    }

    private void OnUIResult(UIResult type)
    {
        if (type == UIResult.COMPLETE_LINE_ANIMATE) _readyForNextLine = true;
    }

    public void StopInterest(Speaker speaker)
    {
        if (_interestedSpeaker != speaker || _interestedSpeaker == null) return;
        _interestedSpeaker = null;
        GlobalUI.i.Do(UIAction.HIDE_PROMPT, _startTalkingPrompt);
    }

    public void ShowInterest(Speaker speaker)
    {
        _interestedSpeaker = speaker;
        GlobalUI.i.Do(UIAction.DISPLAY_PROMPT, _startTalkingPrompt);
    }

    public void StartConversation()
    {
        IsTalking = true;
        _interestedSpeaker.StartConversation();
        GlobalUI.i.Do(UIAction.START_CONVERSATION, _interestedSpeaker.Name);
        GlobalUI.i.Do(UIAction.HIDE_PROMPT);
        _readyForNextLine = true;

        AdvanceConversation();
    }

    public void AdvanceConversation()
    {
        if (_readyForNextLine) ShowNextLine();
        else {
            _readyForNextLine = true;
            GlobalUI.i.Do(UIAction.FINISH_LINE_ANIMATION);
        }
    }

    private void ShowNextLine()
    {
        var nextLine = _interestedSpeaker.GetNextLine();
        if (nextLine != null) {
            GlobalUI.i.Do(UIAction.ANIMATE_LINE, nextLine);
            _readyForNextLine = false;
        }
        else {
            EndConversation();
        }
    }

    public void EndConversation()
    {
        IsTalking = false;
        _interestedSpeaker.EndConversation();
        GetComponent<PMovement>().ResumeMovement();
        _interestedSpeaker = null;
        GlobalUI.i.Do(UIAction.END_CONVERSATION);

        GameManager.i.Camera.ClearTarget();

        _p.SwitchState(PlayerState.MOVING);
    }
}
