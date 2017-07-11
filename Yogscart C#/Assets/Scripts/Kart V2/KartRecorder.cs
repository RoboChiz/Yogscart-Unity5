using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Records the actions of a kart
[RequireComponent(typeof(KartMovement), typeof(Rigidbody))]
public class KartRecorder : MonoBehaviour
{
    public bool isRecording { get; private set; }

    private int frameCount, framesPerSecond;
    public float lastThrottle = -1;
    public float lastSteer = -1;
    public int lastDrift = -1, lastDriftSteer = -1;

    private KartMovement kartMovement;
    private Rigidbody kartRigidbody;

    public const string throttle = "t:", steer = "s:", drift = "d:", driftSteer = "z:", expectedSpeed = "e:", position = "p:", rotation = "r:", velocity = "v:", angularVelocity = "a:", recieveItem = "i:", itemAction = "c:", dropShield = "u:";
    public List<List<string>> actions; //Each List of strings represents all actions that happened that frame. Actions are recorded at Unity's Time Step value (40fps)

    public void Start()
    {
        kartMovement = GetComponent<KartMovement>();
        kartRigidbody = GetComponent<Rigidbody>();
        Reset();
    }

    public void Record()
    {
        if (!isRecording)
        {
            isRecording = true;
            framesPerSecond = (int)(1f / Time.fixedDeltaTime);
            frameCount = framesPerSecond;
        }
    }

    public void Pause()
    {
        isRecording = false;
    }

    public void Reset()
    {
        isRecording = false;
        lastThrottle = -1;
        lastSteer = -1;
        lastDrift = -1;
        lastDriftSteer = -1;
        actions = new List<List<string>>();
    }

    void FixedUpdate()
    {
        if(isRecording && Time.timeScale != 0)
        {
            List<string> currentFrame = new List<string>();
            actions.Add(currentFrame);

            bool throttleChanged = lastThrottle != kartMovement.throttle;
            bool steerChanged = lastSteer != kartMovement.steer;
            bool driftChange = lastDrift != (kartMovement.drift ? 1 : 0);
            bool driftSteerChange = lastDriftSteer != kartMovement.driftSteer;

            //Add Throttle, Steer and Drift if they have changed
            if (throttleChanged)
                currentFrame.Add(throttle + kartMovement.throttle);

            if (steerChanged)
                currentFrame.Add(steer + kartMovement.steer);

            if (driftChange)
                currentFrame.Add(drift + (kartMovement.drift ? 1 : 0));

            if (driftSteerChange)
                currentFrame.Add(driftSteer + kartMovement.driftSteer);

            frameCount++;
            if(frameCount >= framesPerSecond)
            {
                currentFrame.Add(expectedSpeed + kartMovement.expectedSpeed);
                currentFrame.Add(position + transform.position.x + " " + transform.position.y + " " + transform.position.z);
                currentFrame.Add(rotation + transform.rotation.eulerAngles.x + " " + transform.rotation.eulerAngles.y + " " + transform.rotation.eulerAngles.z);
                currentFrame.Add(velocity + kartRigidbody.velocity.x + " " + kartRigidbody.velocity.y + " " + kartRigidbody.velocity.z);
                currentFrame.Add(angularVelocity + kartRigidbody.angularVelocity.x + " " + kartRigidbody.angularVelocity.y + " " + kartRigidbody.angularVelocity.z);

                //Add Throttle, Steer and Drift if they were left out
                if(!throttleChanged)
                    currentFrame.Add(throttle + kartMovement.throttle);

                if (!steerChanged)
                    currentFrame.Add(steer + kartMovement.steer);

                if (!driftChange)
                    currentFrame.Add(drift + (kartMovement.drift ? 1 : 0));

                if (!driftSteerChange)
                    currentFrame.Add(driftSteer + kartMovement.driftSteer);

                frameCount = 0;
            }

            //Set Last Values at End
            lastThrottle = kartMovement.throttle;
            lastSteer = kartMovement.steer;
            lastDrift = (kartMovement.drift ? 1 : 0);
        }
    }

    public override string ToString()
    {
        string finalString = "";

        foreach(List<string> actionList in actions)
        {
            if (actionList.Count > 0)
            {
                foreach (string action in actionList)
                {
                    finalString += action + ";";
                }

                //Remove the last ; to stop errors
                finalString = finalString.Remove(finalString.Length - 1);
            }

            //Denote end of frame
            finalString += ">";
        }

        //Remove the last > to stop errors
        finalString = finalString.Remove(finalString.Length - 1);

        return finalString;
    }

    //Methods called by other components

    void RecievedItem(int item)
    {
        if (isRecording)
        {
            actions[actions.Count - 1].Add(recieveItem + item);
        }
    }

    //ia = 0
    void UsedItem()
    {
        if (isRecording)
        {
            actions[actions.Count - 1].Add(itemAction + 0);
        }
    }

    //ia = 1
    void UsedShield()
    {
        if (isRecording)
        {
            actions[actions.Count - 1].Add(itemAction + 1);
        }
    }

    //ia = 2
    void DroppedShield(float direction)
    {
        if (isRecording)
        {
            actions[actions.Count - 1].Add(dropShield + direction);
        }
    }
}
