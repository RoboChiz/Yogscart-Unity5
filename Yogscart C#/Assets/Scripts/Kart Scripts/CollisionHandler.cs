using UnityEngine;
using System.Collections;

//The Straight Port of the Slightly Efficent Kart Finding System 

public class CollisionHandler : MonoBehaviour
{
    private int kartCount = 0;
    bool[,] collisions;
    int lastKartCount = -1, lastOtherCount;

    private const float distance = 2.75f;

    void LateUpdate()
    {
        kartScript[] otherKarts = FindObjectsOfType<kartScript>();
        kartCollider[] otherThings = FindObjectsOfType<kartCollider>();

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

                if (!collisions[i, j] && compareVect.magnitude < distance)
                {
                    collisions[i, j] = true;
                    //Do Collision between i & j

                    kartScript ksI = otherKarts[i].GetComponent<kartScript>();
                    kartScript ksJ = otherKarts[j].GetComponent<kartScript>();
                    float originalSpeedI = ksI.expectedSpeed, originalSpeedJ = ksJ.expectedSpeed;

                    //I is moving, but J is not reduce I's speed by alot
                    if (Mathf.Abs(ksI.expectedSpeed) > Mathf.Abs(ksJ.expectedSpeed) && Mathf.Abs(ksJ.expectedSpeed) <4f)
                    {
                        ksI.expectedSpeed -= Mathf.Sign(ksI.expectedSpeed) * 12f;
                    }
                    //J is mobing, but I is not reduce J's speed by alot
                    else if (Mathf.Abs(ksJ.expectedSpeed) > Mathf.Abs(ksI.expectedSpeed) && Mathf.Abs(ksI.expectedSpeed) < 4f)
                    {
                        ksJ.expectedSpeed -= Mathf.Sign(ksJ.expectedSpeed) * 12f;
                    }
                    //I is moving, and so is J
                    else if(Mathf.Abs(ksI.expectedSpeed) >4f && Mathf.Abs(ksJ.expectedSpeed) > 4f)
                    {
                        ksI.expectedSpeed -= Mathf.Sign(ksI.expectedSpeed) * 6f;
                        ksJ.expectedSpeed -= Mathf.Sign(ksJ.expectedSpeed) * 6f;
                    }

                    int fastestKart = -1, slowestKart = -1;
                    float fastestSpeed = -1;

                    if (Mathf.Abs(ksI.expectedSpeed) >= Mathf.Abs(ksJ.expectedSpeed))
                    {
                        fastestKart = i;
                        fastestSpeed = originalSpeedI;

                        slowestKart = j;
                    }
                    else
                    {
                        fastestKart = j;
                        fastestSpeed = originalSpeedJ;

                        slowestKart = i;
                    }

                    StartCoroutine(PushKarts(otherKarts[fastestKart].transform, otherKarts[slowestKart].transform, Mathf.Abs(fastestSpeed)));

                }
                else if (collisions[i, j] && compareVect.magnitude >= distance)
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

    private IEnumerator PushKarts(Transform fastestKart, Transform slowestKart, float fastestSpeed)
    {
        //Move I and J apart
        kartScript ksFast = fastestKart.GetComponent<kartScript>();
        kartScript ksSlow = slowestKart.GetComponent<kartScript>();
        Rigidbody rbFast = fastestKart.GetComponent<Rigidbody>();
        Rigidbody rbSlow = slowestKart.GetComponent<Rigidbody>();

        //Find what side slower kart is on
        Vector3 between = rbSlow.position - rbFast.position, fastPushDir;
        if (Vector3.Angle(rbFast.transform.right, between) > 45) //Slow kart is on the left side
            fastPushDir = rbFast.transform.right;
        else
            fastPushDir = -rbFast.transform.right;

        //Push slowest car further
        Vector3 pushAmount = (fastPushDir * Mathf.Clamp(fastestSpeed,0,12));
        //Debug.Log("Before:" + rbFast.transform.InverseTransformDirection((rbFast.velocity)));

        rbFast.freezeRotation = true;
        rbSlow.freezeRotation = true;

        for (int i = 0; i < 2; i++)
        {
            rbFast.AddForceAtPosition(pushAmount / 2f, rbFast.transform.position, ForceMode.VelocityChange);
            rbSlow.AddForceAtPosition(-pushAmount, rbSlow.transform.position, ForceMode.VelocityChange);

            //Debug.Log("During " + i +":" + rbFast.transform.InverseTransformDirection((rbFast.velocity)));
            yield return null;
        }

        for (int i = 0; i < 8; i++)
        {
            //Debug.Log("After " + i + ":" + rbFast.transform.InverseTransformDirection((rbFast.velocity)));
            yield return null;
        }

        rbFast.freezeRotation = false;
        rbSlow.freezeRotation = false;

    }
}
