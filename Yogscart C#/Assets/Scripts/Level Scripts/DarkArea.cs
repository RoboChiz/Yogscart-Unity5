using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkArea : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        Transform mainObject = GetMainParent(other);
        KartLights kartLights = mainObject.GetComponent<KartLights>();

        if(kartLights != null)
        {
            kartLights.EnterDarkArea();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Transform mainObject = GetMainParent(other);
        KartLights kartLights = mainObject.GetComponent<KartLights>();

        if (kartLights != null)
        {
            kartLights.ExitDarkArea();
        }
    }

    Transform GetMainParent(Collider collider)
    {
        Transform parent = collider.transform;

        while(parent.parent != null)
        {
            parent = parent.parent;
        }

        return parent;
    }
}
