using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversations")]
public class Conversation : ScriptableObject
{
    [TextArea(3, 5)]
    public string[] lines;
    public string nextLine { get { return GetNextLine(); } }
    public int step = -1;

    public List<Fact> endConvoFact = new List<Fact>();
    public Fact endConvoRemoveFact;

    string GetNextLine()
    {
        step += 1;
        if (lines.Length > step) return lines[step];
        
        step = -1; 
        return "END"; 
    }

    public void StepBack()
    {
        if (step > -1) step -= 1;
    }

    public void Init()
    {
        step = -1;
    }
}
