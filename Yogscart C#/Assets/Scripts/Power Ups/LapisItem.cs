using UnityEngine;
using System.Collections;

public class LapisItem : MonoBehaviour {

    // Use this for initialization
    IEnumerator Start()
    {

        kartScript ks = transform.parent.GetComponent<kartScript>();
        ks.lapisAmount += 4;

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);

    }
}
