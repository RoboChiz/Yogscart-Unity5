using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI V3 *Sighs*
/// Coded by Robo_Chiz 2017
/// </summary>
[RequireComponent(typeof(KartMovement), typeof(PositionFinding), typeof(KartItem))]
public class AI : MonoBehaviour
{
    public bool canDrive = true;

    //Decides how we'll start the Race
    public enum StartType { WillBoost, WontBoost, WillSpin };
    public StartType myStartType;

    public enum AIStupidity { Stupid, Bad, Good, Great, Perfect };
    public AIStupidity intelligence = AIStupidity.Stupid;

    private KartMovement ks;
    private KartItem kartItem;
    private PositionFinding pf;
    private TrackData td;
    private CurrentGameData gd;

    public PointHandler aimPoint;

    private const float requiredAngle = 7f;
    public bool reversing;

    public enum ItemUse { None, Random, Using, Smart}
    public ItemUse itemUse = ItemUse.None;
    private bool shielding = false;
    private static ItemBox[] itemBoxes;

    // Use this for initialization
    void Start ()
    {
        ks = GetComponent<KartMovement>();
        kartItem = GetComponent<KartItem>();

        pf = GetComponent<PositionFinding>();
        td = FindObjectOfType<TrackData>();
        gd = FindObjectOfType<CurrentGameData>();

        //Decide my Start Boost
        myStartType = StartType.WontBoost;

        switch(intelligence)
        {
            case AIStupidity.Stupid:
                myStartType = StartType.WillSpin;
                break;

            case AIStupidity.Bad:
                if (Random.Range(0, 10) >= 5) //50% chance of Spinning Out
                    myStartType = StartType.WillSpin;
                break;

            case AIStupidity.Good:
                if(Random.Range(0, 10) >= 4) //40% chance of Boosting at Start
                    myStartType = StartType.WillBoost;
                break;

            case AIStupidity.Great:
                if(Random.Range(0, 10) >= 7)//70% chance of Boosting at Start
                    myStartType = StartType.WillBoost;
                break;

            case AIStupidity.Perfect://Will always of Boost at Start
                myStartType = StartType.WillBoost;
                break;
        }         

        if(itemBoxes == null)
            itemBoxes = FindObjectsOfType<ItemBox>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Handles Start Boosting
        if (KartMovement.startBoostVal != -1)
        {
            if ((myStartType == StartType.WillBoost && KartMovement.startBoostVal <= 2) || (myStartType == StartType.WillSpin && KartMovement.startBoostVal <= 3))
                ks.throttle = 1;

            if (KartMovement.startBoostVal <= 1)
                canDrive = true;
        }

        //Stop Driving if reach finish
        if (!td.loopedTrack && pf.currentPercent >= 1f)
        {
            canDrive = false;
            ks.throttle = 0f;
            ks.steer = 0f;
            ks.drift = false;
        }

        if (canDrive && pf.closestPoint != null)
        {
            //Drive towards the nearest point
            if (aimPoint == null || pf.closestPoint == aimPoint || pf.currentPercent >= aimPoint.percent)
                ChooseNewPath();

            //Decide how to steer
            float angle = MathHelper.Angle(MathHelper.ZeroYPos(transform.forward), MathHelper.ZeroYPos(aimPoint.lastPos - transform.position));
            int angleSign = MathHelper.Sign(angle);
            ks.steer = (Mathf.Abs(angle) > requiredAngle) ? angleSign : 0f;

            //Decide how to throttle
            ks.throttle = 1f;

            //Reversing Behaviour
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, 1.5f) && (hit.transform.GetComponent<Collider>() == null || !hit.transform.GetComponent<Collider>().isTrigger) 
                && hit.transform.tag != "Ground" && hit.transform.tag != "OffRoad" && hit.transform != transform)
            {
                Debug.Log("Ahh! I hit " + hit.transform.name);
                reversing = true;
            }

            Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.red);

            if(reversing)
            {
                ks.throttle = -1;
                ks.steer *= -1;

                if(Mathf.Abs(angle) < requiredAngle * 2f)
                    reversing = false;
            }
        }

        //Item Behaviour
        if(kartItem.heldPowerUp >= 0)
        {
            //If we haven't decided what we want to do with it
            if (itemUse == ItemUse.None)
            {
                //Decide how to use Item
                switch (intelligence)
                {
                    case AIStupidity.Perfect: itemUse = ItemUse.Smart; break;
                    case AIStupidity.Great: itemUse = (Random.Range(0, 100) > 85) ? ItemUse.Smart : ItemUse.Random; break;
                    case AIStupidity.Good: itemUse = (Random.Range(0, 100) > 50) ? ItemUse.Smart : ItemUse.Random; break;
                    case AIStupidity.Bad: itemUse = (Random.Range(0, 100) > 15) ? ItemUse.Smart : ItemUse.Random; break;
                    case AIStupidity.Stupid: itemUse = ItemUse.Random; break;
                }
            }

            if (itemUse == ItemUse.Random)
            {
                StartCoroutine(UseItemRandom());
                itemUse = ItemUse.Using;
            }
            else if (itemUse == ItemUse.Smart)
            {
                //Do Smart Behaviour
                PowerUp power = gd.powerUps[kartItem.heldPowerUp];
                RaycastHit hit;

                switch (power.aiUnderstanding)
                {
                    case PowerUp.AIUnderstanding.Dirt:

                        if (!shielding)
                        {
                            shielding = true;
                            kartItem.UseShield();                          
                        }

                        //If kart is behind us drop the dirt                    
                        if(Physics.Raycast(transform.position, -transform.forward,out hit) && hit.transform.GetComponent<KartMovement>() == true)
                        {
                            kartItem.DropShield(-1);
                        }

                        //If we are near more item boxes drop dirt
                        if(NearItemBoxes())
                            kartItem.DropShield(-1);

                        break;
                    case PowerUp.AIUnderstanding.Egg:

                        if (!shielding)
                        {
                            shielding = true;
                            kartItem.UseShield();
                        }

                        //If kart is in front of us drop the dirt
                        if (Physics.Raycast(transform.position + (transform.forward * 2f), transform.forward, out hit) && hit.transform.GetComponent<KartMovement>() == true)
                        {
                            kartItem.DropShield(1);
                        }

                        //If we are near more item boxes fire egg behind
                        if (NearItemBoxes())
                            kartItem.DropShield(-1);

                        break;
                    case PowerUp.AIUnderstanding.JR:

                        if (!shielding)
                        {
                            shielding = true;
                            kartItem.UseShield();
                        }

                        //Fire if we're not in first
                        if (pf.racePosition > 0)
                            kartItem.DropShield(1);

                        //If we are near more item boxes fire egg behind
                        if (NearItemBoxes())
                            kartItem.DropShield(-1);

                        break;
                    case PowerUp.AIUnderstanding.Lapis:
                        kartItem.UseItem();

                        break;
                    case PowerUp.AIUnderstanding.SpeedBoost:   
                                          
                        //Wait till we're on a straight and not near shortcut
                        if(ks.steer == 0)
                            kartItem.UseItem();

                        break;
                }
            }
        }
        else
        {
            itemUse = ItemUse.None;
            kartItem.input = false;
            shielding = false;
        }
    }

    void ChooseNewPath()
    {
        //If Answer is obvious
        if (td.loopedTrack && (pf.closestPoint.style == PointHandler.Point.End || pf.closestPoint.style == PointHandler.Point.Start))
        {
            aimPoint = pf.closestPoint.connections[0];

            for (int i = 1; i < pf.closestPoint.connections.Count; i++)
            {
                if (pf.closestPoint.connections[i].percent < aimPoint.percent)
                    aimPoint = pf.closestPoint.connections[i];
            }          
        }
        else
        {
            int count = 0;
            bool done = false;

            PointHandler target = pf.closestPoint;

            do
            {
                List<PointHandler> validPaths = new List<PointHandler>(target.connections);

                foreach (PointHandler ph in validPaths.ToArray())
                {
                    if (ph.percent <= pf.closestPoint.percent) //If Point goes back remove it
                        validPaths.Remove(ph);
                    else if (ph.style == PointHandler.Point.Shortcut) //If point requires a boost
                    {
                        //If AI has an Item
                        if(kartItem.heldPowerUp >= 0 && gd.powerUps[kartItem.heldPowerUp].aiUnderstanding == PowerUp.AIUnderstanding.SpeedBoost)
                        {
                            //If they are Smart enough
                            if (intelligence >= AIStupidity.Great || (intelligence == AIStupidity.Good && Random.Range(0, 100) > 50))
                            {
                                //Force them to shortcut
                                validPaths = new List<PointHandler>() { ph };
                                kartItem.UseItem();
                                break;
                            }
                        }
                        else
                            validPaths.Remove(ph);
                    }
                }

                //If there's nowhere to go, break
                if (validPaths.Count == 0)
                    return;

                //If Shortcut and have item choose shortcut if smart enough
                aimPoint = validPaths[Random.Range(0, validPaths.Count)];
                count++;

                if (aimPoint.percent < pf.currentPercent)
                    target = aimPoint;
                else
                    done = true;

                //Error Checking
                if (count > 5)
                    Debug.Log("AHHHH CRASH!");

                if (count > 10)
                    return;

            } while (!done);
        }
    }

    private IEnumerator UseItemRandom()
    {

        bool useShield = false;

        if(gd.powerUps[kartItem.heldPowerUp].useableShield)
            useShield = Random.Range(0, 100) > 50;

        yield return new WaitForSeconds(Random.Range(0, 10));

        if (!useShield)
        {
            kartItem.input = true;
            yield return null;
            kartItem.input = false;

        }
        else
        {
            kartItem.input = true;
            yield return new WaitForSeconds(Random.Range(0, 10));
            kartItem.input = false;
        }
    }

    bool NearItemBoxes()
    {
        foreach(ItemBox ib in itemBoxes)
        {
            if (Vector3.Distance(transform.position, ib.transform.position) < 10f)
                return true;
        }
        return false;
    }
}
