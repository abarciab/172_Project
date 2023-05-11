using System.Collections;
using System.Collections.Generic;
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


    public static FactManager i;
    private void Awake() { i = this; }

    [SerializeField] List<Fact> facts = new List<Fact>();
    [SerializeField] List<Fact> autosaveTriggers = new List<Fact>();
    [SerializeField] List<Fact> SavePresets = new List<Fact>();
    bool loadNextPreset;
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
    }

    public void SetFacts(List<Fact> newFacts)
    {
        facts = newFacts;
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
    public void AddFact(Fact fact)
    {
        if (!IsPresent(fact)) {
            facts.Add(fact);
            if (fact.skipToStory > 0) GameManager.i.LoadStory(fact.skipToStory);
            if (autoSave && autosaveTriggers.Contains(fact)) GetComponent<SaveManager>().SaveGame();
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

    private void Update()
    {
        if (!Application.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.M)) loadNextPreset = true;

        if (loadNextPreset) {
            loadNextPreset = false;
            if (SavePresets.Count <= 0) return;
            AddFact(SavePresets[0]);
            SavePresets.RemoveAt(0);
        }
    }
}
