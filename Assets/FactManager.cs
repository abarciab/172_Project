using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactManager : MonoBehaviour
{
    public static FactManager i;
    private void Awake() { i = this; }

    [SerializeField] List<Fact> facts = new List<Fact>();

    public void AddFact(Fact fact)
    {
        if (!IsPresent(fact)) facts.Add(fact);
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
