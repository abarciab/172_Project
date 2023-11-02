using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void SaveGame()
    {
        var factMan = GetComponent<FactManager>();
        var gameMan = GetComponent<GameManager>();

        if (!gameMan || !factMan) return;
        var facts = factMan.GetFacts();
        if (facts.Count == 0) return;

        PlayerPrefs.SetInt("savedFacts", facts.Count);
        for (int i = 0; i < facts.Count; i++) {
            if (facts[i].doNotSave) continue;
            if (facts[i].addWhenSaving != null) {
                PlayerPrefs.SetString("fact" + (i + 1), facts[i].addWhenSaving.name);
            }
            PlayerPrefs.SetString("fact" + i, facts[i].name);
        }
        PlayerPrefs.SetInt("story", gameMan.GetID());
        PlayerPrefs.SetString("currentStory", gameMan.getCurrentStory());

        int auto = PlayerPrefs.GetInt("autoCheckpoint");
        int checkPoint = PlayerPrefs.GetInt("checkpoint");
        if (auto > checkPoint) PlayerPrefs.SetInt("checkpoint", auto);

        AudioManager.instance.SaveVolume();
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
            var fact = Resources.Load<Fact>("facts/" + savedNames[i]);
            if (fact) loadedFacts.Add(fact);
        }
        GetComponent<FactManager>().SetFacts(loadedFacts);
        GetComponent<GameManager>().SetCurrentStory(PlayerPrefs.GetString("currentStory"));
        GetComponent<GameManager>().LoadStory(PlayerPrefs.GetInt("story"));

        if (loadedFacts.Count == 0) FindObjectOfType<MovementTutorial>(true).Activate();
        else {
            Player.i.canRoll = true;
            Player.i.canRun = true;
        }

        //print("Game loaded succsessfully - " + loadedFacts.Count + " facts, story stage: " + GetComponent<GameManager>().GetID());
    }

    public void ResetGame()
    {
        PlayerPrefs.SetInt("savedFacts", 0);
        PlayerPrefs.SetInt("checkpoint", 0);
        PlayerPrefs.SetInt("story", 0);
        PlayerPrefs.SetString("currentStory", "");
        AudioManager.instance.ResetVolumeSaveData();
        //print("Game reset sucsessfully");
    }
}
