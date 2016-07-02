using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class KartNetworker : NetworkBehaviour
{
    [SyncVar]
    public int currentChar, currentHat, currentKart, currentWheel;
    //Make private later
    public int loadedChar = -1, loadedHat = -1, loadedKart = -1, loadedWheel = -1;



	// Update is called once per frame
	void FixedUpdate ()
    {
        //If loaded values do not match current then fix it
        if(currentChar != loadedChar || currentHat != loadedHat || currentKart != loadedKart || loadedWheel != currentWheel)
        {
            LoadKartModel();
        }
	
	}

    //Called on client when Player is created
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //Tell Client which Kart they own
        NetworkRaceClient.myKart = gameObject;
        FindObjectOfType<NetworkRaceClient>().SetupCameras();
       
    }

    void LoadKartModel()
    {
        //Spawn a new version of the Kart
        Transform newKart = FindObjectOfType<KartMaker>().SpawnKart(KartType.Local, transform.position, transform.rotation, currentChar, currentHat, currentKart, currentWheel);
        //Replace the values in the Original Kart Object to point to the new parts
        GetComponent<DeathCatch>().deathParticles = newKart.GetComponent<DeathCatch>().deathParticles;

        GetComponent<kartScript>().wheelColliders = newKart.GetComponent<kartScript>().wheelColliders;
        GetComponent<kartScript>().wheelMeshes = newKart.GetComponent<kartScript>().wheelMeshes;
        GetComponent<kartScript>().flameParticles = newKart.GetComponent<kartScript>().flameParticles;
        GetComponent<kartScript>().driftParticles = newKart.GetComponent<kartScript>().driftParticles;
        GetComponent<kartScript>().trickParticles = newKart.GetComponent<kartScript>().trickParticles;
        GetComponent<kartScript>().engineSound = newKart.GetComponent<kartScript>().engineSound;

        GetComponent<kartAnimator>().ani = newKart.GetComponent<kartAnimator>().ani;

        //Replace the existing model, colliders and canvas (Move into place of existing, then delete original)
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        while (newKart.childCount > 0)
        {
            if (newKart.GetChild(0).GetComponent<RectTransform>() == null)
            {
                newKart.GetChild(0).parent = transform;
            }
            else
            {
                newKart.GetChild(0).GetComponent<RectTransform>().SetParent(transform);
            }
        }

        Destroy(newKart.gameObject);

        loadedChar = currentChar;
        loadedHat = currentHat;
        loadedKart = currentKart;
        loadedWheel = currentWheel;
    }
}
