using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class GameManager : MonoBehaviour
{

    [System.Serializable]
    public class EnemyGroup
    {
        public Fact fact;
        public List<GameObject> enemies = new List<GameObject>();
        public int ID;
        public bool enabled;
    }

    [System.Serializable] 
    public class StoryPorgression
    {
        [HideInInspector] public string name;
        public Fact fact;
        public bool state;
        public string nextQuest;
        public int ID;
        public bool playLong;
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

    List<CheckPoint> checkPoints = new List<CheckPoint>();
    bool started;

    [Header("Manual Setup")]
    [SerializeField] int startingCheckPoint;
    [SerializeField] bool setStarting, resetGame;

    [Header("Quest")]
    [SerializeField] List<StoryPorgression> story = new List<StoryPorgression>();
    List<StoryPorgression> runtimeStory = new List<StoryPorgression>();

    [Header("Enemy groups")]
    [SerializeField] List<EnemyGroup> groups = new List<EnemyGroup>();

    private void OnValidate()
    {
        for (int i = 0; i < story.Count; i++) {
            story[i].name = story[i].nextQuest;
            story[i].ID = i;
        }
    }

    public void removeFromGroup(GameObject enemy, int groupID)
    {
        foreach (var g in groups) if (g.ID == groupID) g.enemies.Remove(enemy); 
    }

    public void AddToGroup(GameObject enemy, int groupID)
    {
        foreach (var g in groups) if (g.ID == groupID) { g.enemies.Add(enemy); g.enabled = true; }
    }

    public int GetID()
    {
        if (runtimeStory.Count == 0) return -1;
        return runtimeStory[0].ID;
    }

    public string getCurrentStory()
    {
        return GlobalUI.i.GetCurrentText();
    }

    public void SetCurrentStory(string text)
    {
        if (GlobalUI.i) GlobalUI.i.UpdateQuestText(text, false);
    }

    public void LoadStory(int ID)
    {
        if (ID == -1) { runtimeStory.Clear(); return; }
        runtimeStory = new List<StoryPorgression>(story);
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
        GlobalUI.i.Pause();
        unlockCursor();
    }

    void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void unlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
       
    }

    public void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
        if (Application.isPlaying) GlobalUI.i.Resume();
        LockCursor();
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
        SceneManager.LoadScene(2);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
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

        UpdateEnemyGroups();

        if (!started) RestartFromCheckPoint();

        CheckStory();
    }

    void UpdateEnemyGroups()
    {
        if (!Application.isPlaying) return;
        foreach (var g in groups) {

            if (g.enabled && FactManager.i.IsPresent(g.fact)) {
                for (int i = 0; i < g.enemies.Count; i++) {
                    Destroy(g.enemies[i]);
                }
                g.enabled = false;
                return;
            }

            for (int i = 0; i < g.enemies.Count; i++) {
                if (g.enemies[i] == null) g.enemies.RemoveAt(i);
            }
            if (g.enabled && g.enemies.Count == 0) FactManager.i.AddFact(g.fact);
        }
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        runtimeStory = new List<StoryPorgression>(story);
        GetComponent<SaveManager>().LoadGame();
        Unpause();
        LockCursor();
    }

    void CheckStory()
    {
        if (runtimeStory.Count == 0 || !Application.isPlaying) return;
        var next = runtimeStory[0];
        if (FactManager.i.IsPresent(next.fact) != next.state) return;

        GlobalUI.i.UpdateQuestText(next.nextQuest, next.playLong);
        runtimeStory.RemoveAt(0);
    }

    void Awake() 
    { 
        i = this;
        if (!PlayerPrefs.HasKey("checkpoint")) PlayerPrefs.SetInt("checkpoint", -1);
    }
}
