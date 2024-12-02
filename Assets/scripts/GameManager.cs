using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    
    public bool paused { get; private set; }

    List<CheckPointData> checkPoints = new List<CheckPointData>();
    bool started;

    [Header("Manual Setup")]
    [SerializeField] int startingCheckPoint;
    [SerializeField] bool setStarting;
    [SerializeField] bool resetGame;

    [Header("Quest")]
    [SerializeField] List<StoryProgressionData> story = new List<StoryProgressionData>();
    List<StoryProgressionData> runtimeStory = new List<StoryProgressionData>();

    [Header("Enemy groups")]
    [SerializeField] List<EnemyGroupData> groups = new List<EnemyGroupData>();

    [Header("Goats")]
    public int NumTotalGoats = 11;
    [SerializeField] private int numGoatsFound;

    [Header("References")]
    [SerializeField] private Transform _player;
    //[SerializeField] private MovementTutorial _firstTutorial;
    //[SerializeField] private List<Transform> _playerTpPoints;
    [SerializeField] private CameraController _cam;
 
    [SerializeField, ReadOnly] public float GameProgress;

    void Awake()
    {
        i = this;
        if (!PlayerPrefs.HasKey("checkpoint")) PlayerPrefs.SetInt("checkpoint", -1);
    }

    private void Start()
    {
        _player = Player.i.transform; 
        SaveManager.i.OnLoad.AddListener(SetGameStateFromSaveData);

        runtimeStory = new List<StoryProgressionData>(story);
        //GetComponent<SaveManager>().LoadGame();
        Unpause();
        LockCursor();
    }

    private void Update()
    {
        if (setStarting) {
            PlayerPrefs.SetInt("checkpoint", startingCheckPoint);
            setStarting = false;
        }
        if (resetGame) {
            resetGame = false;
            //GetComponent<SaveManager>().ResetGame();
        }

        UpdateEnemyGroups();

        if (!started) RestartFromCheckPoint();
        //CheckStory();
    }

    private void SetGameStateFromSaveData(SaveData current)
    {
        _player.transform.position = current.PlayerPosition;
        _player.rotation = current.PlayerRotation;
        GameProgress = current.GameProgress;
        PropogateGameProgressChange();
    }

    private void PropogateGameProgressChange()
    {
        if (GameProgress - 0 < 0.001f) StartGame(); 
    }

    private void StartGame()
    {
        //_firstTutorial.Activate();
        //_player.SetPositionAndRotation(_playerTpPoints[0].position, _playerTpPoints[0].rotation);
        if (_cam) _cam.SnapToState();
    }

    public void AddNewSpeaker(string name)
    {
        if (name == "Engineer") PlayerPrefs.SetInt("speaker1", 1);
        if (name == "Gran") PlayerPrefs.SetInt("speaker2", 1);
        if (name == "Leader") PlayerPrefs.SetInt("speaker3", 1);

        int totalSpeakers = 0;
        if (PlayerPrefs.GetInt("speaker1", 0) == 1) totalSpeakers += 1;
        if (PlayerPrefs.GetInt("speaker2", 0) == 1) totalSpeakers += 1;
        if (PlayerPrefs.GetInt("speaker3", 0) == 1) totalSpeakers += 1;

        if (totalSpeakers >= 3) AchievementController.i.Unlock("CHATTERBOX");
    }

    public void ToggleFullscreen(bool state)
    {
        Screen.fullScreen = state;
    }

    public void ResetGoatData()
    {
        numGoatsFound = 0;
    }

    public void FoundGoat(bool showNotification = true)
    {
        numGoatsFound += 1;
        if (showNotification) GlobalUI.i.Do(UIAction.DISPLAY_GOATS, numGoatsFound);
        if (numGoatsFound == 1) AchievementController.i.Unlock("FIRST_GOAT");
        if (numGoatsFound == NumTotalGoats) AchievementController.i.Unlock("ALL_GOATS");
    }

    private void OnValidate()
    {
        int offset = 0;
        for (int i = 0; i < story.Count; i++) {
            story[i].name = story[i].nextQuest;
            if (story[i].customID) offset += 1;
            else story[i].ID = i-offset;
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

    public void SetCurrentStory(string text)
    {
        if (GlobalUI.i) GlobalUI.i.Do(UIAction.DISPLAY_QUEST_TEXT, text);
    }

    public void LoadStory(int ID)
    {
        if (ID == -1) { runtimeStory.Clear(); return; }
        runtimeStory = new List<StoryProgressionData>(story);
        while (runtimeStory.Count > 0 && runtimeStory[0].ID != ID) {
            runtimeStory.RemoveAt(0);
        }
        UpdateStoryDisplay();
    }

    public void AddCheckPoint(Transform point, int ID)
    {
        checkPoints.Add(new CheckPointData(point, ID));
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        GlobalUI.i.Do(UIAction.PAUSE);
        unlockCursor();
        AudioManager.i.Pause();
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
        if (Application.isPlaying) GlobalUI.i.Do(UIAction.RESUME);
        LockCursor();
        AudioManager.i.Resume();
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
        _player.position = GetCheckPoint(checkPoint);
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

    void UpdateStoryDisplay()
    {
        var text = runtimeStory[0].nextQuest;
        if (runtimeStory[0].playLong) GlobalUI.i.Do(UIAction.DISPLAY_QUEST_TEXT, text);
        else GlobalUI.i.Do(UIAction.DISPLAY_QUEST_TEXT_LONG, text);
    }

}
