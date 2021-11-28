using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTestController : MonoBehaviour
{
    [System.Serializable]
    public class CombatCharacter {
        public GameObject go;
        public Character character;
        public Combat.Intent intent;
        public Combat.Modifier modifier;
        public Combat.State state;
        public float damageDealtMod;
        public float damageTakenMod;
        public float parryMod;
        public float dodgeMod;
        public float stamina = 1;
        public int parriesLeft;
        public int charTargetIndex;
        public Character charTarget;
        public float priority;
        public int team;
        
        public CombatCharacter (MissionCharacter c) {
            go = c.gameObject;
            character = c.character;
        }
        public float GetDamageRoll() {
            float intentMod = modifier == Combat.Modifier.Full ? 1.5f : 1f;
            float strengthNumber = (character.strength * intentMod);
            float roll = Random.Range(0f, 5f);
            float weaponDamage = character.weapon.damage;
            print(character.myName + " damage roll; Str: " + strengthNumber + " Weapon: " + weaponDamage + " Roll: " + roll);
            return strengthNumber + roll + weaponDamage;

            //still needed: Buff, Debuff, State, Stamina
        }

        public float GetDefenseToDamageRoll() {
            float roll = Random.Range(0f, 5f);
            print(character.myName + " defend damage roll; Tough: " + character.toughness + " Armor: " + character.armor.defenseValue + " Roll: " + roll);
            return character.toughness + character.armor.defenseValue + roll;
            //still needed: Buff, Debuff, State, Stamina
        }
    }
    [System.Serializable]
    public class BattleEntry {
        public CombatCharacter attacker;
        public CombatCharacter defender;
        public bool didInterrupt;
        public bool didAttemptParry;
        public bool didLandKillingBlow;
        public enum Result { MeleeMiss, MeleeHit, MeleeArmor, MeleeShield, MeleeDodge, MeleeParry, Death}
        public Result result;
        public int healthDamage;
    }
    [SerializeField]
    public List<BattleEntry> battle = new List<BattleEntry>();
    [SerializeField]
    public List<CombatCharacter>[] teams = { new List<CombatCharacter>(), new List<CombatCharacter>()};
    // Start is called before the first frame update
    bool init;
    public bool combatActive;

    public void Init(List<Character> heroes, List<Character> enemies)
    {
        
        teams = new List<CombatCharacter>[2];
        teams[0] = new List<CombatCharacter>();
        for (int i = 0; i < heroes.Count; i++) {
            teams[0].Add(new CombatCharacter(heroes[i].currentMissionCharacter) { team = 0 });
            heroes[i].currentMissionCharacter.healthBar.Hide(false);
        }
        teams[1] = new List<CombatCharacter>();
        for (int x = 0; x < enemies.Count; x++) {
            teams[1].Add(new CombatCharacter(enemies[x].currentMissionCharacter) { team = 1 });
            enemies[x].currentMissionCharacter.healthBar.Hide(false);
        }
        CombatRound();
        combatActive = true;
        /*
        for (int i = 0; i < 2; i++) {
            teams[i] = new List<CombatCharacter>();
            for (int x = 0; x < ; x++) {
                CombatCharacter tempChar = new CombatCharacter() { go = GameObject.Find("Char" + i + "" + x), team = i };
                tempChar.character = tempChar.go.GetComponent<MissionCharacter>().character;
                tempChar.go.name = tempChar.character.name;
                
                tempChar.character.weapon = new Weapon() { itemName = "Axe", damage = 10 };
                tempChar.character.armor = new Armor() { defense = 10, itemName = "Chain Mail", condition = 100f };
                
                
                teams[i].Add(tempChar);
                
            }
        }*/
    }
    public int combatRound;
    // Update is called once per frame
    void Update()
    {
        /*if (!init && Time.frameCount > 60) {
            init = true;
            Init();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            CombatRound();
        }*/
        if (!combatActive) {
            return;
        }
        if (battle.Count == 0) {
            CombatRound();
        }
    }

    void CombatRound() {
        combatRound++;
        if (CheckIfCombatComplete()) { return; }
        SetInitialIntents();
        SetPriorities();
        SetOrderedList();
        DetermineOutcomes();
        SetIntentGraphics();
        PlayAnimations();
        RemoveDead();

        //add a bit of jockeying or backing off
    }

    
    void SetInitialIntents() {
        int myTeamIndex;
        for (int i = 0; i < 2; i++) {
            myTeamIndex = i;
            List<CombatCharacter> thisTeam = teams[i];
            for (int x = 0; x < thisTeam.Count; x++) {
                thisTeam[x].parriesLeft = 1;
                thisTeam[x].intent = Combat.Intent.Attack;
                thisTeam[x].modifier = (Combat.Modifier)Random.Range(1, 4);
                thisTeam[x].charTargetIndex = GetAttackTargetIndex(myTeamIndex);
                thisTeam[x].charTarget = teams[myTeamIndex.Other()][thisTeam[x].charTargetIndex].character;
                print(thisTeam[x].character.myName + "  " + thisTeam[x].intent + " " + thisTeam[x].modifier);
            }
        }
    }

    void SetPriorities() {
        for (int team = 0; team < 2; team++) {
            List<CombatCharacter> thisTeam = teams[team];
            for (int character = 0; character < thisTeam.Count; character++) {
                thisTeam[character].priority = Random.Range(0,.95f) + (float)thisTeam[character].intent + (float)(thisTeam[character].modifier);
            }
        }
    }
    [SerializeField]
    public List<CombatCharacter> chars;
    void SetOrderedList() {
        chars = new List<CombatCharacter>();
        for (int team = 0; team < 2; team++) {
            List<CombatCharacter> thisTeam = teams[team];
            for (int character = 0; character < thisTeam.Count; character++) {
                chars.Add(thisTeam[character]);
            }
        }
        //playerList.Sort((p1,p2)=>p1.score.CompareTo(p2.score));
        chars.Sort((p1, p2) => p1.priority.CompareTo(p2.priority));
        chars.Reverse();

    }
  
    void DetermineOutcomes() {
        for (int i = 0; i<chars.Count; i++) {
            CombatCharacter thisChar = chars[i];
            switch (thisChar.intent) {
                case Combat.Intent.Attack:
                    CalculateAttack(thisChar);
                    break;
            }
        }
    }

    void PlayAnimations() {
        nextBattleEntry = 0;
        PlayNextAnimation();
    }
   public int nextBattleEntry;
   public int numAnimsNeededToProgress;
    public void PlayNextAnimation() {
        if (nextBattleEntry >= battle.Count) {
            battle = new List<BattleEntry>();
            nextBattleEntry = 0;
            return;
        }
        BattleEntry b = battle[nextBattleEntry];
        
        numAnimsNeededToProgress = 2;
        b.attacker.go.GetComponent<MissionCharacter>().DoCombatAnimation(b, true);
        b.defender.go.GetComponent<MissionCharacter>().DoCombatAnimation(b, false);
        nextBattleEntry++;
    }

    void RemoveDead() {
        bool team2;
        for (int i = 0; i < 2; i++) {
            team2 = i == 0 ? false : true;
            List<CombatCharacter> thisTeam = teams[i];
            for (int x = 0; x < thisTeam.Count; x++) {
                if (thisTeam[x].state == Combat.State.Dead) {
                    thisTeam.RemoveAt(x--);
                    continue;
                }
            }
        }
    }

    bool CheckIfCombatComplete() {
        
        if (teams[0].Count == 0 || teams[1].Count == 0) {
            combatActive = false;
            
            GetComponent<MissionController>().EndCombat();
            return true;
        }
        return false;
    }

    public void AnimComplete() {
        print("AnimComplete Called");
        if (numAnimsNeededToProgress <= 0) {
            return;
        }
        numAnimsNeededToProgress--;
        if (numAnimsNeededToProgress <= 0) {
            PlayNextAnimation();
        }
    }

    void CalculateAttack(CombatCharacter c) {
        
        BattleEntry bEntry = new BattleEntry();
        CombatCharacter t = teams[c.team.Other()][c.charTargetIndex];
        bEntry.attacker = c;
        bEntry.defender = t;
        if (c.state == Combat.State.Dead || t.state == Combat.State.Dead) {
            return;
        }
            Combat.Modifier originalDefenderMod = t.modifier;
        Combat.Intent originalDefenderIntent = t.intent;
        if (c.intent != Combat.Intent.Attack) {
            return;
        }
        float attacker= 0;
        float defender = 0;
        
        if (t.intent == Combat.Intent.Attack && t.modifier == Combat.Modifier.Defensive) {
            t.intent = Combat.Intent.Defend;
            t.modifier = Combat.Modifier.Neutral;
        }
        //attacker
        attacker += c.character.melee;
        attacker += c.modifier.GetValue();
        attacker += Random.Range(-1.5f, 1.5f);

        //defender
        //Check for a parry


        float defStamina = t.stamina;
        if (t.intent == Combat.Intent.Defend && t.parriesLeft > 0) {
            if (true) { 
            //if (Random.Range(0, 100) > 20) {
                t.modifier = Combat.Modifier.Parry;
                t.parriesLeft -= 1;
                bEntry.didAttemptParry = true;
                print(t.character.myName + " trying to parry with: " + t.character.parry);
            }
        }
        float defAbility= t.modifier == Combat.Modifier.Parry ? t.character.parry : t.character.dodging;
        defAbility *= defStamina;
        if (t.modifier == Combat.Modifier.Armor) { defAbility *= .5f; }
        float defIntentVal = t.intent == Combat.Intent.Defend ? 5 : 0;
        defender += defIntentVal + defAbility;
        defender += Random.Range(-1.5f, 1.5f);
        
        
        //determine whether the defender will try to parry or dodge and add the check
        //upon a successful parry, check whether we will add the defender as an additional attacker against the current attacker, and then insert him in the order, or replace him

        //add state effects
        //add defending ability
        //add defending type to indicate whether a failed attack was missed, parried, or dodged
        if (attacker > defender) {
            if (t.intent == Combat.Intent.Attack) {
                t.intent = Combat.Intent.Neutral;
                bEntry.didInterrupt = true;
            }
            bEntry.result = BattleEntry.Result.MeleeHit;

            //need damage amount calc and addition to battle log
            //DAMAGE SECTION
            float aRoll = c.GetDamageRoll();
            float dRoll = t.GetDefenseToDamageRoll();
            float damageIndex = aRoll - dRoll;
            t.character.armor.DamageCondition(aRoll);
            int dotDamage = GetDotDamage(damageIndex);
            t.character.health -= dotDamage;
            
            print(c.character.myName +" Damage: " + aRoll + " vs " + dRoll + " Does "+ dotDamage + " dots.");
            bEntry.healthDamage = dotDamage;
            print(t.character.health + " dots left.");
            if (t.character.health<= 0) {
                bEntry.didLandKillingBlow = true;
                t.state = Combat.State.Dead;
                FindObjectOfType<MissionFinalDetails>().narrative.Add(c.character.myName + " killed a " + t.character.myName);
            }
            //END DAMAGE SECTION

        }
        else {
            switch (t.modifier) {
                case Combat.Modifier.Parry:
                    bEntry.result = BattleEntry.Result.MeleeParry;
                    break;
                case Combat.Modifier.Dodge:
                    bEntry.result = BattleEntry.Result.MeleeDodge;
                    break;
                case Combat.Modifier.Armor:
                    bEntry.result = BattleEntry.Result.MeleeMiss;
                    break;
                case Combat.Modifier.Neutral:
                    bEntry.result = BattleEntry.Result.MeleeMiss;
                    break;
            }
            
        }
        battle.Add(bEntry);
        
        t.modifier = originalDefenderMod;
        print(c.character.myName + " Atk: " + attacker + "  "+t.character.myName+" Def: " + defender);

        //eventually, group the attacks by turn and see whether any combos can be had for cool animations (2 or 3 parries,
        //or 2 attackers hitting the same guy for a double attack, etc)

    }

    void SetIntentGraphics() {
        foreach (CombatCharacter c in chars) {
            c.go.GetComponent<MissionCharacter>().healthBar.IntentIs(c.intent, c.modifier);
        }
    }

    int GetDotDamage(float n) {
        if (n < 4) {
            return 0;
        }
        if (n < 10) {
            return 1;
        }
        if (n < 18) {
            return 2;
        }
        if (n < 28) {
            return 3;
        }
        if (n < 40) {
            return 4;
        }
        return 5;
    }

    //Helpers
    int GetOtherTeam(bool isTeam2) {
        return isTeam2 ? 0 : 1;
    }

    int GetAttackTargetIndex(int myTeamIndex) {
        int targetIndex = Random.Range(0, teams[myTeamIndex.Other()].Count);
        
        return targetIndex;
    }

}
public static class intHelp {
    public static int Other(this int team) {
        return team == 0 ? 1 : 0;
    }
}