using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactManager : MonoBehaviour
{
    public static FactManager i;
    private void Awake() { i = this; }

    [SerializeField] List<Fact> facts = new List<Fact>();
    [SerializeField] List<Fact> saveWhenAdded = new List<Fact>();
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

    private void Start()
    {
        //foreach (var f in facts) print(f.name);
    }
}
