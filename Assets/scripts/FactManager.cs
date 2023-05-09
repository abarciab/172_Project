using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactManager : MonoBehaviour
{
    public static FactManager i;
    private void Awake() { i = this; }

    [SerializeField] List<Fact> facts = new List<Fact>();
    [SerializeField] List<Fact> saveWhenAdded = new List<Fact>();
    [SerializeField] List<Fact> SavePresets = new List<Fact>();
    [SerializeField] bool nextPreset;
    public bool autoSave;

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
            if (autoSave && saveWhenAdded.Contains(fact)) GetComponent<SaveManager>().SaveGame();
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

        if (Input.GetKeyDown(KeyCode.M)) nextPreset = true;

        if (nextPreset) {
            nextPreset = false;
            if (SavePresets.Count <= 0) return;
            AddFact(SavePresets[0]);
            SavePresets.RemoveAt(0);
        }
    }
}
