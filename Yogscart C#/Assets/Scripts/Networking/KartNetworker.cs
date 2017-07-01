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

    private KartScript ks;
    private kartItem ki;

    void Start()
    {
        ks = GetComponent<KartScript>();
        ki = GetComponent<kartItem>();
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        //If loaded values do not match current then fix it
        if(currentChar != loadedChar || currentHat != loadedHat || currentKart != loadedKart || loadedWheel != currentWheel)
        {
            LoadKartModel();
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
            ks.throttle = throttle;
            ks.steer = steer;
            ks.drift = drift;
            ks.ExpectedSpeed = expectedSpeed;
            ks.onlineMode = true;
            ks.lapisAmount = lapisAmount;

            if (boostTime > 0)
            {
                ks.Boost(boostTime, (KartScript.BoostMode)boostType);
                boostTime = 0;
                boostType = 0;
            }

            //Do Spinout
            if(spinOut != lastSpinOut)
            {
                ks.localSpinOut();
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
        CmdRecieveKartInfo(ks.throttle, ks.steer, ks.drift, ks.ExpectedSpeed, FindObjectOfType<NetworkGUI>().playerName, ks.lapisAmount, spinOut);
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
    public void SendBoost(float time, KartScript.BoostMode type)
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
        base.OnStartLocalPlayer();

        //Tell Client which Kart they own
        NetworkRaceClient.myKart = gameObject;
        FindObjectOfType<NetworkRaceClient>().SetupCameras();
       
    }

    void LoadKartModel()
    {
        //Spawn a new version of the Kart
        Transform newKart = FindObjectOfType<KartMaker>().SpawnKart(KartType.Local, transform.position, transform.rotation, currentChar, currentHat, currentKart, currentWheel);

        //Replace the values in the Original Kart Object to point to the new parts
        GetComponent<KartScript>().particleSystems = newKart.GetComponent<KartScript>().particleSystems;
        GetComponent<KartScript>().ResetParticles();

        GetComponent<DeathCatch>().deathParticles = newKart.GetComponent<DeathCatch>().deathParticles;
        GetComponent<kartAnimator>().ani = newKart.GetComponent<kartAnimator>().ani;

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
    }
}
