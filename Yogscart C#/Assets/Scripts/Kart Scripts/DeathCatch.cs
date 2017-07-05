using UnityEngine;
using System.Collections;

public class DeathCatch : MonoBehaviour {

    public ParticleSystem deathParticles;

	void OnTriggerEnter(Collider other)
    {
	    if(other.name == "DeathCatch")
        {
            deathParticles.Play();

            if(transform.GetComponent<PositionFinding>() != null && transform.GetComponent<KartMovement>() != null)
                transform.GetComponent<KartMovement>().expectedSpeed = 0;

            StartCoroutine("DoRespawn");

        }
	}

    IEnumerator DoRespawn()
    {
        yield return new WaitForSeconds(0.75f);

        GetComponent<Rigidbody>().isKinematic = true;

        if(transform.GetComponent<KartMovement>() != null)
        {
            PositionFinding pf = transform.GetComponent<PositionFinding>();

            Vector3 newPos = pf.closestPoint.transform.position;

            PointHandler nextPoint = pf.closestPoint.connections[0];
            for(int i = 1; i < pf.closestPoint.connections.Count; i++)
            {
                PointHandler ph = pf.closestPoint.connections[i];
                if (ph.percent > nextPoint.percent)
                    nextPoint = ph;
            }

            transform.position = newPos;
            transform.rotation = Quaternion.LookRotation(nextPoint.transform.position - newPos, Vector3.up);

            transform.GetComponent<KartMovement>().tricking = false;
        }

        GetComponent<Rigidbody>().isKinematic = false;
        yield return new WaitForSeconds(0.1f);

        deathParticles.Stop();
    }

}
