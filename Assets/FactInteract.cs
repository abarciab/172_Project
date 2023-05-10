using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactInteract : MonoBehaviour
{
    [SerializeField] List<Fact> addWhenInteract = new List<Fact>(), removeWhenInteract = new List<Fact>();
    [SerializeField] string prompt;
    [SerializeField] KeyCode interactKey;
    bool promptUp;

    private void Update()
    {
        if (promptUp && Input.GetKeyDown(interactKey)) {
            foreach (var f in addWhenInteract) FactManager.i.AddFact(f);
            foreach (var f in removeWhenInteract) FactManager.i.RemoveFact(f);
            GlobalUI.i.HidePrompt(prompt);
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();

        if (player != null) {
            GlobalUI.i.DisplayPrompt(prompt);
            promptUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();

        if (player != null) {
            GlobalUI.i.HidePrompt(prompt);
            promptUp = false;
        }
    }
}
