using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PostMissionController : MonoBehaviour
{
    public Text successDescription;
    public Text narrative;
    public Text goldReward;
    public Text businessReward;
    public Text modReward;
    public Text itemReward;
    MissionFinalDetails d;
    int goldRewardAmount;
    public void Continue() {
        if (d.successful) {
            Game game = Game.instance;
            Stable player = game.playerStable;
            goldRewardAmount = (int)(d.goldReward * d.finalMod);
            
            if (d.businessReward != null && d.businessReward.benefit != Finance.Business.Benefit.None) {
                Game.instance.playerStable.finance.AddBusiness(d.businessReward);
            }
            Game.instance.playerStable.finance.AddRevenue(goldRewardAmount, "Mission Reward", "Mission");
            if (d.moveReward != null) {
                //Game.instance.playerStable.availableTrainings.Add(new Training() { cost = 300, duration = 10, moveToTrain = d.moveReward, type = Training.Type.Skill });
            }
            if (d.itemRewards != null) {
                foreach (Item i in d.itemRewards) {
                    Game.instance.playerStable.inventory.Add(Instantiate(i));
                }
            }
        }

        print("#TODO#Add Narrative and Mission to player history log");
        Destroy(d.gameObject);
        Game.instance.playerStable.ProcessDeadHeroes();
        Game.instance.playerStable.SetAllHeroesInactive();
        Game.instance.playerStable.contracts.Remove(Game.instance.playerStable.activeContract);
        Game.instance.playerStable.activeContract = null;
        SceneManager.LoadScene("StableManagement");
    }
    public void Start() {
        Time.timeScale = 1;
        d = FindObjectOfType<MissionFinalDetails>();
        string s = "";
        foreach (string n in d.narrative) {
            s += n + "\n";
        }
        if (!d.successful) {
            d.goldReward = 0;
            d.businessReward = null;
            d.moveReward = null;
            d.itemRewards = null;
        }
        narrative.text = s;
        goldReward.text = "Gold Revenue: " + (int)(d.goldReward * d.finalMod);
        if (d.moveReward != null) { goldReward.text += "\nMove Learned: "+d.moveReward.name; }
        successDescription.text = d.successful ? "Mission Successful" : "Mission Failed";
        modReward.text = "Mod: " + d.finalMod;
        if (d.businessReward != null && d.businessReward.benefit != Finance.Business.Benefit.None) {
            businessReward.text = "Other:\n" + d.businessReward.GetInfo();
        }
        itemReward.text = "Item Rewards\n";
        if (d.itemRewards != null) {
            foreach (var item in d.itemRewards) {
                itemReward.text += item.itemName + "\n";
            }
        }
    }
}
