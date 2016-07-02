using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkPass : NetworkBehaviour
{

    [SyncVar]
    private Vector3 syncPos;
    [SyncVar]
    private Quaternion syncRot;

    const float lerpRate = 15;

    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
    }

    void LerpPosition()
    {
        if (!isLocalPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
            transform.rotation = Quaternion.Lerp(transform.rotation, syncRot, Time.deltaTime * lerpRate);
        }
    }

    [Command]
    void CmdProvidePositiontoServer(Vector3 pos, Quaternion rot)
    {
        syncPos = pos;
        syncRot = rot;
    }

    [ClientCallback]
    void TransmitPosition()
    {
        if(isLocalPlayer)
        {
            CmdProvidePositiontoServer(transform.position, transform.rotation);
        }      
    }
}
