using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryProgressionData
{
    [HideInInspector] public string name;
    public Fact fact;
    public bool state;
    public string nextQuest;
    public int ID;
    public bool playLong;
    public bool customID;
}
