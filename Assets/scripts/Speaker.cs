using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
    [System.Serializable]
    public class ConversationData
    {
        [HideInInspector] public string name;
        public Conversation convo;
        public bool enabled;
        public int priority;
        public Fact deleteWhenTrue;
        public Fact EnableWhenTrue;
    }

    private void OnValidate()
    {
        foreach (var c in conversations) c.name = c.convo.name;
    }

    [SerializeField] List<ConversationData> conversations = new List<ConversationData>();
    public string characterName;


    private void Start()
    {
        foreach (var c in conversations) c.convo.Init();
    }

    private void Update()
    {
        for (int i = 0; i < conversations.Count; i++) {
            CheckStatus(conversations[i]);
        }
    }

    void CheckStatus(ConversationData c)
    {
        if (c.deleteWhenTrue != null && FactManager.i.IsPresent(c.deleteWhenTrue)) conversations.Remove(c); 
        if (c.EnableWhenTrue != null && FactManager.i.IsPresent(c.EnableWhenTrue)) c.enabled = true; 
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player && !string.Equals(GetNextLine(true), "END")) player.SpeakerShowInterest(this);
    }

    public string GetNextLine(bool reset = false)
    {
        var convoData = GetCurrentConvo();
        if (convoData == null) return "END";

        string nextLine = convoData.convo.nextLine;
        if (reset) convoData.convo.StepBack();
        return nextLine;
    }

    ConversationData GetCurrentConvo()
    {
        var valid = new List<ConversationData>();
        foreach (var c in conversations) if (c.enabled) valid.Add(c);

        if (valid.Count == 0) return null;

        var highestPriority = valid[0];
        foreach (var c in valid) if (c.priority > highestPriority.priority) highestPriority = c;

        return highestPriority;
    }

    public void EndConversation()
    {
        var convoData = GetCurrentConvo();
        if (convoData == null) return;

        if (convoData.convo.endConvoFact.Count > 0) foreach (var f in convoData.convo.endConvoFact) FactManager.i.AddFact(f);
        if (convoData.convo.endConvoRemoveFact != null) FactManager.i.RemoveFact(convoData.convo.endConvoRemoveFact);
    }
}
