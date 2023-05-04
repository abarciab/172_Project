using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour {

    public Image continueButton;
    [SerializeField] Color valid, Invalid;

    private void Update()
    {
        continueButton.color = PlayerPrefs.GetInt("savedFacts", 0) > 0 ? valid : Invalid;
        continueButton.GetComponent<Button>().enabled = PlayerPrefs.GetInt("savedFacts", 0) > 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("checkpoint", 0);
        PlayerPrefs.SetInt("savedFacts", 0);
        PlayerPrefs.SetInt("story", 0);
        SceneManager.LoadScene(1);
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }
}
