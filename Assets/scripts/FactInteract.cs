using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactInteract : MonoBehaviour
{

    [SerializeField] Fact prerequisite;
    [SerializeField] List<Fact> addWhenInteract = new List<Fact>(), removeWhenInteract = new List<Fact>();
    [SerializeField] string prompt;
    [SerializeField] KeyCode interactKey;
    bool promptUp, unlocked;
    [SerializeField] Sound PlayWhenInteract;

    private void Start()
    {
        if (PlayWhenInteract) PlayWhenInteract = Instantiate(PlayWhenInteract);
    }

    private void Update()
    {
        unlocked = prerequisite == null || FactManager.i.IsPresent(prerequisite);

        if (promptUp && Input.GetKeyDown(interactKey)) {
            foreach (var f in addWhenInteract) FactManager.i.AddFact(f);
            foreach (var f in removeWhenInteract) FactManager.i.RemoveFact(f);
            GlobalUI.i.HidePrompt(prompt);
            if (PlayWhenInteract) PlayWhenInteract.Play();
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!unlocked) return;

        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();

        if (player != null) {
            GlobalUI.i.DisplayPrompt(prompt);
            promptUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!unlocked) return;

        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();

        if (player != null) {
            GlobalUI.i.HidePrompt(prompt);
            promptUp = false;
        }
    }
}
