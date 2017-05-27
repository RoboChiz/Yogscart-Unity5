using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InterestType { Kart, Attack, Scenary};
public abstract class PointOfInterest
{
    public InterestType interestType { get; protected set; }
    public abstract Vector3 GetLocation();
}

public class TransformPointOfInterest : PointOfInterest
{
    public Transform target { get; protected set; }
    public Vector3 offset;

    public TransformPointOfInterest(Transform _target, InterestType type, Vector3 _offset)
    {
        interestType = type;
        target = _target;
        offset = _offset;
    }

    public TransformPointOfInterest(Transform _target, InterestType type) : this(_target, type, Vector3.zero) { }

    public override Vector3 GetLocation()
    {
        return target.position;
    }
}
public class Vector3PointOfInterest : PointOfInterest
{
    public Vector3 target { get; protected set; }

    public Vector3PointOfInterest(Vector3 _target, InterestType type)
    {
        interestType = type;
        target = _target;
    }

    public override Vector3 GetLocation()
    {
        return target;
    }
}



public class InterestTree
{
    const int maxObjectsPerLeaf = 10, maxLevels = 15;

    public int level { get; private set; }
    public List<PointOfInterest> pointsOfInterest { get; private set; }
    public Rect aabbBounds { get; private set; }
    public InterestTree[] nodes;

    //Constructor
    public InterestTree(int _level, Rect _aabbBounds)
    {
        level = _level;
        pointsOfInterest = new List<PointOfInterest>();
        aabbBounds = _aabbBounds;
        nodes = new InterestTree[4];
    }

    //Useful Functions

    /// <summary>
    /// Clears the Collision Tree and all it's Splits
    /// </summary>
    public void Clear()
    {
        pointsOfInterest.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            InterestTree node = nodes[i];

            if (node != null)
            {
                node.Clear();
                nodes[i] = null;
            }
        }
    }

    /// <summary>
    /// Splits a Tree into 4 SubTrees
    /// </summary>
    public void Split()
    {
        int splitWidth = (int)(aabbBounds.width / 2f), splitHeight = (int)(aabbBounds.height / 2f);
        int x = (int)aabbBounds.x, y = (int)aabbBounds.y;

        nodes[0] = new InterestTree(level + 1, new Rect(x, y, splitWidth, splitHeight));
        nodes[1] = new InterestTree(level + 1, new Rect(x + splitWidth, y, splitWidth, splitHeight));
        nodes[2] = new InterestTree(level + 1, new Rect(x, y + splitHeight, splitWidth, splitHeight));
        nodes[3] = new InterestTree(level + 1, new Rect(x + splitWidth, y + splitHeight, splitWidth, splitHeight));
    }

    private int GetIndex(Rect possibleRect)
    {
        int index = -1;
        float verticalMidPoint = aabbBounds.x + (aabbBounds.width / 2f);
        float horizontalMidPoint = aabbBounds.y + (aabbBounds.height / 2f);

        bool topQuadrant = possibleRect.y < horizontalMidPoint && possibleRect.y + possibleRect.height < horizontalMidPoint;
        bool bottomQuadrant = possibleRect.y > horizontalMidPoint;

        bool leftQuadrent = possibleRect.x < verticalMidPoint && possibleRect.x + possibleRect.width < verticalMidPoint;

        if (leftQuadrent)
        {
            if (topQuadrant)
                index = 1;
            else if (bottomQuadrant)
                index = 2;
        }
        else if (possibleRect.x > verticalMidPoint)
        {
            if (topQuadrant)
                index = 0;
            else
                index = 3;
        }

        return index;
    }

    public void Insert(PointOfInterest point)
    {
        if (nodes[0] != null)
        {
            Vector3 position = point.GetLocation();
            int index = GetIndex(new Rect(position.x, position.y,1f,1f));

            if (index != -1)
            {
                nodes[index].Insert(point);
                return;
            }
        }

        pointsOfInterest.Add(point);

        if (pointsOfInterest.Count > maxObjectsPerLeaf && level < maxLevels)
        {
            if (nodes[0] == null)
                Split();

            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                Vector3 position = pointsOfInterest[i].GetLocation();
                int index = GetIndex(new Rect(position.x, position.y, 1f, 1f));

                if (index != -1)
                {
                    nodes[index].Insert(pointsOfInterest[i]);
                    pointsOfInterest.RemoveAt(i);

                    i--;
                    continue;
                }
            }
        }
    }

    public List<PointOfInterest> Retrieve(List<PointOfInterest> returnList, Vector3 position)
    {
        int index = GetIndex(new Rect(position.x, position.y, 1, 1));
        if (index != -1 && nodes[0] != null)
        {
            nodes[index].Retrieve(returnList, position);
        }

        returnList.AddRange(pointsOfInterest);

        return returnList;
    }

}
