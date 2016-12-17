using UnityEngine;
using System.Collections;

//The Straight Port of the Slightly Efficent Kart Finding System 

public class CollisionHandler : MonoBehaviour
{
   
    bool[,] collisions;
    private int thingCount = 0;

    private const float distance = 2f, pushUpAmount = 3f, pushAcrossAmount = 10f;

    void LateUpdate()
    {
        KartCollider[] things = FindObjectsOfType<KartCollider>();

        if(things.Length != thingCount)
        {
            thingCount = things.Length;
            collisions = new bool[thingCount, thingCount];
        }

        for(int i = 0; i < thingCount; i++)//For every kart on Screen
        {
            //Checks against every kart after the current kart
            for (int j = i + 1; j < thingCount; j++)
            {
                //Check that both things weren't in GodMode
                if (!things[i].godMode && !things[j].godMode)
                {
                    Vector3 compareVect = things[j].transform.position - things[i].transform.position;

                    if (!collisions[i, j] && compareVect.magnitude < distance)
                    {
                        //Do Collision between i & j
                        collisions[i, j] = true;

                        kartScript thingI = things[i].GetComponent<kartScript>(), thingJ = things[j].GetComponent<kartScript>();
                        //Find out which object was travelling fastest or was in God Mode
                        if (thingI != null && thingJ != null)
                        {
                            DoKartCollision(thingI, thingJ);
                        }
                        else if (thingI != null && things[j].godMode)
                        {
                            DoKartGodCollision(thingI, things[j]);
                        }
                        else if (thingJ != null && things[i].godMode)
                        {
                            DoKartGodCollision(thingJ, things[i]);
                        }

                        StartCoroutine(WaitForCollision(i, j));
                    }
                }
            }
        }
    }
    
    private void DoKartCollision(kartScript kartA, kartScript kartB)
    {
        //Find the fastest Kart
        bool aFastest = Mathf.Abs(kartA.actualSpeed) > Mathf.Abs(kartB.actualSpeed);

        kartScript fastest, slowest;
        if(aFastest)
        {
            fastest = kartA;
            slowest = kartB;
        }
        else
        {
            fastest = kartB;
            slowest = kartA;
        }

        //Push the slowest
        float leftSpace = ((fastest.transform.position - fastest.transform.right) - slowest.transform.position).magnitude;
        float rightSpace = ((fastest.transform.position + fastest.transform.right) - slowest.transform.position).magnitude;

        Vector3 sidePush = fastest.transform.right;
        if (leftSpace < rightSpace)
            sidePush *= -1f;

        slowest.GetComponent<Rigidbody>().AddForce((Vector3.up * pushUpAmount) + (sidePush * pushAcrossAmount), ForceMode.VelocityChange);
        StartCoroutine(WaitForKartCollision(slowest));
    } 

    private void DoKartGodCollision(kartScript kart, KartCollider god)
    {

    }

    private IEnumerator WaitForCollision(int i, int j)
    {
        yield return new WaitForSeconds(1f);
        collisions[i, j] = false;
    }

    private IEnumerator WaitForKartCollision(kartScript slowest)
    {
        slowest.isColliding = true;

        foreach (WheelCollider collider in slowest.wheelColliders)
        {
            collider.enabled = false;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (WheelCollider collider in slowest.wheelColliders)
        {
            collider.enabled = true;
        }

        slowest.isColliding = false;
    }
}
