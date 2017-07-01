using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {

        KartScript ks = transform.parent.GetComponent<KartScript>();
        ks.Boost(2,KartScript.BoostMode.Boost);

        yield return null;
        Destroy(gameObject);

    }
	
}
