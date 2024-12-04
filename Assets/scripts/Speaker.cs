using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConversationData
{
    [HideInInspector] public string Name;
    [DisplayInspector] public Conversation Convo;
    public bool Enabled;
}

public class Speaker : MonoBehaviour
{
    public string Name;
    [SerializeField] private List<ConversationData> _conversations = new List<ConversationData>();
    [SerializeField] private Sound _bark;
    [SerializeField] Vector2 _barkCooldownRange = new Vector2(2, 5);
    [SerializeField] private Collider _trigger;
    [SerializeField] private float _rotateSnappiness = 10;

    [Header("Offsets")]
    [SerializeField] private Vector3 _cameraAimOffset;

    private float _barkCooldown;
    private bool _talking;
    private bool _hasConversation;
    private bool _interested;

    private List<string> _currentLines = new List<string>();
    private int _currentIndex;
    private Transform _player;

    public string CurrentLine => _currentLines[_currentIndex];
    private Vector3 _camAimPoint => transform.TransformPoint(_cameraAimOffset);

    private void OnValidate()
    {
        foreach (var c in _conversations) if (c.Convo) c.Name = c.Convo.name;
    }

    private void Start()
    {
        _bark = Instantiate(_bark);

        _hasConversation = GetCurrentConversation() != null;
        _trigger.enabled = _hasConversation;
        _player = Player.i.transform;
    }

    private void Update()
    {
        if (!_hasConversation) return;

        if (_interested) FacePlayer();
        Bark();
    }

    private void FacePlayer()
    {
        var current = transform.rotation;
        transform.LookAt(_player.position);
        var euler = transform.localEulerAngles;
        euler.x = euler.z = 0;
        transform.localEulerAngles = euler;
        transform.rotation = Quaternion.Lerp(current, transform.rotation, _rotateSnappiness * Time.deltaTime);
    }

    private void Bark()
    { 
        _barkCooldown -= Time.deltaTime;
        if (_barkCooldown > 0) return;

        _barkCooldown = Random.Range(_barkCooldownRange.x, _barkCooldownRange.y);
        _bark.Play(restart: false);
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player) {
            player.ShowInterest(this);
            _interested = true; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_talking) return;

        var player = other.GetComponent<Player>();
        if (player) {
            player.StopInterest(this);
            _interested = false;
        }
    }

    public void StartConversation()
    {
        GameManager.i.AddNewSpeaker(Name); //for an achievemnet, refactor later
        GameManager.i.Camera.SetTarget(_camAimPoint);

        var convoData = GetCurrentConversation();
        _currentIndex = -1;
        _currentLines = convoData.Convo.Lines;
        _talking = true;
    }

    public string GetNextLine()
    {
        _currentIndex += 1;

        if (_currentIndex >= _currentLines.Count) return null;
        else return _currentLines[_currentIndex];
    }

    ConversationData GetCurrentConversation()
    {
        foreach (var c in _conversations) if (c.Enabled) return c;
        return null;
    }

    public void EndConversation()
    {
        _talking = false;
        _interested = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_camAimPoint, 0.05f);
    }
}