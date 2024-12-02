using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fact", menuName = "Fact")]
public class Fact : ScriptableObject
{
    public int skipToStory;
    public bool doNotSave;
    public Fact addWhenSaving;
    public bool Achievement;
    public string AchivementName;
}
