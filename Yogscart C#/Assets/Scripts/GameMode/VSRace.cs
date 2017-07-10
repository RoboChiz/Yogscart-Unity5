using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSRace : TournamentRace
{
    public override void NextRace()
    {
        StartCoroutine(ChangeState(RaceState.LevelSelect));

        LevelSelect ls = gameObject.AddComponent<LevelSelect>();
        ls.ShowLevelSelect();
    }

    public void CancelLevelSelect()
    {
        if (FindObjectOfType<MainMenu>() == null)
        {
            StartCoroutine(ActualCancelLS());
        }
    }

    IEnumerator ActualCancelLS()
    {
        while (changingState)
            yield return null;

        StartCoroutine(OnEndLeaderBoard());
        StartCoroutine(KillLevelSelect());
    }

    public override void FinishLevelSelect(int _currentCup, int _currentTrack)
    {
        currentTrack = _currentTrack;
        currentCup = _currentCup;

        if (FindObjectOfType<MainMenu>() == null)
        {
            StartCoroutine(KillLevelSelect());
            StartRace();
        }
    }

    public IEnumerator KillLevelSelect()
    {
            while (GetComponent<LevelSelect>().enabled)
                yield return null;

            Destroy(GetComponent<LevelSelect>());
    }

    protected override void OnSpawnKart()
    {
        SpawnAllKarts(td.spawnPoint.position, td.spawnPoint.rotation);
    }

    protected override void SetRank(Rank rank)
    {
        ranking = rank;      
    }

}
