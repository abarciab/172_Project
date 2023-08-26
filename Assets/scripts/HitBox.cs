using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering.UI;

public class HitBox : MonoBehaviour
{
    public bool checking;
    [HideInInspector] public List<HitReciever> targets = new List<HitReciever>();
    [SerializeField] List<HitBox> linkedBoxes = new List<HitBox>();
    [SerializeField] string ignoreTag;
    [SerializeField] bool printHits, playSoundOnHit;
    [SerializeField] Sound hitSound, altSound;
    [SerializeField] GameObject SpawnOnHit;

    [Header("Blockable")]
    [SerializeField] bool blockable;
    [SerializeField] string blockTag;
    [SerializeField] Sound blockedSound;

    bool hitting;
    float kb;
    int dmg;
    GameObject obj;
    Vector3 offset;
    bool crit, stun;
    [HideInInspector] public UnityEvent OnHit, onTrigger;
    [HideInInspector] public GameObject triggeredBy;
    [SerializeField] bool checkParent;

    private void Start()
    {
        if (hitSound) hitSound = Instantiate(hitSound);
        if (altSound) altSound = Instantiate(altSound);
        if (blockedSound) blockedSound = Instantiate(blockedSound);
    }

    public void StartChecking(bool _hitting = false, int _dmg = 0, float _kb = 0, GameObject _obj = null, Vector3 _offset = default, bool _crit = false, bool _stun = false) {
        hitting = _hitting;
        kb = _kb;
        dmg = _dmg;
        obj = _obj;
        offset = _offset;
        crit = _crit;
        stun = _stun;
        checking = true;
        targets.Clear();
        foreach (var l in linkedBoxes) l.StartChecking(hitting, _dmg ,_kb, _obj, _offset);
    }

    public void Refresh()
    {
        targets.Clear();
        foreach (var l in linkedBoxes) l.Refresh(); 
    }

    public List<HitReciever> EndChecking() {
        checking = false;
        foreach (var l in linkedBoxes) {
            var extras = l.EndChecking();
            foreach (var e in extras) if (!targets.Contains(e)) targets.Add(e);
        }
        return targets;
    }

    private void OnTriggerEnter(Collider other)
    {
        Check(other);   
    }

    private void OnTriggerStay(Collider other)
    {
        Check(other);
    }

    void Check(Collider other)
    {
        if (printHits) print("hit: " + other.gameObject.name);
        triggeredBy = other.gameObject;
        if (checking)onTrigger.Invoke();

        if (!checking) return;

        var reciever = other.GetComponent<HitReciever>();
        if (reciever == null && checkParent) reciever = other.GetComponentInParent<HitReciever>();
        if (reciever == null || targets.Contains(reciever) || (!string.IsNullOrEmpty(ignoreTag) && reciever.gameObject.CompareTag(ignoreTag))) return;

        if (blockable) {
            if (other.gameObject.CompareTag(blockTag)) {
                EndChecking();
                blockedSound.Play();
                return;
            }
        }

        if (hitting) {
            if (playSoundOnHit) {
                bool recalling = FindObjectOfType<PFighting>().Recalling();
                if (hitSound && !recalling) hitSound.Play();
                if (altSound && recalling) altSound.Play();
            }
            if (playSoundOnHit && hitSound) hitSound.Play();
            reciever.Hit(new HitReciever.HitData(dmg, obj, kb, offset, _crit:crit, _stun:stun));
            OnHit.Invoke();
            if (SpawnOnHit) Instantiate(SpawnOnHit, transform.position, Quaternion.identity);
        }
        targets.Add(reciever);
    }
}
