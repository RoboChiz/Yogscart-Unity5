using UnityEngine;
using System.Collections;

public class BoostPad : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {

        Transform parent = other.transform;

        while (parent.parent != null && parent.GetComponent<KartMovement>() == null)
            parent = parent.parent;

        if (parent.GetComponent<KartMovement>() != null)
            parent.GetComponent<KartMovement>().Boost(2f, KartMovement.BoostMode.Boost);

    }

    void Update()
    {
        GetComponent<MeshRenderer>().material.mainTextureOffset -= new Vector2(0, Time.deltaTime);
    }
}
