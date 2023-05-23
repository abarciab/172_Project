using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversations")]
public class Conversation : ScriptableObject
{
    [TextArea(3, 5)]
    public string[] lines;
    public string nextLine { get { return GetNextLine(); } }
    [HideInInspector] public int step = -1;

    public List<Fact> endConvoFact = new List<Fact>();
    public Fact endConvoRemoveFact;
    Transform speaker;

    public Sound voiceLines;
    Sound _lines;
    int silentLines = 0;
    Vector3 sourceLocalPosition;

    public void DontPlay(int num)
    {
        silentLines = num;
    }

    string GetNextLine(bool playLine = true)
    {
        step += 1;

        if (lines.Length > step && playLine && _lines != null) 
        {
            _lines.UpdateLocalPosition(sourceLocalPosition);
            if (silentLines > 0) silentLines -= 1;
            else _lines.PlayLine(speaker, step);
        }
        if (lines.Length > step) return lines[step];

        step = -1; 
        return "END"; 
    }

    public void StepBack()
    {
        if (step > -1) step -= 1;
    }

    public void Init(Transform speaker, Vector3 distanceBehind = default)
    {
        this.speaker = speaker;
        this.sourceLocalPosition = distanceBehind;
        step = -1;
        if (voiceLines) _lines = Instantiate(voiceLines);
    }
}
