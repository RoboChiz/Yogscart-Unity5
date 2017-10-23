using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[NetworkSettings(channel = 0, sendInterval = 0.25f)]
public class KartPositionPass : NetworkBehaviour
{
    [SyncVar]
    private Vector3 syncPos;
    [SyncVar]
    private Quaternion syncRot;

    private Vector3 lastSyncPos;
    private Quaternion lastSyncRot;

    private KartMovement kartMovement;
    private bool isMine = false;

    void Awake()
    {
        syncPos = transform.position;
        syncRot = transform.rotation;         
    }

    private void Start()
    {
        kartMovement = GetComponent<KartMovement>();
    }

    //Called on client when Player is created
    public override void OnStartLocalPlayer()
    {
        isMine = true;
    }

    public void OnStartHost()
    {
        isMine = true;
    }

    void FixedUpdate()
    {
        if (isMine)
        {
            CmdTransmitPosition(transform.position, transform.rotation);
        }
        else
        {
            if (lastSyncPos != syncPos || lastSyncRot != syncRot)
            {
                lastSyncPos = syncPos;
                lastSyncRot = syncRot;

                //Move Kart to it's current position and slide kartbody over
                Vector3 lastPos = transform.position;
                Quaternion lastRot = transform.rotation;

                transform.position = syncPos;
                transform.rotation = syncRot;

                kartMovement.kartBody.position = lastPos;

                if (!kartMovement.spinningOut)
                    kartMovement.kartBody.rotation = lastRot;

                kartMovement.SlideKartBody();
            }
        }
    }

    [Command]
    void CmdTransmitPosition(Vector3 _pos, Quaternion _rot)
    {
        syncPos = _pos;
        syncRot = _rot;
    }
}
