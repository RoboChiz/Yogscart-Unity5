using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestManager : MonoBehaviour
{
    readonly static Rect maxSize = new Rect(-300, -300, 300, 300);
    private InterestTree tree;

    //Used to update every 30 frames
    private float lastUpdate;

	// Use this for initialization
	void Start ()
    {
        tree = new InterestTree(0, maxSize);
    }
	
	// Update is called once per frame
	void Update ()
    {
        lastUpdate += Time.deltaTime;

        if (lastUpdate > 0.3)
        {
            tree.Clear();
            lastUpdate = 0;
        }
               
    }

    //Tell Scripts that are adding Points of Interest to add them now
    public bool CanAddInterestNow()
    {
        return lastUpdate == 0;
    }

    public void AddInterest(Transform transform, InterestType type, Vector3 offset)
    {
        tree.Insert(new TransformPointOfInterest(transform, type, offset));
    }

    //Tell Scripts that are adding Points of Interest to add them now
    public List<PointOfInterest> GetPoI(Vector3 position)
    {
        List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();
        pointsOfInterest = tree.Retrieve(pointsOfInterest, position);

        return pointsOfInterest;
    }
}
