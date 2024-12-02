using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] GameObject background, text;
    [SerializeField] float backgroundSpeed, backgroundTarget, textSpeed, textTarget;

    private void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (background.transform.position.y < backgroundTarget) background.transform.position += Vector3.up * backgroundSpeed * 100 * Time.deltaTime;
        if (text.transform.position.y < textTarget) text.transform.position += Vector3.up * textSpeed * 100 * Time.deltaTime;
    }

    public void Quit()
    {
        AchievementController.i.Unlock("CREDITS");
        SceneManager.LoadScene(0);
    }

}
