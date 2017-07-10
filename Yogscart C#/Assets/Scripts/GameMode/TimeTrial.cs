using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TimeTrial : Race
{
    protected override bool enableAI { get { return false; } }

    private bool skipCS = false, ghostSaved = false;

    //Save Ghost Data

    protected override IEnumerator ActualStartGameMode()
    {
        //Do Character Select
        yield return DoCharacterAndTrackSelect(skipCS);

        //Do Time Trial Menu
        TimeTrialMenu ttm = gameObject.AddComponent<TimeTrialMenu>();
        ttm.Show();
    }

    public void FinishTimeTrialMenu()
    {
        StartCoroutine(KillTTM());
        StartCoroutine(ActualFinishTimeTrialMenu());
    }

    public void CancelTimeTrialMenu()
    {
        currentCup = -1;
        currentTrack = -1;
        skipCS = true;

        StartCoroutine(KillTTM());
        StartCoroutine(ActualStartGameMode());
    }

    public IEnumerator KillTTM()
    {
        TimeTrialMenu ttm = GetComponent<TimeTrialMenu>();
        while (ttm.showing)
            yield return null;

        Destroy(ttm);
    }

    public IEnumerator ActualFinishTimeTrialMenu()
    {
        //Setup the Racers for the Gamemode
        SetupRacers();

        yield return null;
        yield return null;
        yield return null;

        StartRace();
    }

    //Reload the same race
    public override void NextRace()
    {
        StartCoroutine(ChangeState(RaceState.Blank));
        ghostSaved = false;
        StartRace();
    }

    protected override void OnLevelLoad()
    {
        //Clear all itemboxes 
        ItemBox[] itemBoxes = FindObjectsOfType<ItemBox>();
        foreach (ItemBox ib in itemBoxes)
            Destroy(ib.gameObject);
    }

    protected override void OnSpawnKart()
    {
        SpawnLoneKart(td.spawnPoint.position, td.spawnPoint.rotation, 0);
    }

    protected override void OnKartStarting()
    {
        foreach (KartItem ki in FindObjectsOfType<KartItem>())
            ki.RecieveItem(2);
    }

    protected override void OnRaceFinished() { DetermineWinner(); }

    protected override void OnStartLeaderBoard(Leaderboard lb)
    {
        lb.StartTimeTrial(this);
    }

    protected override string[] GetNextMenuOptions()
    {
        return new string[] { "Restart", "Replay", "Save Ghost", "Quit" };
    }

    protected override string GetRaceName()
    {
        return "Time Trial";
    }

    protected override void OnLeaderboardUpdate(Leaderboard lb)
    {
        if (InputManager.controllers[0].GetMenuInput("Submit") != 0 || InputManager.GetClick())
        {
                lb.hidden = true;
        }
    }

    protected void DetermineWinner()
    {
        float bestTime = gd.tournaments[currentCup].tracks[currentTrack].bestTime;

        if (racers[0].timer <= bestTime || bestTime == 0)
        {
            gd.tournaments[currentCup].tracks[currentTrack].bestTime = racers[0].timer;
            gd.SaveGame();
        }
    }

    public override void Restart()
    {
        StartCoroutine(ActualRestart());      
    }

    protected override void PlayerFinished(Racer racer)
    {
        //Stop Time on Finish
        StopTimer();
    }

    protected override void NextMenuSelection()
    {
        base.NextMenuSelection();

        switch (nextMenuOptions[nextMenuSelected])
        {
            case "Save Ghost":
                if(!ghostSaved)
                    StartCoroutine(SaveGhost());
                break;
        }
    }

    private IEnumerator SaveGhost()
    {
        InfoPopUp popUp = null;
        lockInputs = true;

        FileStream sw = null;
        try
        {
            DateTime now = System.DateTime.Now;
            string saveLocation = Application.persistentDataPath + "/Ghost " + now.Day + "_" + now.Month + "_" + now.Year + "_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00") + ".GhostData";

            BinaryFormatter bf = new BinaryFormatter();
            sw = File.Create(saveLocation);
            bf.Serialize(sw, new GhostData(racers[0], preRaceState[0].DataToString()));
            sw.Flush();

            popUp = gameObject.AddComponent<InfoPopUp>();
            popUp.Setup("Ghost saved to " + saveLocation);
            ghostSaved = true;
        }
        catch(Exception error)
        {
            popUp = gameObject.AddComponent<InfoPopUp>();
            popUp.Setup(error.Message);
        }
        finally
        {
            if(sw != null)
                sw.Close();
        }

        yield return null;

        while (popUp.guiAlpha != 0f)
            yield return null;

        Destroy(popUp);

        lockInputs = false;
    }

    private IEnumerator ActualRestart()
    {
        StopCoroutine(currentGame);

        //Finish Stuff
        mapViewer.HideMapViewer();

        //Stop the Timer
        StopTimer();

        //Lock the Pause Menu
        PauseMenu.canPause = false;
        timer = 0;
        finished = false;
        raceFinished = false;

        yield return null;
        yield return null;

        StartRace();
    }

    [System.Serializable]
    public class GhostData
    {
        public int character, hat, kart, wheel, level, cup;
        public float time;
        public string data;

        public GhostData(Racer racer, string _data)
        {
            character = racer.Character;
            hat = racer.Hat;
            kart = racer.Kart;
            wheel = racer.Wheel;

            time = racer.timer;
            data = _data;
        }
    }


}
