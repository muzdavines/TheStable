using System.Collections;
using System.Collections.Generic;
using System.IO;
using com.ootii.Geometry;
using TMPro;
using UnityEngine;

public class BlueManaSaveLoad : MonoBehaviour {
    public string game;
    public string playerStable;
    public string otherStables;
    public string loadData;
    public TextMeshProUGUI status;
    public void Save() {
        Game.instance.playerStable.heroSaves = new List<string>();
        for (int i = 0; i < Game.instance.playerStable.heroes.Count; i++) {
            Game.instance.playerStable.heroSaves.Add(JsonUtility.ToJson(Game.instance.playerStable.heroes[i]));
        }

        foreach (var o in Game.instance.otherStables) {
            o.heroSaves = new List<string>();
            for (int i = 0; i < o.heroes.Count; i++) {
                o.heroSaves.Add(JsonUtility.ToJson(o.heroes[i]));
            }
        }
        //set player stable heroes save data
        //set by loop other stables hero save data
        //reset freeagent market
        //reset contract market
        game = JsonUtility.ToJson(Game.instance);
        WriteStringToFile("SaveGame.txt",game);
        status.text = "Game Saved.";
    }

    public void OnEnable() {
        status.text = "";
    }

    public void Load() {
        loadData = ReadStringFromFile("SaveGame.txt");
        JsonUtility.FromJsonOverwrite(loadData, Game.instance);
        Game.instance.playerStable.heroes = new List<Character>();
        foreach (var h in Game.instance.playerStable.heroSaves) {
            Character c = new Character();
            JsonUtility.FromJsonOverwrite(h, c);
            c.LoadArchetype(c.archetype);
            
            Game.instance.playerStable.heroes.Add(c);
        }

        foreach (var o in Game.instance.otherStables) {
            o.heroes = new List<Character>();
            foreach (var h in o.heroSaves) {
                Character c = new Character();
                JsonUtility.FromJsonOverwrite(h, c);
                c.LoadArchetype(c.archetype);
                o.heroes.Add(c);
            }
        }

        Game.instance.playerStable.heroSaves = new List<string>();
        foreach (var s in Game.instance.otherStables) {
            s.heroSaves = new List<string>();
        }

        Game.instance.playerStable.finance.FullReconciliation();
        Game.instance.missionContractList = Instantiate<MissionList>(Resources.Load<MissionList>("1000"));
        Game.instance.freeAgentMarket.UpdateMarket();
        Game.instance.UpdateContractMarket();
        Helper.UpdateAllUI();
        status.text = "Game Loaded.";
    }

    public void Resolution(string res) {
        switch (res) {
            case "900":
                Screen.SetResolution(1440,900, FullScreenMode.FullScreenWindow);
                break;
            case "1080":
                Screen.SetResolution(1920,1080, FullScreenMode.FullScreenWindow);
                break;
            case "1440":
                Screen.SetResolution(2560,1440, FullScreenMode.FullScreenWindow);
                break;
            case "2160":
                Screen.SetResolution(3840,2160, FullScreenMode.FullScreenWindow);
                break;
        }
    }

    public void WriteStringToFile(string filePath, string content) {
        Debug.Log(Application.dataPath + "/" + filePath);
        File.WriteAllText(Application.dataPath + "/" + filePath, content);
    }

    public string ReadStringFromFile(string filePath) {
        return File.ReadAllText(Application.dataPath + "/" + filePath);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
                Load();
            }
}
}
