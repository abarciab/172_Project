using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationHolder : MonoBehaviour
{
    public static ConversationHolder i;
    private void Awake() { i = this; }

    public List<Conversation> conversations = new List<Conversation>();

    private void Start()
    {
        for (int i = 0; i < conversations.Count; i++) {
            conversations[i] = Instantiate(conversations[i]);
            conversations[i].Init();
        }
    }

    public string GetNextLine(Character speaker)
    {
        foreach (var c in conversations) if (c.speaker == speaker) return c.nextLine;
        return "";
    }
}
