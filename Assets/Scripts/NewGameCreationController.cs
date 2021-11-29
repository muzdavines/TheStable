using Newtonsoft.Json;
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
    public string myText;

    public void Start()
    {
        if (heroes == null || heroes.Count == 0)
        {
            heroes.Add(new Character() { name = Names.Warrior[Random.Range(0, Names.Warrior.Length)], age = 18, modelName = "CharWarrior2" });
        }
        
        
    }
    public void OnClick (string warlord)
    {
        CreateNewGame(warlord);
    }
    public void CreateNewGame(string warlord)
    {
        
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("StableManagement");
        Game game = Game.instance;
        Stable player = game.playerStable = new Stable();

        switch (warlord) {
            case "Warrior":
                player.warlord.InitWarlord(CharClass.Warrior);
                break;
            case "Wizard":
                player.warlord.InitWarlord(CharClass.Wizard);
                break;
            case "Rogue":
                player.warlord.InitWarlord(CharClass.Rogue);
                break;
        }

        foreach (Character h in heroes) {
            Character thisHero = Instantiate<Character>(h);

            player.heroes.Add(thisHero);
        }
        Game.instance.playerStable.finance.AddRevenue(startingGold);
        //Game.instance.playerStable.availableTrainings.Add(new Training() { type = Training.Type.Attribute, training = "negotiating", duration = 2, cost = 50 });
        foreach (var training in trainingAdds) {
            Game.instance.playerStable.availableTrainings.Add(training);
        }
        Game.instance.missionContractList = Instantiate<MissionList>(Resources.Load<MissionList>("1000"));
        //Game.instance.playerStable.finance.businesses.Add(new Finance.Business() { benefit = Finance.Business.Benefit.Gold, description = "Market Stall in Genoa", duration = 12, number = 125 });
        Game.instance.Init();
        Destroy(gameObject);
    }

    public void LoadGame() {
        SceneManager.LoadScene("StableManagement");
    }

}
