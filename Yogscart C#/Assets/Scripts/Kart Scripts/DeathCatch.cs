using UnityEngine;
using System.Collections;

public class DeathCatch : MonoBehaviour {

    public ParticleSystem deathParticles;

	void OnTriggerEnter(Collider other)
    {
	    if(other.name == "DeathCatch")
        {
            deathParticles.Play();

            if(transform.GetComponent<PositionFinding>() != null && transform.GetComponent<kartScript>() != null)
                transform.GetComponent<kartScript>().ExpectedSpeed = 0;

            StartCoroutine("DoRespawn");

        }
	}

    IEnumerator DoRespawn()
    {
        yield return new WaitForSeconds(0.75f);

        GetComponent<Rigidbody>().isKinematic = true;

        if(transform.GetComponent<kartScript>() != null)
        {
            TrackData td = GameObject.Find("Track Manager").GetComponent<TrackData>();
            PositionFinding pf = transform.GetComponent<PositionFinding>();

            Vector3 nPos = td.positionPoints[pf.currentPos].position;
            Vector3 nPos1;

            if (pf.currentPos + 1 >= td.positionPoints.Count)
                nPos1 = td.positionPoints[0].position;
            else
                nPos1 = td.positionPoints[pf.currentPos+1].position;

            transform.position = nPos;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.LookRotation(nPos1 - nPos, Vector3.up);
            transform.GetComponent<kartScript>().tricking = false;

        }

        GetComponent<Rigidbody>().isKinematic = false;
        yield return new WaitForSeconds(0.1f);

        deathParticles.Stop();
    }

}
