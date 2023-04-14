using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character { none, engineer_1}

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversations")]
public class Conversation : ScriptableObject
{
    public Character speaker;
    public List<string> lines = new List<string>();
    public string nextLine { get { return GetNextLine(); } }

    public int step = -1;

    string GetNextLine()
    {
        step += 1;
        if (lines.Count > step) return lines[step];
        
        step = -1; 
        return "END"; 
    }

    public void Init()
    {
        step = -1;
    }
}
