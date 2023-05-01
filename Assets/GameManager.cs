using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class GameManager : MonoBehaviour
{
    [System.Serializable] 
    public class StoryPorgression
    {
        public Fact fact;
        public bool state;
        public string nextQuest;
        public int ID;
    }

    [System.Serializable]
    public class CheckPoint
    {
        public Transform point;
        public int ID;

        public CheckPoint(Transform _point, int _ID)
        {
            point = _point;
            ID = _ID;
        }
    }

    public static GameManager i;
    
    public bool paused { get; private set; }

    [SerializeField] List<CheckPoint> checkPoints = new List<CheckPoint>();
    bool started;

    [Header("Manual Setup")]
    [SerializeField] int startingCheckPoint;
    [SerializeField] bool setStarting, resetGame;

    [Header("Quest")]
    [SerializeField] List<StoryPorgression> story = new List<StoryPorgression>();
    [SerializeField] List<StoryPorgression> runtimeStory = new List<StoryPorgression>();

    public int GetID()
    {
        if (runtimeStory.Count == 0) return -1;
        return runtimeStory[0].ID;
    }

    public string getCurrentStory()
    {
        return GlobalUI.i.currentQuest.text;
    }

    public void SetCurrentStory(string text)
    {
        if (GlobalUI.i) GlobalUI.i.currentQuest.text = text;
    }

    public void LoadStory(int ID)
    {
        if (ID == -1) { runtimeStory.Clear(); return; }
        while (runtimeStory.Count > 0 && runtimeStory[0].ID != ID) {
            runtimeStory.RemoveAt(0);
        }
    }

    public void AddCheckPoint(Transform point, int ID)
    {
        checkPoints.Add(new CheckPoint(point, ID));
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
    }

    public void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
    }

    public void TogglePause()
    {
        if (paused) Unpause();
        else Pause();
    }

    public void RestartFromCheckPoint()
    {
        started = true;
        int checkPoint = PlayerPrefs.GetInt("checkpoint");
        if (GetCheckPoint(checkPoint) == Vector3.zero) return;
        Player.i.transform.position = GetCheckPoint(checkPoint);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(1);
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }

    public void SetCheckPointManually()
    {
        PlayerPrefs.SetInt("checkpoint", startingCheckPoint);
    }

    Vector3 GetCheckPoint(int ID)
    {
        foreach (var c in checkPoints) if (c.ID == ID) return c.point.position;
        return Vector3.zero;
    }

    private void Update()
    {
        if (setStarting) {
            PlayerPrefs.SetInt("checkpoint", startingCheckPoint);
            setStarting = false;
        }
        if (resetGame) {
            resetGame = false;
            GetComponent<SaveManager>().ResetGame();
        }

        if (!started) RestartFromCheckPoint();

        CheckStory();
        
    }

    private void Start()
    {
        runtimeStory = new List<StoryPorgression>(story);
        GetComponent<SaveManager>().LoadGame();
    }

    void CheckStory()
    {
        if (runtimeStory.Count == 0 || !Application.isPlaying) return;
        var next = runtimeStory[0];
        if (FactManager.i.IsPresent(next.fact) != next.state) return;

        GlobalUI.i.currentQuest.text = next.nextQuest;
        runtimeStory.RemoveAt(0);
    }

    void Awake() 
    { 
        i = this;
        if (!PlayerPrefs.HasKey("checkpoint")) PlayerPrefs.SetInt("checkpoint", -1);
    }
}