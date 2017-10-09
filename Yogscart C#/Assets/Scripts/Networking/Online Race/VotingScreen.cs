using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VotingScreen : MonoBehaviour {

    public List<Vote> votes;
    private CurrentGameData gd;
    private SoundManager sm;
    public float guiAlpha = 0f;
    const float fadeTime = 0.5f;

    private int selected = -1;

    public void Awake()
    {
        gd = FindObjectOfType<CurrentGameData>();
        sm = FindObjectOfType<SoundManager>();

        votes = new List<Vote>();
    }

    public void ShowScreen()
    {      
        StartCoroutine(ChangeGUIAlpha(0, 1));
    }

    public void HideScreen()
    {
        StartCoroutine(Kill());
    }

    private IEnumerator Kill()
    {
        yield return ChangeGUIAlpha(1, 0);
        Destroy(this);
    }

    public void AddVote(int cup, int track)
    {
        Debug.Log("Added vote!");
        votes.Add(new Vote(cup, track));
    }

	void OnGUI()
    {
        GUI.color = new Color(1, 1, 1, guiAlpha);
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Online");
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.depth = -5;

        for (int i = 0; i < votes.Count; i++)
        {
            string trackName = gd.tournaments[votes[i].cup].tracks[votes[i].track].name;

            if (i == selected)
                GUI.Box(new Rect(480, 100 + (i * 70), 960, 60), "[" + trackName + "]");
            else
                GUI.Box(new Rect(480, 100 + (i * 70), 960, 60), trackName);

        }

    }

    private IEnumerator ChangeGUIAlpha(float start, float end)
    {
        float startTime = Time.time;
        while (Time.time - startTime < fadeTime)
        {
            guiAlpha = Mathf.Lerp(start, end, (Time.time - startTime) / fadeTime);
            yield return null;
        }

        guiAlpha = end;
    }

    public void StartRoll(int finalNumber)
    {
        StartCoroutine(ActualStartRoll(finalNumber));
    }

    private IEnumerator ActualStartRoll(int finalNumber)
    {
        float t = 0;

        while (t < 3 || selected != finalNumber)
        {
            selected += 1;

            sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/Ting"));

            if (selected >= votes.Count)
                selected = 0;

            yield return new WaitForSeconds(0.2f);

            t += 0.2f;
        }
    }      
}

public class Vote
{
    public int cup;
    public int track;

    public Vote(int nCup, int nTrack)
    {
        cup = nCup;
        track = nTrack;
    }
}