using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using UnityEngine.SceneManagement;
using CoverShooter;

public class MissionController : MonoBehaviour
{

    public Text update;
    public List<MissionPOI> pois;
    public int stageNum = -1;
    public MissionContract contract;
    public List<Character> heroes;
    public List<Character> currentEnemies;
    public GameObject currentStage;
    public bool initialized;
    public CombatController combatController;
    public Character currentActiveStepChar;
    public bool stageCompleteFired;
    public bool missionActive;
    public bool missionFailed = false;
    public MissionFinalDetails details;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        if (FindObjectOfType<Game>()==null) { return; }
        print("Start Init");
        Init();
    }

    public void Init() {
        if (initialized) { return; }
        initialized = true;
        stageCompleteFired = false;
        print("Init");
        contract = Game.instance.playerStable.activeContract;
        combatController = GetComponent<CombatController>();
        heroes = new List<Character>();
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (c.activeForNextMission) {
                c.returnDate = Game.instance.gameDate.Add(7);
                heroes.Add(c);
            }
        }
        CreateNextStage();
        GameObject go = new GameObject();
        go.name = "FinalDetails";
        details = go.AddComponent<MissionFinalDetails>();
        details.goldReward = contract.goldReward;
        details.businessReward = contract.businessReward;
        details.moveReward = contract.moveReward;
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
        if (stageNum <= 0) { StartCoroutine(DelaySpawnChars()); } else { foreach (Character c in heroes) { if (c.incapacitated) { continue; } c.currentMissionCharacter.GetComponent<NavMeshAgent>().enabled = false; c.currentMissionCharacter.transform.position = currentStage.GetComponentInChildren<SpawnLocController>().spawnLocs[Random.Range(0, 4)].position; c.currentMissionCharacter.GetComponent<NavMeshAgent>().enabled = true; } }
        Helper.UIUpdate(null);
        
        //move heroes to spawn locations

    }

    public void MissionComplete() {
        print("Rewards");
        
        SceneManager.LoadScene("PostMission");

    }
    public void MissionFailed() {
        missionFailed = true;
        FindObjectOfType<MissionFinalDetails>().successful = false;
        stageNum = 1000000;
        SceneManager.LoadScene("PostMission");
    }
    IEnumerator DelaySpawnChars()
    {
        yield return new WaitForSeconds(5.0f);
        SpawnChars();
    }
    public void SpawnChars() {
        SpawnChars(heroes, currentStage.GetComponentInChildren<SpawnLocController>().spawnLocs);
       
    }

    public void SpawnChars(List<Character> chars, List<Transform> spawns) {
        BaseSpawnChars(chars, spawns);
    }

   void BaseSpawnChars(List<Character> chars, List<Transform> spawns) {
        print("Spawn Base Chars");
        for (int i = 0; i < chars.Count; i++) {
            if (chars[i].incapacitated) { continue; }
            GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(chars[i].modelName), spawns[i].transform.position, Quaternion.identity);
            MissionCharacter m = co.GetComponent<MissionCharacter>();
            m.Init(chars[i]);
            chars[i].currentMissionCharacter = m;
            co.GetComponent<NavMeshAgent>().enabled = false;
            co.transform.position = spawns[i].transform.position;
            chars[i].currentObject = co;
            co.GetComponent<NavMeshAgent>().enabled = true;
            
        }
        CameraController cam = Camera.main.GetComponent<CameraController>();
        
        cam.cameraTarget = heroes[0].currentObject;
        
        
        return;
        //yield return new WaitForSeconds(1.0f);
        for (int x = 0; x < chars.Count; x++) {
            print(x);
            chars[x].currentObject.transform.position = spawns[0].transform.position;
            chars[x].currentObject.GetComponent<NavMeshAgent>().enabled = true;
        }
        Camera.main.GetComponent<CameraController>().cameraTarget = heroes[0].currentObject;
    }

    // Update is called once per frame
    void Update()
    {
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
    public void BeginCombat(List<CharacterSO> enemies, List<Transform> enemySpawnLoc, MissionPOICombat _poiCombat) {
        print("Combat Begin");
        currentEnemies = new List<Character>();
        foreach (CharacterSO e in enemies) {
            currentEnemies.Add(Instantiate(e).GetCharacter());
        }
        foreach (Character h in heroes) {
            h.currentObject.GetComponent<Actor>().Side = 1;
        }
        SpawnChars(currentEnemies, enemySpawnLoc);
        combatController.Init(heroes, currentEnemies);
        poiCombat = _poiCombat;
    }

    public void EndCombat(bool success = true) {
        print("Combat Complete");
        SetAllHeroesToCombat(false);
        foreach (Character c in heroes) {
            c.currentMissionCharacter.healthBar.Hide(true);
            if (c.health > 0) {
                c.currentMissionCharacter.Idle();
                Helper.Cam().SetTarget(c.currentObject.transform);
            }
        }
        poiCombat.Resolve(success);


    }
    public void POITriggered(MissionPOI poi, Collider character) {
        //make character walk to target of POI
        //need to control which stage of the mission step we are at
        missionActive = true;
        MissionCharacter thisChar = character.GetComponent<MissionCharacter>();
        thisChar.Walk(poi.targetPos);
    }

    public void SetAllHeroesWalkTo(Transform walkTo, bool includeActiveChar, MissionCharacter activeChar) {
        foreach (Character c in heroes) {
            if (!includeActiveChar) { if (c.currentMissionCharacter == activeChar) continue; }
            c.currentMissionCharacter.Walk(walkTo);
        }
    }
    public void SetAllHeroesToCombat(bool shouldCombat) {
        foreach (Character c in heroes) {
            c.currentMissionCharacter.SetCombatComponents(shouldCombat);
        }
    }

    public void SetAllHeroesDontAct(MissionCharacter exception = null) {
        print("SetAllHeroesDontAct "+exception.name);
        foreach (Character m in heroes) {
            if (exception !=null && m.currentMissionCharacter != exception) {
                m.currentMissionCharacter.IdleDontAct();
            }
        }
    }

    public void SetAllHeroesToNextPOI() {
        print("SetAllHeroesToNextPOI");
        if (missionActive) {
            return;
        }
        if (combatController.combatActive) { return; }
        foreach (Character c in heroes) {
            c.currentMissionCharacter.FindNextStep();
        }
    }
    public Transform GetNextPOI() {
        if (pois.Count == 0) {
            print("NO MORE STEPS");
            return null;
        }
        pois[0].gameObject.SetActive(true);
        if (pois[0].cameraAngle != Vector3.zero) {
            FindObjectOfType<CameraController>().offset = pois[0].cameraAngle;
        }
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
   /*TODO: Hierarchy system of required steps so if things go sideways,
    * once all characters are idle they can return to the actual mission
    */
}
