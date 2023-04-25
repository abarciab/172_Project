using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    [SerializeField] Fade fade;

    private void Awake()
    {
        fade.enabled = false;
        fade.GetComponent<Image>().color = Color.black;
    }

    private void Start()
    {
        
        Player.i.GetComponent<PControls>().enabled = false;
    }

    bool started;

    private void Update()
    {
        if (!started) {
            GlobalUI.i.DisplayLine("", "I should probably go speak with gran...");
            started = true;
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            fade.enabled = true;
            Player.i.GetComponent<PControls>().enabled = true;
            enabled = false;
            GlobalUI.i.EndConversation();
        }
    }
}
