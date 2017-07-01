using UnityEngine;
using System.Collections;

public class LapisItem : MonoBehaviour {

    // Use this for initialization
    IEnumerator Start()
    {

        KartScript ks = transform.parent.GetComponent<KartScript>();
        ks.lapisAmount += 4;

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);

    }
}
