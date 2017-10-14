﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSelect : MonoBehaviour
{
    private MainMenu mm;
    private SoundManager sm;
    private KartMaker km;
    private CurrentGameData gd;

    private bool isShowing = false, loading = true, affectAllGUIwithAlpha;

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
    private Texture2D nameList, rotateKey, rotateXbox;
    private float kartHeight;

    //Rotate Rects
    private Rect[] rotateRects;

    //Transtition
    private float scale = 0.9f;
    private float menuAlpha = 0f, overallAlpha = 0f;
    private bool sliding = false;

    private InputManager.InputState lastInputState;

    public IEnumerator ShowCharacterSelect(csState state)
    {
        loading = true;
        lastInputState = InputManager.inputState;

        affectAllGUIwithAlpha = true;

        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();
        km = FindObjectOfType<KartMaker>();

        if (transform.GetComponent<MainMenu>() != null)
            mm = transform.GetComponent<MainMenu>();

        cursorPosition = new Vector2[4];
        nameList = Resources.Load<Texture2D>("UI/Lobby/NamesList");
        rotateKey = Resources.Load<Texture2D>("UI/New Character Select/Rotate_Key");
        rotateXbox = Resources.Load<Texture2D>("UI/New Character Select/Rotate_Xbox");

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
        affectAllGUIwithAlpha = true;

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

        rotateRects = new Rect[4];

        for (int i = 0; i < rotateRects.Length; i++)
            rotateRects[i] = new Rect(1440, 500, 0, 0);

        for (int i = 0; i < 4; i++)
        {
            loadedChoice[i] = new CharacterSelectLoadOut(-1, -1, -1, -1);
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

            bool canInput = ((mm == null || !mm.sliding) && menuAlpha == 1f && Cursor.visible);              

            switch (state)
            {
                case csState.Character:
                    InputManager.SetInputState(lastInputState);
                    InputManager.SetToggleState(InputManager.ToggleState.Any);

                    GUI.Label(new Rect(10, 10, 1920, 95), "Select A Character");
                    GUI.DrawTexture(iconArea, nameList);

                    GUIHelper.BeginGroup(iconArea);

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
                                int id = 0;
                                foreach (InputDevice controller in InputManager.controllers)
                                {
                                    if (controller.inputType == InputType.Keyboard)
                                    {
                                        if (canInput && !showLayout[id] && choice != null && choice.Length > 0 && new Rect(iconArea.x + iconRect.x, iconArea.y + iconRect.y, iconRect.width, iconRect.height).Contains(mousePos))
                                        {
                                            choice[id].character = characterInt;
                                        }

                                        if (GUI.Button(iconRect, ""))
                                        {
                                            SelectCharacter(id);
                                        }
                                    }
                                    id++;

                                    break;
                                }
                            }
                        }
                    }

                    GUIHelper.EndGroup();

                    break;
                case csState.Hat:
                    InputManager.SetInputState(InputManager.InputState.LockedShowing);
                    InputManager.SetToggleState(InputManager.ToggleState.Any);

                    GUI.Label(new Rect(10, 10, 1920, 95), "Select A Hat");

                    GUI.DrawTexture(iconArea, nameList);

                    GUIHelper.BeginGroup(iconArea);

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
                                int id = 0;
                                foreach (InputDevice controller in InputManager.controllers)
                                {
                                    if (controller.inputType == InputType.Keyboard)
                                    {
                                        if (canInput && !showLayout[id] && choice != null && choice.Length > 0 && new Rect(iconArea.x + iconRect.x, iconArea.y + iconRect.y, iconRect.width, iconRect.height).Contains(mousePos))
                                        {
                                            choice[id].hat = hatInt;
                                        }

                                        if (GUI.Button(iconRect, ""))
                                        {
                                            ready[id] = true;
                                        }

                                        break;
                                    }
                                    id++;
                                }
                            }
                        }
                    }

                    GUIHelper.EndGroup();

                    break;
                case (csState.Kart):
                    GUI.Label(new Rect(10, 10, 1920, 95), "Select A Kart");
                    break;
            }

            for (int s = 0; s < InputManager.controllers.Count; s++)
            {
                if (state != csState.Off && state != csState.Kart && !ready[s])
                {
                    int selectedIcon = 0;

                    if (state == csState.Character)
                        selectedIcon = choice[s].character;

                    if (state == csState.Hat)
                        selectedIcon = choice[s].hat;

                    GUIHelper.BeginGroup(iconArea);

                    Vector2 iconSelection = new Vector2(selectedIcon % 5, selectedIcon / 5);
                    cursorPosition[s] = Vector2.Lerp(cursorPosition[s], iconSelection, Time.deltaTime * cursorSpeed);

                    Rect CursorRect = new Rect(10 + cursorPosition[s].x * chunkSize, cursorPosition[s].y * chunkSize, chunkSize, chunkSize);
                    Texture2D CursorTexture = Resources.Load<Texture2D>("UI/Cursors/Cursor_" + s);
                    GUI.DrawTexture(CursorRect, CursorTexture);

                    GUIHelper.EndGroup();
                }

                if (!affectAllGUIwithAlpha)
                    GUIHelper.ResetColor();

                //Show Rotate Icons
                Rect current = rotateRects[s];

                float newWidth = current.width / 4f;
                float ratio = newWidth / rotateKey.width;
                float newHeight = rotateKey.height * ratio;

                GUI.DrawTexture(new Rect(current.x, current.y + current.height - newHeight, current.width, newHeight),
                    (InputManager.controllers[s].inputType == InputType.Keyboard ? rotateKey : rotateXbox), ScaleMode.ScaleToFit);

                if (!affectAllGUIwithAlpha)
                    GUIHelper.SetGUIAlpha(menuAlpha);

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

                    GUIHelper.BeginGroup(areaRect);

                    Rect selectionRect = new Rect(10, heightChunk, (areaRect.width / 2f) - 10, heightChunk * 4f);
                    GUI.DrawTexture(selectionRect, nameList);

                    GUIHelper.BeginGroup(selectionRect);

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
                        if (InputManager.controllers[s].inputType == InputType.Keyboard && !showLayout[s])
                        {
                            //Only scale if icon is onscreen and is yours
                            if (kartI != -2 && kartI != 2)
                                loadedChoice[s].kartScales[kartI + 2] = GUIHelper.SizeHover(kartRect, loadedChoice[s].kartScales[kartI + 2], 1f, 1.25f, 2f);

                            //Kart Clicks
                            if (canInput && kartI == -1 && GUI.Button(kartRect, ""))
                            {
                                if (!kartSelected[s])
                                    StartCoroutine(ScrollKart(loadedChoice[s], kartHeight, choice[s]));
                                else
                                    kartSelected[s] = false;
                            }

                            if (canInput && kartI == 1 && GUI.Button(kartRect, ""))
                            {
                                if (!kartSelected[s])
                                    StartCoroutine(ScrollKart(loadedChoice[s], -kartHeight, choice[s]));
                                else
                                    kartSelected[s] = false;
                            }

                            if (canInput && kartI == 0 && GUI.Button(kartRect, ""))
                            {
                                if (!kartSelected[s])
                                    kartSelected[s] = true;
                                else
                                    kartSelected[s] = false;
                            }
                        }
                        else
                        {
                            loadedChoice[s].kartScales[kartI + 2] = Mathf.Clamp(loadedChoice[s].kartScales[kartI + 2] - Time.deltaTime * 2f, 1f, 2f);
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
                        if (InputManager.controllers[s].inputType == InputType.Keyboard && !showLayout[s])
                        {
                            //Only scale if icon is onscreen and is yours
                            if (kartI != -2 && kartI != 2)
                                loadedChoice[s].wheelScales[kartI + 2] = GUIHelper.SizeHover(wheelRect, loadedChoice[s].wheelScales[kartI + 2], 1f, 1.25f, 2f);

                            //Wheel Clicks
                            if (canInput && kartI == -1 && GUI.Button(wheelRect, ""))
                            {
                                if (kartSelected[s])
                                    StartCoroutine(ScrollWheel(loadedChoice[s], kartHeight, choice[s]));
                                else
                                    kartSelected[s] = true;
                            }


                            if (canInput && kartI == 1 && GUI.Button(wheelRect, ""))
                            {
                                if (kartSelected[s])
                                    StartCoroutine(ScrollWheel(loadedChoice[s], -kartHeight, choice[s]));
                                else
                                    kartSelected[s] = true;
                            }

                            if (canInput && kartI == 0 && GUI.Button(wheelRect, ""))
                            {
                                if (kartSelected[s])
                                    ready[s] = true;
                                else
                                    kartSelected[s] = true;
                            }
                        }
                        else if (s == 0)
                        {
                            loadedChoice[s].wheelScales[kartI + 2] = Mathf.Clamp(loadedChoice[s].wheelScales[kartI + 2] - Time.deltaTime * 2f, 1f, 2f);
                        }
                    }

                    GUIHelper.EndGroup();

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
                            if (canInput && GUI.Button(upArrowRect, ""))
                            {
                                StartCoroutine(ScrollKart(loadedChoice[s], -kartHeight, choice[s]));
                            }
                            if (canInput && GUI.Button(downArrowRect, ""))
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
                            if (canInput && GUI.Button(upArrowRect, ""))
                            {
                                StartCoroutine(ScrollWheel(loadedChoice[s], -kartHeight, choice[s]));
                            }
                            if (canInput && GUI.Button(downArrowRect, ""))
                            {
                                StartCoroutine(ScrollWheel(loadedChoice[s], kartHeight, choice[s]));
                            }
                        }
                    }

                    GUI.DrawTexture(upArrowRect, arrowIcon, ScaleMode.ScaleToFit);
                    GUI.DrawTexture(downArrowRect, downArrowIcon, ScaleMode.ScaleToFit);

                    GUIHelper.EndGroup();
                }
            }

            GUI.skin = Resources.Load<GUISkin>("GUISkins/Options");
            if (!affectAllGUIwithAlpha)
                GUIHelper.ResetColor();
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
                            loadedModels[s] = Instantiate(gd.characters[choice[s].character].CharacterModel_Standing, platforms[s].Find("Spawn").position, oldRot);
                            loadedModels[s].GetComponent<Rigidbody>().isKinematic = true;
                            loadedModels[s].GetComponentInChildren<FaceToCamera>().forceCamera = platforms[s].Find("Camera");
                        }

                        loadedChoice[s].character = choice[s].character;
                        loadedChoice[s].hat = -1;
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
                            Transform HatObject = Instantiate(gd.hats[choice[s].hat].model, loadedModels[s].GetComponent<StandingCharacter>().hatHolder.position, loadedModels[s].GetComponent<StandingCharacter>().hatHolder.rotation);
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

                        loadedModels[s] = km.SpawnKart(KartType.Display, platforms[s].Find("Spawn").position + Vector3.up / 2f, oldRot, choice[s].character, choice[s].hat, choice[s].kart, choice[s].wheel);

                        loadedChoice[s].kart = choice[s].kart;
                        loadedChoice[s].wheel = choice[s].wheel;
                    }
                }

                Camera cam;

                //Default off screen
                if (InputManager.controllers.Count == 0 || !isShowing)
                {
                    cam = platforms[0].Find("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                    LeaveCamOn(cam);
                }

                if (InputManager.controllers.Count <= 1 || !isShowing)
                {
                    cam = platforms[1].Find("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                    LeaveCamOn(cam);
                }

                if (InputManager.controllers.Count <= 2 || !isShowing)
                {
                    cam = platforms[2].Find("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                    LeaveCamOn(cam);
                }

                if (InputManager.controllers.Count <= 3 || !isShowing)
                {
                    cam = platforms[3].Find("Camera").GetComponent<Camera>();
                    cam.rect = GUIHelper.Lerp(cam.rect, new Rect(1f, cam.rect.y, cam.rect.width, cam.rect.height), Time.deltaTime * 5f);
                    LeaveCamOn(cam);
                }

                if (isShowing)
                {

                    float areaX = GUIHelper.guiEdges.x / Screen.width;
                    float areaY = GUIHelper.guiEdges.y / Screen.height;
                    float areaHeight = 0.3f / ((0.5f / Screen.width) * Screen.height);

                    //FIGURE OUT OK AREA
                    Rect okayArea;

                    if (GUIHelper.widthSmaller)
                        okayArea = new Rect(0.5f, 0.5f - (areaHeight / 2f), 0.5f, areaHeight);
                    else
                        okayArea = new Rect(0.5f, areaY, 0.5f - areaX, 1f - areaY);

                    okayArea.y += 0.12f;
                    okayArea.height -= 0.2f;

                    //Figure out ok area for GUI
                    Rect okayGUIArea = new Rect(960, 50, 960, 900);
                    Rect newOkayGUIArea;

                    if (InputManager.controllers.Count == 1)
                    {
                        cam = platforms[0].Find("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, okayArea, Time.deltaTime * 5f);
                        LeaveCamOn(cam);

                        rotateRects[0] = GUIHelper.Lerp(rotateRects[0], okayGUIArea, Time.deltaTime * 5f);
                    }

                    if (InputManager.controllers.Count == 2)
                    {
                        cam = platforms[0].Find("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y + (okayArea.height / 2f), okayArea.width, okayArea.height / 2f), Time.deltaTime * 5f);
                        LeaveCamOn(cam);

                        newOkayGUIArea = new Rect(okayGUIArea.x, okayGUIArea.y, okayGUIArea.width, okayGUIArea.height / 2f);
                        rotateRects[0] = GUIHelper.Lerp(rotateRects[0], newOkayGUIArea, Time.deltaTime * 5f);

                        cam = platforms[1].Find("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y, okayArea.width, okayArea.height / 2f), Time.deltaTime * 5f);
                        LeaveCamOn(cam);

                        newOkayGUIArea = new Rect(okayGUIArea.x, okayGUIArea.y + okayGUIArea.height / 2f, okayGUIArea.width, okayGUIArea.height / 2f);
                        rotateRects[1] = GUIHelper.Lerp(rotateRects[1], newOkayGUIArea, Time.deltaTime * 5f);
                    }

                    if (InputManager.controllers.Count >= 3)
                    {
                        if (state != csState.Kart)
                        {
                            cam = platforms[0].Find("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y + (okayArea.height / 2f), okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                            LeaveCamOn(cam);

                            newOkayGUIArea = new Rect(okayGUIArea.x, okayGUIArea.y, okayGUIArea.width / 2f, okayGUIArea.height / 2f);
                            rotateRects[0] = GUIHelper.Lerp(rotateRects[0], newOkayGUIArea, Time.deltaTime * 5f);

                            cam = platforms[2].Find("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x, okayArea.y, okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                            LeaveCamOn(cam);

                            newOkayGUIArea = new Rect(okayGUIArea.x, okayGUIArea.y + (okayGUIArea.height / 2f), okayGUIArea.width / 2f, okayGUIArea.height / 2f);
                            rotateRects[2] = GUIHelper.Lerp(rotateRects[2], newOkayGUIArea, Time.deltaTime * 5f);

                        }
                        else
                        {
                            cam = platforms[0].Find("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x - okayArea.width / 2f, okayArea.y + (okayArea.height / 2f), okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                            LeaveCamOn(cam);

                            newOkayGUIArea = new Rect(okayGUIArea.x - (okayGUIArea.width / 2f), okayGUIArea.y, okayGUIArea.width / 2f, okayGUIArea.height / 2f);
                            rotateRects[0] = GUIHelper.Lerp(rotateRects[0], newOkayGUIArea, Time.deltaTime * 5f);

                            cam = platforms[2].Find("Camera").GetComponent<Camera>();
                            cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x - okayArea.width / 2f, okayArea.y, okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                            LeaveCamOn(cam);

                            newOkayGUIArea = new Rect(okayGUIArea.x - (okayGUIArea.width / 2f), okayGUIArea.y + (okayGUIArea.height / 2f), okayGUIArea.width / 2f, okayGUIArea.height / 2f);
                            rotateRects[2] = GUIHelper.Lerp(rotateRects[2], newOkayGUIArea, Time.deltaTime * 5f);
                        }

                        cam = platforms[1].Find("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x + okayArea.width / 2f, okayArea.y + (okayArea.height / 2f), okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                        LeaveCamOn(cam);

                        newOkayGUIArea = new Rect(okayGUIArea.x + (okayGUIArea.width / 2f), okayGUIArea.y, okayGUIArea.width / 2f, okayGUIArea.height / 2f);
                        rotateRects[1] = GUIHelper.Lerp(rotateRects[1], newOkayGUIArea, Time.deltaTime * 5f);

                    }

                    if (InputManager.controllers.Count == 4)
                    {
                        cam = platforms[3].Find("Camera").GetComponent<Camera>();
                        cam.rect = GUIHelper.Lerp(cam.rect, new Rect(okayArea.x + okayArea.width / 2f, okayArea.y, okayArea.width / 2f, okayArea.height / 2f), Time.deltaTime * 5f);
                        LeaveCamOn(cam);

                        newOkayGUIArea = new Rect(okayGUIArea.x + (okayGUIArea.width / 2f), okayGUIArea.y + (okayGUIArea.height / 2f), okayGUIArea.width / 2f, okayGUIArea.height / 2f);
                        rotateRects[3] = GUIHelper.Lerp(rotateRects[3], newOkayGUIArea, Time.deltaTime * 5f);
                    }
                }

                if (!InputManager.controllers[s].toggle && loadedModels[s] != null)
                    loadedModels[s].Rotate(Vector3.up, -InputManager.controllers[s].GetInput("Rotate") * Time.deltaTime * rotateSpeed);

                int hori = 0, vert = 0;
                bool submit = false, cancel = false;

                if (canInput && !sliding)
                {
                    if (!ready[s] || showLayout[s])
                    {
                        hori = InputManager.controllers[s].GetIntInputWithLock("MenuHorizontal");
                        vert = InputManager.controllers[s].GetIntInputWithLock("MenuVertical");
                    }

                    submit = (InputManager.controllers[s].GetButtonWithLock("Submit"));
                    cancel = (InputManager.controllers[s].GetButtonWithLock("Cancel"));
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
                    if (state == csState.Character && gd.characters[choice[s].character].unlocked != UnlockedState.Locked && !ready[s])
                    {
                        SelectCharacter(s);
                    }

                    if (state == csState.Hat && gd.hats[choice[s].hat].unlocked != UnlockedState.Locked && !ready[s])
                    {
                        ready[s] = true;
                    }

                    if (state == csState.Kart && !ready[s])
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

    public void SelectCharacter(int s)
    {
        AudioClip playClip = gd.GetCustomSoundPack(CurrentGameData.currentChoices[s].character, CurrentGameData.currentChoices[s].hat).selectedSound;

        if (playClip != null)
            sm.PlaySFX(playClip);

        loadedModels[s].GetComponent<Animator>().CrossFade("Selected", 0.01f);

        ready[s] = true;
    }

    public void LeaveCamOn(Camera cam)
    {
        if (cam.rect.x > 0.95f)
            cam.enabled = false;
        else
            cam.enabled = true;
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
            affectAllGUIwithAlpha = false;

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