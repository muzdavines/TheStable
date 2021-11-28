using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VikingCrew.Tools.UI;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class Game : MonoBehaviour
{
    private static Game _instance;
    //public static Game instance { get { if (_instance == null) { _instance = GameObject.FindObjectOfType<Game>(); } if (_instance == null) { _instance = (new GameObject().AddComponent<Game>()); } return _instance; } }
    public static Game instance { get { if (_instance == null) { _instance = GameObject.FindObjectOfType<Game>(); } if (_instance == null) { _instance = Instantiate<GameObject>(Resources.Load<GameObject>("Game")).GetComponent<Game>(); } return _instance; } }
    public Stable playerStable;
    public List<Stable> otherStables = new List<Stable>();
    [System.Serializable]
    public class GameDate
    {
        public int day = 1;
        public int month = 1;
        public int year = 1000;
        public int dayOfWeek = 0;
        public void Advance()
        {
            day++;
            if (day > 28)
            {
                day = 1;
                month++;
            }
            if (month > 12)
            {
                month = 1;
                year++;
            }
            dayOfWeek++;
            if (dayOfWeek > 6)
            {
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
    [SerializeField]
    public MissionList missionContractList;
    public List<MoveModifier> modifierList;
    public void Start()
    {
        transform.name = "Game";
        DontDestroyOnLoad(gameObject);
        
        LoadModifiers();
    }
    public void Init() {
        freeAgentMarket.UpdateMarket();
        UpdateContractMarket();
        MissionContractTest();
    }
    [SerializeField]
    public List<MissionContractTest> mkt;
    void MissionContractTest() {
        mkt = new List<MissionContractTest>();
        var thismkt = new MissionContractTest();
        thismkt.name = "This";
        for (int i = 0; i < 3; i++) {
            mkt.Add(new MissionContractTest());
        }
    }
    public void Update() {
        if (Input.GetKeyDown(KeyCode.P)){
            MissionContractTest();
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
        print("UpdatingContractMarket");
        if (missionContractList != null) {
            foreach (MissionContract m in missionContractList.GetContracts()) {
                if (contractMarket.Any(i=>i.ID == m.ID))
                    { print("contract contained");  continue; }
                bool contractAvailable = true;
                print(m.attributeReq);
                if (m.attributeReq == "None" || playerStable.heroes.Any(i => i.GetCharacterttributeValue(m.attributeReq) >= m.attributeReqAmount)) { print("Attribute Req 1 met. "+m.attributeReq + "  "+m.attributeReqAmount); } else { contractAvailable = false; }
                if (m.attributeReq2 == "None" || playerStable.heroes.Any(i => i.GetCharacterttributeValue(m.attributeReq2) >= m.attributeReqAmount2)) { print("Attribute Req 2 met"); } else { contractAvailable = false; }
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

    public static int GetCharacterttributeValue (this Character c, string _attribute) {
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
