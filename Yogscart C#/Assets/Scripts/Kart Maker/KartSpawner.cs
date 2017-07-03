using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartSpawner : MonoBehaviour
{
    public float modifer = 1f;
    public bool ai;
    public bool spawnCamera = true;

    public AI.AIStupidity stupidty;

    public int character, hat, kartVal, wheels;
    public int startItem = -1;

   // Use this for initialization
   IEnumerator Start ()
    {
        yield return new WaitForSeconds(0.2f);

        KartMaker km = FindObjectOfType<KartMaker>();
        Transform kart = km.SpawnKart(KartType.Local, transform.position, transform.rotation * Quaternion.Euler(0, -90, 0), character, hat, kartVal, wheels);

        //Set speeds of Kart depending on Difficulty
        KartScript ks = kart.GetComponent<KartScript>();
        ks.modifier = modifer;
        ks.locked = false;
        KartScript.raceStarted = true;

        kartInput ki = kart.GetComponent<kartInput>();
        ki.myController = 0;
        ki.camLocked = false;

        //Spawn Camera
        if (spawnCamera)
        {
            Transform inGameCam = (Transform)Instantiate(Resources.Load<Transform>("Prefabs/Cameras"), transform.position, Quaternion.identity);
            inGameCam.name = "InGame Cams";
       
            ki.frontCamera = inGameCam.GetChild(1).GetComponent<Camera>();
            ki.backCamera = inGameCam.GetChild(0).GetComponent<Camera>();

            inGameCam.GetChild(1).tag = "MainCamera";

            inGameCam.GetChild(0).transform.GetComponent<kartCamera>().target = kart;
            inGameCam.GetChild(1).transform.GetComponent<kartCamera>().target = kart;
        }

        if(ai)
        {
            if (spawnCamera)
                ki.backCamera.enabled = false;

            Destroy(kart.GetComponent<kartInput>());
            AI ai = kart.gameObject.AddComponent<AI>();
            ai.intelligence = stupidty;
        }

        kart.GetComponent<KartItem>().locked = false;
        kart.GetComponent<KartItem>().hidden = false;

        if (startItem >= 0)
        {
            kart.GetComponent<KartItem>().RecieveItem(startItem);
        }
    }
}

