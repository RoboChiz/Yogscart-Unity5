using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentRace : Race
{
    //How many races will there be total
    public int raceCount { get; private set; }

    public Rank ranking;

    protected override bool enableAI { get { return true; } }
    private Racer bestHuman;

    public override void StartGameMode()
    {
        base.StartGameMode();

        //Set Race Count
        raceCount = 4;
    }

    public override void NextRace()
    {
        StartCoroutine(ChangeState(RaceState.Blank));
        StartRace();
        currentTrack++;
    }

    protected override void OnSpawnKart()
    {
        SpawnAllKarts(td.spawnPoint.position, td.spawnPoint.rotation);
    }

    protected override void OnRaceFinished()
    {
        if(currentRace >= raceCount)
        {
            DetermineWinner();
        }
    }

    protected void DetermineWinner()
    {
        List<Racer> pointsSorted = SortingScript.CalculatePoints(racers);
        bestHuman = racers[racers.Count - 1];

        for (int i = 0; i < pointsSorted.Count; i++)
        {
            if (pointsSorted[i].Human != -1)
            {
                bestHuman = pointsSorted[i];
                break;
            }
        }

        int points = bestHuman.points;

        if (points >= raceCount * 15)
            SetRank(Rank.Perfect);
        else if (bestHuman.overallPosition == 0)
            SetRank(Rank.Gold);
        else if (bestHuman.overallPosition == 1)
            SetRank(Rank.Silver);
        else if (bestHuman.overallPosition == 2)
            SetRank(Rank.Bronze);
        else
            SetRank(Rank.NoRank);
    }

    protected virtual void SetRank(Rank rank)
    {
        ranking = rank;

        //Update Cup Ranking
        if (rank > gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty])
        {
            gd.tournaments[currentCup].lastRank[CurrentGameData.difficulty] = rank;
            gd.SaveGame();
        }
    }

    protected override void OnStartLeaderBoard(Leaderboard lb)
    {
        lb.StartLeaderBoard(this);
    }

    protected override string[] GetNextMenuOptions()
    {
        if (currentRace < raceCount)
            return new string[] { "Next Race", "Replay", "Quit" };
        else
            return new string[] { "Finish", "Replay" };
    }

    protected override string GetRaceName()
    {
        string returnVal = (currentRace + 1).ToString();

        int lastVal = (currentRace + 1) % 10;

        if (lastVal == 1 && lastVal != 11)
            returnVal += "st ";
        else if (lastVal == 2 && lastVal != 12)
            returnVal += "nd ";
        else if (lastVal == 3 && lastVal != 13)
            returnVal += "rd ";
        else
            returnVal += "th ";

        returnVal += "Race";

        return returnVal;
    }

    protected override void OnLeaderboardUpdate(Leaderboard lb)
    {
        if(InputManager.controllers[0].GetMenuInput("Submit") != 0 || InputManager.GetClick())
        {
            if (lb.state != LBType.AddedPoints && lb.state != LBType.AddingPoints)
                lb.hidden = true;
            else
                lb.DoInput();
        }
    }

    protected override void NextMenuSelection()
    {
        base.NextMenuSelection();

        switch (nextMenuOptions[nextMenuSelected])
        {
            case "Finish":
                StartCoroutine(ChangeState(RaceState.Blank));
                //Do Final Cutscene Stuff
                StartCoroutine(DoEnd());
                break;
        }
    }

    public IEnumerator DoEnd()
    {
        //Force Screen to black out
        while (!gd.isBlackedOut)
        {
            CurrentGameData.blackOut = true;
            yield return null;
        }

        //Load the Level
        AsyncOperation sync = SceneManager.LoadSceneAsync("WinScreen");

        while (!sync.isDone)
            yield return null;

        FindObjectOfType<WinScreen>().DoWinScreen(racers);

        yield return new WaitForSeconds(0.5f);

        float timer = 0f;

        while (InputManager.controllers[0].GetMenuInput("Submit") == 0 && !InputManager.GetClick() && timer < 10f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return ChangeState(RaceState.Win);

        //Wait for Input
        while (InputManager.controllers[0].GetMenuInput("Submit") == 0 && !InputManager.GetClick())
        {
            yield return null;
        }

        //Quit Gamemode
        StartCoroutine(QuitGame());
    }

    public override void OnGUI()
    {
        base.OnGUI();

        switch (currentState)
        {          
            case RaceState.Win:

                GUIStyle newLabel = new GUIStyle(GUI.skin.label);
                newLabel.fontSize = 40;

                //Background
                GUI.DrawTexture(new Rect(990, 100, 900, 880), boardTexture, ScaleMode.ScaleToFit);

                //Win Text
                if(ranking > Rank.NoRank)
                    GUI.Label(new Rect(1000, 150, 880, 100), "Congratulations!", newLabel);
                else
                    GUI.Label(new Rect(1000, 150, 880, 100), "Well Done!", newLabel);

                string placeString = (bestHuman.overallPosition + 1).ToString();

                int lastVal = (bestHuman.overallPosition + 1) % 10;

                if (lastVal == 1 && lastVal != 11)
                    placeString += "st ";
                else if (lastVal == 2 && lastVal != 12)
                    placeString += "nd ";
                else if (lastVal == 3 && lastVal != 13)
                    placeString += "rd ";
                else
                    placeString += "th ";

                GUI.Label(new Rect(1000, 250, 880, 100), "You came " + placeString, newLabel);

                if (ranking > Rank.NoRank)
                {
                    GUI.Label(new Rect(1000, 450, 880, 100), "Ranking:", newLabel);
                    GUI.Label(new Rect(1000, 550, 880, 100), ranking.ToString(), newLabel);
                }

                break;
        }
    }
}
