using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartLights : MonoBehaviour
{
    private int insideBoxCount;

    public Light carSpotlight;

    public float range = 10f, spotAngle = 100f, intensity = 1.25f;

    private void Start()
    {
        //If we've not been given a spotlight, make one
        if(carSpotlight == null)
        {
            GameObject lightObject = new GameObject("Spotlight");
            lightObject.transform.parent = transform.Find("Kart Body");
            lightObject.transform.localPosition = new Vector3(0f, 0.4f, 1f);
            lightObject.transform.localRotation = Quaternion.identity;

            carSpotlight = lightObject.AddComponent<Light>();
            carSpotlight.type = LightType.Spot;
            carSpotlight.range = range;
            carSpotlight.spotAngle = spotAngle;
            carSpotlight.intensity = intensity;
        }
    }

    // Update is called once per frame
    void Update ()
    {
	    if(carSpotlight != null)
        {
            if(insideBoxCount == 0)
            {
                carSpotlight.enabled = false;
            }
            else
            {
                carSpotlight.enabled = true;
            }
        }
	}

    public void EnterDarkArea()
    {
        insideBoxCount++;
    }

    public void ExitDarkArea()
    {
        insideBoxCount--;
    }
}
