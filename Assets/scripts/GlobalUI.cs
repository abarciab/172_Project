using UnityEngine;
using UnityEngine.Events;

public enum UIAction { START_BOSS_FIGHT, END_BOSS_FIGHT, SET_BOSS_HEALTH, DISPLAY_QUEST_TEXT, DISPLAY_QUEST_TEXT_LONG, END_QUEST, DISPLAY_GOATS, DISPLAY_PLAYER_HP, UPDATE_HP_OVERLAY, UPDATE_GOOP_OVERLAY,
    DISPLAY_IMAGE, UPDATE_CHARGE, BLACK_OUT, START_CONVERSATION, DISPLAY_LINE, END_CONVERSATION, HIDE_PROMPT, DISPLAY_PROMPT, SUNBLAST_READY, DISPLAY_SUNBLAST_COOLDOWN, CATCH_SPEAR, THROW_SPEAR, RECALL_READY, 
    START_COMBAT, END_COMBAT, LOSE_GAME, GO_TO_CREDITS, RESET_GAME, PAUSE, RESUME}

public class GlobalUI : MonoBehaviour
{
    public static GlobalUI i;
    private void Awake() { i = this; }

    [HideInInspector] public UnityEvent<UIAction, object> OnUpdateUI;

    private void Start()
    {
        Player.i.OnHealthChange.AddListener((float percent) => Do(UIAction.DISPLAY_PLAYER_HP, percent));
        Player.i.OnStartCombat.AddListener(() => Do(UIAction.START_COMBAT));
        Player.i.OnEndCombat.AddListener(() => Do(UIAction.END_COMBAT));
        Player.i.OnDie.AddListener(() => Do(UIAction.LOSE_GAME));
    }

    public void Do(UIAction type)
    {
        OnUpdateUI?.Invoke(type, null);
    }

    public void Do<T>(UIAction type, T parameter = default)
    {
        CheckParameter(parameter);
        OnUpdateUI?.Invoke(type, parameter);
    }

    private void CheckParameter<T>(T parameter)
    {
        if (parameter == null || parameter is T) return;
        Debug.LogError("Incorrect parameter passed. expected type: " + typeof(T) + ", got: " + parameter.GetType());
    }
}