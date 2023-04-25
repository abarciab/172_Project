using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CorruptExplorer : MonoBehaviour
{
    [SerializeField] float agroRange = 10;
    [SerializeField] bool debug;
    [SerializeField] GameObject target;
    [SerializeField] AttackStats quickAttack, lungeAttack;
    EnemyMovement move;
    public bool agro;
    [SerializeField] float hitStunTime = 0.7f, lungeDist = 5;
   

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] float walkingThreshold = 0.1f;
    [SerializeField] string hurtAnimParam;

    Vector2 oldPos;
    bool screaming;
    bool busy;
    AttackStats currentAttack;
    bool alreadyHit;

    [Header("patterns")]
    int currentPattern;
    int patternStep;
    [SerializeField] float patternSwapTime, maxPattern3WaitTime;

    [Header("Jump")]
    [SerializeField] float jumpDist;
    [SerializeField] float jumpheight, jumpTime;
    bool jumping;
    float pattern3Time;

    public void EndScream()
    {
        agro = true;
    }

    public void StartChecking()
    {
        currentAttack.hitBox.StartChecking(true, currentAttack.damage, currentAttack.knockBack, gameObject);
    }

    public void EndAttack()
    {
        //print("end attack: " + currentAttack.name);
        busy = false;
        currentAttack.Cooldown = currentAttack.resetTime;
        anim.SetBool(currentAttack.animBool, false);
        if (!alreadyHit) CurrentPattern(_hit: 1);
        
        currentAttack = null;
    }

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        target = Player.i.gameObject;
        oldPos = new Vector2(transform.position.x, transform.position.z);
        var stats = GetComponent<EnemyStats>();
        stats.OnHit.AddListener(Stunned);
        stats.OnHit.AddListener(GetComponentInChildren<EnemySound>().TakeHit);
        currentPattern = 1;
        quickAttack.hitBox.OnHit.AddListener(HitTarget);
        Player.i.enemies.Add(move);
    }

    private void Update()
    {
        DoAnims();
        if (GetComponent<EnemyStats>().health <= 0) {
            anim.SetBool("dead", true);
            enabled = false;
            Player.i.enemies.Remove(move);
        }
       
        if (target == null) target = Player.i.gameObject;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        quickAttack.Cooldown -= Time.deltaTime;
        lungeAttack.Cooldown -= Time.deltaTime;
        if (currentPattern == 3) pattern3Time += Time.deltaTime;

        if (!InAgroRange(dist)) Stop();
        if (!agro || busy) return;

        CurrentPattern(dist);

        

        /*var quickStatus = quickAttack.status(dist);
        var lungeStatus = lungeAttack.status(dist);

        if (quickAttack.TooClose(dist)) Backup();
        else if (quickStatus == AttackStats.StatusType.ready) StartQuickAttack(); 
        else if (lungeStatus == AttackStats.StatusType.ready) StartLunge();
        else if (lungeAttack.TooFar(dist)) MoveTowardTarget();
        else Stop();*/
    }

    void HitTarget()
    {
        alreadyHit = true;
        CurrentPattern(_hit:2);
    }

    //1 is a miss, 2 is a hit
    void CurrentPattern(float dist = -1, int _hit = 0)
    {
        switch (currentPattern) {
            case 1:
                Pattern1(dist, _hit);
                break;
            case 2:
                Pattern2(dist, _hit);
                break;
            case 3:
                Pattern3(dist, _hit);
                break;
        }
    }

    void Pattern1(float dist = -1, int _hit = 0)
    {
        //print("call to pattern1. dist: " + dist + ", hit: " + _hit + ", patternStep: " + patternStep);
        if (patternStep == 0) {
            if (lungeAttack.TooFar(dist)) { MoveTowardTarget(); return;}

            StartLunge();
            patternStep += 1;
        }
        else if (patternStep == 1 && dist == -1) {
            if (_hit == 1) {
                Stunned();
                SwapPattern();
            }
            else {
                InturruptAttack(lungeAttack, "");
                patternStep += 1;
            }
        }
        else if (_hit == 0 && (patternStep == 2 || patternStep == 3)) {
            StartAttack(quickAttack);
            patternStep += 1;
        }
        else if (patternStep == 4) {
            SwapPattern();
        }

        //if 0: lunge
        //if !hit: stun, pick new pattern
        //if hit: 2
        //if 2: swipe, 3
        //if 3: swipe, 4
        //if 4: pick new pattern
    }

    void Pattern2(float dist = -1, int _hit = 0)
    {
        //print("call to pattern2");
        if (patternStep == 0) {
            if (lungeAttack.TooFar(dist)) { MoveTowardTarget(); return; }

            StartLunge();
            patternStep += 1;
        }
        else if (patternStep == 1 && dist == -1) {
            if (_hit == 1) SwapPattern(3);
            else if (_hit == 2) SwapPattern(1, 2);
        }

        //if 0: lunge
        //if !hit: pattern3
        //if hit: pattern1
    }

    void Pattern3(float dist = -1, int _hit = 0)
    {
        if (pattern3Time > maxPattern3WaitTime) {
            SwapPattern();
        }
        else if (patternStep == 0) {
            JumpBack();
            patternStep += 1;
        }
        else if (patternStep == 1) {
            LookAtTarget(0.75f);
            if (quickAttack.status(dist) == AttackStats.StatusType.ready) {
                StartAttack(quickAttack);
                patternStep += 1;
            }
        }
        if (patternStep >= 2 && dist == -1) {
            if (_hit == 1) SwapPattern();
            if (_hit == 2) StartAttack(quickAttack);
        }

        //if 0: jump back from player, 1
        //if 1: face player, move to the right
        //if 1 & distance < quickattack: swipe
        //while hit: swipe
        //if miss: pick new pattern
    }

    void JumpBack()
    {
        if (busy) return;

        StopAllCoroutines();
        StartCoroutine(_JumpBack(Player.i.transform.position));
    }

    IEnumerator _JumpBack(Vector3 threat)
    {
        var targetPos = transform.position + (threat - transform.position).normalized * - jumpDist;
        Vector2 originalPos = new Vector2(transform.position.x, transform.position.z);
        move.gotoTarget = false;

        busy = true;

        float timeLeft = jumpTime;
        float startPos = transform.position.y;

        while (timeLeft > 0) {
            float progress = timeLeft / jumpTime;

            float yPos = startPos + Mathf.Sin(progress * Mathf.PI) * jumpheight;
            Vector2 xzPos = Vector2.Lerp(originalPos, new Vector2(targetPos.x, targetPos.z), 1 - progress);
            transform.position = new Vector3(xzPos.x, yPos, xzPos.y);

            timeLeft -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        PutOnGround();

        busy = false;
        yield break;
    }

    void PutOnGround()
    {
        int layerMask = 1 << 7;
        Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out var hit, 150, layerMask: layerMask);
        if (hit.collider == null) return;
        var pos = transform.position;
        pos.y = hit.point.y;
        transform.position = pos;
    }

    void SwapPattern(int pattern = 0, int step = 0, float stunTime = -1)
    {
        quickAttack.Cooldown = lungeAttack.Cooldown = 0;

        if (pattern == 0) {
            pattern = Random.Range(1, 4);
            if (currentPattern == pattern) pattern = Random.Range(1, 4);
        }
        currentPattern = pattern;
        patternStep = step;
        if (currentPattern == 3) pattern3Time = 0;

        if (stunTime != 0) {
            busy = true;
            StartCoroutine(_Stunned(stunTime == -1 ? patternSwapTime : stunTime));
        }
    }

    void Stunned()
    {
        if (currentAttack == lungeAttack) return;
        if (currentAttack == quickAttack) InturruptAttack(quickAttack, hurtAnimParam);
        else busy = true;
        
        StartCoroutine(_Stunned(hitStunTime));
    }

    void InturruptAttack(AttackStats attack, string inturrupt)
    {
        anim.SetBool(attack.animBool, false);
        anim.SetBool(inturrupt, true);
    }

    IEnumerator _Stunned(float time)
    {
        yield return new WaitForSeconds(time);

        anim.SetBool(hurtAnimParam, false);
        busy = false;
    }

    void DoAnims()
    {
        var currentPos = new Vector2(transform.position.x, transform.position.z);
        float speed = Vector2.Distance(currentPos, oldPos) / Time.deltaTime;
        oldPos = currentPos;

        anim.SetBool("movingForward", speed > walkingThreshold);
        anim.SetBool("movingBackward", speed < -walkingThreshold);
    }

    void StartLunge()
    {
        LookAtTarget(0.9f);

        var dir = (target.transform.position - transform.position).normalized;
        var lungeTarget = transform.position + dir * lungeDist;

        move.target = lungeTarget;
        move.gotoTarget = true;

        StartAttack(lungeAttack, false);
    }

    void StartQuickAttack()
    {
        StartAttack(quickAttack);
    }

    void StartAttack(AttackStats attack, bool stop = true)
    {
        if (busy) return;
        anim.SetBool(hurtAnimParam, false);

        if (stop) Stop();
        StopAllCoroutines();
        anim.SetBool(quickAttack.animBool, false);
        anim.SetBool(lungeAttack.animBool, false);

        //print("start attack: " + attack.name);
        alreadyHit = false;
        LookAtTarget(0.75f);
        currentAttack = attack;
        attack.Cooldown = attack.resetTime;
        busy = true;
        anim.SetBool(attack.animBool, true);
    }

    void Backup()
    {
        //print("BACKUP");
        move.disableRotation();
        LookAtTarget(0.9f);
        var dir = (target.transform.position - transform.position).normalized * -1;
        move.target = transform.position + dir * 2;
        move.gotoTarget = true;
    }

    void LookAtTarget(float smoothness)
    {
        var rot = transform.localEulerAngles;
        var original = rot;
        transform.LookAt(target.transform);
        rot.y = transform.localEulerAngles.y;
        transform.localRotation = Quaternion.Lerp(Quaternion.Euler(original), Quaternion.Euler(rot), smoothness);
    }

    void MoveTowardTarget()
    {
        move.EnableRotation();
        move.target = target.transform.position;
        move.gotoTarget = true;
    }

    void Stop()
    {
        move.gotoTarget = false;
    }

    bool InAgroRange(float dist)
    {
        if (dist > agroRange) return false;

        if (!screaming && !agro) {
            anim.SetTrigger("scream");
            screaming = true;
        }


        return true;
    }

    private void OnDrawGizmos()
    {
        quickAttack.DrawGizmos(transform.position);
        lungeAttack.DrawGizmos(transform.position);

        if (!debug) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);
    }
}
