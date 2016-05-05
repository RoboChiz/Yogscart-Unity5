using UnityEngine;
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

    public Transform myItem { get; protected set; } //Holds the last item spawned by the player
    private bool sheilding;

    public Texture2D renderItem, frame;
    public int renderHeight;
    public float guiAlpha;

    public ItemOwner itemOwner = ItemOwner.Mine;

    private bool spinning = false;

    public bool input = false, locked = true, hidden = true;
    private bool inputLock = false;
    private float inputDirection;
    public float itemDistance = 2f;


    // Use this for initialization
    void Awake()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        ki = GetComponent<kartInfo>();
        kaI = GetComponent<kartInput>();
        pf = GetComponent<PositionFinding>();

        frame = Resources.Load<Texture2D>("UI/Power Ups/item frame");
    }

    //Informs all clients that this kart has recieved an item
    public void RecieveItem(int item)
    {
        heldPowerUp = item;
        iteming = true;
    }

    //Makes the local version of the kart use an item, the effect should appear the same on all clients
    void UseItem()
    {
        if (heldPowerUp != -1)
        {
            myItem = (Transform)Instantiate(gd.powerUps[heldPowerUp].model, transform.position - (transform.forward * itemDistance), transform.rotation);
            myItem.parent = transform;

            EndItemUse();
        }
    }

    void UseShield()
    {
        if (heldPowerUp != -1 && gd.powerUps[heldPowerUp].useableShield)
        {
            myItem = (Transform)Instantiate(gd.powerUps[heldPowerUp].model, transform.position - (transform.forward * itemDistance), transform.rotation);
            myItem.parent = transform;
            myItem.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void DropShield(float dir)
    {
        if (myItem != null)
        {
            inputDirection = dir;

            myItem.parent = null;
            myItem.GetComponent<Rigidbody>().isKinematic = false;
            myItem = null;

            EndItemUse();
        }
    }

    void EndItemUse()
    {
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
    }

    IEnumerator RollItem(int item)
    {
        spinning = true;

        if (itemOwner == ItemOwner.Mine)
        {
            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/Powerup"));

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

            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/Powerup2"));
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
        if(ki == null)
            ki = GetComponent<kartInfo>();

        if (GetComponent<RacerAI>())//If AI detected must be AI
            itemOwner = ItemOwner.Ai;

        if (kaI != null)
        {
            input = InputManager.controllers[kaI.myController].GetInput("Item") != 0;

            if (InputManager.controllers[kaI.myController].GetInput("RearView") != 0)
                inputDirection = -1;
            else
            {
                inputDirection = 1;

                if (InputManager.controllers[kaI.myController].controllerName != "Key_")
                {
                    inputDirection = InputManager.controllers[kaI.myController].GetInput("MenuVertical");
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
                            sheilding = true;
                        }
                    }
                    else
                    {
                        if (sheilding)
                        {
                            DropShield(inputDirection);
                            sheilding = false;
                        }
                    }
                }
            }
        }

        if (sheilding && myItem == null)
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
