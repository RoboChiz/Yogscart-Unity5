using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    
    private void DoKartCollision(KartMovement kartA, KartMovement kartB)
    {
        //Find the fastest Kart
        bool aFastest = Mathf.Abs(kartA.actualSpeed) > Mathf.Abs(kartB.actualSpeed);

        KartMovement fastest, slowest;
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

        if(leftSpace < rightSpace)
        {
            slowest.GetComponent<CowTipping>().pushState = CowTipping.PushState.LeftNormal;
            fastest.GetComponent<CowTipping>().pushState = CowTipping.PushState.RightHalf;
        }
        else
        {
            slowest.GetComponent<CowTipping>().pushState = CowTipping.PushState.RightNormal;
            fastest.GetComponent<CowTipping>().pushState = CowTipping.PushState.LeftHalf;
        }

        slowest.GetComponent<CowTipping>().TipCow(); 
        fastest.GetComponent<CowTipping>().TipCow();

        fastest.GetComponentInChildren<DrivingIK>().ForceLook(slowest.transform);
    } 

    private void DoKartGodCollision(KartMovement kart, KartCollider god)
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
    }

    private IEnumerator WaitForCollision(int i, int j)
    {
        yield return new WaitForSeconds(1f);
        collisions[i, j] = false;
    }

}
