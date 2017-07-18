using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartMovement), typeof(Rigidbody))]
public class KartReplayer : MonoBehaviour
{
    public List<List<string>> replayData;

    public bool isPlaying { get; private set; }
    private int frameCount, framesPerSecond;

    public const char throttle = 't', steer = 's', drift = 'd', driftSteer = 'z', expectedSpeed = 'e', position = 'p', rotation = 'r', velocity = 'v', angularVelocity = 'a', recieveItem = 'i', itemAction = 'c', dropShield = 'u', startBoostVal = 'k';

    private KartMovement kartMovement;
    private KartItem kartItem;
    private Rigidbody kartRigidbody;

    public bool ignoreLocalStartBoost = false;


    public void Start()
    {
        kartMovement = GetComponent<KartMovement>();
        kartRigidbody = GetComponent<Rigidbody>();
        kartItem = GetComponent<KartItem>();
    }

    public void LoadReplay(string data)
    {
        replayData = new List<List<string>>();

        string[] frames = data.Split('>');

        foreach(string frame in frames)
        {
            //Create a new list of strings
            List<string> actionsList = new List<string>();
            replayData.Add(actionsList);

            string[] actions = frame.Split(';');
            actionsList.AddRange(actions);
        }
    }

    public void Play()
    {
        if (replayData != null)
            isPlaying = true;
        else
            Debug.Log("You must load a replay before you can play it!");
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Reset()
    {
        frameCount = 0;
        isPlaying = false;
    }

    public void SetFrame(int frame)
    {
        isPlaying = false;
        frameCount = frame;
        DoFrame();
    }

    void FixedUpdate()
    {
        if (isPlaying && Time.timeScale != 0)
        {
            if (frameCount < replayData.Count)
            {
                DoFrame();
            }

            frameCount ++;
        }
    }

    public void DoFrame()
    {
        //Check each action that happens in this frame
        foreach (string action in replayData[frameCount])
        {
            if (action != "" && action.Length >= 2)
            {
                string value = action.Remove(0, 2);

                switch (action[0])
                {
                    case throttle:
                        kartMovement.throttle = float.Parse(value);
                        break;
                    case steer:
                        kartMovement.steer = float.Parse(value);
                        break;
                    case drift:
                        kartMovement.drift = int.Parse(value) == 1;
                        break;
                    case expectedSpeed:
                        kartMovement.expectedSpeed = float.Parse(value);
                        break;
                    case position:
                        transform.position = ParseForVector3(value);
                        break;
                    case rotation:
                        transform.rotation = Quaternion.Euler(ParseForVector3(value));
                        break;
                    case velocity:
                        kartRigidbody.velocity = ParseForVector3(value);
                        break;
                    case angularVelocity:
                        kartRigidbody.angularVelocity = ParseForVector3(value);
                        break;
                    case recieveItem:
                        if (kartItem != null)
                        {
                            kartItem.RecieveItem(int.Parse(value));
                        }
                        break;
                    case itemAction:
                        if (kartItem != null)
                        {
                            int toDo = int.Parse(value);

                            switch (toDo)
                            {
                                case 0:
                                    kartItem.UseItem();
                                    break;
                                case 1:
                                    kartItem.UseShield();
                                    break;
                            }
                        }
                        break;
                    case dropShield:
                        if (kartItem != null)
                        {
                            kartItem.DropShield(float.Parse(value));
                        }
                        break;
                    case driftSteer:
                        kartMovement.driftSteer = int.Parse(value);
                        break;
                    case startBoostVal:
                        if(!ignoreLocalStartBoost)
                            KartMovement.startBoostVal = int.Parse(value);
                        break;
                }
            }
        }
    }

    Vector3 ParseForVector3(string value)
    {
        string[] axis = value.Split(' ');
        if (axis.Length != 3)
            throw new System.Exception("Data is in incorrect format");

        return new Vector3(float.Parse(axis[0]), float.Parse(axis[1]), float.Parse(axis[2]));
    }
}
