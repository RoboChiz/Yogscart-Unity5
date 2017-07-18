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
    [System.NonSerialized, HideInInspector]
    public GhostData ghost;
    private Transform ghostTransform;
    private bool finishGhost = false;

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
        finishGhost = false;
        StartRace();
    }

    public override void OnLevelLoad()
    {
        //Clear all itemboxes 
        ItemBox[] itemBoxes = FindObjectsOfType<ItemBox>();
        foreach (ItemBox ib in itemBoxes)
            Destroy(ib.gameObject);
    }

    protected override void OnSpawnKart()
    {
        Vector3 spawnPosition = td.spawnPoint.position;
        Quaternion spawnRotation = td.spawnPoint.rotation;

        SpawnLoneKart(spawnPosition, spawnRotation, 0);

        if(ghost != null)
        {
            //Get Spawn Point
            Vector3 startPos = spawnPosition + (spawnRotation * Vector3.forward * (3 * 1.5f) * -1.5f);
            Vector3 x2 = spawnRotation * (Vector3.forward * 4.5f) + (Vector3.forward * .75f * 3);
            Vector3 y2 = spawnRotation * (Vector3.right * 6);
            startPos += x2 + y2;

            ghostTransform = FindObjectOfType<KartMaker>().SpawnKart(KartType.Ghost, startPos, spawnRotation * Quaternion.Euler(0, -90, 0), ghost.character, ghost.hat, ghost.kart, ghost.wheel);
            ghostTransform.GetComponent<KartReplayer>().LoadReplay(ghost.data);
            ghostTransform.GetComponent<KartReplayer>().ignoreLocalStartBoost = true;

            foreach (MeshRenderer mr in ghostTransform.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                Material material = mr.material;

                material.SetFloat("_Mode", 2);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                material.color = new Color(1f, 1f, 1f, 0.4f);              
            }

            foreach (SkinnedMeshRenderer mr in ghostTransform.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Material material = mr.material;

                material.SetFloat("_Mode", 2);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                material.color = new Color(1f, 1f, 1f, 0.4f);
            }
        }
    }

    protected override void AddMapViewObjects()
    {
        if(ghostTransform != null)
            mapViewer.objects.Add(new MapObject(ghostTransform, gd.characters[ghost.character].icon, 0));
    }

    protected override void OnPreKartStarting()
    {
        if(ghostTransform != null)
            ghostTransform.GetComponent<KartReplayer>().Play();
    }

    protected override void OnPostKartStarting()
    {
        foreach (KartItem ki in FindObjectsOfType<KartItem>())
            ki.RecieveItem(2);
    }

    protected override void OnRaceFinished()
    {
        DetermineWinner();
        CheckGhostKill();
    }

    public override void HostUpdate()
    {
        base.HostUpdate();
        CheckGhostKill();
    }

    public void CheckGhostKill()
    {
        //Finish Ghost
        if (ghostTransform != null && ghostTransform.GetComponent<PositionFinding>().lap >= td.Laps && !finishGhost)
        {
            finishGhost = true;
            Destroy(ghostTransform.GetComponent<KartReplayer>());

            StartCoroutine(KillGhost());       
        }
    }

    private IEnumerator KillGhost()
    {
        //Fade Ghost Away
        foreach (MeshRenderer mr in ghostTransform.gameObject.GetComponentsInChildren<MeshRenderer>())
            StartCoroutine(FadeMaterial(mr.material));

        foreach (SkinnedMeshRenderer mr in ghostTransform.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            StartCoroutine(FadeMaterial(mr.material));

        //Spawn Ghost Particles
       GameObject ghostParticles = Instantiate(Resources.Load<GameObject>("Prefabs/GhostFlame"), ghostTransform.transform.position,
       Quaternion.Euler(-90, 0, 0));

        //Make Particles follow ghost
        float startTime = Time.time;
        while(Time.time - startTime < 0.25f)
        {
            ghostParticles.transform.position = ghostTransform.position;
            yield return null;
        }

        //Destroy Ghost
        Destroy(ghostTransform.gameObject);
    }

    private IEnumerator FadeMaterial(Material material)
    {
        float startTime = Time.time, travelTime = 2f;
        Color startVal = material.color;

        while(Time.time - startTime < travelTime)
        {
            material.color = Color.Lerp(startVal, Color.clear, (Time.time - startTime) / travelTime);
            yield return null;
        }

        material.color = Color.clear;
    }

    protected override void OnStartLeaderBoard(Leaderboard lb)
    {
        lb.StartTimeTrial(this);
    }

    public override string[] GetNextMenuOptions()
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
            if (!Directory.Exists(Application.persistentDataPath + "/Ghost Data/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Ghost Data/");

            DateTime now = System.DateTime.Now;
            string saveLocation = Application.persistentDataPath + "/Ghost Data/" + now.Day + "_" + now.Month + "_" + now.Year + "_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00") + ".GhostData";

            BinaryFormatter bf = new BinaryFormatter();
            sw = File.Create(saveLocation);
            bf.Serialize(sw, new GhostData(racers[0], preRaceState[0].DataToString(), currentCup, currentTrack, gd.playerName, gd.version));
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
}

[System.Serializable]
public class GhostData
{
    public int character, hat, kart, wheel, track, cup;
    public float time;
    public string data, playerName, version;

    [System.NonSerialized]
    public string fileLocation;

    public GhostData(Racer racer, string _data, int _cup, int _track, string _playerName, string _version)
    {
        character = racer.Character;
        hat = racer.Hat;
        kart = racer.Kart;
        wheel = racer.Wheel;

        time = racer.timer;
        data = _data;

        track = _track;
        cup = _cup;
        version = _version;
        playerName = _playerName;
    }

    public GhostData()
    {
        data = "";
        playerName = "";
        fileLocation = "";
        version = "";
    }
}
