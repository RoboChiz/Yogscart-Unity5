﻿using UnityEngine;
using System.Collections;

public enum ItemOwner { Mine, Ai, Online };

public class kartItem : MonoBehaviour
{
    private CurrentGameData gd;
    private kartInfo ki;
    private kartInput kaI;
    private PositionFinding pf;
    private SoundManager sm;

    public int heldPowerUp = -1;//Used to reference the item in Current Game Data
    public bool iteming; //True is player is getting / has an item

    public Transform myItem; //Holds the last item spawned by the player
    public bool sheilding;
    public bool itemSpawned;

    public Texture2D renderItem, frame;
    public int renderHeight;
    public float guiAlpha;

    public ItemOwner itemOwner = ItemOwner.Mine;
    public bool onlineGame;

    private bool spinning = false;

    public bool input = false, locked = true, hidden = true;
    public bool inputLock = false;
    private float inputDirection;
    public float itemDistance = 2f;


    // Use this for initialization
    void Awake()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();
            
        frame = Resources.Load<Texture2D>("UI/Power Ups/item frame");

        if (GetComponent<AI>())//If AI detected must be AI
            itemOwner = ItemOwner.Ai;
    }

    void Start()
    {
        ki = GetComponent<kartInfo>();
        kaI = GetComponent<kartInput>();
        pf = GetComponent<PositionFinding>();
    }

    //Informs all clients that this kart has recieved an item
    public void RecieveItem(int item)
    {
        //Debug.Log("Recieved Item " + item.ToString());
        heldPowerUp = item;
        iteming = true;
    }

    //Makes the local version of the kart use an item, the effect should appear the same on all clients
    public void UseItem()
    {
        if (heldPowerUp != -1)
        {
            //Debug.Log("Used Item");
            //If the current game is not online or if there is no online model for the powerup spawn it normally, otherwise the server will do it
            if (!onlineGame || gd.powerUps[heldPowerUp].onlineModel == null)
            {
                myItem = (Transform)Instantiate(gd.powerUps[heldPowerUp].model, transform.position - (transform.forward * itemDistance), transform.rotation);
                myItem.parent = transform;

                if (myItem.GetComponent<Projectile>() != null)
                    myItem.GetComponent<Projectile>().Setup(transform.forward * inputDirection,false);

                itemSpawned = true;
            }       

            EndItemUse();
        }
    }

    public void UseShield()
    {
        if (heldPowerUp != -1 && gd.powerUps[heldPowerUp].useableShield)
        {
            //Debug.Log("Used Shield");
            sheilding = true;

            //If the current game is not online or if there is no online model for the powerup spawn it normally, otherwise the server will do it
            if (!onlineGame || gd.powerUps[heldPowerUp].onlineModel == null)
            {
                myItem = (Transform)Instantiate(gd.powerUps[heldPowerUp].model, transform.position - (transform.forward * itemDistance), transform.rotation);
                myItem.parent = transform;

                if(myItem.GetComponent<Rigidbody>() != null)
                    myItem.GetComponent<Rigidbody>().isKinematic = true;

                if (myItem.GetComponent<Projectile>() != null)
                    myItem.GetComponent<Projectile>().Setup(transform.forward, true);

                itemSpawned = true;
            }
        }
    }

    public void DropShield(float dir)
    {
        //Debug.Log("Dropped Shield");
        if (myItem != null)
        {
            sheilding = false;

            //Fire the Item if it's a Projectile
            if (myItem.GetComponent<Projectile>() != null)
            {
                //Move Item
                if (inputDirection >= 0)
                {
                    myItem.position = transform.position + (transform.forward * itemDistance * 2f) + (transform.up * 0.5f);
                    inputDirection = 1;
                }

                //Start Projectile Behaviour
                myItem.GetComponent<Projectile>().Setup(transform.forward * inputDirection, false);
            }

            myItem.parent = null;

            if(myItem.GetComponent<Rigidbody>() != null)
                myItem.GetComponent<Rigidbody>().isKinematic = false;

            myItem = null;        

            EndItemUse();
        }
    }

    private void EndItemUse()
    {
        itemSpawned = false;

        if (heldPowerUp != -1 && gd.powerUps[heldPowerUp].multipleUses)
        {
            int tempPowerUp = heldPowerUp - 1;
            RecieveItem(tempPowerUp);
        }
        else
        {
            //Nothing left to do turn off items
            heldPowerUp = -1;
            renderItem = null;
            iteming = false;          
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Crate" && !iteming && itemOwner == ItemOwner.Mine)
        {
            StartCoroutine(DecidePowerUp());
            iteming = true;
        }
    }

    IEnumerator DecidePowerUp()
    {
        int nItem = -1;

        if (pf != null)
        {
            int totalChance = 0;

            for (int i = 0; i < gd.powerUps.Length; i++)
                totalChance += gd.powerUps[i].likelihood[pf.position];

            int randomiser = Random.Range(0, totalChance);

            for (int i = 0; i < gd.powerUps.Length; i++)
            {
                randomiser -= gd.powerUps[i].likelihood[pf.position];

                if (randomiser <= 0)
                {
                    nItem = i;
                    break;
                }
            }
        }
        else
        {
            //Random item for testing purposes
            nItem = Random.Range(0, gd.powerUps.Length);
        }

        yield return StartCoroutine(RollItem(nItem));

        RecieveItem(nItem);

        //If Online tell Server about the item
        if (onlineGame && itemOwner == ItemOwner.Mine)
        {
            FindObjectOfType<UnetClient>().client.Send(UnetMessages.recieveItemMsg, new intMessage(nItem));
        }
    }

    IEnumerator RollItem(int item)
    {
        spinning = true;

        if (itemOwner == ItemOwner.Mine)
        {
            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/Powerup"), 0.5f);

            float size = Screen.width / 8f;
            renderHeight = -(int)size;

            int counter = 0;
            float startTime = Time.time;

            while ((Time.time - startTime) < 1.7)
            {

                renderItem = gd.powerUps[counter].icon;

                yield return StartCoroutine(Scroll());

                if (counter + 1 < gd.powerUps.Length)
                    counter += 1;
                else
                    counter = 0;
            }

            renderItem = gd.powerUps[item].icon;

            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/Powerup2"), 0.5f);
            yield return StartCoroutine(Stop());

        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        spinning = false;
    }

    IEnumerator Scroll()
    {
        float size = Screen.width / 8f;
        float startTime = Time.time, travelTime = 0.2f;

        while ((Time.time - startTime) < travelTime)
        {
            renderHeight = (int)Mathf.Lerp(-size, size, (Time.time - startTime) / travelTime);
            yield return null;
        }

        renderHeight = (int)size;
    }

    IEnumerator Stop()
    {
        float size = Screen.width / 8f;
        float startTime = Time.time, travelTime = 0.2f;

        while ((Time.time - startTime) < travelTime)
        {
            renderHeight = (int)Mathf.Lerp(-size, 0, (Time.time - startTime) / travelTime);
            yield return null;
        }

        renderHeight = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (itemOwner != ItemOwner.Ai && GetComponent<AI>())//If AI detected must be AI
            itemOwner = ItemOwner.Ai;

        if (kaI != null && InputManager.controllers.Count > kaI.myController)
        {
            input = InputManager.controllers[kaI.myController].GetInput("Item") != 0;

            if (InputManager.controllers[kaI.myController].GetInput("RearView") != 0)
            {
                inputDirection = -1;
            }
            else
            {
                inputDirection = 1;

                if (InputManager.controllers[kaI.myController].controllerName != "Key_")
                {
                    inputDirection = -InputManager.controllers[kaI.myController].GetInput("MenuVertical");
                }
            }

        }

        if (!input)
            inputLock = false;

        if (itemOwner == ItemOwner.Mine)
        {
            if (heldPowerUp != -1)
            {
                if (!gd.powerUps[heldPowerUp].useableShield)
                {
                    bool itemKey = input && !inputLock && !locked;
                    if (itemKey)
                    {
                        UseItem();
                        //If Online tell Server about the item use
                        if (onlineGame && itemOwner == ItemOwner.Mine)
                            FindObjectOfType<UnetClient>().client.Send(UnetMessages.useItemMsg, new EmptyMessage());

                        inputLock = true;
                    }
                }
                else
                {
                    if (input)
                    {
                        if (!sheilding)
                        {
                            UseShield();
                            //If Online tell Server about the shield use
                            if (onlineGame && itemOwner == ItemOwner.Mine)
                                FindObjectOfType<UnetClient>().client.Send(UnetMessages.useShieldMsg, new EmptyMessage());                           
                        }
                    }
                    else
                    {
                        if (sheilding)
                        {
                            DropShield(inputDirection);
                            //If Online tell Server about the shield drop
                            if (onlineGame && itemOwner == ItemOwner.Mine)
                                FindObjectOfType<UnetClient>().client.Send(UnetMessages.dropShieldMsg, new floatMessage(inputDirection));     
                        }
                    }
                }
            }
        }

        if (sheilding && myItem == null && itemSpawned)
        {
            sheilding = false;
            EndItemUse();
        }
    }

    void OnGUI()
    {
        if(itemOwner == ItemOwner.Mine)
        {
            GUIHelper.SetGUIAlpha(guiAlpha);

            Rect frameRect = new Rect();
            float size = Screen.width / 8f;

            if(ki != null)
            {
                if (ki.screenPos != ScreenType.Full)
                    size = Screen.width / 16f;

                if (ki.screenPos == ScreenType.Full)
                    frameRect = new Rect(Screen.width - 20 - size, 20, size, size);
                else if (ki.screenPos == ScreenType.TopLeft)
                    frameRect = new Rect(Screen.width / 2f - 20 - size, 20, size, size);
                else if (ki.screenPos == ScreenType.TopRight || ki.screenPos == ScreenType.Top)
                    frameRect = new Rect(Screen.width - 20 - size, 20, size, size);
                else if (ki.screenPos == ScreenType.BottomLeft)
                    frameRect = new Rect(Screen.width / 2f - 20 - size, 20 + Screen.height / 2f, size, size);
                else if (ki.screenPos == ScreenType.BottomRight || ki.screenPos == ScreenType.Bottom)
                    frameRect = new Rect(Screen.width - 20 - size, 20 + Screen.height / 2f, size, size);
            }
            else
            {
                frameRect = new Rect(Screen.width - 20 - size, 20, size, size);
            }

            GUI.BeginGroup(frameRect);

            if (!spinning && heldPowerUp != -1)
                renderItem = gd.powerUps[heldPowerUp].icon;

            if (renderItem != null)
            {
                Rect ItemRect = new Rect(5, 5 + renderHeight, size - 10, size - 10);
                GUI.DrawTexture(ItemRect, renderItem);
            }

            GUI.EndGroup();

            GUI.DrawTexture(frameRect, frame);

            if (iteming && !hidden)
                guiAlpha = Mathf.Lerp(guiAlpha, 1f, Time.deltaTime * 5f);
            else
                guiAlpha = Mathf.Lerp(guiAlpha, 0f, Time.deltaTime * 5f);

            GUIHelper.ResetColor();
        }
    }

}
