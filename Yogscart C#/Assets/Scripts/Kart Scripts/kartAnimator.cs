using UnityEngine;
using System.Collections;

public class kartAnimator : MonoBehaviour {

    kartScript ks;
    public Animator ani;

	// Use this for initialization
	void Start ()
    {
        ks = transform.GetComponent<kartScript>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        ani.SetBool("Drift", ks.drift);
        ani.SetFloat("Steer", ks.steer);
    }
}
