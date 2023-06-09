using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class TitleScreen : MonoBehaviour {

    public Image continueButton;
    [SerializeField] Color valid, Invalid;
    public bool fading;
    [SerializeField] GameObject fade, video;
    [SerializeField] VideoPlayer videoPlayer;

    private void Start()
    {
        fade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        fading = false;
        Time.timeScale = 1;
    }

    private void Update()
    {
        continueButton.color = PlayerPrefs.GetInt("savedFacts", 0) > 0 ? valid : Invalid;
        continueButton.GetComponent<Outline>().enabled = PlayerPrefs.GetInt("savedFacts", 0) > 0;
        continueButton.GetComponent<Button>().enabled = PlayerPrefs.GetInt("savedFacts", 0) > 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!fading) return;

        fade.SetActive(true);
        fade.GetComponent<Image>().color = Color.Lerp(fade.GetComponent<Image>().color, Color.black, 0.2f);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        if (fading) return;
        fading = true;
        
        StopAllCoroutines();
        StartCoroutine(_StartGame(1.5f));
    }

    IEnumerator _StartGame(float delay)
    {
        var source = GetComponent<AudioSource>();

        while (delay > 0) {
            yield return new WaitForEndOfFrame();
            delay -= Time.deltaTime;
            source.volume = Mathf.Lerp(source.volume, 0, 0.05f);
        }

        video.SetActive(true);

        while (!videoPlayer.isPlaying) yield return null;
        while (videoPlayer.isPlaying && !Input.GetKeyDown(KeyCode.Escape)) yield return null;

        PlayerPrefs.SetInt("checkpoint", 0);
        PlayerPrefs.SetInt("savedFacts", 0);
        PlayerPrefs.SetInt("story", 0);
        LoadGame(0);
    }

    public void LoadGame(float delay = 1.5f)
    {
        if (fading && delay > 0) return;
        fading = true;

        StartCoroutine(_LoadGame(delay));   
    }

    IEnumerator _LoadGame(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(2);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    public void Credits()
    {
        SceneManager.LoadScene(3);
    }

}
