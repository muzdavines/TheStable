
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VikingCrew.Tools.UI;
using System.Globalization;
using System.IO;

using System.Text.RegularExpressions;
using HardCodeLab.TutorialMaster;
using UnityEngine.AI;

[System.Serializable]
public class Game : MonoBehaviour {
    private static Game _instance;
    public static Game instance { get { if (_instance == null) { _instance = GameObject.FindObjectOfType<Game>(); } if (_instance == null) { _instance = Instantiate(Resources.Load<GameObject>("Game")).GetComponent<Game>(); } return _instance; } }
    public const int XPGame = 500;
    public const int XPGoal = 500;
    public const int XPTackle = 250;
    public const int XPCombatDown = 250;
    public const int XPAssist = 500;
    
    public Stable playerStable;
    public List<Stable> otherStables = new List<Stable>();
    [SerializeField]
    public List<League> leagues = new List<League>();

    public List<NewsItem> news = new List<NewsItem>();
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
    
    public MissionList missionContractList;
    
    public League.Match activeMatch;
    public int tutorialStageFinished = 0;
    public string managementScreenToLoadOnStartup = "";
    
    public void Start() {
    
        transform.name = "Game";
        DontDestroyOnLoad(gameObject);
        
        LoadModifiers();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            foreach (League.Match m in Game.instance.leagues[0].schedule) {
                m.final = true;
            }
        }
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


    bool negativeBalanceNews;
    
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
        playerStable.SortHeroes();
        news.Add(new NewsItem(){body = "Welcome to the Stable. You will need all of your acumen to succeed.", date = Game.instance.gameDate, sender = "The Boss", subject = "Welcome"});
        news.Add(new NewsItem() { body = "All of the veteran players except for your Goalkeeper and Striker have left the team. You will have to make due with Amateurs until you can afford to hire new Heroes.", date = Game.instance.gameDate, sender = "The Boss", subject = "Amateurs" });
    }

    public void InitOtherStables() {
        otherStables = new List<Stable>();
        StableSO cheap = Resources.Load<StableSO>("Tier1/CheapDragonfruitInn");
        cheap = Instantiate(cheap);
        Stable firstStable = cheap.stable;
        for (int i = 0; i < firstStable.heroes.Count; i++) {
            firstStable.heroes[i] = Instantiate(firstStable.heroes[i]);
            firstStable.heroes[i].Init();
        }
        otherStables.Add(cheap.stable);
        
        for (int i = 1; i < 5; i++) {
            var thisStable = new Stable() { stableName = Stable.stableNameList[i] };
            thisStable.heroes = new List<Character>();
            List<int> poss = new List<int>();
            for (int z = 1; z <= 15; z++) {
                Debug.Log("#InitStable#" + z);
                poss.Add(z);
            }
            for (int x = 0; x < 8; x++) {
                Character thisHero = new Character();
                thisHero.currentPosition = Position.NA;
                if (x < 5) {
                    thisHero.activeInLineup = true;
                    int randIndex = Random.Range(0, poss.Count);
                    int thisPos = poss[randIndex];
                    print("#InitStable#Pos:" + (Position)(thisPos));
                    thisHero.currentPosition = (Position)(thisPos);
                    poss.RemoveAt(randIndex);
                }
                thisHero.name = Names.Warrior[Random.Range(0, Names.Warrior.Length)];
                thisHero.GenerateCharacter((Character.Archetype)(Random.Range(5, 8)), 1);
                thisStable.heroes.Add(thisHero);
            }
            var GKHero = new Character();
            GKHero.name = Names.Warrior[Random.Range(0, Names.Warrior.Length)];
            GKHero.GenerateCharacter(Character.Archetype.Goalkeeper);
            GKHero.currentPosition = Position.GK;
            GKHero.activeInLineup = true;
            thisStable.heroes.Add(GKHero);
            StableColorPalette colorPalette = Resources.Load<StableColorPalette>("Colors/League1");
            thisStable.primaryColor = colorPalette.randomPrimaryColor[Random.Range(0, colorPalette.randomPrimaryColor.Length)];
            thisStable.secondaryColor = colorPalette.randomSecondaryColor[Random.Range(0, colorPalette.randomSecondaryColor.Length)];
            otherStables.Add(thisStable);
        }
    }

    public void LoadModifiers() {
        string path = "Assets/Scripts/Moves/Resources/Mod/modJSON.json";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        var myText = reader.ReadToEnd();
        reader.Close();
        //modifierList = JsonConvert.DeserializeObject<List<MoveModifier>>(myText);
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


        if (playerStable.finance.gold < 0) {
            if (!negativeBalanceNews) {
                negativeBalanceNews = true;
                news.Add(new NewsItem() { body = "If you start a match with a negative balance, your heroes will not play for you that game. They don't work for free and need to know they will be paid on payday. Your roster will be filled with Amateurs if this happens.", date = Game.instance.gameDate, sender = "The Boss", subject = "Negative Balance" });
            }
            foreach (Character c in playerStable.heroes) {
                if (c.activeInLineup && c.archetype != Character.Archetype.Amateur && c.archetype != Character.Archetype.Goalkeeper) {
                    foreach (Character d in playerStable.heroes) {
                        if (!d.activeInLineup && d.archetype == Character.Archetype.Amateur) {
                            d.activeInLineup = true;
                            d.currentPosition = c.currentPosition;
                            c.activeInLineup = false;
                            break;
                        }
                    }
                }
            }
        }
        Helper.UpdateAllUI();
        playerStable.SortHeroes();
    }

    public void UpdateContractMarket() {
        if (missionContractList != null) {
            foreach (MissionContract m in missionContractList.GetContracts()) {
                if (contractMarket.Any(i=>i.ID == m.ID))
                    { print("contract contained");  continue; }
                bool contractAvailable = true;
                Debug.Log("#Contract#Name: " + m.name + " Trait1: " + m.traitReq?.traitName);
                if (m.traitReq == null || playerStable.heroes.Any(i => i.GetCharacterTraitValue(m.traitReq) >= m.traitLevelReq)) { contractAvailable = true; } else { contractAvailable = false; }
                if (m.traitReq2 == null || playerStable.heroes.Any(i => i.GetCharacterTraitValue(m.traitReq2) >= m.traitLevelReq2)) { } else { contractAvailable = false; }
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

}

public static class Helper {

    public static string NewLine(this string s) {
       return s.Replace("@n", "\n");
    }
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
        i += (thisDate.year - otherDate.year) * 359;
        i += (thisDate.month - otherDate.month) * 30;
        i += thisDate.day - otherDate.day;
        return Mathf.Abs(i);
    }

    public static NavMeshAgent TotalStop (this NavMeshAgent agent) {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        return agent;
    }

        public static bool IsExpired(MissionContract c) {
        return Game.instance.gameDate.IsOnOrAfter(c.executionDate, false);
    }

    public static com.ootii.Cameras.CameraController Cam() {
        return GameObject.FindObjectOfType<com.ootii.Cameras.CameraController>();
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
        return new Color(1, 1, 1, .8f);
    }
    public static void Speech(Transform _t, string text, float delay = 0f) {
        text = text.Replace("NEWLINE", "\n");
        Debug.Log("TEXT IS: "+text + "  "+_t.name);
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
    public static int GetCharacterTraitValue(this Character c, Trait trait) {
        foreach (Trait t in c.activeTraits){
            if (t.traitName == trait.traitName) {
                return t.level;
            }
        }
        return 0;
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
    public static Color[] primaryColor = { new Color(0.2862745f, 0.4f, 0.4941177f), new Color(0.4392157f, 0.1960784f, 0.172549f), new Color(0.3529412f, 0.3803922f, 0.2705882f), new Color(0.682353f, 0.4392157f, 0.2196079f), new Color(0.4313726f, 0.2313726f, 0.2705882f), new Color(0.5921569f, 0.4941177f, 0.2588235f), new Color(0.482353f, 0.4156863f, 0.3529412f), new Color(0.2352941f, 0.2352941f, 0.2352941f), new Color(0.2313726f, 0.4313726f, 0.4156863f) };
    public static Color[] secondaryColor = { new Color(0.7019608f, 0.6235294f, 0.4666667f), new Color(0.7372549f, 0.7372549f, 0.7372549f), new Color(0.1647059f, 0.1647059f, 0.1647059f), new Color(0.2392157f, 0.2509804f, 0.1882353f) };

    

}

public class NewsItem {
    public string subject;
    public string body;
    public Game.GameDate date;
    public string sender;
    public bool read;
}

public static class JsonHelper {
    public static T[] FromJson<T>(string json) {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T> {
            Items = array
        };
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
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

