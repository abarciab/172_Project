using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class Droppable : HitReciever
{
    [SerializeField] Vector3 fallRot, fallPos;
    Vector3 originalPos, originalRot;
    [SerializeField] bool setFallRot, setfallPos, reset;
    [SerializeField] AnimationCurve fallCurve;
    [SerializeField] float dropTime;
    float timeLeft;
    [SerializeField] HitData hitData;
    [SerializeField] Material glowingMat, normalMat;
    [SerializeField] List<Renderer> meshes = new List<Renderer>();
    bool _hitSnake;

    private void Start()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localEulerAngles;
    }

    private void Update()
    {
        if (setfallPos) {
            setfallPos = false;
            fallPos = transform.localPosition;
        }
        if (setFallRot) {
            setFallRot = false;
            fallRot = transform.localEulerAngles;
        }
        if (reset) {
            reset = false;
            StopAllCoroutines();
            transform.localPosition = originalPos;
            transform.localEulerAngles = originalRot;
            GetComponent<Collider>().isTrigger = false;
        }

        if (Application.isPlaying) 
            foreach (var r in meshes) r.material = Player.i.poweredUp && transform.localPosition == originalPos ? glowingMat : normalMat;
    }

    public override void Hit(HitData hit)
    {
        base.Hit(hit);
        if (Player.i.poweredUp) {
            Drop();
            Player.i.poweredUp = false;
        }
    }

    void Drop()
    {
        GetComponent<Collider>().isTrigger = true;
        StartCoroutine(AnimateDrop());
    }

    IEnumerator AnimateDrop()
    {
        timeLeft = dropTime;
        while (timeLeft > 0) {
            timeLeft -= Time.deltaTime;
            var progress = 1 - (timeLeft / dropTime);
            progress = fallCurve.Evaluate(progress);
            transform.localPosition = Vector3.Lerp(originalPos, fallPos, progress);
            transform.localRotation = Quaternion.Lerp(Quaternion.Euler(originalRot), Quaternion.Euler(fallRot), progress);
            yield return new WaitForEndOfFrame();
        }
        transform.localPosition = fallPos;
        transform.localEulerAngles = fallRot;
        CameraShake.i.Shake();
    }

    private void OnTriggerStay(Collider other)
    {
        if (_hitSnake) return;
        if (other.GetComponent<EnemyStats>()) {
            other.GetComponent<EnemyStats>().Hit(hitData, true);
            _hitSnake = true;
        }
    }
}
