using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using UnityEngine.SceneManagement;
using com.ootii.Cameras;
using FIMSpace.Generating;
using PsychoticLab;

public class MissionController : MonoBehaviour
{

    public Text update;
    public List<MissionPOI> pois;
    public int stageNum = -1;
    public MissionContract contract;
    public List<Character> heroes;
    public List<Character> currentEnemies;
    public List<StableCombatChar> allChars;
    public GameObject currentStage;
    public bool initialized;
    public CombatController combatController;
    public StableCombatChar currentActiveStepChar;
    public bool stageCompleteFired;
    public bool missionActive;
    public bool missionFailed = false;
    public MissionFinalDetails details;
    public AudioSource audioSource;
    public BuzzPanelController buzz;
    public HeroUIController heroUI;
    public HeroUIController enemyUI;
    public GameObject easyDungeon;
    
    // Start is called before the first frame update
    void Start()
    {
        if (FindObjectOfType<Game>() == null && contract == null) {
            return;
        }

        timeout = Mathf.Infinity;
        contract = null;
        print("Start Init");
        Init();
    }

    public void Init() {
        if (initialized) { return; }
        initialized = true;
        stageCompleteFired = false;
        print("Init");
        if (contract == null) {
            contract = Game.instance.playerStable.activeContract;
        }

        combatController = GetComponent<CombatController>();
        
        if (FindObjectOfType<Game>() != null) {
            heroes = new List<Character>();
            foreach (Character c in Game.instance.playerStable.heroes) {
                if (c.activeForNextMission) {
                    c.returnDate = Game.instance.gameDate.Add(7);
                    heroes.Add(c);
                }
            }
        }

        CreateNextStage();
        GameObject go = new GameObject();
        go.name = "FinalDetails";
        details = go.AddComponent<MissionFinalDetails>();
        details.goldReward = contract.goldReward;
        details.businessReward = contract.businessReward;
        details.moveReward = contract.moveReward;
        details.itemRewards = contract.itemRewards;
        details.finalMod = 1;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0;
        
    }

    public void NextStage()
    {
        StartCoroutine(NextStageDelay());
    }
    IEnumerator NextStageDelay()
    {
        yield return new WaitForSeconds(8.0f);
        CreateNextStage();
    }
    public void CreateNextStage() {
        stageNum++;
        timeout = Mathf.Infinity;
        buzz.Reset();
        if (stageNum >= contract.stages.Count) {
            MissionComplete();
            return;
        }
        if (currentStage != null) { Destroy(currentStage); }

        if (contract.stages[stageNum].loadName != "EasyDungeon") {
            GameObject stageToLoad = Resources.Load<GameObject>("Stages/" + contract.stages[stageNum].loadName);
            currentStage = Instantiate<GameObject>(stageToLoad);
            
            currentStage.transform.position = Vector3.zero;
            //currentStage.GetComponent<NavMeshSurface>().BuildNavMesh();
            pois = currentStage.GetComponentInChildren<POIController>().pois;
            foreach (MissionPOI poi in pois) {
                poi.gameObject.SetActive(false);
            }
            pois[0].gameObject.SetActive(true);
            Camera cam = Camera.main;
            SpawnLocController spawns = currentStage.GetComponentInChildren<SpawnLocController>();
            cam.transform.position = spawns.spawnLocs[0].transform.position + new Vector3(5, 15, 5);
            cam.transform.LookAt(spawns.spawnLocs[0].position);
            stageCompleteFired = false;
           // SortTransforms(pois, currentStage.GetComponentInChildren<SpawnLocController>().transform.position);
            if (stageNum <= 0) {
                StartCoroutine(DelaySpawnChars());
            }
            else {
                foreach (StableCombatChar c in allChars) {
                    if (c.myCharacter.incapacitated) { continue; }
                    c.GetComponent<NavMeshAgent>().enabled = false;
                    c.transform.position = currentStage.GetComponentInChildren<SpawnLocController>().spawnLocs[Random.Range(0, 4)].position;
                    c.GetComponent<NavMeshAgent>().enabled = true;
                }
            }
            
            Helper.UIUpdate(null);
        }
        else {

            currentStage = easyDungeon;
            easyDungeon.SetActive(true);
            easyDungeon.GetComponent<BuildPlannerExecutor>().Generate();
            StartCoroutine(DelayNavMesh(easyDungeon));
        }
        
        
        //currentStage.transform.position = Vector3.zero;

    }

    IEnumerator DelayNavMesh(GameObject easyDungeon) {
        yield return new WaitForSeconds(3.0f);
        easyDungeon.GetComponent<NavMeshSurface>().BuildNavMesh();

        var poiArray = FindObjectsOfType<MissionPOI>();
        print(poiArray.Length + " POIS");
        pois = poiArray.ToList();
        foreach (MissionPOI poi in pois) {
            poi.gameObject.SetActive(false);
            (poi as MissionPOICombat).PopulateEnemies(1);
        }
        pois[0].gameObject.SetActive(true);
        
        Camera cam = Camera.main;
        SpawnLocController spawns = currentStage.GetComponentInChildren<SpawnLocController>();
        cam.transform.position = spawns.spawnLocs[0].transform.position + new Vector3(5, 15, 5);
        cam.transform.LookAt(spawns.spawnLocs[0].position);
        stageCompleteFired = false;
        SortTransforms(pois, currentStage.GetComponentInChildren<SpawnLocController>().transform.position);
        if (stageNum <= 0) {
            StartCoroutine(DelaySpawnChars());
        }
        else {
            foreach (StableCombatChar c in allChars) {
                if (c.myCharacter.incapacitated) { continue; }
                c.GetComponent<NavMeshAgent>().enabled = false;
                c.transform.position = spawns.spawnLocs[Random.Range(0, 4)].position;
                c.GetComponent<NavMeshAgent>().enabled = true;
            }
        }
        Helper.UIUpdate(null);
        //move heroes to spawn locations
    }
    /* public void CreateNextStage() {
         stageNum++;
         buzz.Reset();
         if (stageNum >= contract.stages.Count)
         {
             MissionComplete();
             return;
         }
         if (currentStage != null) { Destroy(currentStage);}
         GameObject stageToLoad = Resources.Load<GameObject>("Stages/" + contract.stages[stageNum].loadName);
         currentStage = Instantiate<GameObject>(stageToLoad);
         currentStage.transform.position = Vector3.zero;
         //currentStage.GetComponent<NavMeshSurface>().BuildNavMesh();
         pois = currentStage.GetComponentInChildren<POIController>().pois;
         foreach (MissionPOI poi in pois) {
             poi.gameObject.SetActive(false);
         }
         pois[0].gameObject.SetActive(true);
         Camera cam = Camera.main;
         SpawnLocController spawns = currentStage.GetComponentInChildren<SpawnLocController>();
         cam.transform.position = spawns.spawnLocs[0].transform.position + new Vector3(5,15,5);
         cam.transform.LookAt(spawns.spawnLocs[0].position);
         stageCompleteFired = false;
         if (stageNum <= 0) {
             StartCoroutine(DelaySpawnChars());
         }
         else {
             foreach (StableCombatChar c in allChars) {
                 if (c.myCharacter.incapacitated) { continue; }
                 c.GetComponent<NavMeshAgent>().enabled = false;
                 c.transform.position = currentStage.GetComponentInChildren<SpawnLocController>().spawnLocs[Random.Range(0, 4)].position;
                 c.GetComponent<NavMeshAgent>().enabled = true;
             }
         }
         Helper.UIUpdate(null);

         //move heroes to spawn locations

     }*/
    void DamageDetails() {
        MissionFinalDetails details = FindObjectOfType<MissionFinalDetails>();
        details.damageDetails = new List<MissionFinalDetails.DamageDetails>();
        foreach (var h in heroes) {
            details.damageDetails.Add(new MissionFinalDetails.DamageDetails() { character = h, stats = h.thisQuestStats });
            h.careerQuestStats.Add(h.thisQuestStats);
        }
    }
    public void MissionComplete() {
        print("Rewards");
        DamageDetails();
        SceneManager.LoadScene("PostMission");

    }
    public void MissionFailed() {
        missionFailed = true;
        DamageDetails();
        FindObjectOfType<MissionFinalDetails>().successful = false;
        stageNum = 1000000;
        SceneManager.LoadScene("PostMission");
    }
    IEnumerator DelaySpawnChars()
    {
        yield return new WaitForSeconds(5.0f);
        var theseChars = SpawnChars();
        heroUI.Init(theseChars);
    }
    public List<StableCombatChar> SpawnChars() {
        return SpawnChars(heroes, currentStage.GetComponentInChildren<SpawnLocController>().spawnLocs);
    }

    public List<StableCombatChar> SpawnChars(List<Character> chars, List<Transform> spawns, int team = 0) {
        return BaseSpawnChars(chars, spawns, team);
    }

   List<StableCombatChar> BaseSpawnChars(List<Character> chars, List<Transform> spawns, int team = 0) {
        print("Spawn Base Chars");
        List<StableCombatChar> theseChars = new List<StableCombatChar>();
        for (int i = 0; i < chars.Count; i++) {
            if (chars[i].incapacitated) { continue; }
            Character thisBaseChar = chars[i];
            thisBaseChar.thisQuestStats = new QuestStats();
            GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(thisBaseChar.modelName), spawns[i].transform.position, Quaternion.identity);
            StableCombatChar thisChar = co.GetComponent<StableCombatChar>();
            thisChar.fieldSport = false;
            thisChar.myCharacter = thisBaseChar;
            thisChar.team = team;
            thisChar.combatFocus = thisBaseChar.combatFocus;
            
            thisChar.GetComponent<SCModelSelector>()?.Init(thisBaseChar.modelNum, thisBaseChar.skinNum);
            thisChar.GetComponent<CharacterRandomizer>()?.Init(thisBaseChar, team == 0 ? Game.instance.playerStable.primaryColor : Color.gray, team == 0 ? Game.instance.playerStable.secondaryColor : Color.black);
            thisChar.Init();
            co.GetComponent<NavMeshAgent>().enabled = false;
            co.transform.position = spawns[i].transform.position;
            co.GetComponent<NavMeshAgent>().enabled = true;
            theseChars.Add(thisChar);
        }
        UpdateChars();
        var cam = FindObjectOfType<com.ootii.Cameras.CameraController>();
        cam.transform.position = allChars[0].transform.position + new Vector3(10,20,10);
        cam.transform.LookAt(allChars[0].transform);
        cam.Anchor = allChars[0].transform;
        return theseChars;
        
    }

   private float timeout;
   public void CheckTimeoutPOI() {
       if (Time.time > timeout) {
           timeout = Mathf.Infinity;
           foreach (var h in allChars) {
               if (h.team == 0) {
                   h.agent.enabled = false;
                   h.transform.position = pois[0].col.transform.position;
                   h.agent.enabled = true;
               } 
           }
        } 
   }
    // Update is called once per frame
    void Update() {
        if (Time.timeSinceLevelLoad < 15) {
            return;}
        CheckTimeoutPOI();
        if (!stageCompleteFired)
        {
            if (pois.Count == 0)
            {
                stageCompleteFired = true;
                NextStage();
            }
        }
    }
    MissionPOICombat poiCombat;
    public void BeginCombat(List<Character> enemies, List<Transform> enemySpawnLoc, MissionPOICombat _poiCombat) {
        Debug.Log("#Combat#Combat Begin");
        timeout = Mathf.Infinity;
        currentEnemies = new List<Character>();
        foreach (Character e in enemies) {
            currentEnemies.Add(Instantiate<Character>(e).Init());
        }
        var theseEnemies = SpawnChars(currentEnemies, enemySpawnLoc, 1);
        foreach (StableCombatChar t in theseEnemies) {
            t.team = 1;
            t.CombatIdle();
        }
        foreach (StableCombatChar s in allChars) {
            s.team = 0;
            s.CombatIdle();
        }
        Debug.Log("#TODO#INIT Combat Controller");
        combatController.Init(allChars, theseEnemies);
        poiCombat = _poiCombat;
    }

    public void EndCombat(bool success = true) {
        print("Combat Complete");
        SetAllHeroesToCombat(false);
        foreach (StableCombatChar c in allChars) {
            Debug.Log("#TODO#Hide Health Bar");
            //c.healthBar.Hide(true);
            if (c.health > 0) {
                c.MissionIdle();
                //Helper.Cam().SetTarget(c.transform);
            }
        }
        poiCombat.Resolve(success);


    }
    public void POITriggered(MissionPOI poi, Collider character) {
        //make character walk to target of POI
        //need to control which stage of the mission step we are at
        timeout = Mathf.Infinity;
        missionActive = true;
        StableCombatChar thisChar = character.GetComponent<StableCombatChar>();
        thisChar.MissionMoveTo(poi.targetPos);
    }

    public void SetAllHeroesWalkTo(Transform walkTo, bool includeActiveChar, StableCombatChar activeChar) {
        foreach (StableCombatChar c in allChars) {
            if (!includeActiveChar) { if (c == activeChar) continue; }
            c.MissionMoveTo(walkTo);
        }
    }
    public void SetAllHeroesToCombat(bool shouldCombat) {
        foreach (StableCombatChar c in allChars) {
            Debug.Log("#TODO#SetCombatComponents");
            //c.SetCombatComponents(shouldCombat);
        }
    }

    public void SetAllHeroesDontAct(StableCombatChar exception = null) {
        print("SetAllHeroesDontAct "+exception.name);
        foreach (StableCombatChar m in allChars) {
            if (exception !=null && m != exception) {
                m.MissionIdleDontAct();
            }
        }
    }

    public void SetAllHeroesToNextPOI() {
        print("SetAllHeroesToNextPOI");
        if (missionActive) {
            return;
        }
        if (combatController.combatActive) { return; }
        
        foreach (StableCombatChar c in allChars) {
            c.FindNextStep();
        }
    }
    public Transform GetNextPOI() {
        timeout = Time.time + 20;
        if (pois.Count == 0) {
            print("NO MORE STEPS");
            return null;
        }

        if (pois.Count == 1) {
            pois[0].isFinalForMission = true;
        }
        pois[0].gameObject.SetActive(true);
        com.ootii.Cameras.CameraController _cam = FindObjectOfType<com.ootii.Cameras.CameraController>();
        if (pois[0].cameraAngle != Vector3.zero) {
            _cam.AnchorOffset = pois[0].cameraAngle;
        }
        _cam.SetTarget(allChars[0].transform);
        if (pois[0].backgroundMusic != null) {
            audioSource.clip = pois[0].backgroundMusic;
            audioSource.Play();
        }
        return pois[0].targetPos;
    }
    public void RemovePOI(MissionPOI m) {
        missionActive = false;
        for (int i = 0; i<pois.Count; i++) {
            if (pois[i] = m) {
                pois.RemoveAt(i);
                return;
            }
        }
        if (pois.Count != 0) {
            pois[0].gameObject.SetActive(true);
        }
    }

    public void UpdateChars() {
        var _chars = FindObjectsOfType<StableCombatChar>();
        allChars = new List<StableCombatChar>();
        foreach (var _c in _chars) {
            if (_c.team == 0) {
                allChars.Add(_c);
            }
        }
    }

    void SortTransforms(List<MissionPOI> transforms, Vector3 referencePosition) {
        transforms.Sort((t1, t2) => {
            float distance1 = Vector3.Distance(t1.transform.position, referencePosition);
            float distance2 = Vector3.Distance(t2.transform.position, referencePosition);
            return distance1.CompareTo(distance2);
        });
    }
    /*TODO: Hierarchy system of required steps so if things go sideways,
     * once all characters are idle they can return to the actual mission
     */
}
