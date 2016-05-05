using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {

        kartScript ks = transform.parent.GetComponent<kartScript>();
        ks.Boost(2,kartScript.BoostMode.Boost);

        yield return null;
        Destroy(gameObject);

    }
	
}
