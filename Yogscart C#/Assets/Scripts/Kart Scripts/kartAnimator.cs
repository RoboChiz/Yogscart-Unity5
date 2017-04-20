using UnityEngine;
using System.Collections;

public class kartAnimator : MonoBehaviour {

    kartScript ks;
    public Animator ani;
    private InterestManager interestManager;

    // Use this for initialization
    void Start ()
    {
        ks = transform.GetComponent<kartScript>();
        interestManager = FindObjectOfType<InterestManager>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        ani.SetBool("Drift", ks.drift);
        ani.SetFloat("Steer", ks.steer);

        //Send me as a PoI
        if (interestManager.CanAddInterestNow())
        {
            if (!ks.spinningOut)
                interestManager.AddInterest(transform, InterestType.Kart);
            else
                interestManager.AddInterest(transform, InterestType.Attack);
        }
    }
}
