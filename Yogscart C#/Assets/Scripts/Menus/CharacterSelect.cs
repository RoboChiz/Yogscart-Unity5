using UnityEngine;
using System.Collections;

public class CharacterSelect : MonoBehaviour
{
    private MainMenu mm;
    private SoundManager sm;
    private KartMaker km;
    private CurrentGameData gd;

    private bool isShowing = false, loading = true;

    public GUISkin skin;

    public enum csState { Character, Hat, Kart, Off, Finished};
    public csState state  = csState.Off;
    public csState State
    {
        get { return state; }
        set { }
    }

    //Cursors
    private Vector2[] cursorPosition;
    private const float cursorSpeed = 5f, rotateSpeed = 30f;

    public Transform[] platforms;
    public bool[] ready, kartSelected;

    public CharacterSelectLoadOut[] loadedChoice;
    public Transform[] loadedModels;

    //Content
    private Texture2D nameList;
    private float kartHeight;

    //Transtition
    private float scale = 0.9f;
    private float menuAlpha = 0f;
    private bool sliding = false;

    public IEnumerator ShowCharacterSelect(csState state)
    {
        loading = true;
        gd = GameObject.FindObjectOfType<CurrentGameData>();
        sm = GameObject.FindObjectOfType<SoundManager>();
        km = GameObject.FindObjectOfType<KartMaker>();

        if (transform.GetComponent<MainMenu>() != null)
            mm = transform.GetComponent<MainMenu>();

        cursorPosition = new Vector2[4];
        nameList = Resources.Load<Texture2D>("UI/Lobby/NamesList");

        ResetEverything();

        yield return new WaitForSeconds(0.5f);
        loading = false;
        isShowing = true;
        StartCoroutine(ChangeState(state));

    }

    public void HideCharacterSelect(csState nState)
    {
        StartCoroutine(ActualHideCharacterSelect(nState));
    }

    private IEnumerator ActualHideCharacterSelect(csState nState)
    {
        isShowing = false;
        sliding = true;

        float startTime = Time.time;
        float travelTime = 0.25f;
        float endScale = 0.9f;
        //Slide Off //////////////////////////////
        while (Time.time - startTime < travelTime)
        {
            menuAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
            scale = Mathf.Lerp(1f, endScale, (Time.time - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 0f;
        scale = endScale;

        if (loadedModels != null)
        {
            //Clear any models that are loaded
            foreach (Transform t in loadedModels)
                if (t != null)
                    Destroy(t.gameObject);
        }

        sliding = false;

        state = nState;

        enabled = false;
    }

    private void ResetReady()
    {
        ready = new bool[4];
    }

    private void ResetEverything()
    {
        ResetReady();
        
        if(loadedModels != null)
        {
            //Clear any models that are loaded
            foreach(Transform t in loadedModels)
                if (t != null)
                    Destroy(t.gameObject);
        }

        loadedModels = new Transform[4];
        loadedChoice = new CharacterSelectLoadOut[4];
        kartSelected = new bool[4];

        for(int i = 0; i < 4; i++)
        {
            loadedChoice[i] = new CharacterSelectLoadOut(-1,-1,-1,-1);
        }
    }

    void OnGUI()
    {
        if (!loading)
        {
            GUI.skin = skin;
            GUI.depth = -5;

            Color nGUI = Color.white;
            nGUI.a = menuAlpha;
            GUI.color = nGUI;

            float fontSize = Mathf.Min(Screen.width, Screen.height) / 20f;
            GUI.skin.label.fontSize = (int)fontSize;

            float chunkSize = (Mathf.Min(Screen.width, Screen.height * 2f) / 10f) * scale;
            LoadOut[] choice = CurrentGameData.currentChoices;

            Rect nameListRect = new Rect(0, 0, 0, 0);
            bool canInput = true;

            if (mm != null && mm.sliding)
                canInput = false;

            float choicesPerColumn = 0f;

            float nameWidth = chunkSize * 5f * scale;
            float nameHeight = (Screen.height - (chunkSize * 1.75f)) * scale;
            float nameX = 10f + (nameWidth * (1 - scale));
            float nameY = (chunkSize * 0.75f) + (nameHeight * (1 - scale));

            switch (state)
            {
                case csState.Character:
                    nameListRect = new Rect(nameX, nameY, nameWidth, nameHeight);
                    GUI.DrawTexture(nameListRect, nameList);
                    GUI.Label(new Rect(10, 10, Screen.width, chunkSize / 2f), "Select A Character");

                    //Draw Character heads
                    choicesPerColumn = ((gd.characters.Length / 5f) / 5f) * 5f;
                    for (int i = 0; i < choicesPerColumn; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int characterInt = (i * 5) + j;

                            if (characterInt < gd.characters.Length)
                            {
                                Rect iconRect = new Rect(10 + (j * chunkSize), nameListRect.y + (i * chunkSize), chunkSize, chunkSize);
                                Texture2D icon;

                                if (gd.characters[characterInt].unlocked != UnlockedState.Locked)
                                    icon = gd.characters[characterInt].icon;
                                else
                                    icon = Resources.Load<Texture2D>("UI/Character Icons/question_mark");
                                GUI.DrawTexture(iconRect, icon);

                            }
                        }
                    }
                    break;
                case csState.Hat:

                    InputManager.allowedToChange = false;

                    nameListRect = new Rect(nameX, nameY, nameWidth, nameHeight);
                    GUI.DrawTexture(nameListRect, nameList);
                    GUI.Label(new Rect(10, 10, Screen.width, chunkSize / 2f), "Select A Hat");

                    //Draw Hat icons
                    choicesPerColumn = ((gd.hats.Length / 5f) / 5f) * 5f;
                    for (int i = 0; i < choicesPerColumn; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int hatInt = (i * 5) + j;

                            if (hatInt < gd.hats.Length)
                            {
                                Rect iconRect = new Rect(10 + (j * chunkSize), nameListRect.y + (i * chunkSize), chunkSize, chunkSize);
                                Texture2D icon;

                                if (gd.hats[hatInt].unlocked != UnlockedState.Locked)
                                    icon = gd.hats[hatInt].icon;
                                else
                                    icon = Resources.Load<Texture2D>("UI/Character Icons/question_mark");
                                GUI.DrawTexture(iconRect, icon);

                            }
                        }
                    }

                    break;
                case (csState.Kart):
                    GUI.Label(new Rect(10, 10, Screen.width, chunkSize / 2f), "Select A Kart");
                    break;
            }

            //Draw the back button

            bool readyCheck = true;
            for (int s = 0; s < InputManager.controllers.Count; s++)
            {
                Quaternion oldRot;
                //Load Character
                if (state == csState.Character || state == csState.Hat)
                {
                    if (loadedChoice[s].character != choice[s].character)
                    {
                        if (loadedModels[s] != null)
                        {
                            oldRot = loadedModels[s].rotation;
                            Destroy(loadedModels[s].gameObject);
                        }
                        else
                            oldRot = Quaternion.identity;

                        if (gd.characters[choice[s].character].unlocked != UnlockedState.Locked)
                        {
                            loadedModels[s] = (Transform)Instantiate(gd.characters[choice[s].character].CharacterModel_Standing, platforms[s].FindChild("Spawn").position, oldRot);
                            loadedModels[s].GetComponent<Rigidbody>().isKinematic = true;
                        }

                        loadedChoice[s].character = choice[s].character;
                    }

                    loadedChoice[s].kart = -1;
                    loadedChoice[s].wheel = -1;

                    if (loadedChoice[s].hat != choice[s].hat)
                    {
                        if (loadedModels[s].GetComponent<StandingCharacter>() != null)
                        {
                            Transform allChildren = loadedModels[s].GetComponent<StandingCharacter>().hatHolder.GetComponentInChildren<Transform>();
                            foreach (Transform child in allChildren)
                            {
                                if (child != loadedModels[s].GetComponent<StandingCharacter>().hatHolder)
                                    Destroy(child.gameObject);
                            }
                        }

                        if (gd.hats[choice[s].hat].model != null && gd.hats[choice[s].hat].unlocked != UnlockedState.Locked)
                        {
                            Transform HatObject = (Transform)Instantiate(gd.hats[choice[s].hat].model, loadedModels[s].GetComponent<StandingCharacter>().hatHolder.position, loadedModels[s].GetComponent<StandingCharacter>().hatHolder.rotation);
                            HatObject.parent = loadedModels[s].GetComponent<StandingCharacter>().hatHolder;
                        }

                        loadedChoice[s].character = choice[s].character;
                        loadedChoice[s].hat = choice[s].hat;
                    }
                }

                if (state == csState.Kart)
                {
                    loadedChoice[s].character = -1;
                    loadedChoice[s].hat = -1;

                    if (loadedChoice[s].kart != choice[s].kart || loadedChoice[s].wheel != choice[s].wheel)
                    {
                        if (loadedModels[s] != null)
                        {
                            oldRot = loadedModels[s].rotation;
                            Destroy(loadedModels[s].gameObject);
                        }
                        else
                            oldRot = Quaternion.identity;

                        loadedModels[s] = km.SpawnKart(KartType.Display, platforms[s].FindChild("Spawn").position + Vector3.up / 2f, oldRot, choice[s].character, choice[s].hat, choice[s].kart, choice[s].wheel);

                        loadedChoice[s].kart = choice[s].kart;
                        loadedChoice[s].wheel = choice[s].wheel;
                    }
                }

                Vector4 oldRect, newRect, nRect;
                Camera cam;

                //Default off screen
                if (InputManager.controllers.Count == 0 || !isShowing)
                {
                    cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                    oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                    newRect = new Vector4(1.5f, 0, oldRect.z, oldRect.w);
                    nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                    cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                }

                if (InputManager.controllers.Count <= 1 || !isShowing)
                {
                    cam = platforms[1].FindChild("Camera").GetComponent<Camera>();
                    oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                    newRect = new Vector4(1.5f, 0, oldRect.z, oldRect.w);
                    nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                    cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                }

                if (InputManager.controllers.Count <= 2 || !isShowing)
                {
                    cam = platforms[2].FindChild("Camera").GetComponent<Camera>();
                    oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                    newRect = new Vector4(1.5f, 0, oldRect.z, oldRect.w);
                    nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                    cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                }

                if (InputManager.controllers.Count <= 3 || !isShowing)
                {
                    cam = platforms[3].FindChild("Camera").GetComponent<Camera>();
                    oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                    newRect = new Vector4(1.5f, 0, oldRect.z, oldRect.w);
                    nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                    cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                }

                if (isShowing)
                {
                    if (InputManager.controllers.Count == 1)
                    {
                        cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                        oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                        newRect = new Vector4(0.5f, 0f, 0.5f, 1f);
                        nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                        cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                    }

                    if (InputManager.controllers.Count == 2)
                    {
                        cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                        oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                        newRect = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
                        nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                        cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);

                        cam = platforms[1].FindChild("Camera").GetComponent<Camera>();
                        oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                        newRect = new Vector4(0.5f, 0f, 0.5f, 0.5f);
                        nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                        cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                    }

                    if (InputManager.controllers.Count >= 3)
                    {
                        if (state != csState.Kart)
                        {
                            cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                            oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                            newRect = new Vector4(0.5f, 0.5f, 0.25f, 0.5f);
                            nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                            cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);

                            cam = platforms[2].FindChild("Camera").GetComponent<Camera>();
                            oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                            newRect = new Vector4(0.5f, 0f, 0.25f, 0.5f);
                            nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                            cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                        }
                        else
                        {
                            cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                            oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                            newRect = new Vector4(0.25f, 0.5f, 0.25f, 0.5f);
                            nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                            cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);

                            cam = platforms[2].FindChild("Camera").GetComponent<Camera>();
                            oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                            newRect = new Vector4(0.25f, 0f, 0.25f, 0.5f);
                            nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                            cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                        }

                        cam = platforms[1].FindChild("Camera").GetComponent<Camera>();
                        oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                        newRect = new Vector4(0.75f, 0.5f, 0.25f, 0.5f);
                        nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                        cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);

                    }

                    if (InputManager.controllers.Count == 4)
                    {
                        cam = platforms[3].FindChild("Camera").GetComponent<Camera>();
                        oldRect = new Vector4(cam.rect.x, cam.rect.y, cam.rect.width, cam.rect.height);
                        newRect = new Vector4(0.75f, 0f, 0.25f, 0.5f);
                        nRect = Vector4.Lerp(oldRect, newRect, Time.deltaTime * 5f);
                        cam.rect = new Rect(nRect.x, nRect.y, nRect.z, nRect.w);
                    }
                }

                if (loadedModels[s] != null)
                    loadedModels[s].Rotate(Vector3.up, -InputManager.controllers[s].GetInput("Rotate") * Time.deltaTime * rotateSpeed);

                int hori = 0, vert = 0;
                bool submit = false, cancel = false;

                if (canInput && !sliding)
                {
                    if (!ready[s])
                    {
                        hori = InputManager.controllers[s].GetMenuInput("MenuHorizontal");
                        vert = -InputManager.controllers[s].GetMenuInput("MenuVertical");
                    }

                    submit = (InputManager.controllers[s].GetMenuInput("Submit") != 0);
                    cancel = (InputManager.controllers[s].GetMenuInput("Cancel") != 0);

                }

                if (hori != 0)
                {
                    if (state == csState.Character)
                    {
                        int itemOnRow = 5;
                        int itemsLeft = gd.characters.Length % 5;

                        if (itemsLeft != 0 && choice[s].character >= gd.characters.Length - itemsLeft)
                            itemOnRow = itemsLeft;

                        if ((choice[s].character % 5) + hori >= itemOnRow)
                            choice[s].character -= itemOnRow - 1;
                        else if ((choice[s].character % 5) + hori < 0)
                            choice[s].character += itemOnRow - 1;
                        else
                            choice[s].character += hori;
                    }
                    if (state == csState.Hat)
                    {
                        int itemOnRow = 5;
                        int itemsLeft = gd.hats.Length % 5;

                        if (itemsLeft != 0 && choice[s].hat >= gd.hats.Length - itemsLeft)
                            itemOnRow = itemsLeft;

                        if ((choice[s].hat % 5) + hori >= itemOnRow)
                            choice[s].hat -= itemOnRow - 1;
                        else if ((choice[s].hat % 5) + hori < 0)
                            choice[s].hat += itemOnRow - 1;
                        else
                            choice[s].hat += hori;
                    }
                }

                if (vert != 0)
                {
                    if (state != csState.Kart)
                        vert *= 5;

                    if (state == csState.Character)
                    {

                        int itemsLeft = gd.characters.Length % 5;
                        int rowNumber = gd.characters.Length / 5;

                        if (itemsLeft != 0)
                            rowNumber++;

                        if (choice[s].character + vert >= gd.characters.Length)
                        {
                            choice[s].character = (choice[s].character + vert) % 5;
                        }
                        else if (choice[s].character + vert < 0)
                        {
                            int toAdd = choice[s].character + ((rowNumber - 1) * 5);

                            if (toAdd >= gd.characters.Length)
                                choice[s].character = toAdd - 5;
                            else
                                choice[s].character = toAdd;
                        }
                        else
                            choice[s].character += vert;

                    }
                    if (state == csState.Hat)
                    {
                        int itemsLeft = gd.hats.Length % 5;
                        int rowNumber = gd.hats.Length / 5;

                        if (itemsLeft != 0)
                            rowNumber++;

                        if (choice[s].hat + vert >= gd.hats.Length)
                        {
                            choice[s].hat = (choice[s].hat + vert) % 5;
                        }
                        else if (choice[s].hat + vert < 0)
                        {
                            int toAdd = choice[s].hat + ((rowNumber - 1) * 5);

                            if (toAdd >= gd.hats.Length)
                                choice[s].hat = toAdd - 5;
                            else
                                choice[s].hat = toAdd;
                        }
                        else
                            choice[s].hat += vert;
                    }

                    if (state == csState.Kart && !loadedChoice[s].scrolling)
                    {
                        if (!kartSelected[s])
                        {
                            if (vert > 0)
                                StartCoroutine(ScrollKart(loadedChoice[s], kartHeight));
                            else
                                StartCoroutine(ScrollKart(loadedChoice[s], -kartHeight));
                        }
                        else
                        {
                            if (vert > 0)
                                StartCoroutine(ScrollWheel(loadedChoice[s], kartHeight));
                            else
                                StartCoroutine(ScrollWheel(loadedChoice[s], -kartHeight));
                        }
                    }
                }

                if (submit)
                {
                    if (state == csState.Character && gd.characters[choice[s].character].unlocked != UnlockedState.Locked)
                    {
                        if (gd.characters[choice[s].character].selectedSound != null)
                            sm.PlaySFX(gd.characters[choice[s].character].selectedSound);

                        loadedModels[s].GetComponent<Animator>().CrossFade("Selected", 0.01f);

                        ready[s] = true;
                    }

                    if (state == csState.Hat && gd.hats[choice[s].hat].unlocked != UnlockedState.Locked)
                    {
                        ready[s] = true;
                    }

                    if (state == csState.Kart)
                    {
                        if (kartSelected[s])
                        {
                            ready[s] = true;
                        }
                        else
                        {
                            kartSelected[s] = true;
                        }
                    }
                }

                if (cancel)
                {
                    if (!ready[s])
                    {
                        if (state == csState.Character)
                            Cancel();
                        if (state == csState.Hat)
                            StartCoroutine(ChangeState(csState.Character));
                        if (state == csState.Kart)
                        {
                            if (kartSelected[s])
                                kartSelected[s] = false;
                            else
                                StartCoroutine(ChangeState(csState.Hat));
                        }
                    }
                    else
                    {
                        ready[s] = false;
                    }
                }

                if (state != csState.Off && state != csState.Kart)
                {
                    int selectedIcon = 0;

                    if (state == csState.Character)
                        selectedIcon = choice[s].character;

                    if (state == csState.Hat)
                        selectedIcon = choice[s].hat;

                    Vector2 iconSelection = new Vector2(selectedIcon % 5, selectedIcon / 5);
                    cursorPosition[s] = Vector2.Lerp(cursorPosition[s], iconSelection, Time.deltaTime * cursorSpeed);

                    Rect CursorRect = new Rect(10 + cursorPosition[s].x * chunkSize, nameListRect.y + cursorPosition[s].y * chunkSize, chunkSize, chunkSize);
                    Texture2D CursorTexture = Resources.Load<Texture2D>("UI/Cursors/Cursor_" + s);
                    GUI.DrawTexture(CursorRect, CursorTexture);
                }

                Texture2D backTexture = Resources.Load<Texture2D>("UI/New Main Menu/backnew");
                float backRatio = (Screen.width / 6f) / backTexture.width;
                float topHeight = Screen.height * 0.05f;
                float screenHeight = Screen.height - 10 - (backTexture.height * backRatio) - topHeight;

                //Render Kart And Wheel
                if (state == csState.Kart)
                {
                    Rect areaRect = new Rect(0, 0, 0, 0);

                    if (InputManager.controllers.Count == 1)
                        areaRect = new Rect(0, topHeight, Screen.width, screenHeight);

                    if (InputManager.controllers.Count == 2)
                    {
                        if (s == 0)
                            areaRect = new Rect(0, topHeight, Screen.width, screenHeight / 2f);
                        else
                            areaRect = new Rect(0, topHeight + screenHeight / 2f, Screen.width, screenHeight / 2f);
                    }

                    if (InputManager.controllers.Count > 2)
                    {
                        if (s == 0)
                            areaRect = new Rect(0, topHeight, Screen.width / 2f, screenHeight / 2f);
                        if (s == 1)
                            areaRect = new Rect(Screen.width / 2f, topHeight, Screen.width / 2f, screenHeight / 2f);
                        if (s == 2)
                            areaRect = new Rect(0, topHeight + screenHeight / 2f, Screen.width / 2f, screenHeight / 2f);
                        if (s == 3)
                            areaRect = new Rect(Screen.width / 2f, topHeight + screenHeight / 2f, Screen.width / 2f, screenHeight / 2f);
                    }

                    areaRect = new Rect(areaRect.x + (areaRect.width * (1 - scale)), areaRect.y + (areaRect.height * (1 - scale)), areaRect.width * scale, areaRect.height * scale);

                    float heightChunk = areaRect.height / 6f;

                    GUI.BeginGroup(areaRect);

                    Rect selectionRect = new Rect(10, heightChunk, (areaRect.width / 2f) - 10, heightChunk * 4f);
                    GUI.DrawTexture(selectionRect, nameList);

                    GUI.BeginGroup(selectionRect);

                    Texture2D kartIcon = gd.karts[choice[s].kart].icon;
                    Texture2D arrowIcon = Resources.Load<Texture2D>("UI/New Character Select/Arrow");
                    Texture2D downArrowIcon = Resources.Load<Texture2D>("UI/New Character Select/Arrow_Down");

                    float kartWidth = (selectionRect.width / 2f) - 10;
                    kartHeight = selectionRect.height / 3f;

                    for (int kartI = -2; kartI <= 2; kartI++)
                    {
                        kartIcon = gd.karts[MathHelper.NumClamp(choice[s].kart + kartI, 0, gd.karts.Length)].icon;
                        Rect kartRect = new Rect(20, selectionRect.height / 2f - (kartHeight * (kartI + 0.5f)) - loadedChoice[s].kartChangeHeight, kartWidth, kartHeight);

                        float nAlpha = loadedChoice[s].kartAlpha;
                        if (kartI == 0)
                            nAlpha = 1.4f - nAlpha;
                        if (kartI == -2 || kartI == 2)
                            nAlpha = 0.4f;
                        if (kartI == 1 && loadedChoice[s].kartChangeHeight > 0)
                            nAlpha = 0.4f;
                        if (kartI == -1 && loadedChoice[s].kartChangeHeight < 0)
                            nAlpha = 0.4f;

                        Color nColor = Color.white;
                        nColor.a = nAlpha * menuAlpha;
                        GUI.color = nColor;

                        GUI.DrawTexture(kartRect, kartIcon, ScaleMode.ScaleToFit);

                        Texture2D wheelIcon = gd.wheels[MathHelper.NumClamp(choice[s].wheel + kartI, 0, gd.wheels.Length)].icon;
                        Rect wheelRect = new Rect(30 + kartWidth, selectionRect.height / 2f - (kartHeight * (kartI + 0.5f)) - loadedChoice[s].wheelChangeHeight, kartWidth, kartHeight);

                        nAlpha = loadedChoice[s].wheelAlpha;
                        if (kartI == 0)
                            nAlpha = 1.4f - nAlpha;
                        if (kartI == -2 || kartI == 2)
                            nAlpha = 0.4f;
                        if (kartI == 1 && loadedChoice[s].wheelChangeHeight > 0)
                            nAlpha = 0.4f;
                        if (kartI == -1 && loadedChoice[s].wheelChangeHeight < 0)
                            nAlpha = 0.4f;

                        nColor.a = nAlpha * menuAlpha;
                        GUI.color = nColor;

                        GUI.DrawTexture(wheelRect, wheelIcon, ScaleMode.ScaleToFit);

                        GUI.color = nGUI;

                    }

                    GUI.EndGroup();

                    Rect upArrowRect = new Rect(0, 0, 0, 0);
                    Rect downArrowRect = new Rect(0, 0, 0, 0);

                    if (!kartSelected[s])
                    {

                        upArrowRect = new Rect(20, 10, (selectionRect.width / 2f) - 10, heightChunk - 10);
                        downArrowRect = new Rect(20, heightChunk * 5f, (selectionRect.width / 2f) - 10, heightChunk - 10);

                        if (choice[s].kart >= gd.karts.Length)
                            choice[s].kart = 0;

                        if (choice[s].kart < 0)
                            choice[s].kart = gd.karts.Length - 1;

                    }
                    else
                    {

                        upArrowRect = new Rect(20 + (selectionRect.width / 2f), 10, (selectionRect.width / 2f) - 10, heightChunk - 10);
                        downArrowRect = new Rect(20 + (selectionRect.width / 2f), heightChunk * 5f, (selectionRect.width / 2f) - 10, heightChunk - 10);

                        if (choice[s].wheel >= gd.wheels.Length)
                            choice[s].wheel = 0;

                        if (choice[s].wheel < 0)
                            choice[s].wheel = gd.wheels.Length - 1;

                        if (ready[s])
                        {
                            Texture2D readyTexture = Resources.Load<Texture2D>("UI/New Main Menu/Ready");
                            GUI.DrawTexture(selectionRect, readyTexture, ScaleMode.ScaleToFit);
                        }

                    }

                    GUI.DrawTexture(upArrowRect, arrowIcon, ScaleMode.ScaleToFit);
                    GUI.DrawTexture(downArrowRect, downArrowIcon, ScaleMode.ScaleToFit);

                    GUI.EndGroup();
                }

                if (!ready[s])
                    readyCheck = false;
            }

            if (InputManager.controllers.Count == 0)
                readyCheck = false;

            if (readyCheck)
            {
                if (state == csState.Character)
                {
                    StartCoroutine(ChangeState(csState.Hat));
                    //Reset the hats
                    loadedChoice[0].hat = -1;
                    loadedChoice[1].hat = -1;
                    loadedChoice[2].hat = -1;
                    loadedChoice[3].hat = -1;
                }
                else if (state == csState.Hat)
                    StartCoroutine(ChangeState(csState.Kart));
                else if (state == csState.Kart)
                    Finished();

                ResetReady();
            }
        }
    }

    public void Cancel()
    {
        HideCharacterSelect(csState.Off);
    }

    private void Finished()
    {
        HideCharacterSelect(csState.Finished);
    }

    //Scroll Kart
    private IEnumerator ScrollKart(CharacterSelectLoadOut loadOut, float finalHeight)
    {
        loadOut.scrolling = true;

        float startTime = Time.time;
        float scrollTime = 0.15f;

        while (Time.time - startTime < scrollTime)
        {
            loadOut.kartChangeHeight = Mathf.Lerp(0f, finalHeight, (Time.time - startTime) / scrollTime);
            loadOut.kartAlpha = Mathf.Lerp(0.4f, 1f, (Time.time - startTime) / scrollTime);
            yield return null;
        }

        loadOut.kartChangeHeight = finalHeight;
        loadOut.kartAlpha = 1f;

        loadOut.kart -= (int)Mathf.Sign(finalHeight);

        loadOut.kart = MathHelper.NumClamp(loadOut.kart, 0, gd.karts.Length);

        loadOut.kartChangeHeight = 0;
        loadOut.kartAlpha = 0.4f;

        loadOut.scrolling = false;
    }
    //Scroll Wheel
    private IEnumerator ScrollWheel(CharacterSelectLoadOut loadOut, float finalHeight)
    {
        loadOut.scrolling = true;

        float startTime = Time.time;
        float scrollTime = 0.15f;

        while (Time.time - startTime < scrollTime)
        {
            loadOut.wheelChangeHeight = Mathf.Lerp(0f, finalHeight, (Time.time - startTime) / scrollTime);
            loadOut.wheelAlpha = Mathf.Lerp(0.4f, 1f, (Time.time - startTime) / scrollTime);
            yield return null;
        }

        loadOut.wheelChangeHeight = finalHeight;
        loadOut.wheelAlpha = 1f;

        loadOut.wheel -= (int)Mathf.Sign(finalHeight);

        loadOut.wheel = MathHelper.NumClamp(loadOut.wheel, 0, gd.wheels.Length);

        loadOut.wheelChangeHeight = 0;
        loadOut.wheelAlpha = 0.4f;

        loadOut.scrolling = false;
    }

    private IEnumerator ChangeState(csState nState)
    {
        if (!sliding)
        {
            sliding = true;

            float startTime = Time.time;
            float travelTime = 0.25f;
            float endScale = 0.9f;

            if (scale > endScale)
            {
                //Slide Off //////////////////////////////
                menuAlpha = 1f;
                scale = 1f;

                while (Time.time - startTime < travelTime)
                {
                    menuAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / travelTime);
                    scale = Mathf.Lerp(1f, endScale, (Time.time - startTime) / travelTime);
                    yield return null;
                }

                //Pause at Top///////////////////////////////////////
                menuAlpha = 0f;
                scale = endScale;
            }

            state = nState;

            //Slide Down/////////////////////
            startTime = Time.time;
            while (Time.time - startTime < travelTime)
            {
                menuAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / travelTime);
                scale = Mathf.Lerp(endScale, 1f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            menuAlpha = 1f;
            scale = 1f;

            sliding = false;
        }
    }
}

public class CharacterSelectLoadOut : LoadOut
{
    public bool scrolling;
    public float kartChangeHeight = 0f, wheelChangeHeight = 0f, kartAlpha = 0.4f, wheelAlpha = 0.4f;

    public CharacterSelectLoadOut(int ch, int ha, int ka, int wh) : base (ch,ha,ka,wh)
    {
    }

}