using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartSpawner : MonoBehaviour
{
    public float modifer;

	// Use this for initialization
	IEnumerator Start ()
    {
        yield return new WaitForSeconds(0.5f);

        KartMaker km = FindObjectOfType<KartMaker>();
        Transform kart = km.SpawnKart(KartType.Local, transform.position, transform.rotation * Quaternion.Euler(0, -90, 0), 0,0,0,0);

        //Set speeds of Kart depending on Difficulty
        kartScript ks = kart.GetComponent<kartScript>();
        ks.modifier = modifer;
        ks.locked = false;
        kartScript.raceStarted = true;

        //Spawn Camera
        Transform inGameCam = (Transform)Instantiate(Resources.Load<Transform>("Prefabs/Cameras"), transform.position, Quaternion.identity);
        inGameCam.name = "InGame Cams";

        kartInput ki = kart.GetComponent<kartInput>();
        ki.myController = 0;
        ki.camLocked = true;
        ki.frontCamera = inGameCam.GetChild(1).GetComponent<Camera>();
        ki.backCamera = inGameCam.GetChild(0).GetComponent<Camera>();

        inGameCam.GetChild(1).tag = "MainCamera";

        inGameCam.GetChild(0).transform.GetComponent<kartCamera>().target = kart;
        inGameCam.GetChild(1).transform.GetComponent<kartCamera>().target = kart;

    }
}

