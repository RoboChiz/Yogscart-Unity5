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
    private bool drift;
    [SyncVar]
    private int boostType, lapisAmount;

    //Used to call local Item Functions
    [SyncVar]
    public int recieveItem, useItem, useShield, dropShield, currentItem, spinOut;
    [SyncVar]
    public float currentItemDir;

    private int lastRecieveItem, lastUseItem, lastUseShield, lastDropShield, lastSpinOut;

    [SyncVar]
    public string kartPlayerName = "Player";

    private KartMovement kartMovement;
    private KartItem ki;
    private bool kartLoaded = false;

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

        if (isLocalPlayer)
        {
            SendKartInfo();

            if (ki != null)
            {
                ki.itemOwner = ItemOwner.Mine;
            }

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
            if(spinOut != lastSpinOut)
            {
                kartMovement.localSpinOut();
                lastSpinOut = spinOut;
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

    [ClientCallback]
    private void SendKartInfo()
    {
        CmdRecieveKartInfo(kartMovement.throttle, kartMovement.steer, kartMovement.drift, kartMovement.expectedSpeed, FindObjectOfType<CurrentGameData>().playerName, kartMovement.lapisAmount, spinOut);
    }

    [Command]
    private void CmdRecieveKartInfo(float _throttle, float _steer, bool _drift, float _expectedSpeed, string _playerName, int _lapisAmount, int _spinOut)
    {
        throttle = _throttle;
        steer = _steer;
        drift = _drift;
        expectedSpeed = _expectedSpeed;
        kartPlayerName = _playerName;
        lapisAmount = _lapisAmount;
        spinOut = _spinOut;
    }

    [ClientCallback]
    public void SendBoost(float time, KartMovement.BoostMode type)
    {
        if(isLocalPlayer)
        {
            CmdRecieveBoost(time, (int)type);
        }
    }

    [Command]
    private void CmdRecieveBoost(float time, int type)
    {
        boostTime = time;
        boostType = type;            
    }

    //Called on client when Player is created
    public override void OnStartLocalPlayer()
    {
        //Tell Client which Kart they own
        FindObjectOfType<NetworkRace>().localRacer.ingameObj = transform;

        StartCoroutine(ConnectCameras());
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
