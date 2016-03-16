using UnityEngine;
using System.Collections;

//The Straight Port of the Slightly Efficent Kart Finding System 

public class CollisionHandler : MonoBehaviour
{
    private int kartCount = 0;
    bool[,] collisions;
    int lastKartCount = -1, lastOtherCount;

    void LateUpdate()
    {
        kartScript[] otherKarts = GameObject.FindObjectsOfType<kartScript>();
        kartCollider[] otherThings = GameObject.FindObjectsOfType<kartCollider>();

        if(kartCount != otherKarts.Length)
        {
            if (otherKarts != null)
                kartCount = otherKarts.Length;
            else
                kartCount = 0;
        }

        if (lastKartCount != kartCount || lastOtherCount != otherThings.Length)
        {
            lastKartCount = kartCount;
            lastOtherCount = otherThings.Length;
            collisions = new bool[kartCount, kartCount + otherThings.Length];
        }

        for(int i = 0; i < kartCount; i++)//For every kart on Screen
        {
            //Checks against every kart after the current kart
            for (int j = i + 1; j < kartCount; j++)
            {
                Vector3 compareVect = otherKarts[j].transform.position - otherKarts[i].transform.position;

                if (!collisions[i, j] && compareVect.magnitude < 2f)
                {
                    collisions[i, j] = true;
                    otherKarts[i].KartCollision(otherKarts[j].transform);
                    otherKarts[j].KartCollision(otherKarts[i].transform);
                }
                else if (collisions[i, j] && compareVect.magnitude >= 2f)
                {
                    collisions[i, j] = false;
                }
            }

            //Check for SpinoutObject
            for (int j = 0; j < otherThings.Length; j++)// Checks against every kart after the current kart
            {
                Vector3 compareVect = otherThings[j].transform.position - otherKarts[i].transform.position;

                if (!collisions[i, kartCount + j] && compareVect.magnitude < 2f)
                {
                    collisions[i, kartCount + j] = true;
                    otherKarts[i].KartCollision(otherThings[j].transform);
                    otherKarts[i].SpinOut();
                    Debug.Log("Spin Out");
                }
                else if (collisions[i, kartCount + j] && compareVect.magnitude >= 2f)
                {
                    collisions[i, kartCount + j] = false;
                    Debug.Log("Stop spin Out");
                }
            }
        }

    }
}
