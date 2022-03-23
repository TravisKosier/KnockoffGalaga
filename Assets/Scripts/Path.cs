using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public Color pathColor = Color.green;

    Transform[] objArray;
    public bool visualizePath;
    [Range(1, 20)] public int lineDensity = 1;
    int overload;

    public List<Transform> pathObjList = new List<Transform>();
    public List<Vector3> bezierObjList = new List<Vector3>();
    
    void Start()
    {
        CreatePath();
    }

     void OnDrawGizmos()
        {
            if (visualizePath)
            {
                //Straight path
                Gizmos.color = pathColor;
                //Fill array
                objArray = GetComponentsInChildren<Transform>();
                //Clear list
                pathObjList.Clear();
                //Put children into list
                foreach (Transform obj in objArray)
                {
                    if (obj != this.transform)
                    {
                        pathObjList.Add(obj);
                    }
                }
                //Draw objects
                for (int i = 0; i < pathObjList.Count; i++)
                {
                    Vector3 position = pathObjList[i].position;
                    if (i > 0)
                    {
                        Vector3 previous = pathObjList[i - 1].position;
                        Gizmos.DrawLine(previous, position);
                        Gizmos.DrawWireSphere(position, 0.3f);
                    }
                }

                //Curved Path

                //Check Overload

                if (pathObjList.Count % 2 == 0)
                {
                    pathObjList.Add(pathObjList[pathObjList.Count - 1]);
                    overload = 2;
                }
                else
                {
                    pathObjList.Add(pathObjList[pathObjList.Count - 1]);
                    pathObjList.Add(pathObjList[pathObjList.Count - 1]);
                    overload = 3;
                }

                //Create curve
                bezierObjList.Clear();

                Vector3 lineStart = pathObjList[0].position;

                for (int i = 0; i < pathObjList.Count - overload; i += 2)
                {
                    for (int j = 0; j <= lineDensity; j++)
                    {
                        Vector3 lineEnd = GetPoint(pathObjList[i].position, pathObjList[i + 1].position, pathObjList[i + 2].position, j / (float)lineDensity);

                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(lineStart, lineEnd);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireSphere(lineStart, 0.1f);

                        lineStart = lineEnd;

                        bezierObjList.Add(lineStart);
                    }
                }
            }
            else
            {
                //pathObjList.Clear();
                //bezierObjList.Clear();
            }
        }

    Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);       
    }

    void CreatePath()
    {
        //Fill array
        objArray = GetComponentsInChildren<Transform>();
        //Clear list
        pathObjList.Clear();
        //Put children into list
        foreach (Transform obj in objArray)
        {
            if (obj != this.transform)
            {
                pathObjList.Add(obj);
            }
        }
        //Curved Path
        
        //Check Overload

        if (pathObjList.Count % 2 == 0)
        {
            pathObjList.Add(pathObjList[pathObjList.Count - 1]);
            overload = 2;
        }
        else
        {
            pathObjList.Add(pathObjList[pathObjList.Count - 1]);
            pathObjList.Add(pathObjList[pathObjList.Count - 1]);
            overload = 3;
        }

        //Create curve
        bezierObjList.Clear();

        Vector3 lineStart = pathObjList[0].position;

        for (int i = 0; i < pathObjList.Count - overload; i += 2)
        {
            for (int j = 0; j <= lineDensity; j++)
            {
                Vector3 lineEnd = GetPoint(pathObjList[i].position, pathObjList[i + 1].position, pathObjList[i + 2].position, j / (float)lineDensity);

                lineStart = lineEnd;

                bezierObjList.Add(lineStart);
            }
        }
    }
}
