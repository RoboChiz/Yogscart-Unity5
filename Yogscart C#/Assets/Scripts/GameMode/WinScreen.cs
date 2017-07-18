using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    private List<Racer> audiemce;
    private List<Racer> winners;

    public List<Transform> kartSpawns;
    public List<Transform> audienceSpawns;

    private CurrentGameData gd;
    private KartMaker km;
    private SoundManager sm;

    public AudioClip music;

    const string playerOneReplay = "",
        playerTwoReplay = "",
        playerThreeReplay = "";

    public void Start()
    {
        gd = FindObjectOfType<CurrentGameData>();
        km = FindObjectOfType<KartMaker>();
        sm = FindObjectOfType<SoundManager>();

        sm.PlayMusic(music);
        //Debug
        /*List<Racer> test = new List<Racer>();

        for(int i = 0; i < 12; i++)
        {
            test.Add(new Racer(-1, 0, Random.Range(0, gd.characters.Length), 0, Random.Range(0, gd.karts.Length), Random.Range(0, gd.wheels.Length), i));
        }

        DoWinScreen(test);*/
    }


    public void DoWinScreen(List<Racer> _racers)
    {
        List<Racer> racers = new List<Racer>(_racers);
        winners = new List<Racer>();

        //Find Winners
        if (racers.Count > 3)
        {
            while (winners.Count < 3)
            {
                Racer bestRacer = racers[0];

                for (int i = 1; i < racers.Count; i++)
                {
                    if (racers[i].overallPosition < bestRacer.overallPosition)
                    {
                        bestRacer = racers[i];
                    }
                }

                winners.Add(bestRacer);
                racers.Remove(bestRacer);
            }
        }

        //Find Audience
        audiemce = racers;

        //Spawn Three Winners as Kart
        for(int i = 0; i < winners.Count; i++)
        {
            Racer racer = winners[i];
            racer.ingameObj = km.SpawnKart(KartType.Replay, kartSpawns[i].position, kartSpawns[i].rotation,
                racer.Character, racer.Hat, racer.Kart, racer.Wheel);

            //Find Replay 
            string data = playerOneReplay;

            if (i == 1)
                data = playerTwoReplay;
            if (i == 2)
                data = playerThreeReplay;

            KartReplayer kr = racer.ingameObj.GetComponent<KartReplayer>();
            kr.LoadReplay(data);
        }

        //Spawn Audience
        for (int i = 0; i < audiemce.Count; i++)
        {
            Transform character = Instantiate(gd.characters[audiemce[i].Character].CharacterModel_Standing, audienceSpawns[i].position, audienceSpawns[i].rotation);
            character.GetComponent<Animator>().SetBool("Clap", true);
        }

        //Fade Out
        CurrentGameData.blackOut = false;

        //Play Replay
        foreach (Racer racer in winners)
            racer.ingameObj.GetComponent<KartReplayer>().Play();

        StartCoroutine(DoCutscene());
    }

    private IEnumerator DoCutscene()
    {
        yield return null;
    }

    public void Update()
    {

    }
	
}
