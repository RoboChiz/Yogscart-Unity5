using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

//Used on an Item's Online Model, Literally just attachs it's to the My Item in the Kart Item
public class ItemNetworker : NetworkBehaviour {

    private bool attachedKartItem;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        NetworkRaceClient.myKart.GetComponent<kartItem>().myItem = transform;
        transform.parent = NetworkRaceClient.myKart.transform;

        transform.parent.GetComponent<kartItem>().itemSpawned = true;
    }

    public void Update()
    {
        if(!hasAuthority && !attachedKartItem && transform.parent != null)
        {
            transform.parent.GetComponent<kartItem>().myItem = transform;
            transform.parent.GetComponent<kartItem>().itemSpawned = true;

            attachedKartItem = true;
        }
    }
}
