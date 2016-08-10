using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSelect : MonoBehaviour
{
    private MainMenu mm;
    private SoundManager sm;
    private KartMaker km;
    private CurrentGameData gd;

    private bool isShowing = false, loading = true;

    public GUISkin skin;

    public enum csState { Character, Hat, Kart, Off, Finished };
    public csState state = csState.Off;
    public csState State
    {
        get { return state; }
        set { }
    }

    //Cursors
    private Vector2[] cursorPosition;
    private const float cursorSpeed = 5f, rotateSpeed = 60f;

    public Transform[] platforms;
    public bool[] ready, kartSelected, showLayout;

    public CharacterSelectLoadOut[] loadedChoice;
    public Transform[] loadedModels;

    //Content
    private Texture2D nameList;
    private float kartHeight;

    //Transtition
    private float scale = 0.9f;
    private float menuAlpha = 0f, overallAlpha = 0f;
    private bool sliding = false;

    private bool lastAllowed = false, mouseLast = false, canClick = false;
    private Vector2 lastMousePos;

    //Controller Layouts
    private float[] controlLayoutBoxHeights;
    private int[] selectedLayout;
    private Vector2[] layoutScrollPositions;
    private const int layoutBoxStartHeight = 75;

    public IEnumerator ShowCharacterSelect(csState state)
    {
        loading = true;
        lastAllowed = InputManager.allowedToChange;

        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();
        km = FindObjectOfType<KartMaker>();

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
            overallAlpha = menuAlpha;
            scale = Mathf.Lerp(1f, endScale, (Time.time - startTime) / travelTime);
            yield return null;
        }

        menuAlpha = 0f;
        overallAlpha = menuAlpha;
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

        if (loadedModels != null)
        {
            //Clear any models that are loaded
            foreach (Transform t in loadedModels)
                if (t != null)
                    Destroy(t.gameObject);
        }

        loadedModels = new Transform[4];
        loadedChoice = new CharacterSelectLoadOut[4];
        kartSelected = new bool[4];
        showLayout = new bool[4];
        selectedLayout = new int[4];
        layoutScrollPositions = new Vector2[4];

        controlLayoutBoxHeights = new float[4];

        for (int i = 0; i < 4; i++)
        {
            loadedChoice[i] = new CharacterSelectLoadOut(-1, -1, -1, -1);
            controlLayoutBoxHeights[i] = layoutBoxStartHeight;
        }
    }

    void OnGUI()
    {
        if (!loading)
        {
            GUI.skin = skin;
            GUI.depth = -5;
            GUI.matrix = GUIHelper.GetMatrix();
            GUIHelper.SetGUIAlpha(menuAlpha);

            LoadOut[] choice = CurrentGameData.currentChoices;

            float choicesPerColumn = 0f;

            Rect iconArea = GUIHelper.CentreRect(new Rect(10, 115, 950, 800), scale);
            float chunkSize = iconArea.height / 4.5f;

            Vector2 mousePos = GUIHelper.GetMousePosition();
            canClick = false;

            if (mousePos != lastMousePos)
            {
                mouseLast = true;
                lastMousePos = mousePos;
            }

            //Make character text lookat camera

            switch (state)
            {
                case csState.Character:
                    InputManager.allowedToChange = lastAllowed;

                    GUI.Label(new Rect(10, 10, 1920, 95), "Select A Character");
                    GUI.DrawTexture(iconArea, nameList);

                    GUI.BeginGroup(iconArea);

                    //Draw Character heads
                    choicesPerColumn = ((gd.characters.Length / 5f) / 5f) * 5f;
                    for (int i = 0; i < choicesPerColumn; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int characterInt = (i * 5) + j;

                            if (characterInt < gd.characters.Length)
                            {
                                Rect iconRect = new Rect(10 + (j * chunkSize), i * chunkSize, chunkSize, chunkSize);
                                Texture2D icon;

                                if (gd.characters[characterInt].unlocked != UnlockedState.Locked)
                                    icon = gd.characters[characterInt].icon;
                                else
                                    icon = Resources.Load<Texture2D>("UI/Character Icons/question_mark");
                                GUI.DrawTexture(iconRect, icon);

                                //Mouse Controls
                                if (!showLayout[0] && choice != null && choice.Length > 0 && mouseLast && new Rect(iconArea.x + iconRect.x, iconArea.y + iconRect.y, iconRect.width, iconRect.height).Contains(mousePos))
                                {
                                    choice[0].character = characterInt;
                                    canClick = true;
                                }

                            }
                        }
                    }

                    GUI.EndGroup();

                    break;
                case csState.Hat:

                    InputManager.allowedToChange = false;

                    GUI.Label(new Rect(10, 10, 1920, 95), "Select A Hat");

                    GUI.DrawTexture(iconArea, nameList);

                    GUI.BeginGroup(iconArea);

                    //Draw Hat icons
                    choicesPerColumn = ((gd.hats.Length / 5f) / 5f) * 5f;
                    for (int i = 0; i < choicesPerColumn; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int hatInt = (i * 5) + j;

                            if (hatInt < gd.hats.Length)
                            {
                                Rect iconRect = new Rect(10 + (j * chunkSize), i * chunkSize, chunkSize, chunkSize);
                                Texture2D icon;

                                if (gd.hats[hatInt].unlocked != UnlockedState.Locked)
                                    icon = gd.hats[hatInt].icon;
                                else
                                    icon = Resources.Load<Texture2D>("UI/Character Icons/question_mark");
                                GUI.DrawTexture(iconRect, icon);

                                //Mouse Controls
                                if (!showLayout[0] && choice != null && choice.Length > 0 && mouseLast && new Rect(iconArea.x + iconRect.x, iconArea.y + iconRect.y, iconRect.width, iconRect.height).Contains(mousePos))
                                {
                                    choice[0].hat = hatInt;
                                    canClick = true;
                                }
                            }
                        }
                    }

                    GUI.EndGroup();

                    break;
                case (csState.Kart):
                    GUI.Label(new Rect(10, 10, 1920, 95), "Select A Kart");
                    break;
            }

            for (int s = 0; s < InputManager.controllers.Count; s++)
            {

                if (state != csState.Off && state != csState.Kart)
                {
                    int selectedIcon = 0;


                    if (state == csState.Character)
                        selectedIcon = choice[s].character;

                    if (state == csState.Hat)
                        selectedIcon = choice[s].hat;

                    GUI.BeginGroup(iconArea);

                    Vector2 iconSelection = new Vector2(selectedIcon % 5, selectedIcon / 5);
                    cursorPosition[s] = Vector2.Lerp(cursorPosition[s], iconSelection, Time.deltaTime * cursorSpeed);

                    Rect CursorRect = new Rect(10 + cursorPosition[s].x * chunkSize, cursorPosition[s].y * chunkSize, chunkSize, chunkSize);
                    Texture2D CursorTexture = Resources.Load<Texture2D>("UI/Cursors/Cursor_" + s);
                    GUI.DrawTexture(CursorRect, CursorTexture);

                    GUI.EndGroup();
                }

                float topHeight = 115;

                //Render Kart And Wheel
                if (state == csState.Kart)
                {
                    Rect areaRect = new Rect(0, 0, 0, 0);
                    //Rect iconArea = GUIHelper.CentreRect(new Rect(10, 115, 950, 840), scale);

                    if (InputManager.controllers.Count == 1)
                        areaRect = GUIHelper.CentreRect(new Rect(0, topHeight, 1920, 840), scale);

                    if (InputManager.controllers.Count == 2)
                    {
                        if (s == 0)
                            areaRect = GUIHelper.CentreRect(new Rect(0, topHeight, 1920, 420), scale);
                        else
                            areaRect = GUIHelper.CentreRect(new Rect(0, topHeight + 420, 1920, 420), scale);
                    }

                    if (InputManager.controllers.Count > 2)
                    {
                        if (s == 0)
                            areaRect = GUIHelper.CentreRect(new Rect(0, topHeight, 960, 420), scale);
                        if (s == 1)
                            areaRect = GUIHelper.CentreRect(new Rect(960, topHeight, 960, 420), scale);
                        if (s == 2)
                            areaRect = GUIHelper.CentreRect(new Rect(0, topHeight + 420, 960, 420), scale);
                        if (s == 3)
                            areaRect = GUIHelper.CentreRect(new Rect(960, topHeight + 420, 960, 420), scale);
                    }

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
                        Rect kartRect = GUIHelper.CentreRect(new Rect(20, selectionRect.height / 2f - (kartHeight * (kartI + 0.5f)) - loadedChoice[s].kartChangeHeight, kartWidth, kartHeight), loadedChoice[s].kartScales[kartI + 2]);

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

                        //KartScaling
                        if (s == 0 && !showLayout[s])
                        {
                            Rect kartClickArea = new Rect(areaRect.x + selectionRect.x + kartRect.x, areaRect.y + selectionRect.y + kartRect.y, kartRect.width, kartRect.height);
                            //Only scale if icon is onscreen and is yours
                            if (kartI != -2 && kartI != 2)
                                loadedChoice[s].kartScales[kartI + 2] = GUIHelper.SizeHover(kartClickArea, loadedChoice[s].kartScales[kartI + 2], 1f, 1.25f, 2f);

                            //Kart Clicks
                            if (kartI == -1 && GUI.Button(kartRect, ""))
                            {
                                if (!kartSelected[s])
                                    StartCoroutine(ScrollKart(loadedChoice[s], kartHeight, choice[s]));
                                else
                                    kartSelected[s] = false;
                            }

                            if (kartI == 1 && GUI.Button(kartRect, ""))
                            {
                                if (!kartSelected[s])
                                    StartCoroutine(ScrollKart(loadedChoice[s], -kartHeight, choice[s]));
                                else
                                    kartSelected[s] = false;
                            }

                            if (kartI == 0 && GUI.Button(kartRect, ""))
                            {
                                if (!kartSelected[s])
                                    canClick = true;
                                else
                                    kartSelected[s] = false;
                            }
                        }


                        Texture2D wheelIcon = gd.wheels[MathHelper.NumClamp(choice[s].wheel + kartI, 0, gd.wheels.Length)].icon;
                        Rect wheelRect = GUIHelper.CentreRect(new Rect(30 + kartWidth, selectionRect.height / 2f - (kartHeight * (kartI + 0.5f)) - loadedChoice[s].wheelChangeHeight, kartWidth, kartHeight), loadedChoice[s].wheelScales[kartI + 2]);

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

                        //WheelScaling
                        if (!showLayout[s] && s == 0)
                        {
                            Rect wheelClickArea = new Rect(areaRect.x + selectionRect.x + wheelRect.x, areaRect.y + selectionRect.y + wheelRect.y, wheelRect.width, wheelRect.height);
                            //Only scale if icon is onscreen and is yours
                            if (kartI != -2 && kartI != 2)
                                loadedChoice[s].wheelScales[kartI + 2] = GUIHelper.SizeHover(wheelClickArea, loadedChoice[s].wheelScales[kartI + 2], 1f, 1.25f, 2f);

                            //Wheel Clicks
                            if (kartI == -1 && GUI.Button(wheelRect, ""))
                            {
                                if (kartSelected[s])
                                    StartCoroutine(ScrollWheel(loadedChoice[s], kartHeight, choice[s]));
                                else
                                    kartSelected[s] = true;
                            }


                            if (kartI == 1 && GUI.Button(wheelRect, ""))
                            {
                                if (kartSelected[s])
                                    StartCoroutine(ScrollWheel(loadedChoice[s], -kartHeight, choice[s]));
                                else
                                    kartSelected[s] = true;
                            }

                            if (kartI == 0 && GUI.Button(wheelRect, ""))
                            {
                                if (kartSelected[s])
                                    canClick = true;
                                else
                                    kartSelected[s] = true;
                            }
                        }


                    }

                    GUI.EndGroup();

                    Rect upArrowRect = new Rect(0, 0, 0, 0);
                    Rect downArrowRect = new Rect(0, 0, 0, 0);

                    if (kartSelected[s] && ready[s])
                    {
                        Texture2D readyTexture = Resources.Load<Texture2D>("UI/New Main Menu/Ready");
                        GUI.DrawTexture(selectionRect, readyTexture, ScaleMode.ScaleToFit);
                    }

                    if (!kartSelected[s])
                    {
                        upArrowRect = new Rect(20, 10, (selectionRect.width / 2f) - 10, heightChunk - 10);
                        downArrowRect = new Rect(20, heightChunk * 5f, (selectionRect.width / 2f) - 10, heightChunk - 10);

                        if (s == 0 && !showLayout[0])
                        {
                            if (GUI.Button(upArrowRect, ""))
                            {
                                StartCoroutine(ScrollKart(loadedChoice[s], -kartHeight, choice[s]));
                            }
                            if (GUI.Button(downArrowRect, ""))
                            {
                                StartCoroutine(ScrollKart(loadedChoice[s], kartHeight, choice[s]));
                            }
                        }
                    }
                    else
                    {
                        upArrowRect = new Rect(20 + (selectionRect.width / 2f), 10, (selectionRect.width / 2f) - 10, heightChunk - 10);
                        downArrowRect = new Rect(20 + (selectionRect.width / 2f), heightChunk * 5f, (selectionRect.width / 2f) - 10, heightChunk - 10);

                        if (s == 0 && !showLayout[0])
                        {
                            if (GUI.Button(upArrowRect, ""))
                            {
                                StartCoroutine(ScrollWheel(loadedChoice[s], -kartHeight, choice[s]));
                            }
                            if (GUI.Button(downArrowRect, ""))
                            {
                                StartCoroutine(ScrollWheel(loadedChoice[s], kartHeight, choice[s]));
                            }
                        }
                    }

                    GUI.DrawTexture(upArrowRect, arrowIcon, ScaleMode.ScaleToFit);
                    GUI.DrawTexture(downArrowRect, downArrowIcon, ScaleMode.ScaleToFit);

                    GUI.EndGroup();
                }
            }

            GUI.skin = Resources.Load<GUISkin>("GUISkins/Options");
            GUIHelper.SetGUIAlpha(overallAlpha);

            //Draw Input Layouts
            if (InputManager.controllers != null)
            {
                float maxAlpha = overallAlpha * 0.5f;

                for (int i = 0; i < InputManager.controllers.Count; i++)
                {
                    //Detect if Toggle is pressed
                    if (InputManager.controllers[i].GetMenuInput("Toggle") != 0)
                    {
                        showLayout[i] = !showLayout[i];
                    }

                    //Draw closed box if closed
                    if (showLayout[i])
                    {
                        if (controlLayoutBoxHeights[i] < 450f)
                            controlLayoutBoxHeights[i] += Time.deltaTime * 350f;
                        else
                            controlLayoutBoxHeights[i] = 450f;
                    }
                    else
                    {
                        if (controlLayoutBoxHeights[i] > layoutBoxStartHeight)
                            controlLayoutBoxHeights[i] -= Time.deltaTime * 350f;
                        else
                            controlLayoutBoxHeights[i] = layoutBoxStartHeight;
                    }

                    float startX = 300 + (i * 370);

                    Color rectangleColour;

                    if (i == 0)
                        rectangleColour = new Color(0.53f, 0.64f, 0.80f, maxAlpha);
                    else if (i == 1)
                        rectangleColour = new Color(0.48f, 0.8f, 0.47f, maxAlpha);
                    else if (i == 2)
                        rectangleColour = new Color(0.83f, 0.63f, 0.31f, maxAlpha);
                    else
                        rectangleColour = new Color(0.95f, 0.37f, 0.37f, maxAlpha);

                    float boxHeight = controlLayoutBoxHeights[i];

                    Rect roundedRectangleRect = new Rect(startX, 1050 - boxHeight, 350, boxHeight);
                    GUIShape.RoundedRectangle(roundedRectangleRect, 25, rectangleColour);

                    //Open box if clicked
                    if (i == 0 && !showLayout[i] && GUI.Button(roundedRectangleRect, ""))
                        showLayout[i] = true;

                    GUI.Label(new Rect(startX + 20, 1060 - boxHeight, 250, 50), InputManager.controllers[i].controlLayout.Name);
                    GUI.DrawTexture(new Rect(startX + 270, 1060 - boxHeight, 50, 50), Resources.Load<Texture2D>("UI/Controls/" + ((InputManager.controllers[i].controlLayout.Type == ControllerType.Keyboard) ? "Keyboard" : "Xbox_1")));

                    if (boxHeight != layoutBoxStartHeight)
                    {
                        GUI.DrawTexture(new Rect(startX + 20, 1110 - boxHeight, 310, 5), Resources.Load<Texture2D>("UI/Lobby/Line"));

                        int controllerType = (int)InputManager.controllers[i].controlLayout.Type;

                        Rect scrollViewRect = new Rect(startX + 10, 1125 - boxHeight, 330, boxHeight - 75);
                        layoutScrollPositions[i] = GUI.BeginScrollView(scrollViewRect, layoutScrollPositions[i], new Rect(0, 0, 310, (boxHeight == 450f) ? InputManager.splitConfigs[controllerType].Count * 70 : boxHeight - 85));

                        //Scroll to Current Selection
                        if (i != 0 || !mouseLast)
                        {
                            float diff = (selectedLayout[i] * 70f) - layoutScrollPositions[i].y;
                            if(Mathf.Abs(diff) > 10f)
                                layoutScrollPositions[i].y += Mathf.Sign(diff) * Time.deltaTime * 200f;
                        }

                        for (int j = 0; j < InputManager.splitConfigs[controllerType].Count; j++)
                        {
                            Rect labelRect = new Rect(0, j * 70, 250, 70);

                            GUIHelper.CentreRectLabel(labelRect, 1f, InputManager.splitConfigs[controllerType][j].Name, (selectedLayout[i] == j) ? Color.yellow : Color.white);
                            GUI.DrawTexture(new Rect(250, j * 70, 50, 50), Resources.Load<Texture2D>("UI/Controls/" + ((InputManager.splitConfigs[controllerType][j].Type == ControllerType.Keyboard) ? "Keyboard" : "Xbox_1")));

                            if (i == 0 && mouseLast)
                            {
                                labelRect.width += 50;

                                if (GUI.Button(labelRect, ""))
                                {
                                    InputManager.controllers[i].controlLayout = InputManager.splitConfigs[controllerType][selectedLayout[i]];
                                    showLayout[i] = false;
                                }

                                labelRect.x += scrollViewRect.x - layoutScrollPositions[i].x;
                                labelRect.y += scrollViewRect.y - layoutScrollPositions[i].y;

                                if (labelRect.Contains(GUIHelper.GetMousePosition()))
                                    selectedLayout[i] = j;
                            }
                        }

                        GUI.EndScrollView();
                    }                 
                }
            }

        }
    }



    //Input
    void Update()
    {
        LoadOut[] choice = CurrentGameData.currentChoices;
        //Load the Character Model
        bool readyCheck = true;

        bool canInput = true;
        if (mm != null && mm.sliding)
            canInput = false;

        if (loadedChoice != null && choice != null && loadedChoice.Length > 0 && choice.Length > 0)
        {

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

                Camera cam;

                //Default off screen
                if (InputManager.controllers.Count == 0 || !isShowing)
                {
                    cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                }

                if (InputManager.controllers.Count <= 1 || !isShowing)
                {
                    cam = platforms[1].FindChild("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                }

                if (InputManager.controllers.Count <= 2 || !isShowing)
                {
                    cam = platforms[2].FindChild("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                }

                if (InputManager.controllers.Count <= 3 || !isShowing)
                {
                    cam = platforms[3].FindChild("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                }

                if (isShowing)
                {

                    float areaX = GUIHelper.guiEdges.x / Screen.width;
                    float areaY = GUIHelper.guiEdges.y / Screen.height;
                    float areaHeight = 0.3f / ((0.5f / Screen.width) * Screen.height);

                    //FIGURE OUT OK AREA
                    Rect okayArea = new Rect(0.5f, 0f, 0.5f, 1f);

                    if (GUIHelper.widthSmaller)
                        okayArea = new Rect(0.5f, 0.5f - (areaHeight / 2f), 0.5f, areaHeight);
                    else
                        okayArea = new Rect(0.5f, areaY, 0.5f - areaX, 1f - areaY);

                    if (InputManager.controllers.Count == 1)
                    {
                        cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, okayArea, Time.deltaTime * 5f);
                    }

                    if (InputManager.controllers.Count == 2)
                    {
                        cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y + (okayArea.height / 2f), okayArea.width, okayArea.height / 2f), Time.deltaTime * 5f);

                        cam = platforms[1].FindChild("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y, okayArea.width, okayArea.height / 2f), Time.deltaTime * 5f);
                    }

                    if (InputManager.controllers.Count >= 3)
                    {
                        if (state != csState.Kart)
                        {
                            cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y + (okayArea.height / 2f), okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);

                            cam = platforms[2].FindChild("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y, okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);

                        }
                        else
                        {
                            cam = platforms[0].FindChild("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x - okayArea.width / 2f, okayArea.y + (okayArea.height / 2f), okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);


                            cam = platforms[2].FindChild("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x - okayArea.width / 2f, okayArea.y, okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                        }

                        cam = platforms[1].FindChild("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x + okayArea.width / 2f, okayArea.y + (okayArea.height / 2f), okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);

                    }

                    if (InputManager.controllers.Count == 4)
                    {
                        cam = platforms[3].FindChild("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x + okayArea.width / 2f, okayArea.y, okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                    }
                }

                if (loadedModels[s] != null)
                    loadedModels[s].Rotate(Vector3.up, -InputManager.controllers[s].GetInput("Rotate") * Time.deltaTime * rotateSpeed);

                int hori = 0, vert = 0;
                bool submit = false, cancel = false;

                if (canInput && !sliding)
                {
                    if (!ready[s] || showLayout[s])
                    {
                        hori = InputManager.controllers[s].GetMenuInput("MenuHorizontal");
                        vert = InputManager.controllers[s].GetMenuInput("MenuVertical");
                    }

                    submit = (InputManager.controllers[s].GetMenuInput("Submit") != 0);
                    cancel = (InputManager.controllers[s].GetMenuInput("Cancel") != 0);

                    if (s == 0)
                    {
                        if (hori != 0 || vert != 0 || submit || cancel)
                            mouseLast = false;

                        if (canClick && (state == csState.Kart || Input.GetMouseButton(0)))
                            submit = true;
                    }
                }

                if (!showLayout[s])
                {

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
                                    StartCoroutine(ScrollKart(loadedChoice[s], kartHeight, choice[s]));
                                else
                                    StartCoroutine(ScrollKart(loadedChoice[s], -kartHeight, choice[s]));
                            }
                            else
                            {
                                if (vert > 0)

                                    StartCoroutine(ScrollWheel(loadedChoice[s], kartHeight, choice[s]));
                                else
                                    StartCoroutine(ScrollWheel(loadedChoice[s], -kartHeight, choice[s]));
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
                            Back(s);
                        }
                        else
                        {
                            ready[s] = false;
                        }
                    }

                    if (!kartSelected[s])
                    {
                        if (choice[s].kart >= gd.karts.Length)
                            choice[s].kart = 0;

                        if (choice[s].kart < 0)
                            choice[s].kart = gd.karts.Length - 1;

                    }
                    else
                    {
                        if (choice[s].wheel >= gd.wheels.Length)
                            choice[s].wheel = 0;

                        if (choice[s].wheel < 0)
                            choice[s].wheel = gd.wheels.Length - 1;
                    }
                    if (!ready[s])
                        readyCheck = false;
                }
                else //Change Input Layout Controls
                {

                    readyCheck = false;

                    int currentInputType = (int)InputManager.controllers[s].controlLayout.Type;
                    if (vert != 0)
                        selectedLayout[s] = MathHelper.NumClamp(selectedLayout[s] + vert, 0, InputManager.splitConfigs[currentInputType].Count);
                    if (submit)
                    {
                        InputManager.controllers[s].controlLayout = InputManager.splitConfigs[currentInputType][selectedLayout[s]];
                        showLayout[s] = false;
                    }
                    if (cancel)
                        showLayout[s] = false;
                }
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

    public void Back(int s)
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

    public void Cancel()
    {
        HideCharacterSelect(csState.Off);
    }

    private void Finished()
    {
        HideCharacterSelect(csState.Finished);
    }

    //Scroll Kart
    private IEnumerator ScrollKart(CharacterSelectLoadOut loadOut, float finalHeight, LoadOut choice)
    {
        if (!loadOut.scrolling)
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

            choice.kart = MathHelper.NumClamp(choice.kart - (int)Mathf.Sign(finalHeight), 0, gd.karts.Length);

            loadOut.kartChangeHeight = 0;
            loadOut.kartAlpha = 0.4f;

            loadOut.scrolling = false;
        }
    }
    //Scroll Wheel
    private IEnumerator ScrollWheel(CharacterSelectLoadOut loadOut, float finalHeight, LoadOut choice)
    {
        if (!loadOut.scrolling)
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

            choice.wheel = MathHelper.NumClamp(choice.wheel - (int)Mathf.Sign(finalHeight), 0, gd.wheels.Length);

            loadOut.wheelChangeHeight = 0;
            loadOut.wheelAlpha = 0.4f;

            loadOut.scrolling = false;
        }
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
                if (overallAlpha < 1f)
                    overallAlpha = menuAlpha;
                scale = Mathf.Lerp(endScale, 1f, (Time.time - startTime) / travelTime);
                yield return null;
            }

            menuAlpha = 1f;
            overallAlpha = menuAlpha;
            scale = 1f;

            sliding = false;
        }
    }
}

[System.Serializable]
public class CharacterSelectLoadOut : LoadOut
{
    public bool scrolling;
    public float kartChangeHeight = 0f, wheelChangeHeight = 0f, kartAlpha = 0.4f, wheelAlpha = 0.4f;

    public float[] kartScales, wheelScales;

    public CharacterSelectLoadOut(int ch, int ha, int ka, int wh) : base(ch, ha, ka, wh)
    {
        kartScales = new float[] { 1f, 1f, 1f, 1f, 1f };
        wheelScales = new float[] { 1f, 1f, 1f, 1f, 1f };
    }

}