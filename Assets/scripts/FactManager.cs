using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class FactManager : MonoBehaviour
{
    [System.Serializable]
    public class FactRule
    {
        [HideInInspector] public string name;
        public Fact trigger;

        public List<Fact> toAdd = new List<Fact>();
        public List<Fact> toRemove = new List<Fact>();

        public int triggersLeft = 1;
    }

    [System.Serializable]
    public class SaveState
    {
        [HideInInspector] public string name;
        public List<Fact> facts = new List<Fact>();
        public int checkPoint, storyID;
        [HideInInspector] public int saveID;
    }


    public static FactManager i;
    private void Awake() { i = this; }

    [SerializeField] List<Fact> facts = new List<Fact>();
    [SerializeField] List<Fact> autosaveTriggers = new List<Fact>();
    [SerializeField] List<SaveState> saveStates = new List<SaveState>();
    public bool autoSave;

    [Header("Rules")]
    [SerializeField] List<FactRule> rules = new List<FactRule>();

    private void Start()
    {
        FindObjectOfType<ShaderTransitionController>().LoadShaders();
    }

    private void OnValidate()
    {
        foreach (FactRule rule in rules) {
            if (rule.trigger != null)
                rule.name = rule.trigger.name + ": " + (rule.toAdd.Count > 0 ? "+" : (rule.toRemove.Count > 0 ? "-" : "")) +
                    (rule.toAdd.Count > 0 && rule.toAdd[0] != null ? rule.toAdd[0].name : (rule.toRemove.Count > 0 && rule.toRemove[0] != null ? rule.toRemove[0].name: ""));
        }
        for (int i = 0; i < saveStates.Count; i++) {
            saveStates[i].name = "save: " + i;
            saveStates[i].saveID = i;
        }
    }

    public void SetFacts(List<Fact> newFacts)
    {
        facts = new List<Fact>(newFacts);
        FindObjectOfType<ShaderTransitionController>().LoadShaders();
    }

    public List<Fact> GetFacts()
    {
        return facts;
    }

    public int numFacts()
    {
        return facts.Count;
    }
    public void AddFact(Fact fact, bool respectAutoSave = true)
    {
        if (!IsPresent(fact)) {
            facts.Add(fact);
            if (fact.skipToStory > 0 && fact.skipToStory > GameManager.i.GetID()) GameManager.i.LoadStory(fact.skipToStory);
            if (respectAutoSave && autoSave && autosaveTriggers.Contains(fact)) GetComponent<SaveManager>().SaveGame();
        }
    }

    public void RemoveFact(Fact fact)
    {
        facts.Remove(fact);
    }

    public bool IsPresent(Fact fact)
    {
        return (facts.Contains(fact));
    }

    public void LoadSaveState(int stateID)
    {
        var state = GetState(stateID);
        if (state == null) return;

        SetFacts(state.facts);
        GameManager.i.LoadStory(state.storyID);
        PlayerPrefs.SetInt("checkpoint", state.checkPoint);
        PlayerPrefs.SetInt("autoCheckpoint", state.checkPoint);
        while (true) if (!CheckRules()) break;
        GetComponent<SaveManager>().SaveGame();
        GameManager.i.RestartScene();
    }

    SaveState GetState(int ID)
    {
        foreach (var s in saveStates) if (s.saveID == ID) return s;
        return null;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        CheckRules();
    }

    bool CheckRules()
    {
        int factCount = facts.Count;
        foreach (var r in rules) CheckRule(r);
        return facts.Count != factCount;
    }

    void CheckRule(FactRule rule)
    {
        if (rule.triggersLeft == 0 || !IsPresent(rule.trigger)) return;

        foreach (var f in rule.toAdd) AddFact(f, false);
        foreach (var f in rule.toRemove) RemoveFact(f);
        rule.triggersLeft -= 1;
    }
}
