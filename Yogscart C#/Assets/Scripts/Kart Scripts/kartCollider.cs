using UnityEngine;
using System.Collections;

//Use this script to cause a kart to bounce off the object as if it was a Kart
public class KartCollider : MonoBehaviour
{
    //Makes the object invulnerable
    public bool godMode;

	// Update is called once per frame
	void Update ()
    {
        if(gameObject.layer != 8)
            gameObject.layer = 8; //Set the layer of the object to Kart to ensure it collides properly
    }
}
