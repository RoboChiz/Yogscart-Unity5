using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

//Used on an Item's Online Model, Literally just attachs it's to the My Item in the Kart Item
public class ItemNetworker : NetworkBehaviour {

    private bool attachedKartItem;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        NetworkRaceClient.myKart.GetComponent<KartItem>().myItem = transform;
        transform.parent = NetworkRaceClient.myKart.transform;

        transform.parent.GetComponent<KartItem>().itemSpawned = true;
    }

    public void Update()
    {
        if(!hasAuthority && !attachedKartItem && transform.parent != null)
        {
            transform.parent.GetComponent<KartItem>().myItem = transform;
            transform.parent.GetComponent<KartItem>().itemSpawned = true;

            attachedKartItem = true;
        }
    }
}
