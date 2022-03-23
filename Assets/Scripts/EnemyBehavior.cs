using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public Path pathToFollow;
    //Path info
    public int currentWayPointID = 0;
    public float speed = 2;
    public float reachDistance = 0.4f;
    public float rotationSpeed = 5f;

    float distance; //Distance to next waypoint
    public bool useBezier;

    //State Machine
    public enum EnemyStates
    {
        ON_PATH, //On bezier path
        FLY_IN, //Moving into formation
        IDLE,
        DIVE //Moving to attack
    }
    public EnemyStates enemyState;

    public int enemyID;
    public Formation formation;

    //Health
    public int health = 2;

    //Effects
    public GameObject fx_Explosion;

    //Enemy Shots
    public GameObject bullet;
    public GameObject captureBullet;
    float cur_delay;
    public float fireRate = 2f;
    Transform target; //Player
    public Transform spawnPoint;
    public int bulletDamage = -1; //All damage vals should be negative

    //Score
    public int inFormationScore;
    public int notInFormationScore;

    //CaptureMechanic
    public bool canCapture;
    private bool canFireCapture;
    //public GameObject dummyShip;
    public bool isCapturedShip = false;
    public GameObject powerup;

    void Start()
    {
        target = GameObject.Find("PlayerShip").transform;
    }

    
    void Update()
    {
        switch (enemyState)
        {
            case EnemyStates.ON_PATH:
                {
                    MoveOnThePath(pathToFollow);
                }
                break;
            case EnemyStates.FLY_IN:
                {
                    MoveToFormation();
                }
                break;
            case EnemyStates.IDLE:
                break;
            case EnemyStates.DIVE:
                MoveOnThePath(pathToFollow);
                //Shooting while diving
                SpawnBullet(); //Shoots towards the player once the shot delay has finished
                break;
        }

    }

    void MoveToFormation()
    {
        transform.position = Vector3.MoveTowards(transform.position, formation.GetVector(enemyID), speed * Time.deltaTime);
        //Rotation
        var direction = formation.GetVector(enemyID) - transform.position;
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            direction = direction.normalized;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        //
        if (Vector3.Distance(transform.position, formation.GetVector(enemyID)) <= 0.0001f)
        {
            if (!isCapturedShip)
            {
                transform.SetParent(formation.gameObject.transform);
            } 
            transform.eulerAngles = new Vector3(0,0,0);

            formation.enemyList.Add(new Formation.EnemyFormation(enemyID,transform.localPosition.x,transform.localPosition.z,this.gameObject));

            enemyState = EnemyStates.IDLE;
        }
    }

    void MoveOnThePath(Path path)
    {
        if (useBezier)
        {
            //Enemy Movement
            distance = Vector3.Distance(path.bezierObjList[currentWayPointID], transform.position);
            transform.position = Vector3.MoveTowards(transform.position, path.bezierObjList[currentWayPointID], speed * Time.deltaTime);
            //Enemy Rotation
            var direction = path.bezierObjList[currentWayPointID] - transform.position;
            if(direction != Vector3.zero)
            {
                direction.y = 0;
                direction = direction.normalized;
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            //Enemy Movement
            distance = Vector3.Distance(path.pathObjList[currentWayPointID].position, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, path.pathObjList[currentWayPointID].position, speed * Time.deltaTime);
            //Enemy Rotation
            var direction = path.pathObjList[currentWayPointID].position - transform.position;
            if (direction != Vector3.zero)
            {
                direction.y = 0;
                direction = direction.normalized;
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
            }
        }

        if (useBezier)
        {
            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }
            if (currentWayPointID >= path.bezierObjList.Count)
            {
                currentWayPointID = 0;

                if (enemyState == EnemyStates.DIVE && canCapture)
                {
                    Destroy(pathToFollow.gameObject);
                }
                else if(enemyState == EnemyStates.DIVE && !canCapture)
                {
                    transform.position = GameObject.Find("SpawnManager").transform.position;
                    Destroy(pathToFollow.gameObject);
                }

                if (canCapture && enemyState == EnemyStates.DIVE)
                {
                    StartCoroutine(CapturePeriod());//Literally just idle and shoot for 5 seconds, then fly in
                }
                else
                {

                    enemyState = EnemyStates.FLY_IN;
                }
                
            }
        }
        else
        {
            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }
            if (currentWayPointID >= path.pathObjList.Count)
            {
                currentWayPointID = 0;

                if (enemyState == EnemyStates.DIVE)
                {
                    transform.position = GameObject.Find("SpawnManager").transform.position;
                    Destroy(pathToFollow.gameObject);
                }

                enemyState = EnemyStates.FLY_IN;
            }
        }
    }

    public void SpawnSetup(Path path, int ID, Formation form)
    {
        pathToFollow = path;
        enemyID = ID;
        formation = form;
    }

    public void DiveSetup(Path path)
    {
        pathToFollow = path;
        transform.SetParent(transform.parent.parent); //'Un-parent' by setting parent back to the larger scene, from the formation
        enemyState = EnemyStates.DIVE;
    }

    public void TakeDamage(int amount)
    {
        health += amount;
        if (health<=0)// On kill
        {
            //Play sound

            //Instantiate particle effect
            if (fx_Explosion != null)
            {
                Instantiate(fx_Explosion, transform.position, Quaternion.identity);
            }
            //Add score
            if (enemyState == EnemyStates.IDLE)
            {
                GameManager.instance.AddScore(inFormationScore);
            }
            else
            {
                GameManager.instance.AddScore(notInFormationScore);
            }

            //Report destruction to formation (& Cull from parent formation list)
            for (int i = 0; i < formation.enemyList.Count; i++)
            {
                if (formation.enemyList[i].index == enemyID)
                {
                    formation.enemyList.Remove(formation.enemyList[i]);
                }
            }
            //Report destruction to spawn manager
            SpawnManager sp = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
            sp.UpdateSpawnedEnemies(this.gameObject);

            //If ship has child object (only dummy ship)
            if (transform.childCount > 1)
            {
                foreach (Transform child in transform)
                {
                    if (child.CompareTag("CapturedShip"))//If any of the children of the boss ship are a captured ship, give +1 bullet level
                    {
                        GameObject newPowerUp = Instantiate(powerup, new Vector3(0,0,-6), spawnPoint.rotation) as GameObject; /*Do this via spawning a 'powerup' bullet at origin, 
                                                                                                                     so large it will undoubtedly hit the player*/
                        GameManager.instance.IncreaseBulletLevel();
                    }
                }
            }
            
            Destroy(gameObject);
        }
    }

    void SpawnBullet()
    {
        if (!canCapture)//Normal enemy, don't check canFireCapture
        {
            cur_delay += Time.deltaTime;
            if (cur_delay >= fireRate && bullet != null && spawnPoint != null)
            {
                spawnPoint.LookAt(target); //aim at target
                GameObject newBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation) as GameObject;
                newBullet.GetComponent<Bullet>().SetDamage(bulletDamage);
                cur_delay = 0;
            }
        }
        else//Boss capture shot
        {
            cur_delay += Time.deltaTime;
            if (cur_delay >= fireRate && captureBullet != null && spawnPoint != null && canFireCapture)
            {
                spawnPoint.LookAt(target); //aim at target
                GameObject newBullet = Instantiate(captureBullet, spawnPoint.position, spawnPoint.rotation) as GameObject;
                newBullet.GetComponent<Bullet>().SetDamage(bulletDamage);
                newBullet.transform.parent = this.gameObject.transform; //Make bullet child of ship that fired it
                cur_delay = 0;
            }
        }
    }

    IEnumerator CapturePeriod()
    {
        enemyState = EnemyStates.IDLE;
        canFireCapture = true;
        yield return new WaitForSeconds(2);
        canFireCapture = false;
        enemyState = EnemyStates.FLY_IN;
    }
    /*
    public void SpawnCapturedShip()
    {
        GameObject newDummyShip = Instantiate(dummyShip, spawnPoint.position, spawnPoint.rotation) as GameObject; //Spawn dummy ship object
        newDummyShip.transform.parent = this.gameObject.transform.parent; //Make dummy ship child of ship that fired bullet
        //Get dummyship to fly in
    }*/
}
