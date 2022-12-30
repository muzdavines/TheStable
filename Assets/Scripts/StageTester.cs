using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageTester : MonoBehaviour
{

    public MissionContract contract;
    public bool active;
    public float timeScale = 1.0f;
    public List<Character> heroes = new List<Character>();
    public Slider slider;
    void Start()
    {
        slider.value = timeScale;
        
        if (!active) {
            return;
        }
        
        Time.timeScale = timeScale;
        Game game = Game.instance;
        Stable player = game.playerStable = new Stable();
        player.warlord.InitWarlord(StableMasterType.Warrior);
        if (heroes.Count == 0) {
            for (int i = 0; i < 4; i++) {
                Character c = new Character() { name = Names.Warrior[Random.Range(0, Names.Warrior.Length)], age = 18, modelName = "CharRogue", activeForNextMission = true };
                Weapon w = new Weapon() { condition = 100, damage = 10, itemName = "Long Sword" };
                Armor a = new Armor() { condition = 100, defense = 10, health = 0, itemName = "Chain Mail" };
                c.meleeWeapon = w;
                c.armor = a;
                player.heroes.Add(c);
            }
        } else {
            foreach (Character c in heroes) {
                Character thisChar = Instantiate<Character>(c);
                thisChar.activeForNextMission = true;
                thisChar.armor = Instantiate(thisChar.armor);
                thisChar.meleeWeapon = Instantiate(thisChar.meleeWeapon);
                player.heroes.Add(thisChar);
            }
        }
        player.activeContract = contract;
        FindObjectOfType<MissionController>().Init();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
    }
    public void UpdateSpeed() {
        timeScale = slider.value;
    }
    
}
