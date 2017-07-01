using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirtblock : DamagingItem
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<KartScript>() != null)
        {
            //Spin the Kart Out
            collision.transform.GetComponent<KartScript>().SpinOut(true);

            //Make Owner Taunt
            DamagingItem di = GetComponent<DamagingItem>();
            if (di.owner != collision.transform.GetComponent<KartScript>())
                di.owner.DoTaunt();

            //Get rid of the GameObject
            Destroy(gameObject);
        }
        else if (collision.transform.GetComponent<Egg>()) //If hit another Power Up
        {
            Destroy(gameObject);
        }
    }
}
