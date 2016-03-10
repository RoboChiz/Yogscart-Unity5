using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

/*
Offline Races Class v1.0
Created by Robert (Robo_Chiz)
FOR THE LOVE OF ALL THAT IS HOLY, DO NOT EDIT ANYTHING IN THIS SCRIPT!
Thanks
*/

public enum RaceType { GrandPrix, VSRace, TimeTrial};

//Default Class for all races
class Race : GameMode
{
    public RaceType raceType;
    public int currentCup = -1;
    public int currentTrack = -1;

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
        CurrentGameData.blackOut = false;
        yield return new WaitForSeconds(0.5f);
        Debug.Log("LET'S DO THIS INTRO");
    }

    public override void HostUpdate()
    {
        
    }

    public override void ClientUpdate()
    {
        
    }

}