using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;


/*
//Default Class for all races
class Race : GameMode
{
    public RaceType raceType;
    public int currentCup = -1;
    public int currentTrack = -1;
    public int currentRace = 1;

    private TrackData td;
 
    public override IEnumerator MyStart()
    {
        CharacterSelect cs = GameObject.FindObjectOfType<CharacterSelect>();
        bool firstTime = true;

        if(raceType == RaceType.TimeTrial)
        {
            aiEnabled = false;
        }
        else
        {
            aiEnabled = true;
        }

        while (currentTrack == -1 || currentCup == -1)
        {
            cs.enabled = true;

            if(firstTime)
            {
                firstTime = false;
                yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);
            }
            else
            {
                yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Kart);
            }

            yield return new WaitForSeconds(0.5f);

            //Wait until all characters have been selected
            while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
            {
                yield return null;
            }

            if (cs.State == CharacterSelect.csState.Off)
            {
                //Cancel everything!
                MainMenu mm = GameObject.FindObjectOfType<MainMenu>();
                if (mm != null && mm.enabled)
                    mm.BackMenu();

                //Stop all Gamemode Coroutines
                ForceStop();
                //Wait a Frame for Coroutines to stop
                yield return null;

                Debug.Log("It didn't worked");
            }

            //Everything worked out perfect!
            Debug.Log("It worked");
            LevelSelect ls = GameObject.FindObjectOfType<LevelSelect>();

            if (ls != null)
            {
                ls.enabled = true;
                ls.ShowLevelSelect();
            }

            while (ls.enabled)
            {
                yield return null;
            }

            if (currentCup == -1 || currentTrack == -1)
            {
                Debug.Log("Back out of Level Select");
            }
            else
            {
                Debug.Log("Beaten the Level Select!");
                Debug.Log("Cup:" + currentCup + " Track:" + currentTrack);

                CurrentGameData.blackOut = true;
 
                yield return new WaitForSeconds(0.5f);

                SceneManager.LoadScene(gd.tournaments[currentCup].tracks[currentTrack].sceneID);

                yield return null;

                td = GameObject.FindObjectOfType<TrackData>();

                spawnPosition = td.spawnPoint.position;
                spawnRotation = td.spawnPoint.rotation;
            }
        }
    }

    public override IEnumerator DoIntro()
    {
        yield return new WaitForSeconds(1f);

        ChangeState(GUIState.CutScene);

        GameObject cutsceneCam = new GameObject();
        cutsceneCam.AddComponent<Camera>();
        cutsceneCam.tag = "MainCamera";

        sm.PlayMusic(Resources.Load<AudioClip>("Music & Sounds/RaceStart"));

        cutsceneCam.transform.position = td.introPans[0].startPoint;
        cutsceneCam.transform.rotation = Quaternion.Euler(td.introPans[0].startRotation);

        CurrentGameData.blackOut = false;
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < td.introPans.Count; i++)
        {
            yield return StartCoroutine(Play(cutsceneCam.transform, td.introPans[i]));
        }

        ChangeState(GUIState.RaceInfo);

        CurrentGameData.blackOut = true;
        yield return new WaitForSeconds(0.5f);

        cutsceneCam.GetComponent<Camera>().depth = -5;

        yield return new WaitForSeconds(0.5f);
        CurrentGameData.blackOut = false;

        Destroy(cutsceneCam);
        sm.PlayMusic(td.backgroundMusic);

    }

    private IEnumerator Play(Transform cam, CameraPoint clip)
    {
        float startTime = Time.time;

        while ((Time.time - startTime) < clip.travelTime)
        {
            cam.position = Vector3.Lerp(clip.startPoint, clip.endPoint, (Time.time - startTime) / clip.travelTime);
            cam.rotation = Quaternion.Slerp(Quaternion.Euler(clip.startRotation), Quaternion.Euler(clip.endRotation), (Time.realtimeSinceStartup - startTime) / clip.travelTime);
            yield return null;
        }
    }

    void OnGUI()
    {

        ParentGUI();

        Color nWhite = Color.white;
        nWhite.a = guiAlpha;
        GUI.color = nWhite;

        switch (currentGUI)
        {

            case GUIState.CutScene:

                float idealWidth = Screen.width / 3f;
                Texture2D previewTexture = gd.tournaments[currentCup].tracks[currentTrack].preview;
                float previewRatio = idealWidth / previewTexture.width;
                Rect previewRect = new Rect(Screen.width - idealWidth - 20, Screen.height - (previewTexture.height * previewRatio * 2f), idealWidth, previewTexture.height * previewRatio);

                GUI.DrawTexture(previewRect, previewTexture);

                break;
            case GUIState.RaceInfo:

                Texture2D raceTexture;

                if (Network.isClient || Network.isServer)
                {
                    raceTexture = Resources.Load<Texture2D>("UI/Level Selection/Online");
                }
                else
                {
                    if (raceType == RaceType.TimeTrial)
                        raceTexture = Resources.Load<Texture2D>("UI/Level Selection/TimeTrial");
                    else
                        raceTexture = Resources.Load<Texture2D>("UI/Level Selection/" + currentRace);
                }

                GUI.DrawTexture(new Rect(10, 10, Screen.width - 20, Screen.height), raceTexture, ScaleMode.ScaleToFit);


                break;
        }
    }

    public override void HostUpdate()
    {
        
    }

    public override void ClientUpdate()
    {
        
    }
    
}*/