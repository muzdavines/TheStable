using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VikingCrew.Tools.UI;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using HardCodeLab.TutorialMaster;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Game : MonoBehaviour {
    private static Game _instance;
    public static Game instance { get { if (_instance == null) { _instance = GameObject.FindObjectOfType<Game>(); } if (_instance == null) { _instance = Instantiate(Resources.Load<GameObject>("Game")).GetComponent<Game>(); } return _instance; } }

    public Stable playerStable;
    public List<Stable> otherStables = new List<Stable>();
    [SerializeField]
    public List<League> leagues = new List<League>();
    [System.Serializable]
    public class GameDate {
        public int day = 1;
        public int month = 1;
        public int year = 1000;
        public int dayOfWeek = 0;
        public void Advance() {
            day++;
            if (day > 28) {
                day = 1;
                month++;
            }
            if (month > 12) {
                month = 1;
                year++;
            }
            dayOfWeek++;
            if (dayOfWeek > 6) {
                dayOfWeek = 0;
            }

        }

        public string GetDateString() {
            return year + "." + month + "." + day;
        }
    }
    [SerializeField]
    public GameDate gameDate = new GameDate();
    [SerializeField]
    public FreeAgentMarket freeAgentMarket = new FreeAgentMarket();
    [SerializeField]
    public List<MissionContract> contractMarket = new List<MissionContract>();
    public List<MissionContractSave> contractMarketSave = new List<MissionContractSave>();
    public MissionList missionContractList;
    public List<MoveModifier> modifierList;
    public League.Match activeMatch;
    public void Start() {
        transform.name = "Game";
        DontDestroyOnLoad(gameObject);
        
        LoadModifiers();
    }
    public void OnLoad() {
        freeAgentMarket.OnLoad();
        LoadMarketContracts();
        playerStable.OnLoad();
    }
    public void TutorialComplete(Tutorial thisTutorial) {
        tutorialsComplete.Add(thisTutorial.Name);
    }
    public void CheckIfTutorialComplete(Tutorial thisTutorial) {
        if (tutorialsComplete.Contains(thisTutorial.Name)) {
            thisTutorial.Stop();
        }
    }

    public List<string> tutorialsComplete;


    public void Init() {
        freeAgentMarket.UpdateMarket();
        InitOtherStables();
        UpdateContractMarket();
        leagues = new List<League>();
        leagues.Add(new League() { leagueLevel = 0, leagueName = "Premiere League" });
        playerStable.stableName = "Player's Stable";
        leagues[0].InitLeague();
        if (playerStable == null) {
            playerStable.inventory = new List<Item>();
        }
        for (int i=0; i < 4; i++) {
           // playerStable.inventory.Add(Instantiate(Resources.Load<Item>("LongswordSO")));
        }
        //playerStable.inventory.Add(Instantiate(Resources.Load<Item>("BowSO")));
    }

    public void InitOtherStables() {
        otherStables = new List<Stable>();
        for (int i = 0; i < 5; i++) {
            var thisStable = new Stable() { stableName = Stable.stableNameList[i] };
            thisStable.heroes = new List<Character>();
            for (int x = 0; x < 8; x++) {
                Character thisHero = new Character();
                if (x < 5) {
                    thisHero.activeInLineup = true;
                }
                thisHero.name = Names.Warrior[Random.Range(0, Names.Warrior.Length)];
                thisHero.GenerateCharacter((Character.Archetype)(Random.Range(0, 4)), 1);
                thisStable.heroes.Add(thisHero);
            }
            otherStables.Add(thisStable);
        }
    }

    public void LoadModifiers() {
        string path = "Assets/Scripts/Moves/Resources/Mod/modJSON.json";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        var myText = reader.ReadToEnd();
        reader.Close();
        modifierList = JsonConvert.DeserializeObject<List<MoveModifier>>(myText);
    }

    public void Advance() {
        if (IsPlayerMatchDay()) {
            FindObjectOfType<DailyPopup>().Popup("Cannot Advance. Player Match Day. See League.");
            return;
        }
        gameDate.Advance();
        playerStable.ExpireContracts();
        ExpireMarketContracts();
        ProcessTraining();
        if (gameDate.dayOfWeek == 0) {
            //money stuff
            Finance f = playerStable.finance;
            freeAgentMarket.UpdateMarket();
            UpdateContractMarket();
            if (gameDate.day == 1 && gameDate.month == 1 && gameDate.year == 1000) { return; }
            f.ProcessBusinesses();
            foreach (Character c in playerStable.heroes) {
                f.AddExpense(c.contract.weeklySalary, LedgerAccount.Personnel);
                c.contract.weeksLeft--;
            }
        }

        //Process Relationship stuff
        //Process Hero Quests
        //Process Correspondence/Diplomacy
        //Process Contract Offers
        //Process Mission Offers/Bids
        //Process Wages if necessary (weekly)
        //Process Interim stories - pop up stories, one offs that affect some aspect of the stable
        //Process facilities upgrades
        //Process business income
        //Create notes for every upgrade and dump them in an "Inbox", see FM2020

        Helper.UpdateAllUI();

    }

    public void UpdateContractMarket() {
        if (missionContractList != null) {
            foreach (MissionContract m in missionContractList.GetContracts()) {
                if (contractMarket.Any(i=>i.ID == m.ID))
                    { print("contract contained");  continue; }
                bool contractAvailable = true;
                print(m.attributeReq);
                if (m.attributeReq == "None" || playerStable.heroes.Any(i => i.GetCharacterAttributeValue(m.attributeReq) >= m.attributeReqAmount)) { print("Attribute Req 1 met. "+m.attributeReq + "  "+m.attributeReqAmount); } else { contractAvailable = false; }
                if (m.attributeReq2 == "None" || playerStable.heroes.Any(i => i.GetCharacterAttributeValue(m.attributeReq2) >= m.attributeReqAmount2)) { print("Attribute Req 2 met"); } else { contractAvailable = false; }
                if (!contractAvailable) { continue; }
                m.executionDate = Helper.Today().Add(10);
                contractMarket.Add(m);
            }
            return;
        }
        for (int i = 0; i < 2; i++) {
            MissionContract m = new MissionContract();
            m.GenerateRandom(Random.Range(1, 3));
            contractMarket.Add(m);
        }
        
    }
    public void ExpireMarketContracts() {
        contractMarket.RemoveAll(Helper.IsExpired);
        MarketContractScrollerController scsc = GameObject.FindObjectOfType<MarketContractScrollerController>();
        
    }
    public void PrepContractMarketForSave() {
        var saveList = new List<MissionContractSave>();
        for (int i = 0; i < contractMarket.Count; i++) {
            saveList.Add(new MissionContractSave().CopyValues(contractMarket[i]));
        }
        contractMarketSave = saveList;
    }
    public void LoadMarketContracts() {
        if (contractMarketSave!=null && contractMarketSave.Count > 0) {
            contractMarket = contractMarketSave.LoadMissionContracts();
        }
        contractMarketSave = new List<MissionContractSave>();
    }
    public void ProcessTraining() {
        foreach (Character c in playerStable.heroes) {
            if (c.currentTraining == null) {
                continue;
            }
            if (c.ProcessTraining()) {  //return true if processed and complete
                c.currentTraining = new Training();
            }
        }
    }

    public bool IsPlayerMatchDay() {
        bool isMatchDay = false;
        foreach (League.Match match in Game.instance.leagues[0].schedule) {
            
            if (!match.IsPlayerMatch()) {
                continue;
            }
            if (match.date.IsToday()) {
                if (match.final) {
                    
                }
                else {
                    
                    isMatchDay = true;
                    
                    
                }
                
                break;
            }
            else {
                
            }
        }
        return isMatchDay;
    }
   

    public void PrepForSave() {
        freeAgentMarket.PrepForSave();
        PrepContractMarketForSave();
        playerStable.PrepForSave();
    }

}

public static class Helper {
    public static void PlayOneShot (AudioClip clip) {
        if (clip == null) { return; }
        Debug.Log("Play");
        GameObject.Find("OneShot").GetComponent<AudioSource>().PlayOneShot(clip);
    }
    public static Game.GameDate Add (this Game.GameDate thisDate, int days) {
        Game.GameDate newDate = new Game.GameDate() { day = thisDate.day, month = thisDate.month, year = thisDate.year, dayOfWeek = thisDate.dayOfWeek };
        
        for (int i = 0; i < days; i++) {
            newDate.Advance();
        }
        return newDate;
    }

    public static bool IsAvailable(this Character c) {
        return !c.returnDate.IsOnOrAfter(Game.instance.gameDate, false);
    }
    public static bool IsOnOrAfter(this Game.GameDate thisDate, Game.GameDate otherDate, bool returnTrueOnDate = true) {
        if (thisDate.year > otherDate.year) { return true; } else if (thisDate.year < otherDate.year) { return false; }
        if (thisDate.month > otherDate.month) { return true; } else if (thisDate.month < otherDate.month) { return false; }
        if (returnTrueOnDate) {
            if (thisDate.day >= otherDate.day) { return true; } else { return false; }
        } else { if (thisDate.day > otherDate.day) { return true; } else { return false; } }
    }
    public static bool IsOn(this Game.GameDate thisDate, Game.GameDate otherDate) {
        return (thisDate.day == otherDate.day && thisDate.month == otherDate.month && thisDate.year == otherDate.year);
    }
    public static bool IsToday(this Game.GameDate thisDate) {
        return thisDate.IsOn(Helper.Today());
    }
    public static int DaysBetween(this Game.GameDate thisDate, Game.GameDate otherDate) {
        int i = 0;
        //int monthMod = thisDate.day < otherDate.day ? -1 : 0;
        i += (thisDate.month - otherDate.month) * 30;
        i += thisDate.day - otherDate.day;
        return Mathf.Abs(i);
    }

        public static bool IsExpired(MissionContract c) {
        return Game.instance.gameDate.IsOnOrAfter(c.executionDate, false);
    }

    public static CameraController Cam() {
        return Camera.main.GetComponent<CameraController>();
    }

    public static Canvas GetMainCanvas() {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases) {
            if (c.CompareTag("MainCanvas")) {
                return c;
            }
        }
        return null;
    }
    
    public static Color GetCellViewColor() {
        return new Color(1, 1, 1, .396f);
    }
    public static void Speech(Transform _t, string text, float delay = 0f) {
        text = text.Replace("NEWLINE", "\n");
        Debug.Log("TEXT IS: "+text);
        SpeechBubbleManager.Instance.AddDelayedSpeechBubble(delay, _t, text);
    }

    public static void UIUpdate(string text) {
        MissionController control = GameObject.FindObjectOfType<MissionController>();
        if (text == null) { control.update.text = ""; return; }
        control.update.text += "\n" + text;
        
    }

    public static Game.GameDate Today() {
        return Game.instance.gameDate;
    }

    public static int GetCharacterAttributeValue (this Character c, string _attribute) {
        _attribute = _attribute.ToLower();
        if (_attribute == null || _attribute == "none") {
            return 0;
        }
        
        Debug.Log("GetChar " + _attribute);
        return (int)typeof(Character).GetField(_attribute).GetValue(c);
    }

    public static string Title(this string s) {
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
        s = myTI.ToTitleCase(s);
        return s;
    }

    public static string Space(this string s) {
        s = Regex.Replace(s, "([A-Z0-9])", " $1").Trim();
        return s;
    }
    public static void UpdateAllUI() {
        foreach (UIElement e in GameObject.FindObjectsOfType<MonoBehaviour>().OfType<UIElement>()) {
            e.UpdateOnAdvance();
        }
    }

    public static MoveModifier GetModifier(this List<MoveModifier> mods, string modSearch) => mods.Where(m => m.modName == modSearch).FirstOrDefault();

}

public static class JsonHelper {
    public static T[] FromJson<T>(string json) {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }
    public static List<Character> LoadCharacters(this List<CharacterSave> source) {
        var returnList = new List<Character>();
        foreach (var character in source) {
            returnList.Add(character.LoadCharacter());
        }
        return returnList;
    }
    public static Character LoadCharacter(this CharacterSave source) {
        var thisChar = new Character();
        thisChar.strength = source.strength;
        thisChar.agility = source.agility;
        thisChar.reaction = source.reaction;
        thisChar.running = source.running;
        thisChar.swordsmanship = source.swordsmanship;
        thisChar.dualwielding = source.dualwielding;
        thisChar.dodging = source.dodging;
        thisChar.archery = source.archery;
        thisChar.toughness = source.toughness;
        thisChar.toughness = source.toughness;
        thisChar.intelligence = source.intelligence;
        thisChar.education = source.education;
        thisChar.motivation = source.motivation;
        thisChar.strategist = source.strategist;
        thisChar.economics = source.economics;
        thisChar.negotiating = source.negotiating;
        thisChar.insight = source.insight;
        thisChar.deception = source.deception;
        thisChar.intimidation = source.intimidation;
        thisChar.lockpicking = source.lockpicking;
        thisChar.pickpocketing = source.pickpocketing;
        thisChar.trapSetting = source.trapSetting;
        thisChar.trapDisarming = source.trapDisarming;
        thisChar.pugilism = source.pugilism;
        thisChar.martialarts = source.martialarts;
        thisChar.melee = source.melee;
        thisChar.parry = source.parry;
        thisChar.shieldDefense = source.shieldDefense;
        thisChar.survivalist = source.survivalist;
        thisChar.landNavigation = source.landNavigation;
        thisChar.hunting = source.hunting;
        thisChar.foraging = source.foraging;
        thisChar.herbLore = source.herbLore;
        thisChar.camping = source.camping;
        thisChar.attackmagic = source.attackMagic;
        thisChar.condition = source.condition;
        thisChar.sharpness = source.sharpness;
        thisChar.health = source.health;
        thisChar.maxStamina = source.maxStamina;
        thisChar.maxBalance = source.maxBalance;
        thisChar.maxMind = source.maxMind;
        thisChar.maxHealth = source.maxHealth;
        thisChar.knownMoves = source.knownMovesSave.LoadMoves();
        thisChar.activeMeleeMoves = source.activeMovesSave.LoadMoves();
        thisChar.startingArmor = source.startingArmor;
        thisChar.startingMeleeWeapon = source.startingWeapon;
        thisChar.armor = source.armor;
        thisChar.meleeWeapon = source.weapon;
        thisChar.mat = source.mat;
        thisChar.contract = source.contract;
        thisChar.currentTraining = source.currentTraining;
        thisChar.returnDate = source.returnDate;
        thisChar.activeForNextMission = source.activeForNextMission;
        thisChar.incapacitated = source.incapacitated;
        thisChar.modelName = source.modelName;
        thisChar.currentObject = source.currentObject;
        thisChar.currentMissionCharacter = source.currentMissionCharacter;
        thisChar.defensemagic = source.defenseMagic;
        thisChar.supportmagic = source.supportMagic;
        thisChar.name = source.name;
        return thisChar;
    }
    public static List<MissionContract> LoadMissionContracts(this List<MissionContractSave> source) {
        var returnList = new List<MissionContract>();
        foreach (var contract in source) {
            returnList.Add(contract.LoadMissionContract());
        }
        return returnList;
    }
    public static MissionContract LoadMissionContract(this MissionContractSave source) {
        var thisContract = new MissionContract();
        thisContract.description = source.description;
        thisContract.ID = source.ID;
        thisContract.contractType = source.contractType;
        thisContract.difficulty = source.difficulty;
        thisContract.stages = source.stages;
        thisContract.goldReward = source.goldReward;
        thisContract.businessReward = source.businessReward;
        thisContract.moveReward = source.moveReward;
        thisContract.executionDate = source.executionDate;
        thisContract.dayCost = source.dayCost;
        thisContract.minHeroes = source.minHeroes;
        thisContract.maxHeroes = source.maxHeroes;
        thisContract.attributeReq = source.attributeReq;
        thisContract.attributeReqAmount = source.attributeReqAmount;
        thisContract.attributeReq2 = source.attributeReq2;
        thisContract.attributeReqAmount2 = source.attributeReqAmount2;
        return thisContract;
    }

    public static List<Training> LoadTrainings(this List<TrainingSave> source) {
        var returnList = new List<Training>();
        foreach (var training in source) {
            returnList.Add(training.LoadTraining());
        }
        return returnList;
    }
    public static Training LoadTraining(this TrainingSave source) {
        var thisTraining = new Training();
        thisTraining.type = source.type;
        thisTraining.training = source.training;
        thisTraining.duration = source.duration;
        thisTraining.cost = source.cost;
        thisTraining.dateToTrain = source.dateToTrain;
        thisTraining.moveToTrain = source.moveToTrain;
        return thisTraining;
    }

    public static List<Move> LoadMoves(this List<MoveSave> source) {
        var returnList = new List<Move>();
        foreach (var move in source) {
            returnList.Add(move.LoadMove());
        }
        return returnList;
    }
    public static Move LoadMove(this MoveSave source) {
        var thisMove = new Move();
        thisMove.description = source.description;
        thisMove.cooldown = source.cooldown;
        thisMove.accuracy = source.accuracy;
        thisMove.staminaDamage = source.staminaDamage;
        thisMove.balanceDamage = source.balanceDamage;
        thisMove.mindDamage = source.mindDamage;
        thisMove.healthDamage = source.healthDamage;
        thisMove.keyPhysicalAttribute = source.keyPhysicalAttribute;
        thisMove.keyTechnicalAttribute = source.keyTechnicalAttribute;
        thisMove.limb = source.limb;
        thisMove.moveType = source.moveType;
        thisMove.moveWeaponType = source.moveWeaponType;
        thisMove.modifiers = source.modifiers;
        return thisMove;
    }


    [System.Serializable]
    private class Wrapper<T> {
        public T[] Items;
    }
}

public interface UIElement {
    void UpdateOnAdvance();
}

public interface PopupElement {
    void Close();
}

