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
        foreach (var c in conversations) if (c.convo) c.name = c.convo.name;
    }

    [SerializeField] List<ConversationData> conversations = new List<ConversationData>();
    public string characterName;
    [SerializeField] GameObject speechBubble;
    public bool talking;
    public Vector3 cameraOffset;
    public Vector2 speakerDistance;
    [SerializeField] Vector3 SourceLocalPosition;
    [SerializeField] Vector2 callRange = new Vector2(2, 5);
    float callCooldown;
    Sound convoSound;

    private void Start()
    {
        foreach (var c in conversations) c.convo.Init(transform.GetChild(0), SourceLocalPosition);
        callCooldown = Random.Range(callRange.x, callRange.y);
        if (conversations.Count > 0 && conversations[0].convo.voiceLines != null) convoSound = Instantiate(conversations[0].convo.voiceLines);
    }

    private void Update()
    {
        callCooldown -= Time.deltaTime;
        if (!GlobalUI.i.talking && callCooldown <= 0) {
            callCooldown = Random.Range(callRange.x, callRange.y);
            //print("Activating");
            if (convoSound) convoSound.Play(transform);
        }
        if (GlobalUI.i.talking) convoSound.Stop();

        for (int i = 0; i < conversations.Count; i++) {
            CheckStatus(conversations[i]);
        }

        speechBubble.SetActive(false);
        if (!GlobalUI.i.talking) foreach (var c in conversations) if (c.enabled) speechBubble.SetActive(true);
    }

    void CheckStatus(ConversationData c)
    {
        if (c.EnableWhenTrue != null && FactManager.i.IsPresent(c.EnableWhenTrue)) c.enabled = true;
        if (c.deleteWhenTrue != null && FactManager.i.IsPresent(c.deleteWhenTrue)) c.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player && !string.Equals(GetNextLine(true), "END")) player.SpeakerShowInterest(this);
    }

    public string GetNextLine(bool reset = false)
    {
        var convoData = GetCurrentConvo();
        if (convoData == null) {
            StartCoroutine(WaitThenShowInterest());
            return "END";
        }

        if (reset) convoData.convo.DontPlay(1);
        string nextLine = convoData.convo.nextLine;
        if (reset) convoData.convo.StepBack();
        return nextLine;
    }

    IEnumerator WaitThenShowInterest()
    {
        bool valid = false;
        foreach (var c in conversations) if (c.enabled) valid = true;
        if (!valid) yield break;

        yield return new WaitForSeconds(0.6f);
        Player.i.SpeakerShowInterest(this);
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
        talking = false;
        var convoData = GetCurrentConvo();
        if (convoData == null) return;

        if (convoData.convo.endConvoFact.Count > 0) foreach (var f in convoData.convo.endConvoFact) FactManager.i.AddFact(f);
        if (convoData.convo.endConvoRemoveFact != null) FactManager.i.RemoveFact(convoData.convo.endConvoRemoveFact);
    }

    public Vector3 GetStandPosition()
    {
        Transform model = transform.GetChild(0);
        return transform.position + (model.forward * speakerDistance.x) + (model.right * speakerDistance.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + cameraOffset, 0.05f);
        Gizmos.DrawWireSphere(GetStandPosition(), 0.05f);
        Gizmos.DrawWireSphere(transform.position + (transform.forward * SourceLocalPosition.z), 0.05f);
    }
}
