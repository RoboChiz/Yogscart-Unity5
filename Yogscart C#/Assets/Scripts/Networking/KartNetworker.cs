using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class KartNetworker : NetworkBehaviour
{
    //Current and Loaded values of the Player used to load the correct models onto networked karts
    [SyncVar]
    public int currentChar = -1, currentHat = -1, currentKart = -1, currentWheel = -1;

    private int loadedChar = -1, loadedHat = -1, loadedKart = -1, loadedWheel = -1;

    //Stores the essential values of the clients kart script
    [SyncVar]
    private float throttle, steer, expectedSpeed, boostTime;
    [SyncVar]
    private bool drift, spinningOut;
    [SyncVar]
    private int boostType, lapisAmount;

    //Used to call local Item Functions
    public int recieveItem, useItem, useShield, dropShield, currentItem;
    public float currentItemDir;

    private int lastRecieveItem, lastUseItem, lastUseShield, lastDropShield;

    public string kartPlayerName = "Player";

    private KartMovement kartMovement;
    private KartItem ki;
    private bool kartLoaded = false;
    private bool isMine = false;

    void Start()
    {
        kartMovement = GetComponent<KartMovement>();
        ki = GetComponent<KartItem>();
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        //If loaded values do not match current then fix it
        if(currentChar != loadedChar || currentHat != loadedHat || currentKart != loadedKart || loadedWheel != currentWheel)
        {
            LoadKartModel();
            kartLoaded = true;
        }

        if (isMine)
        {
            SendKartInfo();
        }
        else
        {
            //Update the Kart Scripts Values 
            kartMovement.throttle = throttle;
            kartMovement.steer = steer;
            kartMovement.drift = drift;
            kartMovement.expectedSpeed = expectedSpeed;
            kartMovement.onlineMode = true;
            kartMovement.lapisAmount = lapisAmount;

            if (boostTime > 0)
            {
                kartMovement.Boost(boostTime, (KartMovement.BoostMode)boostType);
                boostTime = 0;
                boostType = 0;
            }

            //Do Spinout
            if(spinningOut)
            {
                kartMovement.localSpinOut();
                spinningOut = false;
            }

            //Add Player Name
            Text text = GetComponentInChildren<Text>();
            text.text = kartPlayerName;

            if(ki != null)
            {
                //If a function has been again since last time call the function locally
                if(recieveItem != lastRecieveItem)
                {
                    ki.RecieveItem(currentItem);
                    lastRecieveItem = recieveItem;
                }

                if (useItem != lastUseItem)
                {
                    ki.UseItem();
                    lastUseItem = useItem;
                }

                if(useShield != lastUseShield)
                {
                    ki.UseShield();
                    lastUseShield = useShield;
                }

                if (dropShield != lastDropShield)
                {
                    ki.DropShield(currentItemDir);
                    lastDropShield = dropShield;
                }
            }
        }         
    }

    private void SendKartInfo()
    {
        if(throttle != kartMovement.throttle)
            throttle = kartMovement.throttle;

        if (steer != kartMovement.steer)
            steer = kartMovement.steer;

        if (drift != kartMovement.drift)
            drift = kartMovement.drift;

        if (expectedSpeed != kartMovement.expectedSpeed)
            expectedSpeed = kartMovement.expectedSpeed;

        if (lapisAmount != kartMovement.lapisAmount)
            lapisAmount = kartMovement.lapisAmount;

        if(spinningOut != kartMovement.spinningOut)
            spinningOut = kartMovement.spinningOut;

        if(boostType != (int)kartMovement.isBoosting)
            boostType = (int)kartMovement.isBoosting;

        if (boostTime != kartMovement.boostTime)
            boostTime = kartMovement.boostTime;
    }


    //Called on client when Player is created
    public override void OnStartLocalPlayer()
    {
        isMine = true;

        //Tell Client which Kart they own
        FindObjectOfType<NetworkRace>().localRacer.ingameObj = transform;
        StartCoroutine(ConnectCameras());
    }

    public void OnStartHost()
    {
        isMine = true;
    }

    private IEnumerator ConnectCameras()
    {
        yield return new WaitForSeconds(0.2f);

        FindObjectOfType<NetworkRace>().SetupCameras();
    }

    void LoadKartModel()
    {
        //Spawn a new version of the Kart
        Transform newKart = FindObjectOfType<KartMaker>().SpawnKart(KartType.Local, transform.position, transform.rotation, currentChar, currentHat, currentKart, currentWheel);
        newKart.GetComponent<KartMovement>().SetupKart();

        //Replace the values in the Original Kart Object to point to the new parts
        GetComponent<DeathCatch>().deathParticles = newKart.GetComponent<DeathCatch>().deathParticles;
        GetComponent<KartAnimator>().ani = newKart.GetComponent<KartAnimator>().ani;

        //Replace the existing model, colliders and canvas (Move into place of existing, then delete original)
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        while (newKart.childCount > 0)
        {
            if (newKart.GetChild(0).GetComponent<RectTransform>() == null)
            {
                newKart.GetChild(0).parent = transform;
            }
            else
            {
                newKart.GetChild(0).GetComponent<RectTransform>().SetParent(transform);
            }
        }   

        Destroy(newKart.gameObject);      

        loadedChar = currentChar;
        loadedHat = currentHat;
        loadedKart = currentKart;
        loadedWheel = currentWheel;

        GetComponent<KartMovement>().CopyFrom(newKart.GetComponent<KartMovement>());
        GetComponent<KartMovement>().SetupOnlineKart();
    }
}
