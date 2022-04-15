
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NewGameCreationController : MonoBehaviour
{

    public List<Character> heroes = new List<Character>();
    public int startingGold = 400;
    public List<MoveModifier> mods;
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
        Stable player = game.playerStable = new Stable();
        var warlord = activeStablemasterType;
        switch (warlord) {
            case "Warlord":
                player.warlord.InitWarlord(CharClass.Warrior);
                break;
            case "Wizard":
                player.warlord.InitWarlord(CharClass.Wizard);
                break;
            case "Rogue":
                player.warlord.InitWarlord(CharClass.Rogue);
                break;
        }
        int activeInLineup = 0;
        foreach (Character h in heroes) {
            Character thisHero = Instantiate<Character>(h);
            player.heroes.Add(thisHero);
            thisHero.activeInLineup = true;
            thisHero.currentPosition = (Position)(activeInLineup + 1);
            activeInLineup++;
        }
        foreach (Finance.Business business in startingBusinesses) {
            player.finance.AddBusiness(business);
        }
        for (int i = 0; i < 6; i++) {
            Character thisHero = new Character();
            thisHero.name = Names.Warrior[Random.Range(0, Names.Warrior.Length)];
            thisHero.GenerateCharacter((Character.Archetype)(Random.Range(5,8)), 1);
            thisHero.currentPosition = Position.NA;
            if (activeInLineup < 5) {
                thisHero.activeInLineup = true;
                thisHero.currentPosition = (Position)(activeInLineup + 1);
                activeInLineup++;
            }
            player.heroes.Add(thisHero);
        }
        var GKHero = new Character();
        GKHero.name = Names.Warrior[Random.Range(0, Names.Warrior.Length)];
        GKHero.GenerateCharacter(Character.Archetype.Goalkeeper);
        GKHero.currentPosition = Position.GK;
        GKHero.activeInLineup = true;
        player.heroes.Add(GKHero);
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
