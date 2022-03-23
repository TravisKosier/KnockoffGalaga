using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Formation : MonoBehaviour
{
    public int gridSizeX = 10;
    public int gridSizeY = 2;

    public float gridOffsetX = 1f;
    public float gridOffsetZ = 1f;

    public int div = 4; //Control size of formation

    public List<Vector3> gridList = new List<Vector3>();

    //For moving the formation
    public float maxMoveOffsetX = 5f;

    float curPosX; //moving position
    Vector3 startPosition;

    public float speed = 1f;
    int direction = -1; //Only +/-1

    //Spread out formation
    bool canSpread;
    bool spreadStarted;

    float spreadAmount = 1f;
    float curSpread;
    float spreadSpeed = 0.5f;
    int spreadDir = 1;

    //Attack dive
    bool canDive;
    public List<GameObject> divePathList = new List<GameObject>();
    [HideInInspector]public List<EnemyFormation> enemyList = new List<EnemyFormation>();

    [System.Serializable]
    public class EnemyFormation
    {
        public int index;
        public float xPos;
        public float zPos;
        public GameObject enemy;

        public Vector3 goal; //Max size
        public Vector3 start; //Min size

        public EnemyFormation(int _index, float _xPos, float _zPos, GameObject _enemy)
        {
            index = _index;
            xPos = _xPos;
            zPos = _zPos;
            enemy = _enemy;

            start = new Vector3(_xPos, 0, _zPos);
            goal = new Vector3(_xPos + (_xPos * 0.3f),0,_zPos);

        }
    }

    void Start()
    {
        startPosition = transform.position;
        curPosX = transform.position.x;
        CreateGrid();
    }

    void Update()
    {
        if(!canSpread && !spreadStarted) //Not spreading
        {
            curPosX += Time.deltaTime * speed * direction;
            if (curPosX >= maxMoveOffsetX)
            {
                direction *= -1; //flip dir
                curPosX = maxMoveOffsetX;
            }
            else if (curPosX <= -maxMoveOffsetX)
            {
                direction *= -1;
                curPosX = -maxMoveOffsetX;
            }
            transform.position = new Vector3(curPosX, startPosition.y, startPosition.z);
        }

        if (canSpread)
        {
            curSpread += Time.deltaTime * spreadSpeed * spreadDir;
            if(curSpread >= spreadAmount || curSpread <= 0) //Change spread direction
            {
                spreadDir *= -1;
            }
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (Vector3.Distance(enemyList[i].enemy.transform.position, enemyList[i].goal) >= 0.001f)
                {
                    enemyList[i].enemy.transform.position = Vector3.Lerp(transform.position+enemyList[i].start, transform.position + enemyList[i].goal,curSpread);
                }
            }
        }

                if (canDive)
                {
                    Invoke("SetDiving", Random.Range(3, 10));
                    canDive = false;
                }
    }

    public IEnumerator ActivateSpread()
    {
        if (spreadStarted)
        {
            yield break;
        }
        spreadStarted = true;

        while (transform.position.x != startPosition.x)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
            yield return null;
        }
        canSpread = true;
        canDive = true;
    }

    void CreateGrid()
    {
        gridList.Clear();

        int num = 0;

        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                float x = (gridOffsetX + gridOffsetX * 2 * (num / div)) * Mathf.Pow(-1, num % 2 + 1);
                float z = gridOffsetZ * ((num % div) / 2);

                Vector3 vec = new Vector3(x, 0, z);

                num++;

                gridList.Add(vec);
            }
        }
    }
    public Vector3 GetVector(int ID)
    {
        return transform.position + gridList[ID];
    }

    void SetDiving()
    {
        if(enemyList.Count > 0)
        {
            int chosenPath = Random.Range(0,divePathList.Count);
            int chosenEnemy = Random.Range(0, enemyList.Count);

            GameObject newPath = Instantiate(divePathList[chosenPath], enemyList[chosenEnemy].start + transform.position,Quaternion.identity) as GameObject;

            enemyList[chosenEnemy].enemy.GetComponent<EnemyBehavior>().DiveSetup(newPath.GetComponent<Path>()); //Pass path to the enemy that will dive in EnemyBehavior
            enemyList.RemoveAt(chosenEnemy);
            Invoke("SetDiving",Random.Range(3,10));
        }
        else
        {
            CancelInvoke("SetDiving");
            return;
        }
    }
}
