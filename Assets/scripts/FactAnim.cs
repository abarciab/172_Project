using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactAnim : MonoBehaviour
{
    [SerializeField] Fact fact1;
    [SerializeField] string anim1;
    [SerializeField] Animator anim;
    [SerializeField] Speaker speaker;
    [SerializeField] string speakerTalkingAnim;

    private void Update()
    {
        if (FactManager.i.IsPresent(fact1)) anim.SetBool(anim1, true);
        if (speaker) anim.SetBool(speakerTalkingAnim, speaker.talking);
    }
}
