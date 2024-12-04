using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Snake : BaseEnemy
{
    [Header("Projectile")]
    [SerializeField] Vector3 projectileStartOffset, projectileSize;
    [SerializeField] float projectileAngle = 45, shortDist;

    [Header("Spit")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] float spitResetTime;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    float spitCooldown;

    [Header("Spray")]
    [SerializeField] float sprayResetTime;
    [SerializeField] float sprayDelay;
    float sprayCooldown;
    bool spraying;

    [Header("Goop Orb")]
    [SerializeField] GameObject orbPrefab;
    [SerializeField] float orbResetTime, orbSpeed, orbScale, orbTargetYOffset;
    [SerializeField] int orbDamage;
    float orbCooldown;

    [Header("Sounds")]
    [SerializeField] Sound goopThrowSound;
    [SerializeField] Sound transitionSound, slitherSound, battleStartSound, hissSound, buildUp, strike;

    [Header("Anims")]
    [SerializeField] float introWaitTime = 5;
    [SerializeField] Animator anim;
    [SerializeField] string sprayAnim, spitAnim, tailWhipAnim, slitherAnim, coiledAnim = "coiled", pillarHitAnim = "pillarHit", spikeHitAnim = "spikeHit", p3AdvanceAnim = "phase3Advance";

    [Header("phases")]
    [SerializeField] int Phase = 1;
    [SerializeField] GameObject phase1Pickups, phase2Spawners;

    [Header("p3 shooting")]
    [SerializeField] float phase3ShootResetTime;
    [SerializeField] float phase3SustainTime, p3ShootSpeed, p3ProjectileMass, p3TargetOffset, p3ShootPredictMult = 2, p3ShootAngle = 30, p3ShortDist, p3RandomizeRadius;
    [SerializeField] int p3ShootDamage = 15;
    float p3ShootTimeCooldown;

    [Header("p3 darkness")]
    [SerializeField] Transform playerTPtarget;
    [SerializeField] float p3strikeBuildUpResetTime = 7, p3VulnerableTime = 2, p3StrikeTime = 5, p3TransitionTime;
    float darknessPhaseTimePassed;
    [SerializeField] HitReciever.HitData p3Strike;
    int p3DarknessStartingHealth;
    [SerializeField] Material snakeEyesMat, pickupMat;
    [Space()]
    [SerializeField] Transform restingPos;
    [SerializeField] Transform leftPos, rightPos;
    [SerializeField] float snakeMoveLeftRightSpeed;

    [Space(3)]
    [SerializeField] bool p3AdvanceOnPlayer = true;
    [SerializeField] float p3AdvanceSpeed = 1;
    [SerializeField] Collider headColldier;
    [SerializeField] List<Collider> tailColliders = new List<Collider>();

    [Header("p3 obstacles")]
    [SerializeField] int obstacleCount;
    [SerializeField] float obstacleRevealTime = 1, obstacleHiddenYPos = -10f, obstacleRevealedYPos = 7.5f;
    [SerializeField] Transform p3ObstacleParent;
    [SerializeField] AnimationCurve obstacleRevealCurve;
    [SerializeField] GameObject snakeEyes;
    [SerializeField] int snakeEyesLayer, defaultLayer;
    float phase3shootCooldown;
    bool obstaclesRevealed, buildingUp, vulnerable, movingLeft;

    GameObject projectileSource;


    protected override void Start()
    {
        base.Start();
        transitionSound = Instantiate(transitionSound);
        battleStartSound = Instantiate(battleStartSound);
        slitherSound = Instantiate(slitherSound);
        hissSound = Instantiate(hissSound);
        buildUp = Instantiate(buildUp);
        strike = Instantiate(strike);
        goopThrowSound = Instantiate(goopThrowSound);
        
        battleStartSound.Play();
        projectileSource = gameObject;
        busy = true;
        stats.OnHit.AddListener(HitRecieved);
    }

    public void SetBusy(bool state)
    {
        busy = state;
    }

    void HitRecieved()
    {
        if (Phase == 1) {
            anim.SetTrigger(pillarHitAnim);
            busy = true;
        }
        if (Phase == 2) {
            anim.SetTrigger(spikeHitAnim);
            busy = false;
        }
    }

    public override void EndAttack()
    {
        base.EndAttack();
        currentAttack = null;
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
        AchievementController.i.Unlock("SNAKE_KILLED");
        if (PlayerPrefs.GetInt("deaths", 0) == 0) AchievementController.i.Unlock("SUN_WARRIOR");
    }

    protected override void Update()
    {
        base.Update();

        if (busy) return;

        if (Phase == 1) Phase1Behavior();
        if (Phase == 2) Phase2Behavior();
        if (Phase == 3) Phase3Behavior();
    }

    void Phase1Behavior()
    {
        if (stats.health <= 2000) {
            Phase = 2;
            phase1Pickups.SetActive(false);
            return;
        }

        phase1Pickups.SetActive(true);
        stats.invincible = true;
        if (dist < RangedRange.y && dist > RangedRange.x) RangedAttack();
        
    }

    void Phase2Behavior()
    {
        if (stats.health <= 1000) {
            Player.i.SetSpearDamage(340);
            StartPhase3Shooting();
            return;
        }

        phase2Spawners.SetActive(true);
        if (orbCooldown <= 0) FireOrb();
    }    

    void Phase3Behavior()
    {
        p3ShootTimeCooldown -= Time.deltaTime;
        if (p3ShootTimeCooldown <= 0) {
            if (obstaclesRevealed) {
                StartPhase3Darkness();
            }
            if (stats.health < p3DarknessStartingHealth) {
                EndVulnerable();
                StartPhase3Shooting();
                buildUp.Stop();
                return;
            }

            if (p3AdvanceOnPlayer) Phase3MoveTowardPlayer();
            else MoveBackAndForth();

            darknessPhaseTimePassed += Time.deltaTime;
            if (darknessPhaseTimePassed > p3strikeBuildUpResetTime && buildingUp) BecomeVulnerable();
            if (darknessPhaseTimePassed > p3strikeBuildUpResetTime + p3VulnerableTime && vulnerable) p3StrikeAttack();
            if (darknessPhaseTimePassed > p3strikeBuildUpResetTime + p3VulnerableTime + p3StrikeTime) StartPhase3Shooting();

            return;
        }

        if (p3ShootTimeCooldown <= p3TransitionTime) return;

        phase3shootCooldown -= Time.deltaTime;
        if (phase3shootCooldown <= 0) {
            p3LaunchProjectile();
            phase3shootCooldown = phase3ShootResetTime;
        }
    }

    void Phase3MoveTowardPlayer()
    {
        anim.SetBool(p3AdvanceAnim, true);
        move.target = target.position;
        move.ChangeSpeed(p3AdvanceSpeed);
        move.gotoTarget = true;

        if (!headColldier.enabled) {
            headColldier.enabled = true;
            foreach (var c in tailColliders) c.enabled = false;
        }
    }

    void BecomeVulnerable()
    {
        vulnerable = true;
        buildingUp = false;
        stats.invincible = false;
        snakeEyes.GetComponent<Renderer>().material = pickupMat;
    }

    void EndVulnerable()
    {
        vulnerable = false;
        stats.invincible = true;
        snakeEyes.GetComponent<Renderer>().material = snakeEyesMat;
    }

    void p3LaunchProjectile()
    {
        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, p3ShootDamage);
        var variation = Random.insideUnitSphere * p3RandomizeRadius;

        var targetPos = target.position;
        //Vector3 targetPos = target.position + Player.i.speed3D * p3ShootPredictMult + variation;    

        AimAndFire(projectile, p3ShootAngle, targetPos, projectileStartOffset.y, shortDist:0, source: projectileSource);
        goopThrowSound.Play();
    }

    void StartPhase3Shooting()
    {
        if (p3AdvanceOnPlayer) {
            move.gotoTarget = false;
            anim.SetBool(p3AdvanceAnim, false);
            headColldier.enabled = false;
            foreach (var c in tailColliders) c.enabled = true;
        }

        transform.position = restingPos.position;
        stats.invincible = true;
        p3ShootTimeCooldown = phase3SustainTime + p3TransitionTime;
        StopAllCoroutines();
        Phase = 3;
        phase2Spawners.SetActive(false);
        ShaderTransitionController.i.BrightenNight();
        snakeEyes.layer = defaultLayer;
        StartCoroutine(SummonObstacles());
        Player.i.UnfreezePlayer();
        Player.i.SetSpearLayer(default);
    }

    void StartPhase3Darkness()
    {
        buildUp.Play();
        buildingUp = true;

        GoopManager.i.ClearAllFloorGoop();
        darknessPhaseTimePassed = 0;
        p3DarknessStartingHealth = stats.health;
        HideObstacles();
        snakeEyes.layer = snakeEyesLayer;
        ShaderTransitionController.i.DarkenNight();
        TPandLockPlayer();
        Player.i.SetSpearLayer(snakeEyesLayer);
    }

    void p3StrikeAttack()
    {
        strike.Play();
        CameraShake.i.Shake();
        EndVulnerable();
        Player.i.GetComponent<HitReciever>().Hit(p3Strike);
    }

    void MoveBackAndForth()
    {
        var dir = movingLeft ? leftPos.position - transform.position : rightPos.position - transform.position;
        dir.y = 0;
        transform.position +=  snakeMoveLeftRightSpeed * Time.deltaTime * dir.normalized;

        float distLeft = Vector2.Distance(transform.position, leftPos.position);
        float distRight = Vector2.Distance(transform.position, rightPos.position);
        if (movingLeft && distLeft < 0.1f) movingLeft = false;
        if (!movingLeft && distRight < 0.1f) movingLeft = true;
    }

    void TPandLockPlayer()
    {
        target.transform.position = playerTPtarget.position;
        Player.i.FreezePlayer();
    }

    void HideObstacles()
    {
        obstaclesRevealed = false;
        foreach (Transform o in p3ObstacleParent) o.localPosition = new Vector3(o.localPosition.x, obstacleHiddenYPos, o.localPosition.z);
    }

    IEnumerator SummonObstacles()
    {
        var chosenObstacles = new List<Transform>();
        for (int i = 0; i < obstacleCount; i++) {
            chosenObstacles.Add(p3ObstacleParent.GetChild(Random.Range(0, p3ObstacleParent.childCount)));
        }

        float timePassed = 0;
        while (timePassed < obstacleRevealTime) {
            timePassed += Time.deltaTime;
            var progress = timePassed / obstacleRevealTime;
            progress = obstacleRevealCurve.Evaluate(progress);
            var y = Mathf.Lerp(obstacleHiddenYPos, obstacleRevealedYPos, progress);
            foreach (var o in chosenObstacles) o.localPosition = new Vector3(o.localPosition.x, y, o.localPosition.z);
            yield return new WaitForEndOfFrame();
        }
        obstaclesRevealed = true;
    }

    void FireOrb()
    {
        orbCooldown = orbResetTime;

        busy = false;
        var orb = InstantiateProjectile(orbPrefab, projectileStartOffset, Vector3.one * orbScale);
        orb.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        orb.GetComponent<HitBox>().StartChecking(transform, orbDamage);

        //Vector3 targetPos = target.position + Player.i.speed3D;
        var targetPos = target.position;
        var dir =  (targetPos + Vector3.up * orbTargetYOffset) - (transform.position + projectileStartOffset*2);
        orb.GetComponent<Rigidbody>().AddForce(orbSpeed * dir.normalized);

        goopThrowSound.Play();
    }

    protected override void Cooldowns()
    {
        base.Cooldowns();
        float dt = Time.deltaTime;
        spitCooldown -= dt;
        sprayCooldown -= dt;
        orbCooldown -= dt;
    }

    void RangedAttack()
    {
        busy = true;
        if (sprayCooldown <= 0) Spray();
        else if (spitCooldown <= 0) Spit();
        else busy = false;
    }

    void Spray()
    {
        spraying = true;
        sprayCooldown = sprayResetTime;
        anim.SetBool(sprayAnim, true);
    }

    
    void Spit()
    {
        spitCooldown = spitResetTime;
        anim.SetBool(spitAnim, true);
    }

    public void LaunchProjectile(float predictMultiplier = 1)
    {
        if (spraying) {
            spraying = false;
            StartCoroutine(LaunchAfterDelay(sprayDelay * 1));
            StartCoroutine(LaunchAfterDelay(sprayDelay * 2));
        }
       
        busy = false;
        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, rangedDmg);
        //Vector3 targetPos = target.position + Player.i.speed3D * predictMultiplier;
        var targetPos = target.position;
        
        AimAndFire(projectile, projectileAngle, targetPos, projectileStartOffset.y, shortDist, source: projectileSource);
        goopThrowSound.Play();

        anim.SetBool(spitAnim, false);
        anim.SetBool(sprayAnim, false);
    }

    IEnumerator LaunchAfterDelay(float delay, int recurse = 0)
    {
        if (recurse < 0) yield break;
        yield return new WaitForSeconds(delay);
        LaunchProjectile();

        if (recurse > 0) StartCoroutine(LaunchAfterDelay(delay, recurse - 1));
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
    }
}
