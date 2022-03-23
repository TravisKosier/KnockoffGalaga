using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Intervals")]
    public float enemySpawnInterval; //Between individual enemy spawns
    public float waveSpawnInterval; //Between waves
    int currentWave;

    int flyID = 0;
    int waspID = 0;
    int bossID = 0;

    [Header("Enemy Prefabs")]
    public GameObject flyPrefab; //Common enemy
    public GameObject waspPrefab; //Uncommon enemy
    public GameObject bossPrefab; //Rarest enemy
    [Header("Formations")]
    public Formation flyFormation;
    public Formation waspFormation;
    public Formation bossFormation;

    [System.Serializable]
    public class Wave
    {
        public int flyAmount; //# of flies to be spawned
        public int waspAmount;
        public int bossAmount;

        public GameObject[] pathPrefabsOne;
        public GameObject[] pathPrefabsTwo;
        public GameObject[] pathPrefabsThree;
    }
    [Header("Waves")]
    public List<Wave> waveList = new List<Wave>();

    List<Path> activePathList = new List<Path>();

    [HideInInspector]public List<GameObject> spawnedEnemies = new List<GameObject>();

    bool spawnComplete;
    void Start()
    {
        Invoke("StartSpawn",3f);
    }

    IEnumerator SpawnWaves()
    {
        while (currentWave < waveList.Count)
        {
            if (currentWave==waveList.Count-1)
            {
                spawnComplete = true;
            }
            if (GameManager.instance.GetLevel() == 1)
            {
                for (int i = 0; i < waveList[currentWave].pathPrefabsOne.Length; i++)
                {
                    GameObject newPathObj = Instantiate(waveList[currentWave].pathPrefabsOne[i], transform.position, Quaternion.identity) as GameObject;
                    Path newPath = newPathObj.GetComponent<Path>();
                    activePathList.Add(newPath);
                }
            }
            else if(GameManager.instance.GetLevel() == 2)
            {
                for (int i = 0; i < waveList[currentWave].pathPrefabsTwo.Length; i++)
                {
                    GameObject newPathObj = Instantiate(waveList[currentWave].pathPrefabsTwo[i], transform.position, Quaternion.identity) as GameObject;
                    Path newPath = newPathObj.GetComponent<Path>();
                    activePathList.Add(newPath);
                }

            }
            else if (GameManager.instance.GetLevel() == 3)
            {
                for (int i = 0; i < waveList[currentWave].pathPrefabsThree.Length; i++)
                {
                    GameObject newPathObj = Instantiate(waveList[currentWave].pathPrefabsThree[i], transform.position, Quaternion.identity) as GameObject;
                    Path newPath = newPathObj.GetComponent<Path>();
                    activePathList.Add(newPath);
                }
            }

            //Flies
            for (int i = 0; i < waveList[currentWave].flyAmount; i++)
            {
                GameObject newFly = Instantiate(flyPrefab, transform.position, Quaternion.identity) as GameObject;
                newFly.transform.parent = this.gameObject.transform;
                EnemyBehavior flyBehavior = newFly.GetComponent<EnemyBehavior>();

                flyBehavior.SpawnSetup(activePathList[PathPingPong()],flyID, flyFormation);
                flyID++;

                spawnedEnemies.Add(newFly);

                //wait out spawn interval
                yield return new WaitForSeconds(enemySpawnInterval);
            }
            //Wasps
            for (int i = 0; i < waveList[currentWave].waspAmount; i++)
            {
                GameObject newWasp = Instantiate(waspPrefab, transform.position, Quaternion.identity) as GameObject;
                newWasp.transform.parent = this.gameObject.transform;
                EnemyBehavior waspBehavior = newWasp.GetComponent<EnemyBehavior>();

                waspBehavior.SpawnSetup(activePathList[PathPingPong()], waspID, waspFormation);
                waspID++;

                spawnedEnemies.Add(newWasp);

                //wait out spawn interval
                yield return new WaitForSeconds(enemySpawnInterval);
            }
            //Bosses
            for (int i = 0; i < waveList[currentWave].bossAmount; i++)
            {
                GameObject newBoss = Instantiate(bossPrefab, transform.position, Quaternion.identity) as GameObject;

                EnemyBehavior bossBehavior = newBoss.GetComponent<EnemyBehavior>();
                newBoss.transform.parent = this.gameObject.transform;
                bossBehavior.SpawnSetup(activePathList[PathPingPong()], bossID, bossFormation);
                bossID++;

                spawnedEnemies.Add(newBoss);

                //wait out spawn interval
                yield return new WaitForSeconds(enemySpawnInterval);
            }
            yield return new WaitForSeconds(waveSpawnInterval);
            currentWave++;
            foreach(Path p in activePathList)
            {
                Destroy(p.gameObject);
            }
            activePathList.Clear();
        }

        Invoke("CheckEnemyState",1f);
    }

    void CheckEnemyState()//Is enemy idle or not
    {
        bool inFormation = false;
        for (int i = spawnedEnemies.Count-1; i >= 0; i--)
        {
            if (spawnedEnemies[i].GetComponent<EnemyBehavior>().enemyState != EnemyBehavior.EnemyStates.IDLE)
            {
                inFormation = false;
                Invoke("CheckEnemyState",1f);
                break;
            }
        }
        inFormation = true;

        if (inFormation) //Start all spread routines, stop checking
        {
            StartCoroutine(flyFormation.ActivateSpread());
            StartCoroutine(waspFormation.ActivateSpread());
            StartCoroutine(bossFormation.ActivateSpread());
            CancelInvoke();
        }
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnWaves());
        CancelInvoke("StartSpawn");
    }

    int PathPingPong()
    {
        return (flyID + bossID + waspID)%activePathList.Count;
    }

    void OnValidate()
    {
        int currentFlyAmount = 0;
        for (int i = 0; i < waveList.Count; i++)
        {
            currentFlyAmount += waveList[i].flyAmount;
        }
        if (currentFlyAmount > 20)
        {
            Debug.LogError("<color=red>Too many fly ships</color>: " + currentFlyAmount + "/20");
        }
        /*else
        {
            Debug.Log("Current Flies: " + currentFlyAmount);
        }*/

        int currentWaspAmount = 0;
        for (int i = 0; i < waveList.Count; i++)
        {
            currentWaspAmount += waveList[i].waspAmount;
        }
        if (currentWaspAmount > 20)
        {
            Debug.LogError("<color=red>Too many wasp ships</color>: " + currentWaspAmount + "/16");
        }
        /*else
        {
            Debug.Log("Current Wasps: " + currentWaspAmount);
        }*/

        int currentBossAmount = 0;
        for (int i = 0; i < waveList.Count; i++)
        {
            currentBossAmount += waveList[i].bossAmount;
        }
        if (currentBossAmount > 20)
        {
            Debug.LogError("<color=red>Too many boss ships</color>: " + currentBossAmount + "/4");
        }
        /*else
        {
            Debug.Log("Current Bosses: " + currentBossAmount);
        }*/
    }

    void ReportToGameManager()
    {
        if (spawnedEnemies.Count == 0 && spawnComplete)
        {
            GameManager.instance.WinCondition();
        }
    }

    public void UpdateSpawnedEnemies(GameObject enemy)
    {
        spawnedEnemies.Remove(enemy);

        ReportToGameManager();
    }
}
