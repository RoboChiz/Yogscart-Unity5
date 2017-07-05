using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {

        KartMovement ks = transform.parent.GetComponent<KartMovement>();
        ks.Boost(2,KartMovement.BoostMode.Boost);

        yield return null;
        Destroy(gameObject);

    }
	
}
