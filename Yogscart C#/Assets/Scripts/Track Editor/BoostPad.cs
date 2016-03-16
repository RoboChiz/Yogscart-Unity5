using UnityEngine;
using System.Collections;

public class BoostPad : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {

        Transform parent = other.transform;

        while (parent.parent != null && parent.GetComponent<kartScript>() == null)
            parent = parent.parent;

        if (parent.GetComponent<kartScript>() != null)
            parent.GetComponent<kartScript>().Boost(2f, kartScript.BoostMode.Boost);

    }

    void Update()
    {
        GetComponent<MeshRenderer>().material.mainTextureOffset -= new Vector2(0, Time.deltaTime);
    }
}
