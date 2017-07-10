using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PositionFinding))]
public class JR : Egg
{
    private GameObject owner;

    private List<KartMovement> possibleTargets;
    private KartMovement mainTarget;

    private TrackData.Route followRoute;

    private TrackData td;
    private PositionFinding myPositionFinding;

    private bool cancelFollowing = false;

    public override void Setup(float _direction, bool _actingShield)
    {
        if (_direction < 0)
            cancelFollowing = true;

        base.Setup(_direction, _actingShield);
    }

    protected void Start()
    {
        if (!cancelFollowing)
        {
            //Setup the Egg
            owner = transform.parent.gameObject;
            bounces = 0;

            //Get Track Data
            td = FindObjectOfType<TrackData>();
            myPositionFinding = GetComponent<PositionFinding>();

            //Get a list of possible targets
            possibleTargets = new List<KartMovement>();
            KartMovement ownerKS = owner.GetComponent<KartMovement>();
            KartMovement[] allKS = FindObjectsOfType<KartMovement>();

            PositionFinding ownerPF = ownerKS.GetComponent<PositionFinding>();

            foreach (KartMovement ks in allKS)
            {
                PositionFinding pf = ks.GetComponent<PositionFinding>();
                if (ks != ownerKS && pf.racePosition < ownerPF.racePosition)
                    possibleTargets.Add(ks);

            }

            StartCoroutine(FindRoute());
        }
    }

    private IEnumerator FindRoute()
    {
        while (myPositionFinding.closestPoint == null)
            yield return null;

        //Get a Default Route to follow
        foreach (TrackData.Route route in td.validRoutes)
        {
            int routeIndex = route.points.IndexOf(myPositionFinding.closestPoint);
            if (routeIndex != -1)
            {
                followRoute = route;
                break;
            }
        }
    }

	// Update is called once per frame
	protected override void FixedUpdate()
    {
        if (!cancelFollowing)
        {
            //Pick a main target based on who we're closer to
            if (mainTarget == null)
            {
                //Order Karts by distance
                KartMovement[] sortedArray = new KartMovement[possibleTargets.Count];

                while(possibleTargets.Count > 0)
                {
                    KartMovement closest = possibleTargets[0];
                    float closestDistance = Vector3.Distance(closest.transform.position, transform.position);

                    for(int i = 1; i < possibleTargets.Count; i++)
                    {
                        if(Vector3.Distance(possibleTargets[i].transform.position, transform.position) < closestDistance)
                        {
                            closest = possibleTargets[i];
                            closestDistance = Vector3.Distance(possibleTargets[i].transform.position, transform.position);
                        }
                    }

                    sortedArray[sortedArray.Length - possibleTargets.Count] = closest;
                    possibleTargets.Remove(closest);                   
                }

                possibleTargets = sortedArray.ToList();

                foreach (KartMovement ks in possibleTargets)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(transform.position - (Vector3.up * 0.25f), ks.transform.position - transform.position), out hit) && hit.transform == ks.transform)
                    {
                        mainTarget = ks;

                        //Tell Target we're attacking it
                        kartInfo kartInfo = mainTarget.GetComponent<kartInfo>();
                        if (kartInfo != null)
                            kartInfo.NewAttack(Resources.Load<Texture2D>("UI/Power Ups/Clucky_1JR"), gameObject);
                        break;
                    }
                }
            }

            //Lets hunt our target
            if (!actingShield && followRoute != null)
            {
                //Follow Connections
                if (mainTarget == null)
                {
                    int routeIndex = followRoute.points.IndexOf(myPositionFinding.closestPoint);

                    if (routeIndex != -1)
                    {
                        if (td.loopedTrack)
                            direction = MathHelper.ZeroYPos(followRoute.points[MathHelper.NumClamp(routeIndex + 1, 0, followRoute.points.Count)].lastPos - transform.position).normalized;
                        else if (routeIndex + 1 < followRoute.points.Count)
                            direction = MathHelper.ZeroYPos(followRoute.points[routeIndex + 1].lastPos - transform.position).normalized;
                    }
                }
                //If close enough attack target
                else
                {
                    direction = (mainTarget.transform.position - transform.position).normalized;
                }
            }
        }

        base.FixedUpdate();

	}
}
