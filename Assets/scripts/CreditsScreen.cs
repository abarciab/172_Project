using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
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

        if (background.transform.position.y < backgroundTarget) background.transform.position += Vector3.up * backgroundSpeed;
        if (text.transform.position.y < textTarget) text.transform.position += Vector3.up * textSpeed;
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

}
