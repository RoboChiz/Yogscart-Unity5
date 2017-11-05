using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//The Straight Port of the Slightly Efficent Kart Finding System 

public class CollisionHandler : MonoBehaviour
{
   
    bool[,] collisions;
    private int thingCount = 0;

    private const float distance = 1.5f;

    void LateUpdate()
    {
        List<KartCollider> things = FindObjectsOfType<KartCollider>().ToList();

        //Remove Invalid Things
        foreach(KartCollider thing in things.ToArray())
        {
            if (thing.gameObject.layer != 8)
                things.Remove(thing);
        }

        if(things.Count != thingCount)
        {
            thingCount = things.Count;
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
                        if (thingI != null && things[j].godMode)
                        {
                            DoKartGodCollision(thingI, things[j], distance - compareVect.magnitude);
                        }
                        else if (thingJ != null && things[i].godMode)
                        {
                            DoKartGodCollision(thingJ, things[i], distance - compareVect.magnitude);
                        }
                        else if (thingI != null && thingJ != null)
                        {
                            DoKartCollision(thingI, thingJ, distance - compareVect.magnitude);
                        }

                        StartCoroutine(WaitForCollision(i, j));
                    }
                }
            }
        }
    }
    
    private void DoKartCollision(KartMovement kartA, KartMovement kartB, float distance)
    {
        KartMovement fastest, slowest;

        //Find Faster Vehicle
        if (kartA.actualSpeed > kartB.actualSpeed)
        {
            fastest = kartA;
            slowest = kartB;
        }
        else
        {
            fastest = kartB;
            slowest = kartA;
        }


        Vector3 fastRight = Vector3.zero, slowRight = Vector3.zero;

        //Move Vehicles so they aren't colliding anymore
        Vector3 relativePosition = fastest.transform.InverseTransformPoint(slowest.transform.position);

        if (relativePosition.x > 0f)
        {
            //Move Slowest Right, Fastest Left
            fastRight = -fastest.transform.right;
            slowRight = fastest.transform.right;
        }
        else
        {
            //Move Slowest Left, Fastest Right
            fastRight = fastest.transform.right;
            slowRight = -fastest.transform.right;
        }

        //Project onto Ground
        RaycastHit hit;
        if (Physics.Raycast(fastest.transform.position, -fastest.transform.up, out hit, 2f))
            fastRight = Vector3.ProjectOnPlane(fastRight, hit.normal);

        if (Physics.Raycast(slowest.transform.position, -slowest.transform.up, out hit, 2f))
            slowRight = Vector3.ProjectOnPlane(slowRight, hit.normal);

        //Store where kart was
        Vector3 fastestWorldPos = fastest.transform.position, slowestWorldPos = slowest.transform.position;

        //Force Kart Move
        fastest.transform.position += fastRight;
        slowest.transform.position += slowRight;

        //Leave Kart Body where kart was
        fastest.kartBody.position = fastestWorldPos;
        slowest.kartBody.position = slowestWorldPos;

        //Slide Kartbody back to actual kart position
        fastest.SlideKartBody();
        slowest.SlideKartBody();

        //Do Twist Animation
        StartCoroutine(TwistKartBody(fastest.kartBody, (relativePosition.x > 0f) ? 1f : -1f));
        StartCoroutine(TwistKartBody(slowest.kartBody, (relativePosition.x > 0f) ? -1f : 1f));

        //Spawn Sparks
        Instantiate(Resources.Load<GameObject>("Prefabs/Sparks"), fastest.transform.position + ((relativePosition.x > 0f) ? fastest.transform.right : -fastest.transform.right),
            fastest.transform.rotation * Quaternion.AngleAxis(20f, fastest.transform.forward) * Quaternion.AngleAxis(90f + ((relativePosition.x > 0f) ? 0f : 180f), fastest.transform.up),
            fastest.transform);

        Instantiate(Resources.Load<GameObject>("Prefabs/Sparks"), slowest.transform.position + ((relativePosition.x > 0f) ? -slowest.transform.right : slowest.transform.right),
            slowest.transform.rotation * Quaternion.AngleAxis(20f, slowest.transform.forward) * Quaternion.AngleAxis(90f + ((relativePosition.x > 0f) ? 180f : 0f), slowest.transform.up), 
            slowest.transform);

        FindObjectOfType<InterestManager>().AddInterest(fastest.transform, InterestType.Kart, Vector3.up);
        FindObjectOfType<InterestManager>().AddInterest(slowest.transform, InterestType.Kart, Vector3.up);
    }

    private void DoKartGodCollision(KartMovement kart, KartCollider god, float distance)
    {

        Vector3 kartRight = Vector3.zero;

        //Move Vehicles so they aren't colliding anymore
        Vector3 relativePosition = god.transform.InverseTransformPoint(kart.transform.position);

        if (relativePosition.x > 0f)
        {
            //Move Kart Right
            kartRight = kart.transform.right;
        }
        else
        {
            //Move Kart Left
            kartRight = -kart.transform.right;
        }

        //Project onto Ground
        RaycastHit hit;
        if (Physics.Raycast(kart.transform.position, -kart.transform.up, out hit, 2f))
            kartRight = Vector3.ProjectOnPlane(kartRight, hit.normal);

        //Store where kart was
        Vector3 kartWorldPos = kart.transform.position;

        //Force Kart Move
        kart.transform.position += kartRight;

        //Leave Kart Body where kart was
        kart.kartBody.position = kartWorldPos;

        //Slide Kartbody back to actual kart position
        kart.SlideKartBody();

        //Do Twist Animation
        StartCoroutine(TwistKartBody(kart.kartBody, (relativePosition.x > 0f) ? -1f : 1f));

        kart.SpinOut(true);

        //Tell God
        god.SendMessage("OnKartHit", kart, SendMessageOptions.DontRequireReceiver);

        FindObjectOfType<InterestManager>().AddInterest(kart.transform, InterestType.Attack, Vector3.up);
        FindObjectOfType<InterestManager>().AddInterest(god.transform, InterestType.Attack, Vector3.zero);
    } 

    private IEnumerator TwistKartBody(Transform child, float modifier)
    {
        float startTime = Time.time, travelTime = 0.2f, twistAmount= 15f;

        while (Time.time - startTime < travelTime)
        {
            SetZRotation(child, Mathf.Lerp(0f, twistAmount * modifier, (Time.time - startTime) / travelTime));
            yield return null;
        }

        startTime = Time.time;
        while (Time.time - startTime < travelTime)
        {
            SetZRotation(child, Mathf.Lerp(twistAmount * modifier, 0f, (Time.time - startTime) / travelTime));
            yield return null;
        }

        SetZRotation(child, 0f);
    }

    private void SetZRotation(Transform toTransform, float value)
    {
        Vector3 euler = toTransform.localRotation.eulerAngles;
        euler.z = value;
        toTransform.localRotation = Quaternion.Euler(euler);
    }

    private IEnumerator WaitForCollision(int i, int j)
    {
        yield return new WaitForSeconds(0.3f);
        collisions[i, j] = false;
    }

}
