using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PFighting : HitReciever
{
    [Header("Testing")]
    [SerializeField] private PlayerAbilityData _sunblast;

    [Header("Misc")]
    [SerializeField] private PlayerAbilityController _abilityController;
    [SerializeField, SearchableEnum] private KeyCode _specialKey = KeyCode.E;

    private Player _p;

    //
    //
    //

    [SerializeField] Rigidbody staffProjectile;
    [SerializeField] Vector3 offset;
    [SerializeField] Vector3 aimOffset;
    [SerializeField] GameObject spearObj;

    [Header("Stab")]
    [SerializeField] float stabKB;
    [SerializeField] int stabDmg;
    public bool stabbing;
    public bool Stabbing() => stabbing;
    [SerializeField] HitBox stabHB;

    [Header("Misc")]
    [SerializeField, ReadOnly] private bool _hasSpear;
    [SerializeField, ReadOnly] bool recalling;
    [SerializeField, ReadOnly] bool charging;
    [SerializeField, ReadOnly] bool spearDrawn;
    public bool RecallReady;
    public bool chargingSpear() => charging;
    public bool Recalling() => recalling;
    public int throwDmg { get; private set; }
    public int critDmg { get; private set; }
    public int CritDmg => critDmg;
    public bool SpearOut() => spearDrawn;
    public void DrawSpear() => spearDrawn = true;
    public bool HasSpear() => _hasSpear;
    public void PutAwaySpear() => spearDrawn = false;
    public void StartChecking() => stabHB.StartChecking(true, stabDmg, stabKB, stabHB.gameObject);
    Vector3 GetAimDir() => (Camera.main.transform.forward + aimOffset).normalized;

    private bool _initialized;

    private void OnEnable() {
        if (!_p) _p = GetComponent<Player>();
        if (_initialized) _p.Anim.DrawSpear();
    }

    private void Start()
    {
        _hasSpear = true;
        _p.Anim.DrawSpear();
        _initialized = true;
        spearObj.SetActive(true);
    }

    private void Update() {

        SendInputsToAbilityController();


        //
        //

        //spearObj.SetActive(_hasSpear && (!spearDrawn || (!staffProjectile.gameObject.activeInHierarchy || !charging)));
    }

    private void SendInputsToAbilityController() {
        var leftClick = Input.GetMouseButtonDown(0) ? InputType.DOWN : (Input.GetMouseButton(0) ? InputType.STAY : (Input.GetMouseButtonUp(0) ? InputType.UP : InputType.NONE));
        if (leftClick != InputType.NONE) _abilityController.OnLeftClick(leftClick);
        var rightClick = Input.GetMouseButtonDown(1) ? InputType.DOWN : (Input.GetMouseButton(1) ? InputType.STAY : (Input.GetMouseButtonUp(1) ? InputType.UP : InputType.NONE));
        if (rightClick != InputType.NONE) _abilityController.OnRightClick(rightClick);
        var special = Input.GetKeyDown(_specialKey) ? InputType.DOWN : (Input.GetKey(_specialKey) ? InputType.STAY : (Input.GetKeyUp(_specialKey) ? InputType.UP : InputType.NONE));
        if (special != InputType.NONE) _abilityController.OnSpecial(special);
    }

    private void OnSunblastReady()
    {
        _p.Sounds.Get(PSoundKey.SUNBLASTREADY).Play(transform);  
        GlobalUI.i.Do(UIAction.SUNBLAST_READY);
    }

    public void SetSpearDmg(int dmg)
    {
        throwDmg = dmg;
        critDmg = dmg;
    }

    public void RecallReadyNotice()
    {
        RecallReady = true;
        GlobalUI.i.Do(UIAction.RECALL_READY);
    }

    public void ReturnSpear()
    {
        _hasSpear = true;
        recalling = charging = false;
        GlobalUI.i.Do(UIAction.CATCH_SPEAR);
        _p.Sounds.Get(PSoundKey.SPEAR_CATCH).Play();
    }

    public void ThrowSpear()
    {
        charging = false;
        _hasSpear = false;

        /*
        staffProjectile.GetComponent<ThrownStaff>().Throw();
        
        staffProjectile.gameObject.SetActive(false);
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
        staffProjectile.transform.parent = null;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.AddForce(dir * (throwForce * power));

        staffProjectile.GetComponentInChildren<HitBox>().StartChecking(transform, damage, _crit: perfectThrow);
        */
    }

    public void Stab()
    {
        if (!enabled) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!_hasSpear) {RetrieveSpear(); return; }
        if (charging || stabbing) return;
        stabbing = true;
    }

    public void EndAttack()
    {
        stabHB.EndChecking();
        stabbing = false;
    }

    public override void Hit(HitData hit)
    {
        if (GetComponent<PMovement>().rolling) return;

        base.Hit(hit);

        Player.i.ChangeHealth(-hit.damage);
        GetComponent<PMovement>().KnockBack(hit.source, hit.KB, hit.offset);
    }

    public void StartAimingSpear()
    {
        if (!enabled || charging) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!_hasSpear) { RetrieveSpear(); return; }
        charging = true;
        stabbing = false;
        _p.Sounds.Get(PSoundKey.THROW_CHARGE).Play();

        //CameraState.i.SwitchToState(CameraState.StateName.MouseOverShoulder);

        staffProjectile.gameObject.SetActive(false);

        staffProjectile.transform.parent = transform;
        staffProjectile.transform.localPosition = offset;

        staffProjectile.isKinematic = true;
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
       
    }

    public void SwapSpear()
    {
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
    }

    void RetrieveSpear()
    {
        if (recalling) return;
        if (!RecallReady) _p.Sounds.Get(PSoundKey.RECALL_ERROR).Play(restart:false);

        recalling = staffProjectile.GetComponent<ThrownStaff>().Recall();
        if (recalling) _p.Sounds.Get(PSoundKey.RECALL).Play();
    }    

}
