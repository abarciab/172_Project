using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPrompt : MonoBehaviour
{
    bool promptUp;
    [SerializeField] Fact tutorialComplete;

    private void OnTriggerEnter(Collider other)
    {
        if (promptUp) return;

        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();
        if (!player) return;

        Player.i.FreezePlayer();
        GlobalUI.i.tutorialSkip.SetActive(true);
        promptUp = true;
    }

    private void Update()
    {
        if (promptUp) {
            if (Input.GetKeyDown(KeyCode.F)) {
                Player.i.UnfreezePlayer();
                GlobalUI.i.tutorialSkip.SetActive(false);
                FactManager.i.AddFact(tutorialComplete);
                Destroy(gameObject);
                promptUp = false;
            }
            if (Input.GetKeyDown(KeyCode.R)) {
                Player.i.UnfreezePlayer();
                GlobalUI.i.tutorialSkip.SetActive(false);
                StartCoroutine(Resume(3));
            }
        }
    }

    IEnumerator Resume(float time)
    {
        yield return new WaitForSeconds(time);
        promptUp = false;
    }

}
