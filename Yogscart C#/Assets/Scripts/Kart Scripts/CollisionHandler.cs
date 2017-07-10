using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The Straight Port of the Slightly Efficent Kart Finding System 

public class CollisionHandler : MonoBehaviour
{
   
    bool[,] collisions;
    private int thingCount = 0;

    private const float distance = 1.5f;

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
                if (!(things[i].godMode && things[j].godMode))
                {
                    Vector3 compareVect = things[j].transform.position - things[i].transform.position;

                    if (!collisions[i, j] && compareVect.magnitude < distance)
                    {
                        //Do Collision between i & j
                        collisions[i, j] = true;

                        KartMovement thingI = things[i].GetComponent<KartMovement>(), thingJ = things[j].GetComponent<KartMovement>();
                        //Find out which object was travelling fastest or was in God Mode
                        if (thingI != null && thingJ != null)
                        {
                            DoKartCollision(thingI, thingJ, distance - compareVect.magnitude);
                        }
                        else if (thingI != null && things[j].godMode)
                        {
                            DoKartGodCollision(thingI, things[j], distance - compareVect.magnitude);
                        }
                        else if (thingJ != null && things[i].godMode)
                        {
                            DoKartGodCollision(thingJ, things[i], distance - compareVect.magnitude);
                        }

                        StartCoroutine(WaitForCollision(i, j));
                    }
                }
            }
        }
    }
    
    private void DoKartCollision(KartMovement kartA, KartMovement kartB, float distance)
    {
    } 

    private void DoKartGodCollision(KartMovement kart, KartCollider god, float distance)
    {
        //Push the kart
        float leftSpace = ((god.transform.position - god.transform.right) - kart.transform.position).magnitude;
        float rightSpace = ((god.transform.position + god.transform.right) - kart.transform.position).magnitude;

        bool pushLeft = (leftSpace < rightSpace);

        if(pushLeft)
            kart.GetComponent<CowTipping>().pushState = CowTipping.PushState.LeftNormal;
        else
            kart.GetComponent<CowTipping>().pushState = CowTipping.PushState.RightNormal;

        kart.GetComponent<CowTipping>().TipCow();
        kart.GetComponent<KartMovement>().SpinOut();

        //Push Player away
        Vector3 dir = (kart.transform.position - god.transform.position);
        Vector3 normal = Vector3.up;

        RaycastHit hit;
        if (Physics.Raycast(kart.transform.position, -kart.transform.up, out hit, 4f))
            normal = hit.normal;

        dir = Vector3.ProjectOnPlane(dir, normal).normalized * distance;
        kart.transform.position += dir;
    }

    private IEnumerator WaitForCollision(int i, int j)
    {
        yield return new WaitForSeconds(0.1f);
        collisions[i, j] = false;
    }

}
