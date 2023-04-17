using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversations")]
public class Conversation : ScriptableObject
{
    public List<string> lines = new List<string>();
    public string nextLine { get { return GetNextLine(); } }
    public int step = -1;
    public Fact endConvoFact;

    string GetNextLine()
    {
        step += 1;
        if (lines.Count > step) return lines[step];
        
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
