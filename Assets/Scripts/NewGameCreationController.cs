
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NewGameCreationController : MonoBehaviour
{

    public List<Character> heroes = new List<Character>();
    public int startingGold = 400;
    public List<Training> trainingAdds;
    public List<Item> startingItems;
    public List<Finance.Business> startingBusinesses;
    public string myText;
    public string activeStablemasterType;
   
    public void Start()
    {
        activeStablemasterType = "Warlord";
        
    }
    public void OnClick (string warlord)
    {
        CreateNewGame(warlord);
    }
    public void CreateNewGame(string sceneToLoad = "CutScene1") {

        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(sceneToLoad);
        Game game = Game.instance;
        Stable player = game.playerStable =(Instantiate<StableSO>(Resources.Load<StableSO>("StartingStable")).stable);
        var warlord = activeStablemasterType;
        player.warlord = new Warlord();
        switch (warlord) {
            case "Warlord":
                player.warlord.InitWarlord(StableMasterType.Warrior);
                break;
            case "Wizard":
                player.warlord.InitWarlord(StableMasterType.Wizard);
                break;
            case "Rogue":
                player.warlord.InitWarlord(StableMasterType.Rogue);
                break;
        }
        for (int i = 0; i<game.playerStable.heroes.Count; i++) {
            game.playerStable.heroes[i] = Instantiate(game.playerStable.heroes[i]);
            game.playerStable.heroes[i].Init();
        }
        Game.instance.playerStable.finance.AddRevenue(startingGold);
        //Game.instance.playerStable.availableTrainings.Add(new Training() { type = Training.Type.Attribute, training = "negotiating", duration = 2, cost = 50 });
        foreach (var training in trainingAdds) {
            Game.instance.playerStable.availableTrainings.Add(training);
        }
        Game.instance.playerStable.inventory = new List<Item>();
        foreach (var item in startingItems) {
            Game.instance.playerStable.inventory.Add(Instantiate(item));
        }
        Game.instance.missionContractList = Instantiate<MissionList>(Resources.Load<MissionList>("1000"));
        //Game.instance.playerStable.finance.businesses.Add(new Finance.Business() { benefit = Finance.Business.Benefit.Gold, description = "Market Stall in Genoa", duration = 12, number = 125 });
        if (sceneToLoad == "StableManagement") {
            Game.instance.tutorialStageFinished = 99;
        }
        Game.instance.Init();
        
        Destroy(gameObject);
    }
    public void ChangeStablemasterType(string s) {
        activeStablemasterType = s;
    }
    public void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            CreateNewGame("StableManagement");
        }
    }

}
