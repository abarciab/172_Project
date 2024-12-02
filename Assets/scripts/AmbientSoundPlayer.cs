using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundPlayer : MonoBehaviour
{
    [SerializeField] Sound sound;
    float playCooldown;
    [SerializeField] bool Nonloop = true;
    [SerializeField, ConditionalField(nameof(Nonloop))] Vector2 playFrequency = new Vector2(4, 10);

    private void Start()
    {
        sound = Instantiate(sound);
        if (!Nonloop) sound.Play(transform);
        else playCooldown = Random.Range(playFrequency.x, playFrequency.y);
    }

    private void Update()
    {
        if (!Nonloop) return;

        playCooldown -= Time.deltaTime;
        if (playCooldown <= 0) {
            sound.Play(transform);
            playCooldown = Random.Range(playFrequency.x, playFrequency.y);
        }
    }
}
