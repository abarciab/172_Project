using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.InputSystem;

public class SaveManager : MonoBehaviour
{
    [SerializeField] List<Fact> allFacts = new List<Fact>();

    public void SaveGame()
    {
        var factMan = GetComponent<FactManager>();
        var gameMan = GetComponent<GameManager>();

        if (!gameMan || !factMan) return;
        var facts = factMan.GetFacts();
        if (facts.Count == 0) return;

        PlayerPrefs.SetInt("savedFacts", facts.Count);
        for (int i = 0; i < facts.Count; i++) {
            PlayerPrefs.SetString("fact" + i, facts[i].name);
        }
        PlayerPrefs.SetInt("story", gameMan.GetID());
        PlayerPrefs.SetString("currentStory", gameMan.getCurrentStory());

        int auto = PlayerPrefs.GetInt("autoCheckpoint");
        int checkPoint = PlayerPrefs.GetInt("checkpoint");
        if (auto > checkPoint) PlayerPrefs.SetInt("checkpoint", auto);

        print("Game saved succsessfully - " + facts.Count + " facts, story stage: " + gameMan.GetID());
    }

    public void LoadGame()
    {
        List<Fact> loadedFacts = new List<Fact>();
        List<string> savedNames = new List<string>();

        int count = PlayerPrefs.GetInt("savedFacts");
        for (int i = 0; i < count; i++) {
            savedNames.Add(PlayerPrefs.GetString("fact" + i));
        }
        for (int i = 0; i < savedNames.Count; i++) {
            foreach (var f in allFacts) {
                if (f.name == savedNames[i]) loadedFacts.Add(f);
            }
        }
        GetComponent<FactManager>().SetFacts(loadedFacts);
        GetComponent<GameManager>().SetCurrentStory(PlayerPrefs.GetString("currentStory"));
        GetComponent<GameManager>().LoadStory(PlayerPrefs.GetInt("story"));
        print("Game loaded succsessfully - " + loadedFacts.Count + " facts, story stage: " + GetComponent<GameManager>().GetID());
    }

    public void ResetGame()
    {
        PlayerPrefs.SetInt("savedFacts", 0);
        PlayerPrefs.SetInt("checkpoint", 0);
        PlayerPrefs.SetInt("story", 0);
        PlayerPrefs.SetString("currentStory", "");
        print("Game reset sucsessfully");
    }
}
