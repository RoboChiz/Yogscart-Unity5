using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkPass : NetworkBehaviour
{
    [SyncVar]
    public Vector3 syncPos;
    [SyncVar]
    public Quaternion syncRot;
    [SyncVar]
    private NetworkInstanceId parentID;

    private uint lastParentID = 0;

    const float lerpRate = 15;

    void Awake()
    {
        syncPos = transform.position;
        syncRot = transform.rotation;         
    }

    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
        UpdateParent();
    }

    void LerpPosition()
    {
        if (!hasAuthority)
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
        if(hasAuthority)
        {
            CmdProvidePositiontoServer(transform.position, transform.rotation);
        }      
    }

    void UpdateParent()
    {
        if (hasAuthority)
        {
            if(transform.parent != null)
            {
                parentID = transform.parent.GetComponent<NetworkIdentity>().netId;
            }
            else
            {
                parentID = new NetworkInstanceId(0);
            }            
        }
        else
        {
            if (lastParentID != parentID.Value)
            {
                if (parentID.IsEmpty())
                    transform.parent = null;
                else
                    transform.parent = ClientScene.FindLocalObject(parentID).transform;

                lastParentID = parentID.Value;
            }
        }
    }

}
